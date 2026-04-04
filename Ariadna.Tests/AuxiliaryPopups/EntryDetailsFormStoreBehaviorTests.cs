using System.Runtime.ExceptionServices;
using Ariadna.AuxiliaryPopups;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public class EntryDetailsFormStoreBehaviorTests
{
    [TestMethod]
    public void DocumentaryDetailsForm_StoreWorkflow_AllStepsSucceed_StoresGenresEntryAndRelations()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDocumentaryDetailsForm(@"A:\Documentaries\Universe.mkv");

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "StoreGenres", "StoreEntry", "StoreEntryGenres:42" }, testee.Calls);
        });
    }

    [TestMethod]
    public void DocumentaryDetailsForm_StoreWorkflow_EntryStoreFails_DoesNotStoreRelations()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDocumentaryDetailsForm(@"A:\Documentaries\Universe.mkv")
            {
                StoreEntryResult = false,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsFalse(result);
            CollectionAssert.AreEqual(new[] { "StoreGenres", "StoreEntry" }, testee.Calls);
        });
    }

    [TestMethod]
    public void GameDetailsForm_StoreWorkflow_AllStepsSucceed_StoresGenresEntryAndRelations()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestGameDetailsForm(@"A:\Games\Half-Life Alyx")
            {
                StoredDbEntryIdAfterEntryStore = 77,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "StoreGenres", "StoreEntry", "StoreEntryGenres:77" }, testee.Calls);
        });
    }

    [TestMethod]
    public void LibraryDetailsForm_StoreWorkflow_AllStepsSucceed_StoresAuthorsGenresAndRelations()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestLibraryDetailsForm(@"A:\Library\Programming\Clean Code.pdf")
            {
                StoredDbEntryIdAfterEntryStore = 11,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "StoreGenres", "StoreEntry", "StoreAuthors", "StoreLibraryAuthors:11", "StoreEntryGenres:11" }, testee.Calls);
        });
    }

    [TestMethod]
    public void MovieDetailsForm_StoreWorkflow_AllStepsSucceed_StoresBaseEntitiesBeforeRelations()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestMovieDetailsForm(@"A:\Movies\Terminator.mkv")
            {
                StoredDbEntryIdAfterEntryStore = 7,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "StoreGenres", "StoreCast", "StoreDirectors", "StoreEntry", "StoreMovieCast:7", "StoreMovieDirectors:7", "StoreEntryGenres:7" }, testee.Calls);
        });
    }

    [TestMethod]
    public void MovieDetailsForm_StoreWorkflow_CastStoreFails_StopsBeforeRemainingStoreSteps()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestMovieDetailsForm(@"A:\Movies\Terminator.mkv")
            {
                StoreCastResult = false,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsFalse(result);
            CollectionAssert.AreEqual(new[] { "StoreGenres", "StoreCast" }, testee.Calls);
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private sealed class TestDocumentaryDetailsForm(string filePath) : DocumentaryDetailsForm(filePath, NullLogger.Instance)
    {
        public List<string> Calls { get; } = [];
        public bool StoreGenresResult { get; set; } = true;
        public bool StoreEntryResult { get; set; } = true;
        public int StoredDbEntryIdAfterEntryStore { get; set; } = 42;

        public bool RunStore()
        {
            return DoStore();
        }

        protected override bool StorePreEntryData()
        {
            Calls.Add("StoreGenres");
            return StoreGenresResult;
        }

        protected override bool StoreMainEntry()
        {
            Calls.Add("StoreEntry");
            if (StoreEntryResult)
            {
                StoredDbEntryId = StoredDbEntryIdAfterEntryStore;
            }
            return StoreEntryResult;
        }

        protected override void StoreRelatedData()
        {
            Calls.Add($"StoreEntryGenres:{StoredDbEntryId}");
        }
    }

    private sealed class TestGameDetailsForm(string filePath) : GameDetailsForm(filePath, NullLogger.Instance)
    {
        public List<string> Calls { get; } = [];
        public bool StoreGenresResult { get; set; } = true;
        public bool StoreEntryResult { get; set; } = true;
        public int StoredDbEntryIdAfterEntryStore { get; set; } = 42;

        public bool RunStore()
        {
            return DoStore();
        }

        protected override bool StorePreEntryData()
        {
            Calls.Add("StoreGenres");
            return StoreGenresResult;
        }

        protected override bool StoreMainEntry()
        {
            Calls.Add("StoreEntry");
            if (StoreEntryResult)
            {
                StoredDbEntryId = StoredDbEntryIdAfterEntryStore;
            }
            return StoreEntryResult;
        }

        protected override void StoreRelatedData()
        {
            Calls.Add($"StoreEntryGenres:{StoredDbEntryId}");
        }
    }

    private sealed class TestLibraryDetailsForm(string filePath) : LibraryDetailsForm(filePath, NullLogger.Instance)
    {
        public List<string> Calls { get; } = [];
        public bool StoreGenresResult { get; set; } = true;
        public bool StoreEntryResult { get; set; } = true;
        public bool StoreAuthorsResult { get; set; } = true;
        public int StoredDbEntryIdAfterEntryStore { get; set; } = 42;

        public bool RunStore()
        {
            return DoStore();
        }

        protected override bool StorePreEntryData()
        {
            Calls.Add("StoreGenres");
            return StoreGenresResult;
        }

        protected override bool StoreMainEntry()
        {
            Calls.Add("StoreEntry");
            if (StoreEntryResult)
            {
                StoredDbEntryId = StoredDbEntryIdAfterEntryStore;
            }
            return StoreEntryResult;
        }

        protected override bool StorePostEntryData()
        {
            Calls.Add("StoreAuthors");
            return StoreAuthorsResult;
        }

        protected override void StoreRelatedData()
        {
            Calls.Add($"StoreLibraryAuthors:{StoredDbEntryId}");
            Calls.Add($"StoreEntryGenres:{StoredDbEntryId}");
        }
    }

    private sealed class TestMovieDetailsForm(string filePath) : MovieDetailsForm(filePath, NullLogger.Instance)
    {
        public List<string> Calls { get; } = [];
        public bool StoreGenresResult { get; set; } = true;
        public bool StoreCastResult { get; set; } = true;
        public bool StoreDirectorsResult { get; set; } = true;
        public bool StoreEntryResult { get; set; } = true;
        public int StoredDbEntryIdAfterEntryStore { get; set; } = 42;

        public bool RunStore()
        {
            return DoStore();
        }

        protected override bool StorePreEntryData()
        {
            Calls.Add("StoreGenres");
            var bSuccess = StoreGenresResult;
            if (bSuccess)
            {
                Calls.Add("StoreCast");
                bSuccess = StoreCastResult;
            }
            if (bSuccess)
            {
                Calls.Add("StoreDirectors");
                bSuccess = StoreDirectorsResult;
            }
            return bSuccess;
        }

        protected override bool StoreMainEntry()
        {
            Calls.Add("StoreEntry");
            if (StoreEntryResult)
            {
                StoredDbEntryId = StoredDbEntryIdAfterEntryStore;
            }
            return StoreEntryResult;
        }

        protected override void StoreRelatedData()
        {
            Calls.Add($"StoreMovieCast:{StoredDbEntryId}");
            Calls.Add($"StoreMovieDirectors:{StoredDbEntryId}");
            Calls.Add($"StoreEntryGenres:{StoredDbEntryId}");
        }
    }
}