using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Ariadna.Properties;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class MoviesDbStrategyTests
{
    private static readonly string MediaPlayerPath = GetSettingValue("MediaPlayerPath");
    private static readonly string TotalCommanderPath = GetSettingValue("TotalCommanderPath");
    private static readonly string DefaultMoviesPath = GetSettingValue("DefaultMoviesPath");
    private static readonly string DefaultMoviesPathTmp2 = GetSettingValue("DefaultMoviesPathTMP2");
    private static readonly string DefaultSeriesPath = GetSettingValue("DefaultSeriesPath");
    private static readonly string MoviePostersRootPath = GetSettingValue("MoviePostersRootPath");

    [TestMethod]
    public void ExecuteEntry_FilePathAndMediaPlayerExists_StartsMediaPlayerWithQuotedPath()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            StoredPath = @"A:\Movies\Terminator.mkv",
            ExistingFiles = { @"A:\Movies\Terminator.mkv", MediaPlayerPath },
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.AreEqual(MediaPlayerPath, testee.StartedFileName);
        Assert.AreEqual("\"A:\\Movies\\Terminator.mkv\"", testee.StartedArguments);
    }

    [TestMethod]
    public void ExecuteEntry_DirectoryPath_StartsTotalCommanderForDirectory()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            StoredPath = @"A:\Series\X-Files",
            ExistingDirectories = { @"A:\Series\X-Files" },
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.IsNotNull(testee.StartedProcessInfo);
        Assert.AreEqual(TotalCommanderPath, testee.StartedProcessInfo.FileName);
        Assert.AreEqual($"/O /L=\"A:\\Series\\X-Files\"", testee.StartedProcessInfo.Arguments);
    }

    [TestMethod]
    public void ExecuteEntry_PathDoesNotExist_ShowsPathNotFoundMessage()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            StoredPath = @"A:\Movies\Missing.mkv",
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.AreEqual(@"A:\Movies\Missing.mkv", testee.LastMessageText);
        Assert.AreEqual(Resources.PathNotFound, testee.LastMessageCaption);
    }

    [TestMethod]
    public void ShowEntryDetails_PathExists_OpensDetailsForThatPath()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            StoredPath = @"A:\Movies\Terminator.mkv",
        };

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(@"A:\Movies\Terminator.mkv", testee.ShownDetailsPath);
    }

    [TestMethod]
    public void ShowEntryDetails_EmptyPath_DoesNothing()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy();

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.IsNull(testee.ShownDetailsPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_FirstMoviePathContainsNotInsertedItem_OpensThatItem()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            FilesByPath =
            {
                [DefaultMoviesPath] = [@"A:\Movies\Terminator.mkv"],
                [DefaultMoviesPathTmp2] = [@"A:\MoviesTmp\Alien.mkv"],
            },
            FirstNotInsertedBySource =
            {
                [DefaultMoviesPath] = @"A:\Movies\Terminator.mkv",
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\Movies\Terminator.mkv", testee.FetchedMoviePath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_FirstMoviePathHasNoCandidate_UsesSecondMoviePath()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            FilesByPath =
            {
                [DefaultMoviesPath] = [@"A:\Movies\AlreadyStored.mkv"],
                [DefaultMoviesPathTmp2] = [@"A:\MoviesTmp\Alien.mkv"],
            },
            FirstNotInsertedBySource =
            {
                [DefaultMoviesPath] = string.Empty,
                [DefaultMoviesPathTmp2] = @"A:\MoviesTmp\Alien.mkv",
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\MoviesTmp\Alien.mkv", testee.FetchedMoviePath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_NoCandidates_ReturnsFalse()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            FilesByPath =
            {
                [DefaultMoviesPath] = [@"A:\Movies\AlreadyStored.mkv"],
                [DefaultMoviesPathTmp2] = [@"A:\MoviesTmp\AlreadyStoredToo.mkv"],
            },
            DirectoriesByPath =
            {
                [DefaultSeriesPath] = [@"A:\Series\AlreadyStored"],
            },
            FirstNotInsertedBySource =
            {
                [DefaultMoviesPath] = string.Empty,
                [DefaultMoviesPathTmp2] = string.Empty,
                [DefaultSeriesPath] = string.Empty,
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(testee.FetchedMoviePath);
    }

    [TestMethod]
    public void RemoveEntry_PosterFileExists_DeletesPosterFile()
    {
        // Arrange
        var testee = new TestMoviesDbStrategy
        {
            ExistingFiles = { MoviePostersRootPath + "42" },
        };

        // Act
        testee.RemoveEntry(42);

        // Assert
        Assert.AreEqual(MoviePostersRootPath + "42", testee.DeletedPath);
        Assert.AreEqual(42, testee.RemovedEntryId);
    }

    [TestMethod]
    public void RemoveEntry_DeletePosterFails_ShowsErrorMessage()
    {
        // Arrange
        var posterPath = MoviePostersRootPath + "42";
        var testee = new TestMoviesDbStrategy
        {
            ExistingFiles = { posterPath },
            DeleteException = new IOException("poster is locked")
            {
                Source = "DeletePoster",
            },
        };

        // Act
        testee.RemoveEntry(42);

        // Assert
        Assert.AreEqual("DeletePoster", testee.LastMessageText);
        Assert.AreEqual("poster is locked", testee.LastMessageCaption);
    }

    private static string GetSettingValue(string propertyName)
    {
        var settingsType = typeof(Resources).Assembly.GetType("Ariadna.Properties.Settings", true)!;
        var defaultProperty = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)!;
        var settingsInstance = defaultProperty.GetValue(null)!;
        var property = settingsType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
        return (string)property.GetValue(settingsInstance)!;
    }

    private sealed class TestMoviesDbStrategy : MoviesDbStrategy
    {
        public TestMoviesDbStrategy() : base(NullLogger.Instance)
        {
        }

        public string StoredPath { get; set; } = string.Empty;
        public string StartedFileName { get; private set; } = string.Empty;
        public string StartedArguments { get; private set; } = string.Empty;
        public ProcessStartInfo? StartedProcessInfo { get; private set; }
        public string LastMessageText { get; private set; } = string.Empty;
        public string LastMessageCaption { get; private set; } = string.Empty;
        public string? FetchedMoviePath { get; private set; }
        public string? ShownDetailsPath { get; private set; }
        public string DeletedPath { get; private set; } = string.Empty;
        public int? RemovedEntryId { get; private set; }
        public IOException? DeleteException { get; set; }
        public HashSet<string> ExistingFiles { get; } = [];
        public HashSet<string> ExistingDirectories { get; } = [];
        public Dictionary<string, string[]> FilesByPath { get; } = [];
        public Dictionary<string, string[]> DirectoriesByPath { get; } = [];
        public Dictionary<string, string> FirstNotInsertedBySource { get; } = [];

        protected override string FindStoredEntryPathById(int id) => StoredPath;
        protected override bool FileExists(string path) => ExistingFiles.Contains(path);
        protected override bool DirectoryExists(string path) => ExistingDirectories.Contains(path);
        protected override void DeleteFile(string path)
        {
            DeletedPath = path;
            if (DeleteException != null)
            {
                throw DeleteException;
            }
        }
        protected override void StartProcess(string fileName, string arguments)
        {
            StartedFileName = fileName;
            StartedArguments = arguments;
        }
        protected override void StartProcess(ProcessStartInfo startInfo)
        {
            StartedProcessInfo = startInfo;
        }
        protected override void ShowDataDialog(string path)
        {
            ShownDetailsPath = path;
        }
        protected override void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            LastMessageText = text;
            LastMessageCaption = caption;
        }
        protected override void RemoveEntryFromDatabase(int id)
        {
            RemovedEntryId = id;
        }
        protected override string[] GetFiles(string path) => FilesByPath.TryGetValue(path, out var files) ? files : [];
        protected override string[] GetDirectories(string path) => DirectoriesByPath.TryGetValue(path, out var directories) ? directories : [];
        protected override string FindFirstNotInserted(string[] paths)
        {
            foreach (var pair in FilesByPath)
            {
                if (ReferenceEquals(pair.Value, paths))
                {
                    return FirstNotInsertedBySource.TryGetValue(pair.Key, out var value) ? value : string.Empty;
                }
            }

            foreach (var pair in DirectoriesByPath)
            {
                if (ReferenceEquals(pair.Value, paths))
                {
                    return FirstNotInsertedBySource.TryGetValue(pair.Key, out var value) ? value : string.Empty;
                }
            }

            return string.Empty;
        }
        protected override bool TryOpenFirstNotInserted(string[] paths)
        {
            var foundPath = FindFirstNotInserted(paths);
            if (string.IsNullOrEmpty(foundPath))
            {
                return false;
            }

            FetchedMoviePath = foundPath;
            return true;
        }
    }
}