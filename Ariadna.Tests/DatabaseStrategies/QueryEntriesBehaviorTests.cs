extern alias ariadna;
using Ariadna.DatabaseStrategies;
using Ariadna.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DocumentaryEntity = ariadna::DbProvider.Documentary;
using DocumentaryGenreEntity = ariadna::DbProvider.DocumentaryGenre;
using GameEntity = ariadna::DbProvider.Game;
using LibraryAuthorEntity = ariadna::DbProvider.LibraryAuthor;
using LibraryEntity = ariadna::DbProvider.Library;
using LibraryGenreEntity = ariadna::DbProvider.LibraryGenre;
using MovieCastEntity = ariadna::DbProvider.MovieCast;
using MovieDirectorEntity = ariadna::DbProvider.MovieDirector;
using MovieGenreEntity = ariadna::DbProvider.MovieGenre;
using MovieEntity = ariadna::DbProvider.Movie;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class QueryEntriesBehaviorTests
{
    private static readonly string DefaultMoviesPath = TestSettings.Get("DefaultMoviesPath");
    private static readonly string DefaultMoviesPathTmp2 = TestSettings.Get("DefaultMoviesPathTMP2");
    private static readonly string DefaultSeriesPath = TestSettings.Get("DefaultSeriesPath");

    [TestMethod]
    public void QueryEntries_MoviesRecentFilter_AppliesDescendingCreationTimeOrder()
    {
        // Arrange
        var newerMovie = new MovieEntity { Id = 1, title = "Alien", creation_time = DateTime.Now.AddDays(-1), file_path = @"A:\Movies\Alien.mkv" };
        var olderMovie = new MovieEntity { Id = 2, title = "Blade Runner", creation_time = DateTime.Now.AddDays(-10), file_path = @"A:\Movies\BladeRunner.mkv" };
        var testee = new TestMoviesDbStrategy(new[] { olderMovie, newerMovie }.AsQueryable(), _ => null, _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsRecent = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Alien", "Blade Runner" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_MoviesDirectorFilter_ReturnsOnlyDirectedMovies()
    {
        // Arrange
        var targetMovie = new MovieEntity
        {
            Id = 1,
            title = "Terminator",
            file_path = @"A:\Movies\Terminator.mkv",
            MovieDirectors = { new MovieDirectorEntity { directorId = 7 } },
        };
        var otherMovie = new MovieEntity
        {
            Id = 2,
            title = "Aliens",
            file_path = @"A:\Movies\Aliens.mkv",
            MovieDirectors = { new MovieDirectorEntity { directorId = 8 } },
        };
        var testee = new TestMoviesDbStrategy(new[] { targetMovie, otherMovie }.AsQueryable(), name => name == "James Cameron" ? 7 : null, _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Director = "James Cameron" });

        // Assert
        CollectionAssert.AreEqual(new[] { "Terminator" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_MoviesActorFilter_ReturnsOnlyMatchingMovies()
    {
        // Arrange
        var targetMovie = new MovieEntity
        {
            Id = 1,
            title = "The Matrix",
            file_path = @"A:\Movies\TheMatrix.mkv",
            MovieCasts = { new MovieCastEntity { actorId = 4 } },
        };
        var otherMovie = new MovieEntity
        {
            Id = 2,
            title = "Minority Report",
            file_path = @"A:\Movies\MinorityReport.mkv",
            MovieCasts = { new MovieCastEntity { actorId = 5 } },
        };
        var testee = new TestMoviesDbStrategy(new[] { otherMovie, targetMovie }.AsQueryable(), _ => null, name => name == "Keanu Reeves" ? 4 : null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Actor = "Keanu Reeves" });

        // Assert
        CollectionAssert.AreEqual(new[] { "The Matrix" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_MoviesGenreFilter_ReturnsOnlyMatchingMovies()
    {
        // Arrange
        var targetMovie = new MovieEntity
        {
            Id = 1,
            title = "Se7en",
            file_path = @"A:\Movies\Se7en.mkv",
            MovieGenres = { new MovieGenreEntity { genreId = 9 } },
        };
        var otherMovie = new MovieEntity
        {
            Id = 2,
            title = "Toy Story",
            file_path = @"A:\Movies\ToyStory.mkv",
            MovieGenres = { new MovieGenreEntity { genreId = 10 } },
        };
        var testee = new TestMoviesDbStrategy(new[] { otherMovie, targetMovie }.AsQueryable(), _ => null, _ => null, name => name == "Thriller" ? 9 : null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Genre = "Thriller" });

        // Assert
        CollectionAssert.AreEqual(new[] { "Se7en" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_MoviesSeriesFilter_ReturnsOnlySeriesEntriesOutsideTmpMoviesPath()
    {
        // Arrange
        var tmpMoviePath = System.IO.Path.Combine(DefaultMoviesPathTmp2, "Alien.mkv");
        var seriesPath = System.IO.Path.Combine(DefaultSeriesPath, "X-Files");
        var sameDriveMoviePath = DefaultSeriesPath.Substring(0, 1) + @":\Movies\Terminator.mkv";
        var seriesEntry = new MovieEntity { Id = 1, title = "X-Files", file_path = seriesPath };
        var tmpMovieEntry = new MovieEntity { Id = 2, title = "Alien", file_path = tmpMoviePath };
        var sameDriveMovieEntry = new MovieEntity { Id = 3, title = "Terminator", file_path = sameDriveMoviePath };
        var testee = new TestMoviesDbStrategy(new[] { sameDriveMovieEntry, tmpMovieEntry, seriesEntry }.AsQueryable(), _ => null, _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsSeries = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Terminator", "X-Files" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_MoviesMoviesFilter_ReturnsDefaultMoviesAndTmpMoviesEntries()
    {
        // Arrange
        var defaultMoviePath = System.IO.Path.Combine(DefaultMoviesPath, "Arrival.mkv");
        var tmpMoviePath = System.IO.Path.Combine(DefaultMoviesPathTmp2, "Alien.mkv");
        var seriesPath = System.IO.Path.Combine(DefaultSeriesPath, "Fringe");
        var defaultMovieEntry = new MovieEntity { Id = 1, title = "Arrival", file_path = defaultMoviePath };
        var tmpMovieEntry = new MovieEntity { Id = 2, title = "Alien", file_path = tmpMoviePath };
        var seriesEntry = new MovieEntity { Id = 3, title = "Fringe", file_path = seriesPath };
        var testee = new TestMoviesDbStrategy(new[] { seriesEntry, tmpMovieEntry, defaultMovieEntry }.AsQueryable(), _ => null, _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsMovies = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Alien", "Arrival" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_DocumentariesGenreFilter_ReturnsOnlyMatchingDocumentaries()
    {
        // Arrange
        var matchingDocumentary = new DocumentaryEntity
        {
            Id = 1,
            title = "Planet Earth",
            file_path = @"A:\Docs\PlanetEarth.mkv",
            DocumentaryGenres = { new DocumentaryGenreEntity { genreId = 3 } },
        };
        var otherDocumentary = new DocumentaryEntity
        {
            Id = 2,
            title = "Apollo 11",
            file_path = @"A:\Docs\Apollo11.mkv",
            DocumentaryGenres = { new DocumentaryGenreEntity { genreId = 4 } },
        };
        var testee = new TestDocumentariesDbStrategy(new[] { otherDocumentary, matchingDocumentary }.AsQueryable(), name => name == "Nature" ? 3 : null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Genre = "Nature" });

        // Assert
        CollectionAssert.AreEqual(new[] { "Planet Earth" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_DocumentariesNameFilter_MatchesTitleOriginalTitleAndPath()
    {
        // Arrange
        var titleMatch = new DocumentaryEntity { Id = 1, title = "Ocean World", title_original = "Ocean World", file_path = @"A:\Docs\BluePlanet.mkv" };
        var originalTitleMatch = new DocumentaryEntity { Id = 2, title = "Earth", title_original = "Deep Ocean", file_path = @"A:\Docs\Earth.mkv" };
        var pathMatch = new DocumentaryEntity { Id = 3, title = "Nature", title_original = "Nature", file_path = @"A:\Docs\Ocean\Nature.mkv" };
        var otherDocumentary = new DocumentaryEntity { Id = 4, title = "Apollo 11", title_original = "Apollo 11", file_path = @"A:\Docs\Apollo11.mkv" };
        var testee = new TestDocumentariesDbStrategy(new[] { otherDocumentary, pathMatch, originalTitleMatch, titleMatch }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Name = "ocean" });

        // Assert
        CollectionAssert.AreEqual(new[] { "Earth", "Nature", "Ocean World" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_DocumentariesWishFilter_ReturnsOnlyWantedEntries()
    {
        // Arrange
        var wantedDocumentary = new DocumentaryEntity { Id = 1, title = "Wanted", file_path = @"A:\Docs\Wanted.mkv", want_to_see = true };
        var skippedDocumentary = new DocumentaryEntity { Id = 2, title = "Skipped", file_path = @"A:\Docs\Skipped.mkv", want_to_see = false };
        var testee = new TestDocumentariesDbStrategy(new[] { skippedDocumentary, wantedDocumentary }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsWish = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Wanted" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_DocumentariesRecentFilter_ReturnsOnlyRecentEntries()
    {
        // Arrange
        var recentDocumentary = new DocumentaryEntity { Id = 1, title = "Recent", file_path = @"A:\Docs\Recent.mkv", creation_time = DateTime.Now.AddDays(-3) };
        var oldDocumentary = new DocumentaryEntity { Id = 2, title = "Old", file_path = @"A:\Docs\Old.mkv", creation_time = DateTime.Now.AddYears(-1) };
        var testee = new TestDocumentariesDbStrategy(new[] { oldDocumentary, recentDocumentary }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsRecent = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Recent" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_DocumentariesNewFilter_ReturnsOnlyCurrentAndPreviousYearEntries()
    {
        // Arrange
        var currentYearDocumentary = new DocumentaryEntity { Id = 1, title = "Current", file_path = @"A:\Docs\Current.mkv", year = DateTime.Now.Year };
        var previousYearDocumentary = new DocumentaryEntity { Id = 2, title = "Previous", file_path = @"A:\Docs\Previous.mkv", year = DateTime.Now.Year - 1 };
        var oldDocumentary = new DocumentaryEntity { Id = 3, title = "Classic", file_path = @"A:\Docs\Classic.mkv", year = DateTime.Now.Year - 2 };
        var testee = new TestDocumentariesDbStrategy(new[] { oldDocumentary, previousYearDocumentary, currentYearDocumentary }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsNew = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Current", "Previous" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesVrFilter_ReturnsOnlyVrEntries()
    {
        // Arrange
        var vrGame = new GameEntity { Id = 1, title = "Half-Life Alyx", file_path = @"A:\GamesVR\Alyx", vr = true };
        var nonVrGame = new GameEntity { Id = 2, title = "Portal 2", file_path = @"A:\Games\Portal2", vr = false };
        var testee = new TestGamesDbStrategy(new[] { nonVrGame, vrGame }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsVr = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Half-Life Alyx" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesNameFilter_MatchesTitleOriginalTitleAndPath()
    {
        // Arrange
        var titleMatch = new GameEntity { Id = 1, title = "Portal", title_original = "Portal", file_path = @"A:\Games\Portal" };
        var originalTitleMatch = new GameEntity { Id = 2, title = "Game", title_original = "Portal Stories", file_path = @"A:\Games\Stories" };
        var pathMatch = new GameEntity { Id = 3, title = "Mod", title_original = "Mod", file_path = @"A:\Games\PortalMods\Reloaded" };
        var otherGame = new GameEntity { Id = 4, title = "Half-Life", title_original = "Half-Life", file_path = @"A:\Games\HalfLife" };
        var testee = new TestGamesDbStrategy(new[] { otherGame, pathMatch, originalTitleMatch, titleMatch }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Name = "portal" });

        // Assert
        CollectionAssert.AreEqual(new[] { "Game", "Mod", "Portal" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesGenreFilter_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var matchingGame = new GameEntity
        {
            Id = 1,
            title = "StarCraft",
            file_path = @"A:\Games\StarCraft",
            GameGenres = { new ariadna::DbProvider.GameGenre { genreId = 6 } },
        };
        var otherGame = new GameEntity
        {
            Id = 2,
            title = "FIFA",
            file_path = @"A:\Games\FIFA",
            GameGenres = { new ariadna::DbProvider.GameGenre { genreId = 7 } },
        };
        var testee = new TestGamesDbStrategy(new[] { otherGame, matchingGame }.AsQueryable(), name => name == "Strategy" ? 6 : null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Genre = "Strategy" });

        // Assert
        CollectionAssert.AreEqual(new[] { "StarCraft" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesNonVrFilter_ReturnsOnlyNonVrEntries()
    {
        // Arrange
        var vrGame = new GameEntity { Id = 1, title = "Beat Saber", file_path = @"A:\GamesVR\BeatSaber", vr = true };
        var nonVrGame = new GameEntity { Id = 2, title = "Disco Elysium", file_path = @"A:\Games\DiscoElysium", vr = false };
        var testee = new TestGamesDbStrategy(new[] { vrGame, nonVrGame }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsNonVr = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Disco Elysium" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesWishFilter_ReturnsOnlyWantedEntries()
    {
        // Arrange
        var wantedGame = new GameEntity { Id = 1, title = "Wanted", file_path = @"A:\Games\Wanted", want_to_play = true };
        var skippedGame = new GameEntity { Id = 2, title = "Skipped", file_path = @"A:\Games\Skipped", want_to_play = false };
        var testee = new TestGamesDbStrategy(new[] { skippedGame, wantedGame }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsWish = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Wanted" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesRecentFilter_ReturnsOnlyRecentEntries()
    {
        // Arrange
        var recentGame = new GameEntity { Id = 1, title = "Recent", file_path = @"A:\Games\Recent", creation_time = DateTime.Now.AddDays(-2) };
        var oldGame = new GameEntity { Id = 2, title = "Old", file_path = @"A:\Games\Old", creation_time = DateTime.Now.AddYears(-1) };
        var testee = new TestGamesDbStrategy(new[] { oldGame, recentGame }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsRecent = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Recent" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_GamesNewFilter_ReturnsOnlyCurrentAndPreviousYearEntries()
    {
        // Arrange
        var currentYearGame = new GameEntity { Id = 1, title = "Current", file_path = @"A:\Games\Current", year = DateTime.Now.Year };
        var previousYearGame = new GameEntity { Id = 2, title = "Previous", file_path = @"A:\Games\Previous", year = DateTime.Now.Year - 1 };
        var oldGame = new GameEntity { Id = 3, title = "Classic", file_path = @"A:\Games\Classic", year = DateTime.Now.Year - 2 };
        var testee = new TestGamesDbStrategy(new[] { oldGame, previousYearGame, currentYearGame }.AsQueryable(), _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsNew = true });

        // Assert
        CollectionAssert.AreEqual(new[] { "Current", "Previous" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_LibrarySubgenreSelected_PrefersSubgenreOverGenre()
    {
        // Arrange
        var programmingBook = new LibraryEntity
        {
            Id = 1,
            title = "CLR via C#",
            file_path = @"A:\Library\Programming\CLR.pdf",
            LibraryGenres = { new LibraryGenreEntity { genreId = 22 } },
        };
        var literatureBook = new LibraryEntity
        {
            Id = 2,
            title = "Dune",
            file_path = @"A:\Library\Literature\Dune.epub",
            LibraryGenres = { new LibraryGenreEntity { genreId = 11 } },
        };
        var testee = new TestLibraryDbStrategy(
            new[] { literatureBook, programmingBook }.AsQueryable(),
            _ => null,
            name => name switch
            {
                "Literature" => 11,
                "Programming" => 22,
                _ => null,
            });

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams
        {
            Genre = "Literature",
            Subgenre = "Programming",
        });

        // Assert
        CollectionAssert.AreEqual(new[] { "CLR via C#" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_LibraryAuthorFilter_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var matchingBook = new LibraryEntity
        {
            Id = 1,
            title = "Clean Code",
            file_path = @"A:\Library\Programming\CleanCode.pdf",
            LibraryAuthors = { new LibraryAuthorEntity { authorId = 5 } },
        };
        var otherBook = new LibraryEntity
        {
            Id = 2,
            title = "Domain-Driven Design",
            file_path = @"A:\Library\Programming\DDD.pdf",
            LibraryAuthors = { new LibraryAuthorEntity { authorId = 6 } },
        };
        var testee = new TestLibraryDbStrategy(new[] { otherBook, matchingBook }.AsQueryable(), name => name == "Robert C. Martin" ? 5 : null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { Director = "Robert C. Martin", Subgenre = Utilities.EmptyDots });

        // Assert
        CollectionAssert.AreEqual(new[] { "Clean Code" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_LibraryWishFilter_ReturnsOnlyWantedEntries()
    {
        // Arrange
        var wantedBook = new LibraryEntity { Id = 1, title = "Wanted", file_path = @"A:\Library\Wanted.pdf", want_to_see = true };
        var skippedBook = new LibraryEntity { Id = 2, title = "Skipped", file_path = @"A:\Library\Skipped.pdf", want_to_see = false };
        var testee = new TestLibraryDbStrategy(new[] { skippedBook, wantedBook }.AsQueryable(), _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsWish = true, Subgenre = Utilities.EmptyDots });

        // Assert
        CollectionAssert.AreEqual(new[] { "Wanted" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_LibraryRecentFilter_ReturnsOnlyRecentEntries()
    {
        // Arrange
        var recentBook = new LibraryEntity { Id = 1, title = "Recent", file_path = @"A:\Library\Recent.pdf", creation_time = DateTime.Now.AddDays(-5) };
        var oldBook = new LibraryEntity { Id = 2, title = "Old", file_path = @"A:\Library\Old.pdf", creation_time = DateTime.Now.AddYears(-1) };
        var testee = new TestLibraryDbStrategy(new[] { oldBook, recentBook }.AsQueryable(), _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsRecent = true, Subgenre = Utilities.EmptyDots });

        // Assert
        CollectionAssert.AreEqual(new[] { "Recent" }, result.Select(r => r.Title).ToArray());
    }

    [TestMethod]
    public void QueryEntries_LibraryNewFilter_ReturnsOnlyCurrentAndPreviousYearEntries()
    {
        // Arrange
        var currentYearBook = new LibraryEntity { Id = 1, title = "Current", file_path = @"A:\Library\Current.pdf", year = DateTime.Now.Year };
        var previousYearBook = new LibraryEntity { Id = 2, title = "Previous", file_path = @"A:\Library\Previous.pdf", year = DateTime.Now.Year - 1 };
        var oldBook = new LibraryEntity { Id = 3, title = "Classic", file_path = @"A:\Library\Classic.pdf", year = DateTime.Now.Year - 2 };
        var testee = new TestLibraryDbStrategy(new[] { oldBook, previousYearBook, currentYearBook }.AsQueryable(), _ => null, _ => null);

        // Act
        var result = testee.ExecuteQuery(new AbstractDbStrategy.QueryParams { IsNew = true, Subgenre = Utilities.EmptyDots });

        // Assert
        CollectionAssert.AreEqual(new[] { "Current", "Previous" }, result.Select(r => r.Title).ToArray());
    }

    private sealed class TestMoviesDbStrategy : MoviesDbStrategy
    {
        private readonly MovieQuerySource m_Source;

        public TestMoviesDbStrategy(IQueryable<MovieEntity> entries, Func<string, int?> findDirectorId, Func<string, int?> findActorId, Func<string, int?> findGenreId) : base(NullLogger.Instance)
        {
            m_Source = new MovieQuerySource
            {
                Entries = entries,
                FindDirectorId = findDirectorId,
                FindActorId = findActorId,
                FindGenreId = findGenreId,
            };
        }

        public List<EntryDto> ExecuteQuery(AbstractDbStrategy.QueryParams values) => QueryEntries(values, m_Source);
    }

    private sealed class TestDocumentariesDbStrategy : DocumentariesDbStrategy
    {
        private readonly DocumentaryQuerySource m_Source;

        public TestDocumentariesDbStrategy(IQueryable<DocumentaryEntity> entries, Func<string, int?> findGenreId) : base(NullLogger.Instance)
        {
            m_Source = new DocumentaryQuerySource
            {
                Entries = entries,
                FindGenreId = findGenreId,
            };
        }

        public List<EntryDto> ExecuteQuery(AbstractDbStrategy.QueryParams values) => QueryEntries(values, m_Source);
    }

    private sealed class TestGamesDbStrategy : GamesDbStrategy
    {
        private readonly GameQuerySource m_Source;

        public TestGamesDbStrategy(IQueryable<GameEntity> entries, Func<string, int?> findGenreId) : base(NullLogger.Instance)
        {
            m_Source = new GameQuerySource
            {
                Entries = entries,
                FindGenreId = findGenreId,
            };
        }

        public List<EntryDto> ExecuteQuery(AbstractDbStrategy.QueryParams values) => QueryEntries(values, m_Source);
    }

    private sealed class TestLibraryDbStrategy : LibraryDbStrategy
    {
        private readonly LibraryQuerySource m_Source;

        public TestLibraryDbStrategy(IQueryable<LibraryEntity> entries, Func<string, int?> findAuthorId, Func<string, int?> findGenreId) : base(NullLogger.Instance)
        {
            m_Source = new LibraryQuerySource
            {
                Entries = entries,
                FindAuthorId = findAuthorId,
                FindGenreId = findGenreId,
            };
        }

        public List<EntryDto> ExecuteQuery(AbstractDbStrategy.QueryParams values) => QueryEntries(values, m_Source);
    }
}