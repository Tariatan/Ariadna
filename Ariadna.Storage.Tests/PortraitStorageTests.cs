using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Storage.Tests;

[TestClass]
public sealed class PortraitStorageTests
{
    private string directory = string.Empty;
    private CatalogDatabase database = null!;
    private CatalogStore store = null!;

    [TestInitialize]
    public void Initialize()
    {
        directory = Path.Combine(Path.GetTempPath(), "Ariadna-portraits-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        database = new CatalogDatabase(Path.Combine(directory, "catalog.sqlite"));
        CatalogDatabase.Create(Path.Combine(directory, "catalog.sqlite"));
        store = new CatalogStore(database);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(directory, true);

    [TestMethod]
    public void Save_SameTmdbPersonInTwoRoles_SharesOnePortraitAndIdentity()
    {
        // Arrange
        var person = new PersonPhoto("Same Person", [1, 2, 3]) { TmdbId = 123 };

        // Act
        var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie" }, [], [person], [person]));
        var details = store.GetDetails(CatalogKind.Movie, id)!;

        // Assert
        Assert.AreEqual(details.Actors.Single().PersonId, details.Directors.Single().PersonId);
        Assert.AreEqual(123, details.Actors.Single().TmdbId);
        Assert.HasCount(1, Directory.GetFiles(Path.Combine(directory, "people")));
        CollectionAssert.AreEqual(person.Photo!, details.Actors.Single().Photo!);
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Person WHERE tmdbId=123";
        Assert.AreEqual(1L, command.ExecuteScalar());
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Actor') WHERE name='photo'";
        Assert.AreEqual(0L, command.ExecuteScalar());
        database.CheckIntegrity();
    }

    [TestMethod]
    public void Save_DifferentTmdbPeopleWithSameName_RemainDistinct()
    {
        // Arrange
        var first = new PersonPhoto("Same Name", [1]) { TmdbId = 123 };
        var second = new PersonPhoto("Same Name", [2]) { TmdbId = 456 };

        // Act
        var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie" }, [], [], [first, second]));
        var people = store.GetDetails(CatalogKind.Movie, id)!.Actors.ToArray();

        // Assert
        Assert.HasCount(2, people);
        Assert.AreNotEqual(people[0].Id, people[1].Id);
        Assert.AreNotEqual(people[0].PersonId, people[1].PersonId);
        CollectionAssert.AreEqual((int[])[123, 456], people.Select(person => person.TmdbId!.Value).ToArray());
        Assert.HasCount(2, Directory.GetFiles(Path.Combine(directory, "people")));
    }

    [TestMethod]
    public void Save_RenamedPersonWithId_PreservesIdentityRelationshipAndPortrait()
    {
        // Arrange
        var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie" }, [], [], [new("Before", [1, 2]) { TmdbId = 123 }]));
        var details = store.GetDetails(CatalogKind.Movie, id)!;
        var before = details.Actors.Single();
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM MovieCast";
        var relationshipId = command.ExecuteScalar();

        // Act
        store.Save(CatalogKind.Movie, details with { Actors = [before with { Name = "After" }] });
        var after = store.GetDetails(CatalogKind.Movie, id)!.Actors.Single();

        // Assert
        Assert.AreEqual(before.Id, after.Id);
        Assert.AreEqual(before.PersonId, after.PersonId);
        Assert.AreEqual(relationshipId, command.ExecuteScalar());
        Assert.AreEqual("After", after.Name);
        CollectionAssert.AreEqual(before.Photo!, after.Photo!);
    }

    [TestMethod]
    public void Save_NewRoleForExistingSharedId_UsesIdRatherThanAnotherPersonsName()
    {
        // Arrange
        var firstId = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "One" }, [], [], [new("Original Name", [1])]));
        var secondId = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Two" }, [], [new("Same Name", [2])], []));
        var identity = store.GetDetails(CatalogKind.Movie, firstId)!.Actors.Single().PersonId;
        var details = store.GetDetails(CatalogKind.Movie, secondId)!;

