using System.Runtime.ExceptionServices;
using Ariadna.AuxiliaryPopups;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public class EntryDetailsFormBehaviorTests
{
    [TestMethod]
    public void MovieDetailsForm_LoadWithoutTmdb_RemovesExtensionAndShowsCurrentInsertButtonText()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestMovieDetailsForm(@"A:\Movies\Terminator.mkv")
            {
                TmdbMovieIndex = -1,
                TmdbTvShowIndex = -1,
            };

            // Act
            testee.Show();

            // Assert
            Assert.AreEqual("Terminator", testee.TitleText);
            Assert.AreEqual("Insert entry", testee.InsertButtonText);
        });
    }

    [TestMethod]
    public void DocumentaryDetailsForm_Load_HidesDirectorAndCastControlsAndRemovesExtension()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDocumentaryDetailsForm(@"A:\Documentaries\Universe.mkv");

            // Act
            testee.Show();

            // Assert
            Assert.AreEqual("Universe", testee.TitleText);
            Assert.IsFalse(testee.DirectorsVisible);
            Assert.IsFalse(testee.CastVisible);
            Assert.IsFalse(testee.DirectorLabelVisible);
            Assert.IsFalse(testee.CastLabelVisible);
        });
    }

    [TestMethod]
    public void GameDetailsForm_Load_ShowsGameSpecificControlsAndMarksVrPath()
    {
        RunInSta(() =>
        {
            // Arrange
            var vrPath = TestSettings.Get("DefaultGamesPathVR") + "\\Half-Life Alyx";
            using var testee = new TestGameDetailsForm(vrPath);

            // Act
            testee.Show();

            // Assert
            Assert.IsTrue(testee.PreviewVisible);
            Assert.IsTrue(testee.PreviewFullVisible);
            Assert.IsTrue(testee.VrVisible);
            Assert.IsTrue(testee.VersionVisible);
            Assert.IsTrue(testee.IsVrChecked);
            Assert.IsFalse(testee.DescriptionVisible);
            Assert.IsFalse(testee.DurationVisible);
        });
    }

    [TestMethod]
    public void GameDetailsForm_Load_NonVrPath_LeavesVrUnchecked()
    {
        RunInSta(() =>
        {
            // Arrange
            var nonVrPath = TestSettings.Get("DefaultGamesPath") + "\\Portal 2";
            using var testee = new TestGameDetailsForm(nonVrPath);

            // Act
            testee.Show();

            // Assert
            Assert.IsFalse(testee.IsVrChecked);
        });
    }

    [TestMethod]
    public void LibraryDetailsForm_Load_HidesCastAndLeavesCurrentTitleAndAuthorLabelText()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestLibraryDetailsForm(@"A:\Library\Programming\Clean Code.pdf");

            // Act
            testee.Show();

            // Assert
            Assert.AreEqual("Clean Code.pdf", testee.TitleText);
            Assert.AreEqual("Authors", testee.DirectorLabelText);
            Assert.IsFalse(testee.CastVisible);
        });
    }

    [TestMethod]
    public void LibraryDetailsForm_Load_LanguagePath_UsesLanguageGenres()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestLibraryDetailsForm(@"A:\Library\Languages\Deutsch.pdf");

            // Act
            testee.Show();

            // Assert
            CollectionAssert.Contains(testee.AvailableGenres, "English");
            CollectionAssert.DoesNotContain(testee.AvailableGenres, "Testing");
        });
    }

    [TestMethod]
    public void LibraryDetailsForm_Load_ProgrammingPath_UsesProgrammingGenres()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestLibraryDetailsForm(@"A:\Library\Programming\CSharp in Depth.pdf");

            // Act
            testee.Show();

            // Assert
            CollectionAssert.Contains(testee.AvailableGenres, "Testing");
            CollectionAssert.DoesNotContain(testee.AvailableGenres, "English");
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
        public string TitleText => m_TxtTitle.Text;
        public bool DirectorsVisible => m_DirectorsList.Visible;
        public bool CastVisible => m_CastList.Visible;
        public bool DirectorLabelVisible => m_LblDirector.Visible;
        public bool CastLabelVisible => m_LblCast.Visible;

        protected override int GetStoredEntryId()
        {
            return -1;
        }
    }

    private sealed class TestMovieDetailsForm(string filePath) : MovieDetailsForm(filePath, NullLogger.Instance)
    {
        public string TitleText => m_TxtTitle.Text;
        public string InsertButtonText => m_BtnInsert.Text;

        protected override int GetStoredEntryId()
        {
            return -1;
        }
    }

    private sealed class TestGameDetailsForm(string filePath) : GameDetailsForm(filePath, NullLogger.Instance)
    {
        public bool PreviewVisible => m_Preview1.Visible;
        public bool PreviewFullVisible => m_PreviewFull.Visible;
        public bool VrVisible => m_VR.Visible;
        public bool VersionVisible => m_TxtVersion.Visible;
        public bool IsVrChecked => m_VR.Checked;
        public bool DescriptionVisible => m_TxtDescription.Visible;
        public bool DurationVisible => m_LblDuration.Visible;

        protected override int GetStoredEntryId()
        {
            return -1;
        }
    }

    private sealed class TestLibraryDetailsForm(string filePath) : LibraryDetailsForm(filePath, NullLogger.Instance)
    {
        public string TitleText => m_TxtTitle.Text;
        public string DirectorLabelText => m_LblDirector.Text;
        public bool CastVisible => m_CastList.Visible;
        public string[] AvailableGenres => GetGenres().ToArray();

        protected override int GetStoredEntryId()
        {
            return -1;
        }
    }
}