using System.Diagnostics;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Ariadna.Properties;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class GamesDbStrategyTests
{
    private static readonly string TotalCommanderPath = TestSettings.Get("TotalCommanderPath");
    private static readonly string DefaultGamesPath = TestSettings.Get("DefaultGamesPath");
    private static readonly string DefaultGamesPathVr = TestSettings.Get("DefaultGamesPathVR");
    private static readonly string GamePostersRootPath = TestSettings.Get("GamePostersRootPath");
    private static readonly string PreviewSuffix = TestSettings.Get("PreviewSuffix");

    [TestMethod]
    public void ExecuteEntry_DirectoryPath_StartsTotalCommanderForDirectory()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            StoredPath = @"A:\Games\Mass Effect",
            ExistingDirectories = { @"A:\Games\Mass Effect" },
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.IsNotNull(testee.StartedProcessInfo);
        Assert.AreEqual(TotalCommanderPath, testee.StartedProcessInfo.FileName);
        Assert.AreEqual($"/O /L=\"A:\\Games\\Mass Effect\"", testee.StartedProcessInfo.Arguments);
    }

    [TestMethod]
    public void ExecuteEntry_PathDoesNotExist_ShowsPathNotFoundMessage()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            StoredPath = @"A:\Games\Missing",
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.AreEqual(@"A:\Games\Missing", testee.LastMessageText);
        Assert.AreEqual(Resources.PathNotFound, testee.LastMessageCaption);
    }

    [TestMethod]
    public void ShowEntryDetails_PathExists_OpensDetailsForThatPath()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            StoredPath = @"A:\Games\Mass Effect",
        };

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(@"A:\Games\Mass Effect", testee.OpenedDataPath);
    }

    [TestMethod]
    public void ShowEntryDetails_EmptyPath_DoesNothing()
    {
        // Arrange
        var testee = new TestGamesDbStrategy();

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(string.Empty, testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_DefaultGamesPathContainsCandidate_OpensThatDirectory()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultGamesPath] = [@"A:\Games\Mass Effect"],
                [DefaultGamesPathVr] = [@"A:\Games\_VR_\Beat Saber"],
            },
            FirstNotInsertedBySource =
            {
                [DefaultGamesPath] = @"A:\Games\Mass Effect",
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\Games\Mass Effect", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_DefaultGamesPathHasNoCandidate_UsesVrPath()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultGamesPath] = [@"A:\Games\Mass Effect"],
                [DefaultGamesPathVr] = [@"A:\Games\_VR_\Beat Saber"],
            },
            FirstNotInsertedBySource =
            {
                [DefaultGamesPath] = string.Empty,
                [DefaultGamesPathVr] = @"A:\Games\_VR_\Beat Saber",
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\Games\_VR_\Beat Saber", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_NoCandidates_ReturnsFalse()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultGamesPath] = [@"A:\Games\Mass Effect"],
                [DefaultGamesPathVr] = [@"A:\Games\_VR_\Beat Saber"],
            },
            FirstNotInsertedBySource =
            {
                [DefaultGamesPath] = string.Empty,
                [DefaultGamesPathVr] = string.Empty,
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(string.Empty, testee.OpenedDataPath);
    }

    [TestMethod]
    public void RemoveEntry_PosterAndPreviewFilesExist_DeletesAllRelatedFiles()
    {
        // Arrange
        var posterPath = GamePostersRootPath + "42";
        var testee = new TestGamesDbStrategy
        {
            ExistingFiles =
            {
                posterPath,
                posterPath + PreviewSuffix + "1",
                posterPath + PreviewSuffix + "2",
                posterPath + PreviewSuffix + "3",
                posterPath + PreviewSuffix + "4",
            },
            RemoveEntryFromDatabaseResult = true,
        };

        // Act
        testee.RemoveEntry(42);

        // Assert
        CollectionAssert.AreEquivalent(
            new[]
            {
                posterPath,
                posterPath + PreviewSuffix + "1",
                posterPath + PreviewSuffix + "2",
                posterPath + PreviewSuffix + "3",
                posterPath + PreviewSuffix + "4",
            },
            testee.DeletedPaths);
        Assert.AreEqual(42, testee.RemovedEntryId);
    }

    [TestMethod]
    public void RemoveEntry_EntryDoesNotExist_DoesNotDeleteFiles()
    {
        // Arrange
        var testee = new TestGamesDbStrategy
        {
            RemoveEntryFromDatabaseResult = false,
            ExistingFiles = { GamePostersRootPath + "42" },
        };

        // Act
        testee.RemoveEntry(42);

        // Assert
        Assert.AreEqual(0, testee.DeletedPaths.Count);
        Assert.IsNull(testee.RemovedEntryId);
    }

    private sealed class TestGamesDbStrategy : GamesDbStrategy
    {
        public TestGamesDbStrategy() : base(NullLogger.Instance)
        {
        }

        public string StoredPath { get; set; } = string.Empty;
        public ProcessStartInfo? StartedProcessInfo { get; private set; }
        public string LastMessageText { get; private set; } = string.Empty;
        public string LastMessageCaption { get; private set; } = string.Empty;
        public string OpenedDataPath { get; private set; } = string.Empty;
        public int? RemovedEntryId { get; private set; }
        public bool RemoveEntryFromDatabaseResult { get; set; } = true;
        public HashSet<string> ExistingFiles { get; } = [];
        public HashSet<string> ExistingDirectories { get; } = [];
        public Dictionary<string, string[]> DirectoriesByPath { get; } = [];
        public Dictionary<string, string> FirstNotInsertedBySource { get; } = [];
        public List<string> DeletedPaths { get; } = [];

        protected override string FindStoredEntryPathById(int id) => StoredPath;
        protected override bool FileExists(string path) => ExistingFiles.Contains(path);
        protected override bool DirectoryExists(string path) => ExistingDirectories.Contains(path);
        protected override void DeleteFile(string path)
        {
            DeletedPaths.Add(path);
        }
        protected override void StartProcess(ProcessStartInfo startInfo)
        {
            StartedProcessInfo = startInfo;
        }
        protected override void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            LastMessageText = text;
            LastMessageCaption = caption;
        }
        protected override bool RemoveEntryFromDatabase(int id)
        {
            if (!RemoveEntryFromDatabaseResult)
            {
                return false;
            }

            RemovedEntryId = id;
            return true;
        }
        protected override string[] GetDirectories(string path) => DirectoriesByPath.TryGetValue(path, out var directories) ? directories : [];
        protected override string FindFirstNotInserted(string[] paths)
        {
            foreach (var pair in DirectoriesByPath)
            {
                if (ReferenceEquals(pair.Value, paths))
                {
                    return FirstNotInsertedBySource.TryGetValue(pair.Key, out var value) ? value : string.Empty;
                }
            }

            return string.Empty;
        }
        protected override void ShowDataDialog(string path)
        {
            OpenedDataPath = path;
        }
    }
}