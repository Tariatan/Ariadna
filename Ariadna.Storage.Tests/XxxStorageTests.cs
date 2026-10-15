using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Storage.Tests;

[TestClass]
public sealed class XxxStorageTests
{
    [TestMethod]
    public void EnsureXxxCollection_ExistingSchemaTwo_PreservesRecordsAndCreatesVerifiedBackupOnce()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), $"Ariadna-xxx-upgrade-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var path = Path.Combine(root, "catalog.sqlite");
            CatalogDatabase.Create(path);
            var database = new CatalogDatabase(path);
            var store = new CatalogStore(database);
            var original = new CatalogDetails(new CatalogEntry { Title = "Original", OriginalTitle = "Original title", Path = "sample.mkv", Year = 2020 }, ["Drama"], [], [new PersonPhoto("Actor", null)]);
            var id = store.Save(CatalogKind.Movie, original);
            using (var connection = database.Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DROP TABLE XxxCast; DROP TABLE Xxx";
                command.ExecuteNonQuery();
            }

            // Act
            database.EnsureXxxCollection();
            database.EnsureXxxCollection();
            var backup = Directory.GetFiles(root, "*.before-xxx-*.sqlite").Single();
            var baseline = new CatalogStore(new CatalogDatabase(backup));

            // Assert
            Assert.AreEqual(baseline.GetEntry(CatalogKind.Movie, id), store.GetEntry(CatalogKind.Movie, id));
            Assert.AreEqual("Actor", store.GetDetails(CatalogKind.Movie, id)!.Actors.Single().Name);
            Assert.AreEqual("Drama", store.GetDetails(CatalogKind.Movie, id)!.Genres.Single());
            Assert.HasCount(0, store.Query(CatalogKind.Xxx, new CatalogQuery()));
            database.CheckIntegrity();
            new CatalogDatabase(backup).CheckIntegrity();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [TestMethod]
    public void Save_XxxWithFourActors_RejectsBeforeWriting()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), $"Ariadna-xxx-limit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var path = Path.Combine(root, "catalog.sqlite");
            CatalogDatabase.Create(path);
            var store = new CatalogStore(new CatalogDatabase(path));
            var details = new CatalogDetails(new CatalogEntry { Title = "Sample", Path = "sample.mp4" }, [], [], Enumerable.Range(1, 4).Select(index => new PersonPhoto($"Actor {index}", null)).ToArray());

            // Act
            Assert.ThrowsExactly<ArgumentException>(() => store.Save(CatalogKind.Xxx, details));

            // Assert
            Assert.HasCount(0, store.Query(CatalogKind.Xxx, new CatalogQuery()));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
