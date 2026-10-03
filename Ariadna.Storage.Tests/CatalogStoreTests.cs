using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Storage.Tests;

[TestClass]
public sealed class CatalogStoreTests
{
    private string directory = string.Empty;
    private CatalogDatabase database = null!;
    private CatalogStore store = null!;
    private static readonly DateTime Now = new(2026, 10, 3, 12, 0, 0);

    [TestInitialize]
    public void Initialize()
    {
        directory = Path.Combine(Path.GetTempPath(), "Ariadna-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "catalog.sqlite");
        CatalogDatabase.Create(path);
        database = new CatalogDatabase(path);
        store = new CatalogStore(database);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Directory.Delete(directory, true);
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Query_UnicodeNameAndLiteralWildcards_SearchesTitleOriginalAndPath(CatalogKind kind)
    {
        // Arrange
        Save(kind, "ЧУДО_100%", original: "Original");
        Save(kind, "Other", original: "чудо_100%");
        Save(kind, "Third", path: @"C:\чудо_100%.mkv");
        Save(kind, "No match");

        // Act
        var result = store.Query(kind, new CatalogQuery { Name = "ЧуДо_100%" });

        // Assert
        Assert.HasCount(3, result);
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Query_GenreAndWishlistCombined_AppliesBothFilters(CatalogKind kind)
    {
        // Arrange
        var wanted = Save(kind, "Wanted", wanted: true, genres: ["Drama"]);
        Save(kind, "Not wanted", wanted: false, genres: ["Drama"]);
        Save(kind, "Wrong genre", wanted: true, genres: ["Comedy"]);

        // Act
        var result = store.Query(kind, new CatalogQuery { Wish = true, Genre = "drama" });

        // Assert
        CollectionAssert.AreEqual(new[] { wanted }, result.Select(entry => entry.Id).ToArray());
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Query_RecentFilter_PreservesCategoryWindowAndMidnightBoundary(CatalogKind kind)
    {
        // Arrange
        var cutoff = Now.AddMonths(kind == CatalogKind.Game ? -6 : -3);
        var recent = Save(kind, "Recent", date: DateOnly.FromDateTime(cutoff.AddDays(1)));
        Save(kind, "Boundary", date: DateOnly.FromDateTime(cutoff));
        Save(kind, "Old", date: DateOnly.FromDateTime(cutoff.AddDays(-1)));

        // Act
        var result = store.Query(kind, new CatalogQuery { Recent = true, Now = Now });

        // Assert
        CollectionAssert.AreEqual(new[] { recent }, result.Select(entry => entry.Id).ToArray());
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Query_NewFilter_ReturnsCurrentAndPreviousYear(CatalogKind kind)
    {
        // Arrange
        Save(kind, "Current", year: Now.Year);
        Save(kind, "Previous", year: Now.Year - 1);
        Save(kind, "Older", year: Now.Year - 2);
        Save(kind, "Future", year: Now.Year + 1);

        // Act
        var result = store.Query(kind, new CatalogQuery { New = true, Now = Now });

        // Assert
        Assert.HasCount(2, result);
        Assert.IsTrue(result.All(entry => entry.Year is 2026 or 2025));
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Query_UnknownLookup_PreservesUnfilteredResult(CatalogKind kind)
    {
        // Arrange
        Save(kind, "One");
        Save(kind, "Two");

        // Act
        var result = store.Query(kind, new CatalogQuery { Genre = "Missing", Director = "Missing", Actor = "Missing" });

        // Assert
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void Query_PeopleFilters_ReturnsMatchingMovieAndLibraryEntries()
    {
        // Arrange
        var movie = Save(CatalogKind.Movie, "Movie", directors: [new("Réka", [1, 2])], actors: [new("Actor", null)]);
        Save(CatalogKind.Movie, "Other movie");
        var book = Save(CatalogKind.Library, "Book", directors: [new("Author", [3])]);
        Save(CatalogKind.Library, "Other book");

        // Act
        var movies = store.Query(CatalogKind.Movie, new CatalogQuery { Director = "réka", Actor = "actor" });
        var books = store.Query(CatalogKind.Library, new CatalogQuery { Director = "author" });

        // Assert
        CollectionAssert.AreEqual(new[] { movie }, movies.Select(entry => entry.Id).ToArray());
        CollectionAssert.AreEqual(new[] { book }, books.Select(entry => entry.Id).ToArray());
    }

    [TestMethod]
    public void Query_VrFlags_ExcludesNullAndSupportsBothFlags()
    {
        // Arrange
        Save(CatalogKind.Game, "VR", vr: true);
        Save(CatalogKind.Game, "Desktop", vr: false);
        Save(CatalogKind.Game, "Unknown", vr: null);

        // Act
        var vr = store.Query(CatalogKind.Game, new CatalogQuery { Vr = true });
        var desktop = store.Query(CatalogKind.Game, new CatalogQuery { NonVr = true });
        var both = store.Query(CatalogKind.Game, new CatalogQuery { Vr = true, NonVr = true });

        // Assert
        Assert.AreEqual("VR", vr.Single().Title);
        Assert.AreEqual("Desktop", desktop.Single().Title);
        Assert.HasCount(0, both);
    }

    [TestMethod]
    public void Query_MovieLocationFilters_HandlesSeriesAndTemporaryMovies()
    {
        // Arrange
        Save(CatalogKind.Movie, "Series", path: @"S:\Series");
        Save(CatalogKind.Movie, "Temporary", path: @"S:\TMP\Movie");
        Save(CatalogKind.Movie, "Movie", path: @"M:\Movie.mkv");
        var query = new CatalogQuery { SeriesDrive = "S", MoviesDrive = "M", TemporaryMoviesPath = @"S:\TMP" };

        // Act
        var series = store.Query(CatalogKind.Movie, query with { Series = true });
        var movies = store.Query(CatalogKind.Movie, query with { Movies = true });

        // Assert
        Assert.AreEqual("Series", series.Single().Title);
        CollectionAssert.AreEquivalent(new[] { "Temporary", "Movie" }, movies.Select(entry => entry.Title).ToArray());
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Query_RecentOrdering_PreservesCategorySort(CatalogKind kind)
    {
        // Arrange
        var older = Save(kind, "A", date: new DateOnly(2026, 9, 1));
        var newer = Save(kind, "Z", date: new DateOnly(2026, 10, 1));

        // Act
        var result = store.Query(kind, new CatalogQuery { Recent = true, Now = Now });

        // Assert
        var expected = kind is CatalogKind.Movie or CatalogKind.Documentary ? new[] { newer, older } : new[] { older, newer };
        CollectionAssert.AreEqual(expected, result.Select(entry => entry.Id).ToArray());
    }

    [TestMethod]
    public void Save_EmptyRelations_ClearsCastDirectorsAuthorsAndGenres()
    {
        // Arrange
        var movie = Save(CatalogKind.Movie, "Movie", genres: ["Drama"], directors: [new("Director", [1])], actors: [new("Actor", [2])]);
        var book = Save(CatalogKind.Library, "Book", genres: ["Book genre"], directors: [new("Author", [3])]);

        // Act
        store.Save(CatalogKind.Movie, new CatalogDetails(store.GetDetails(CatalogKind.Movie, movie)!.Entry, [], [], []));
        store.Save(CatalogKind.Library, new CatalogDetails(store.GetDetails(CatalogKind.Library, book)!.Entry, [], [], []));

        // Assert
        var movieDetails = store.GetDetails(CatalogKind.Movie, movie)!;
        var bookDetails = store.GetDetails(CatalogKind.Library, book)!;
        Assert.HasCount(0, movieDetails.Genres);
        Assert.HasCount(0, movieDetails.Directors);
        Assert.HasCount(0, movieDetails.Actors);
        Assert.HasCount(0, bookDetails.Genres);
        Assert.HasCount(0, bookDetails.Directors);
        database.CheckIntegrity();
    }

    [TestMethod]
    public void Save_DuplicatePaths_ReturnsActualInsertedIdAndUpdatesById()
    {
        // Arrange
        var first = Save(CatalogKind.Movie, "First", path: @"M:\Same.mkv");

        // Act
        var second = Save(CatalogKind.Movie, "Second", path: @"M:\Same.mkv");
        var details = store.GetDetails(CatalogKind.Movie, second)!;
        store.Save(CatalogKind.Movie, details with { Entry = details.Entry with { Title = "Edited" } });

        // Assert
        Assert.AreNotEqual(first, second);
        Assert.AreEqual("First", store.GetDetails(CatalogKind.Movie, first)!.Entry.Title);
        Assert.AreEqual("Edited", store.GetDetails(CatalogKind.Movie, second)!.Entry.Title);
    }

    [TestMethod]
    public void Save_ImagePromotionFailure_RollsBackDatabaseRelationsAndEarlierImage()
    {
        // Arrange
        var id = Save(CatalogKind.Game, "Before", genres: ["Old"]);
        var assetRoot = Path.Combine(directory, "images");
        Directory.CreateDirectory(assetRoot);
        File.WriteAllBytes(Path.Combine(assetRoot, id.ToString()), [9]);
        File.WriteAllBytes(Path.Combine(assetRoot, $"{id}_preview1"), [8]);
        using var blocked = new FileStream(Path.Combine(assetRoot, $"{id}_preview1"), FileMode.Open, FileAccess.Read, FileShare.Read);
        var original = store.GetDetails(CatalogKind.Game, id)!;
        var assets = new CatalogAssets(assetRoot, new Dictionary<string, byte[]> { [string.Empty] = [1], ["_preview1"] = [2] });

        // Act
        Assert.Throws<IOException>(() => store.Save(CatalogKind.Game, original with { Entry = original.Entry with { Title = "After" }, Genres = ["New"] }, assets));

        // Assert
        Assert.AreEqual("Before", store.GetDetails(CatalogKind.Game, id)!.Entry.Title);
        CollectionAssert.AreEqual(new[] { "Old" }, store.GetDetails(CatalogKind.Game, id)!.Genres.ToArray());
        CollectionAssert.AreEqual(new byte[] { 9 }, File.ReadAllBytes(Path.Combine(assetRoot, id.ToString())));
    }

    [TestMethod]
    public void Save_CommittedImages_ReopenFinalizesJournalAndPreservesData()
    {
        // Arrange
        var assets = new CatalogAssets(Path.Combine(directory, "images"), new Dictionary<string, byte[]> { [string.Empty] = [1, 2, 3] });

        // Act
        var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Saved", Wanted = null }, [], [], []), assets);
        database.RecoverAssets();

        // Assert
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(Path.Combine(directory, "images", id.ToString())));
        Assert.IsNull(store.GetDetails(CatalogKind.Movie, id)!.Entry.Wanted);
        Assert.HasCount(0, Directory.GetDirectories(directory, ".ariadna-save-*"));
    }

    [TestMethod]
    public void RecoverAssets_InterruptedBeforeCommit_RestoresOldImagesAndRemovesNewImages()
    {
        // Arrange
        var root = Path.Combine(directory, "images");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, "42"), [9]);
        var assets = new CatalogAssets(root, new Dictionary<string, byte[]> { [string.Empty] = [1], ["_preview1"] = [2] });
        assets.Prepare(directory, 42);
        assets.Promote();

        // Act
        database.RecoverAssets();

        // Assert
        CollectionAssert.AreEqual(new byte[] { 9 }, File.ReadAllBytes(Path.Combine(root, "42")));
        Assert.IsFalse(File.Exists(Path.Combine(root, "42_preview1")));
        Assert.HasCount(0, Directory.GetDirectories(directory, ".ariadna-save-*"));
    }

    [TestMethod]
    public void RecoverAssets_InterruptedDuringStaging_RemovesIncompleteJournalDirectory()
    {
        // Arrange
        var staging = Path.Combine(directory, ".ariadna-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        File.WriteAllBytes(Path.Combine(staging, "poster.new"), [1]);

        // Act
        database.RecoverAssets();

        // Assert
        Assert.IsFalse(Directory.Exists(staging));
        database.CheckIntegrity();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void RecoverAssets_ReadOnlyEmptyRecoveryDirectory_RemovesDirectoryAndCommitMarker(bool committed)
    {
        // Arrange
        var token = Guid.NewGuid().ToString("N");
        var staging = Path.Combine(directory, ".ariadna-save-" + token);
        Directory.CreateDirectory(staging);
        File.SetAttributes(staging, File.GetAttributes(staging) | FileAttributes.ReadOnly);
        if (committed)
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO CatalogAssetCommit(token) VALUES ($token)";
            command.Parameters.AddWithValue("$token", token);
            command.ExecuteNonQuery();
        }

        try
        {
            // Act
            database.RecoverAssets();

            // Assert
            Assert.IsFalse(Directory.Exists(staging));
            using var connection = database.Open(true);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM CatalogAssetCommit";
            Assert.AreEqual(0L, command.ExecuteScalar());
            database.CheckIntegrity();
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                File.SetAttributes(staging, File.GetAttributes(staging) & ~FileAttributes.ReadOnly);
            }
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void RecoverAssets_ReadOnlyRecoveryDirectory_PreservesCommittedImagesOrRestoresUncommittedImages(bool committed)
    {
        // Arrange
        var root = Path.Combine(directory, "images");
        Directory.CreateDirectory(root);
        var poster = Path.Combine(root, "42");
        var preview = Path.Combine(root, "42_preview1");
        byte[] expectedPoster = committed ? [1] : [9];
        byte[] expectedPreview = [2];
        File.WriteAllBytes(poster, [9]);
        var assets = new CatalogAssets(root, new Dictionary<string, byte[]> { [string.Empty] = [1], ["_preview1"] = [2] });
        assets.Prepare(directory, 42);
        assets.Promote();
        var staging = Directory.GetDirectories(directory, ".ariadna-save-*").Single();
        File.SetAttributes(staging, File.GetAttributes(staging) | FileAttributes.ReadOnly);
        if (committed)
        {
            using var connection = database.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO CatalogAssetCommit(token) VALUES ($token)";
            command.Parameters.AddWithValue("$token", assets.Token);
            command.ExecuteNonQuery();
        }

        try
        {
            // Act
            database.RecoverAssets();

            // Assert
            CollectionAssert.AreEqual(expectedPoster, File.ReadAllBytes(poster));
            if (committed)
            {
                CollectionAssert.AreEqual(expectedPreview, File.ReadAllBytes(preview));
            }
            else
            {
                Assert.IsFalse(File.Exists(preview));
            }
            Assert.IsFalse(Directory.Exists(staging));
            database.CheckIntegrity();
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                File.SetAttributes(staging, File.GetAttributes(staging) & ~FileAttributes.ReadOnly);
            }
        }
    }

    [TestMethod]
    public void Delete_RelatedEntry_RemovesRelationsAndKeepsSharedPeople()
    {
        // Arrange
        var first = Save(CatalogKind.Movie, "First", genres: ["Shared"], actors: [new("Shared actor", [1])]);
        var second = Save(CatalogKind.Movie, "Second", genres: ["Shared"], actors: [new("Shared actor", [1])]);

        // Act
        store.Delete(CatalogKind.Movie, first);

        // Assert
        Assert.IsNull(store.GetDetails(CatalogKind.Movie, first));
        Assert.HasCount(1, store.GetDetails(CatalogKind.Movie, second)!.Actors);
        database.CheckIntegrity();
    }

    [TestMethod]
    public void GetRegisteredPaths_MixedCaseAndIgnore_PreservesWindowsPathComparison()
    {
        // Arrange
        Save(CatalogKind.Movie, "Movie", path: @"M:\Путь\Movie.mkv");
        store.Ignore(@"M:\Ignored.mkv");
        store.Ignore(@"m:\ignored.mkv");

        // Act
        var paths = store.GetRegisteredPaths(CatalogKind.Movie);

        // Assert
        Assert.HasCount(2, paths);
        Assert.IsTrue(paths.Any(path => CatalogStore.PathsEqual(path, @"m:\путь\movie.MKV")));
    }

    [TestMethod]
    public void Save_ConcurrentConnections_PersistsBothCompleteEntries()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 4).Select(index => Task.Run(() => Save(CatalogKind.Movie, $"Movie {index}", genres: ["Shared"]))).ToArray();

        // Act
        Task.WaitAll(tasks);

        // Assert
        Assert.HasCount(4, store.Query(CatalogKind.Movie, new CatalogQuery()));
        database.CheckIntegrity();
    }

    [TestMethod]
    public void Backup_ExistingCatalog_RestoresCompleteSnapshot()
    {
        // Arrange
        Save(CatalogKind.Movie, "Movie", actors: [new("Actor", [1, 2, 3])]);
        var backupPath = Path.Combine(directory, "backup.sqlite");

        // Act
        database.Backup(backupPath);

        // Assert
        var restored = new CatalogStore(new CatalogDatabase(backupPath));
        var entry = restored.Query(CatalogKind.Movie, new CatalogQuery()).Single();
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, restored.GetDetails(CatalogKind.Movie, entry.Id)!.Actors.Single().Photo);
        Assert.Throws<IOException>(() => database.Backup(backupPath));
    }

    [TestMethod]
    public void Open_MissingOrForeignDatabase_RefusesImplicitCatalogCreation()
    {
        // Arrange
        var missing = Path.Combine(directory, "missing.sqlite");
        var foreign = Path.Combine(directory, "foreign.sqlite");
        File.WriteAllBytes(foreign, []);

        // Act
        Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() => new CatalogDatabase(missing).Open());
        Assert.Throws<InvalidDataException>(() => new CatalogDatabase(foreign).Open());

        // Assert
        Assert.IsFalse(File.Exists(missing));
    }

    private int Save(CatalogKind kind, string title, string original = "", string? path = null, bool? wanted = null, DateOnly? date = null, int year = 2020, bool? vr = null, string[]? genres = null, PersonPhoto[]? directors = null, PersonPhoto[]? actors = null)
        => store.Save(kind, new CatalogDetails(new CatalogEntry
        {
            Title = title,
            OriginalTitle = original,
            Path = path ?? title,
            Wanted = wanted,
            CreationDate = date,
            Year = year,
            Vr = vr,
        }, genres ?? [], directors ?? [], actors ?? []));
}
