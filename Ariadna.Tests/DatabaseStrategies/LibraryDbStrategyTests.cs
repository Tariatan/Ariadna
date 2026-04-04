using System.Diagnostics;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class LibraryDbStrategyTests
{
    private static readonly string TotalCommanderPath = TestSettings.Get("TotalCommanderPath");
    private static readonly string DefaultLibraryPath = TestSettings.Get("DefaultLibraryPath");
    private static readonly string LibraryPostersRootPath = TestSettings.Get("LibraryPostersRootPath");

    [TestMethod]
    public void ExecuteEntry_PathExists_StartsTotalCommanderForPath()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            StoredPath = @"A:\Library\Programming\Clean Code.pdf",
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.IsNotNull(testee.StartedProcessInfo);
        Assert.AreEqual(TotalCommanderPath, testee.StartedProcessInfo.FileName);
        Assert.AreEqual($"/O /L=\"A:\\Library\\Programming\\Clean Code.pdf\"", testee.StartedProcessInfo.Arguments);
    }

    [TestMethod]
    public void ExecuteEntry_EmptyPath_DoesNothing()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy();

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.IsNull(testee.StartedProcessInfo);
    }

    [TestMethod]
    public void ShowEntryDetails_PathExists_OpensDetailsForThatPath()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            StoredPath = @"A:\Library\Programming\Clean Code.pdf",
        };

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(@"A:\Library\Programming\Clean Code.pdf", testee.OpenedDataPath);
    }

    [TestMethod]
    public void ShowEntryDetails_EmptyPath_DoesNothing()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy();

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(string.Empty, testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_FinderReturnsCandidate_OpensThatPath()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            NextEntryPathAutomatically = @"A:\Library\Programming\Refactoring.pdf",
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\Library\Programming\Refactoring.pdf", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_FinderReturnsEmptyPath_ReturnsFalse()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            NextEntryPathAutomatically = string.Empty,
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(string.Empty, testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_SubDirectoryWithLowercaseParent_SkipsThatBranch()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultLibraryPath] = [@"A:\LIBRARY\PROGRAMMING"],
            },
            RecursiveDirectoriesByPath =
            {
                [@"A:\LIBRARY\PROGRAMMING"] =
                [
                    @"A:\LIBRARY\PROGRAMMING\topic\DRAFTS",
                    @"A:\LIBRARY\PROGRAMMING\TOPIC\BOOKS",
                ],
            },
            FilesByPath =
            {
                [@"A:\LIBRARY\PROGRAMMING\TOPIC\BOOKS"] = [@"A:\LIBRARY\PROGRAMMING\TOPIC\BOOKS\Refactoring.pdf"],
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\LIBRARY\PROGRAMMING\TOPIC\BOOKS\Refactoring.pdf", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_SubDirectoryNameContainsLowercase_UsesDirectoryCandidate()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultLibraryPath] = [@"A:\LIBRARY\PROGRAMMING"],
            },
            RecursiveDirectoriesByPath =
            {
                [@"A:\LIBRARY\PROGRAMMING"] = [@"A:\LIBRARY\PROGRAMMING\TOPIC\Refactoring"],
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\LIBRARY\PROGRAMMING\TOPIC\Refactoring", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_WhenRecursiveDirectoriesHaveNoCandidate_UsesBaseDirectoryFile()
    {
        // Arrange
        var baseFileCandidate = @"A:\LIBRARY\PROGRAMMING\Clean Code.pdf";
        var testee = new TestLibraryDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultLibraryPath] = [@"A:\LIBRARY\PROGRAMMING"],
            },
            RecursiveDirectoriesByPath =
            {
                [@"A:\LIBRARY\PROGRAMMING"] = [@"A:\LIBRARY\PROGRAMMING\TOPIC\BOOKS"],
            },
            FilesByPath =
            {
                [@"A:\LIBRARY\PROGRAMMING\TOPIC\BOOKS"] = [],
                [@"A:\LIBRARY\PROGRAMMING"] = [baseFileCandidate],
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(baseFileCandidate, testee.OpenedDataPath);
    }

    [TestMethod]
    public void RemoveEntry_PosterFileExists_DeletesPosterFile()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            ExistingPoster = true,
        };

        // Act
        testee.RemoveEntry(42);

        // Assert
        Assert.AreEqual(LibraryPostersRootPath + "42", testee.DeletedPath);
        Assert.AreEqual(42, testee.RemovedEntryId);
    }

    [TestMethod]
    public void RemoveEntry_DeletePosterFails_ShowsErrorMessage()
    {
        // Arrange
        var testee = new TestLibraryDbStrategy
        {
            ExistingPoster = true,
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

    private sealed class TestLibraryDbStrategy : LibraryDbStrategy
    {
        public TestLibraryDbStrategy() : base(NullLogger.Instance)
        {
        }

        public string StoredPath { get; set; } = string.Empty;
        public string NextEntryPathAutomatically { get; set; } = string.Empty;
        public bool ExistingPoster { get; set; }
        public IOException? DeleteException { get; set; }
        public ProcessStartInfo? StartedProcessInfo { get; private set; }
        public string OpenedDataPath { get; private set; } = string.Empty;
        public string DeletedPath { get; private set; } = string.Empty;
        public int? RemovedEntryId { get; private set; }
        public string LastMessageText { get; private set; } = string.Empty;
        public string LastMessageCaption { get; private set; } = string.Empty;
        public Dictionary<string, string[]> DirectoriesByPath { get; } = [];
        public Dictionary<string, string[]> RecursiveDirectoriesByPath { get; } = [];
        public Dictionary<string, string[]> FilesByPath { get; } = [];
        public HashSet<string> InsertedPaths { get; } = [];

        protected override string FindStoredEntryPathById(int id) => StoredPath;
        protected override void ShowDataDialog(string path)
        {
            OpenedDataPath = path;
        }
        protected override void StartProcess(ProcessStartInfo startInfo)
        {
            StartedProcessInfo = startInfo;
        }
        protected override void RemoveEntryFromDatabase(int id)
        {
            RemovedEntryId = id;
        }
        protected override bool FileExists(string path) => ExistingPoster;
        protected override void DeleteFile(string path)
        {
            DeletedPath = path;
            if (DeleteException != null)
            {
                throw DeleteException;
            }
        }
        protected override void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            LastMessageText = text;
            LastMessageCaption = caption;
        }
        protected override string FindNextEntryPathAutomatically()
            => !string.IsNullOrEmpty(NextEntryPathAutomatically) || DirectoriesByPath.Count == 0
                ? NextEntryPathAutomatically
                : base.FindNextEntryPathAutomatically();
        protected override string FindNextEntryPathInBaseDirectory(string baseDir, Func<string, bool> isAlreadyInserted)
            => base.FindNextEntryPathInBaseDirectory(baseDir, path => InsertedPaths.Contains(path));
        protected override string FindNextEntryPathInSubDirectory(string subDir, Func<string, bool> isAlreadyInserted)
            => base.FindNextEntryPathInSubDirectory(subDir, path => InsertedPaths.Contains(path));
        protected override string[] GetDirectories(string path) => DirectoriesByPath.TryGetValue(path, out var directories) ? directories : [];
        protected override string[] GetDirectoriesRecursive(string path) => RecursiveDirectoriesByPath.TryGetValue(path, out var directories) ? directories : [];
        protected override string[] GetFiles(string path) => FilesByPath.TryGetValue(path, out var files) ? files : [];
    }
}