using System.Security.Cryptography;
using System.Text.Json;
using Ariadna.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Migration.Tests;

[TestClass]
public sealed class CatalogTransferTests
{
    private string directory = string.Empty;
    private string package = string.Empty;
    private readonly CatalogTransfer transfer = new(NullLogger.Instance);

    [TestInitialize]
    public void Initialize()
    {
        directory = Path.Combine(Path.GetTempPath(), "Ariadna-transfer-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        foreach (var folder in (string[])["movies", "documentary", "games", "library"])
        {
            Directory.CreateDirectory(Path.Combine(directory, folder));
        }
        var source = Path.Combine(directory, "source.sqlite");
        CatalogDatabase.Create(source);
        var database = new CatalogDatabase(source);
        using (var connection = database.Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO [Ignore](Id,path) VALUES (0,'Zero'); INSERT INTO Actor(Id,name,photo) VALUES (7,'Актёр',X'010203'); INSERT INTO Movie(Id,title,title_original,year,file_path,description,want_to_see) VALUES (5,'Чудо','Original',2026,'movie.mkv',NULL,NULL); INSERT INTO MovieCast(movieId,actorId) VALUES (5,7),(5,7); UPDATE sqlite_sequence SET seq=100 WHERE name='Movie'";
            command.ExecuteNonQuery();
        }
        File.WriteAllBytes(Path.Combine(directory, "movies", "5"), [9, 8, 7]);
        package = Path.Combine(directory, "package");
        transfer.Snapshot(source, directory, package);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(directory, true);

    [TestMethod]
    public void Import_SnapshotPackage_PreservesZeroIdSequenceNullsDuplicateRelationsAndPhotos()
    {
        // Arrange
        var target = Path.Combine(directory, "restored.sqlite");

        // Act
        transfer.Import(package, target);
        transfer.Verify(package, target);

        // Assert
        var store = new CatalogStore(new CatalogDatabase(target));
        var movie = store.GetDetails(CatalogKind.Movie, 5)!;
        Assert.IsNull(movie.Entry.Description);
        Assert.IsNull(movie.Entry.Wanted);
        Assert.HasCount(2, movie.Actors);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, movie.Actors.First().Photo);
        Assert.AreEqual(101, store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Next" }, [], [], [])));
        using var connection = new CatalogDatabase(target).Open(true);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM [Ignore] WHERE path='Zero'";
        Assert.AreEqual(0L, command.ExecuteScalar());
        CollectionAssert.AreEqual(new byte[] { 9, 8, 7 }, File.ReadAllBytes(Path.Combine(package, "assets", "movies", "5")));
    }

    [TestMethod]
    [DataRow("table")]
    [DataRow("asset")]
    public void Import_TamperedPackage_RejectsBeforeCreatingDestination(string component)
    {
        // Arrange
        var target = Path.Combine(directory, "rejected.sqlite");
        var file = component == "table" ? Path.Combine(package, "tables", "Movie.jsonl") : Path.Combine(package, "assets", "movies", "5");
        File.AppendAllText(file, "tampered");

        // Act
        Assert.Throws<InvalidDataException>(() => transfer.Import(package, target));

        // Assert
        Assert.IsFalse(File.Exists(target));
        Assert.HasCount(0, Directory.GetFiles(directory, "rejected.sqlite*"));
    }

    [TestMethod]
    public void Import_ValidHashesButBrokenForeignKey_LeavesFinalDestinationAbsent()
    {
        // Arrange
        var target = Path.Combine(directory, "rejected.sqlite");
        var tableFile = Path.Combine(package, "tables", "MovieCast.jsonl");
        File.WriteAllText(tableFile, "[1,999,7]\n[2,5,7]\n");
        var manifestPath = Path.Combine(package, "manifest.json");
        var manifest = JsonSerializer.Deserialize<ExportManifest>(File.ReadAllText(manifestPath))!;
        var tables = manifest.Tables.Select(table => table.Name == "MovieCast" ? table with { Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(tableFile))) } : table).ToArray();
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest with { Tables = tables }));

        // Act
        Assert.Throws<SqliteException>(() => transfer.Import(package, target));

        // Assert
        Assert.IsFalse(File.Exists(target));
        new CatalogDatabase(Path.Combine(directory, "source.sqlite")).CheckIntegrity();
    }

    [TestMethod]
    public void Import_ExistingDestination_RefusesOverwrite()
    {
        // Arrange
        var target = Path.Combine(directory, "existing.sqlite");
        File.WriteAllBytes(target, [1, 2, 3]);

        // Act
        Assert.Throws<IOException>(() => transfer.Import(package, target));

        // Assert
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(target));
    }
}