        // Act
        store.Save(CatalogKind.Movie, details with { Directors = [new("Same Name", [1]) { PersonId = identity }] });
        var director = store.GetDetails(CatalogKind.Movie, secondId)!.Directors.Single();

        // Assert
        Assert.AreEqual(identity, director.PersonId);
        CollectionAssert.AreEqual((byte[])[1], director.Photo!);
        Assert.AreNotEqual(details.Directors.Single().Id, director.Id);
    }

    [TestMethod]
    public void GetDetails_MissingPortraitFile_ReturnsMissingPhoto()
    {
        // Arrange
        var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie" }, [], [], [new("Person", [1, 2])]));
        var person = store.GetDetails(CatalogKind.Movie, id)!.Actors.Single();
        File.Delete(Path.Combine(directory, "people", $"{person.PersonId}.png"));

        // Act
        var missing = store.GetDetails(CatalogKind.Movie, id)!.Actors.Single();

        // Assert
        Assert.IsNull(missing.Photo);
        Assert.AreEqual(person.Id, missing.Id);
        Assert.AreEqual("Person", missing.Name);
    }

    [TestMethod]
    public void Save_PortraitPromotionFails_RollsBackMetadataRelationshipsAndFiles()
    {
        // Arrange
        var root = Path.Combine(directory, "blocked");
        File.WriteAllText(root, "not a directory");
        var blocked = new CatalogStore(database, root);

        // Act
        Assert.Throws<IOException>(() => blocked.Save(CatalogKind.Movie,
            new CatalogDetails(new CatalogEntry { Title = "Unsaved" }, [], [], [new("Person", [1])])));
        database.RecoverAssets();

        // Assert
        Assert.IsEmpty(store.Query(CatalogKind.Movie, new CatalogQuery()));
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Person";
        Assert.AreEqual(0L, command.ExecuteScalar());
        Assert.IsEmpty(Directory.GetDirectories(directory, ".ariadna-save-*"));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void RecoverAssets_PortraitSaveInterrupted_RestoresOrKeepsFiles(bool committed)
    {
        // Arrange
        var root = Path.Combine(directory, "people");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, "5.png"), [1]);
        var assets = new CatalogAssets(directory, new Dictionary<string, byte[]>());
        assets.AddPortrait(root, 5, [2]);
        assets.AddPortrait(root, 6, [3]);
        assets.Prepare(directory, 42);
        assets.Promote();
        if (committed)
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO CatalogAssetCommit(token) VALUES ($token)";
            command.Parameters.AddWithValue("$token", assets.Token);
            command.ExecuteNonQuery();
        }

        // Act
        database.RecoverAssets();

        // Assert
        CollectionAssert.AreEqual((byte[])[committed ? (byte)2 : (byte)1], File.ReadAllBytes(Path.Combine(root, "5.png")));
        Assert.AreEqual(committed, File.Exists(Path.Combine(root, "6.png")));
        Assert.IsEmpty(Directory.GetDirectories(directory, ".ariadna-save-*"));
    }

    [TestMethod]
    public void Save_AssigningTmdbIdToExistingPerson_PreservesLocalIdentity()
    {
        // Arrange
        var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie" }, [], [], [new("Person", [1, 2])]));
        var details = store.GetDetails(CatalogKind.Movie, id)!;
        var original = details.Actors.Single();

        // Act
        store.Save(CatalogKind.Movie, details with { Actors = [original with { TmdbId = 123 }] });
        var updated = store.GetDetails(CatalogKind.Movie, id)!.Actors.Single();

        // Assert
        Assert.AreEqual(original.Id, updated.Id);
        Assert.AreEqual(original.PersonId, updated.PersonId);
        Assert.AreEqual(123, updated.TmdbId);
        Assert.HasCount(1, Directory.GetFiles(Path.Combine(directory, "people")));
    }

    [TestMethod]
    public void ResetPortraitStorage_VerifiedSchemaOneBackup_DropsPhotosAndPreservesCatalog()
    {
        // Arrange
        var (source, backup) = CreateSchemaOne();

        // Act
        var reset = new CatalogDatabase(source);
        reset.ResetPortraitStorage(backup);

        // Assert
        using var connection = reset.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Actor') WHERE name='photo'";
        Assert.AreEqual(0L, command.ExecuteScalar());
        command.CommandText = "SELECT name FROM Actor WHERE Id=0";
        Assert.AreEqual("Original Actor", command.ExecuteScalar());
        command.CommandText = "SELECT group_concat(Id) FROM MovieCast ORDER BY Id";
        Assert.AreEqual("7,8", command.ExecuteScalar());
        command.CommandText = "SELECT seq FROM sqlite_sequence WHERE name='Actor'";
        Assert.AreEqual(50L, command.ExecuteScalar());
        command.CommandText = "SELECT COUNT(*) FROM Person";
        Assert.AreEqual(2L, command.ExecuteScalar());
        Assert.IsNull(new CatalogStore(reset).GetDetails(CatalogKind.Movie, 5)!.Actors.First().Photo);
        reset.CheckIntegrity();
        Assert.Throws<InvalidDataException>(() => reset.ResetPortraitStorage(backup));
    }

    [TestMethod]
    public void ResetPortraitStorage_BackupDoesNotMatch_LeavesOriginalDatabaseUntouched()
    {
        // Arrange
        var (source, backup) = CreateSchemaOne();
        using (var connection = OpenRaw(backup))
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "UPDATE Actor SET name='Different' WHERE Id=0";
            command.ExecuteNonQuery();
        }

        // Act
        Assert.Throws<InvalidDataException>(() => new CatalogDatabase(source).ResetPortraitStorage(backup));

        // Assert
        using var original = OpenRaw(source);
        using var query = original.CreateCommand();
        query.CommandText = "PRAGMA user_version";
        Assert.AreEqual(1L, query.ExecuteScalar());
        query.CommandText = "SELECT hex(photo) FROM Actor WHERE Id=0";
        Assert.AreEqual("010203", query.ExecuteScalar());
    }

    private (string Source, string Backup) CreateSchemaOne()
    {
        var source = Path.Combine(directory, "schema-one.sqlite");
        var backup = Path.Combine(directory, "before.sqlite");
        using var stream = typeof(CatalogDatabase).Assembly.GetManifestResourceStream("Ariadna.Storage.Schema.sql")!;
        using var reader = new StreamReader(stream);
        var schema = Regex.Replace(reader.ReadToEnd(), @"CREATE TABLE \[Person\] \([\s\S]*?\);\r?\n", string.Empty)
            .Replace("[personId] INTEGER NOT NULL REFERENCES [Person]([Id])", "[photo] BLOB")
            .Replace("PRAGMA user_version=2", "PRAGMA user_version=1");
        schema = Regex.Replace(schema, @"CREATE INDEX \[IX_(Actor|Author|Director)_personId\].*?;\r?\n", string.Empty);
        using var connection = OpenRaw(source);
        using var command = connection.CreateCommand();
        command.CommandText = schema;
        command.ExecuteNonQuery();
        command.CommandText = "INSERT INTO Actor(Id,name,photo) VALUES (0,'Original Actor',X'010203'); INSERT INTO Director(Id,name,photo) VALUES (0,'Original Director',X'0405'); INSERT INTO Movie(Id,title,title_original,year,file_path) VALUES (5,'Movie','Original',2026,'movie.mkv'); INSERT INTO MovieCast(Id,movieId,actorId) VALUES (7,5,0),(8,5,0); INSERT INTO MovieDirector(movieId,directorId) VALUES (5,0); UPDATE sqlite_sequence SET seq=50 WHERE name='Actor'";
        command.ExecuteNonQuery();
        using var target = OpenRaw(backup);
        connection.BackupDatabase(target);
        return (source, backup);
    }

    private static SqliteConnection OpenRaw(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString());
        connection.Open();
        connection.CreateCollation("ARIADNA", (left, right) => StringComparer.OrdinalIgnoreCase.Compare(left, right));
        return connection;
    }
}
