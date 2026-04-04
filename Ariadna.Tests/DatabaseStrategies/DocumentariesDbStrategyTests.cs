using System.Diagnostics;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Ariadna.Properties;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class DocumentariesDbStrategyTests
{
    private static readonly string MediaPlayerPath = TestSettings.Get("MediaPlayerPath");
    private static readonly string TotalCommanderPath = TestSettings.Get("TotalCommanderPath");
    private static readonly string DefaultDocumentariesPath = TestSettings.Get("DefaultDoocumentariesPath");
    private static readonly string DocumentaryPostersRootPath = TestSettings.Get("DocumentaryPostersRootPath");

    [TestMethod]
    public void ExecuteEntry_FilePathAndMediaPlayerExists_StartsMediaPlayerWithQuotedPath()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            StoredPath = @"A:\Documentaries\Universe.mkv",
            ExistingFiles = { @"A:\Documentaries\Universe.mkv", MediaPlayerPath },
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.AreEqual(MediaPlayerPath, testee.StartedFileName);
        Assert.AreEqual("\"A:\\Documentaries\\Universe.mkv\"", testee.StartedArguments);
    }

    [TestMethod]
    public void ExecuteEntry_DirectoryPath_StartsTotalCommanderForDirectory()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            StoredPath = @"A:\Documentaries\Travel",
            ExistingDirectories = { @"A:\Documentaries\Travel" },
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.IsNotNull(testee.StartedProcessInfo);
        Assert.AreEqual(TotalCommanderPath, testee.StartedProcessInfo.FileName);
        Assert.AreEqual($"/O /L=\"A:\\Documentaries\\Travel\"", testee.StartedProcessInfo.Arguments);
    }

    [TestMethod]
    public void ExecuteEntry_PathDoesNotExist_ShowsPathNotFoundMessage()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            StoredPath = @"A:\Documentaries\Missing.mkv",
        };

        // Act
        testee.ExecuteEntry(1);

        // Assert
        Assert.AreEqual(@"A:\Documentaries\Missing.mkv", testee.LastMessageText);
        Assert.AreEqual(Resources.PathNotFound, testee.LastMessageCaption);
    }

    [TestMethod]
    public void ShowEntryDetails_PathExists_OpensDetailsForThatPath()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            StoredPath = @"A:\Documentaries\Universe.mkv",
        };

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(@"A:\Documentaries\Universe.mkv", testee.OpenedDataPath);
    }

    [TestMethod]
    public void ShowEntryDetails_EmptyPath_DoesNothing()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy();

        // Act
        testee.ShowEntryDetails(1);

        // Assert
        Assert.AreEqual(string.Empty, testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_FirstBaseDirectoryContainsDirectoryCandidate_OpensThatDirectory()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultDocumentariesPath] = [@"A:\Documentary\Nature"],
                [@"A:\Documentary\Nature"] = [@"A:\Documentary\Nature\Travel"],
            },
            FilesByPath =
            {
                [@"A:\Documentary\Nature"] = [@"A:\Documentary\Nature\Universe.mkv"],
            },
            FirstNotInsertedBySource =
            {
                [@"A:\Documentary\Nature"] = @"A:\Documentary\Nature\Travel",
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\Documentary\Nature\Travel", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_DirectoryCandidatesMissing_UsesFileCandidate()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultDocumentariesPath] = [@"A:\Documentary\Nature"],
                [@"A:\Documentary\Nature"] = [@"A:\Documentary\Nature\Travel"],
            },
            FilesByPath =
            {
                [@"A:\Documentary\Nature"] = [@"A:\Documentary\Nature\Universe.mkv"],
            },
            FirstNotInsertedBySource =
            {
                [@"A:\Documentary\Nature"] = string.Empty,
                [@"A:\Documentary\Nature|files"] = @"A:\Documentary\Nature\Universe.mkv",
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(@"A:\Documentary\Nature\Universe.mkv", testee.OpenedDataPath);
    }

    [TestMethod]
    public void FindNextEntryAutomatically_NoCandidates_ReturnsFalse()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            DirectoriesByPath =
            {
                [DefaultDocumentariesPath] = [@"A:\Documentary\Nature"],
                [@"A:\Documentary\Nature"] = [@"A:\Documentary\Nature\Travel"],
            },
            FilesByPath =
            {
                [@"A:\Documentary\Nature"] = [@"A:\Documentary\Nature\Universe.mkv"],
            },
            FirstNotInsertedBySource =
            {
                [@"A:\Documentary\Nature"] = string.Empty,
                [@"A:\Documentary\Nature|files"] = string.Empty,
            },
        };

        // Act
        var result = testee.FindNextEntryAutomatically();

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(string.Empty, testee.OpenedDataPath);
    }

    [TestMethod]
    public void RemoveEntry_PosterFileExists_DeletesPosterFile()
    {
        // Arrange
        var testee = new TestDocumentariesDbStrategy
        {
            ExistingFiles = { DocumentaryPostersRootPath + "42" },
        };

        // Act
        testee.RemoveEntry(42);

        // Assert
        Assert.AreEqual(DocumentaryPostersRootPath + "42", testee.DeletedPath);
        Assert.AreEqual(42, testee.RemovedEntryId);
    }

    [TestMethod]
    public void RemoveEntry_DeletePosterFails_ShowsErrorMessage()
    {
        // Arrange
        var posterPath = DocumentaryPostersRootPath + "42";
        var testee = new TestDocumentariesDbStrategy
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

    private sealed class TestDocumentariesDbStrategy : DocumentariesDbStrategy
    {
        public TestDocumentariesDbStrategy() : base(NullLogger.Instance)
        {
        }

        public string StoredPath { get; set; } = string.Empty;
        public string StartedFileName { get; private set; } = string.Empty;
        public string StartedArguments { get; private set; } = string.Empty;
        public ProcessStartInfo? StartedProcessInfo { get; private set; }
        public string LastMessageText { get; private set; } = string.Empty;
        public string LastMessageCaption { get; private set; } = string.Empty;
        public string OpenedDataPath { get; private set; } = string.Empty;
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
            foreach (var pair in DirectoriesByPath)
            {
                if (ReferenceEquals(pair.Value, paths))
                {
                    return FirstNotInsertedBySource.TryGetValue(pair.Key, out var value) ? value : string.Empty;
                }
            }

            foreach (var pair in FilesByPath)
            {
                if (ReferenceEquals(pair.Value, paths))
                {
                    var key = pair.Key + "|files";
                    return FirstNotInsertedBySource.TryGetValue(key, out var value) ? value : string.Empty;
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