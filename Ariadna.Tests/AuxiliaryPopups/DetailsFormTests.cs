using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Properties;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public class DetailsFormTests
{
    [TestMethod]
    public void AddGenre_NewGenre_AddsSortedItemAndHidesButtonAtLimit()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();

            // Act
            testee.AddGenreForTest("delta");
            testee.AddGenreForTest("beta");
            testee.AddGenreForTest("alpha");
            testee.AddGenreForTest("gamma");
            testee.AddGenreForTest("epsilon");

            // Assert
            CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Delta", "Epsilon", "Gamma" }, testee.GenresList.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
            Assert.IsFalse(testee.AddGenreButton.Visible);
        });
    }

    [TestMethod]
    public void AddGenre_DuplicateGenre_DoesNotAddSecondItem()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();
            testee.AddGenreForTest("alpha");

            // Act
            testee.AddGenreForTest("alpha");

            // Assert
            Assert.AreEqual(1, testee.GenresList.Items.Count);
            Assert.AreEqual("Alpha", testee.GenresList.Items[0].Text);
        });
    }

    [TestMethod]
    public void OnKeyDown_ShiftPressed_ChangesInsertButtonTextToIgnore()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();

            // Act
            testee.TriggerKeyDown(new KeyEventArgs(Keys.ShiftKey | Keys.Shift));

            // Assert
            Assert.AreEqual(Resources.Ignore, testee.InsertButton.Text);
        });
    }

    [TestMethod]
    public void OnKeyUp_ShiftReleased_RestoresInsertButtonText()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();
            testee.TriggerKeyDown(new KeyEventArgs(Keys.ShiftKey | Keys.Shift));

            // Act
            testee.TriggerKeyUp(new KeyEventArgs(Keys.None));

            // Assert
            Assert.AreEqual(Resources.Insert, testee.InsertButton.Text);
        });
    }

    [TestMethod]
    public void OnKeyUp_ShiftReleasedInUpdateMode_RestoresUpdateButtonText()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv")
            {
                StoredDbEntryId = 7,
            };
            testee.Show();
            testee.TriggerKeyDown(new KeyEventArgs(Keys.ShiftKey | Keys.Shift));

            // Act
            testee.TriggerKeyUp(new KeyEventArgs(Keys.None));

            // Assert
            Assert.AreEqual(Resources.Update, testee.InsertButton.Text);
            Assert.AreEqual(Resources.UpdateEntry, testee.Text);
        });
    }

    [TestMethod]
    public void OnLoad_MissingPath_SetsTitlePathAndZeroVolume()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Some Folder\Example.mkv");

            // Act
            testee.Show();

            // Assert
            Assert.AreEqual("Example.mkv", testee.TitleTextBox.Text);
            Assert.AreEqual(@"A:\Media\Some Folder\Example.mkv", testee.PathTextBox.Text);
            Assert.AreEqual("0 Mb", testee.VolumeTextBox.Text);
        });
    }

    [TestMethod]
    public void OnLoad_FileExists_SetsVolumeFromFileSize()
    {
        RunInSta(() =>
        {
            // Arrange
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
            try
            {
                File.WriteAllBytes(path, new byte[2 * 1024 * 1024]);
                using var testee = new TestDetailsForm(path);

                // Act
                testee.Show();

                // Assert
                Assert.AreEqual("2 Mb", testee.VolumeTextBox.Text);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        });
    }

    [TestMethod]
    public void OnLoad_DirectoryExists_SetsVolumeFromDirectorySize()
    {
        RunInSta(() =>
        {
            // Arrange
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            try
            {
                File.WriteAllBytes(Path.Combine(path, "a.bin"), new byte[1024 * 1024]);
                File.WriteAllBytes(Path.Combine(path, "b.bin"), new byte[512 * 1024]);
                using var testee = new TestDetailsForm(path);

                // Act
                testee.Show();

                // Assert
                Assert.AreEqual("1 Mb", testee.VolumeTextBox.Text);
            }
            finally
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        });
    }

    [TestMethod]
    public void DoStore_PostEntryStepFails_DoesNotStoreRelatedData()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestStoreWorkflowDetailsForm(@"A:\Media\Example.mkv")
            {
                StorePostEntryDataResult = false,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsFalse(result);
            CollectionAssert.AreEqual(new[] { "PrepareForStore", "StorePreEntryData", "StoreMainEntry", "StorePostEntryData" }, testee.Calls.ToArray());
        });
    }

    [TestMethod]
    public void DoStore_ShouldStoreRelatedDataReturnsFalse_SkipsRelatedData()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestStoreWorkflowDetailsForm(@"A:\Media\Example.mkv")
            {
                ShouldStoreRelatedDataResult = false,
            };

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "PrepareForStore", "StorePreEntryData", "StoreMainEntry", "StorePostEntryData", "ShouldStoreRelatedData" }, testee.Calls.ToArray());
        });
    }

    [TestMethod]
    public void DoStore_AllStepsSucceed_StoresRelatedDataAfterGuard()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestStoreWorkflowDetailsForm(@"A:\Media\Example.mkv");

            // Act
            var result = testee.RunStore();

            // Assert
            Assert.IsTrue(result);
            CollectionAssert.AreEqual(new[] { "PrepareForStore", "StorePreEntryData", "StoreMainEntry", "StorePostEntryData", "ShouldStoreRelatedData", "StoreRelatedData" }, testee.Calls.ToArray());
        });
    }

    [TestMethod]
    public void OnDescriptionPasteClick_ClipboardTextPresent_DecoratesDescription()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();
            Clipboard.SetText("First line\r\n\r\nSecond line");

            // Act
            testee.TriggerDescriptionPasteClick();

            // Assert
            Assert.AreEqual("\tFirst line\r\n\t\r\n\tSecond line", testee.DescriptionTextBox.Text);
        });
    }

    [TestMethod]
    public void OnGenrePasteClick_MoreGenresThanAllowed_AddsGenresUpToConfiguredLimit()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();
            Clipboard.SetText("alpha,beta,gamma,delta,epsilon,zeta");

            // Act
            testee.TriggerGenrePasteClick();

            // Assert
            Assert.AreEqual(6, testee.GenresList.Items.Count);
            CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Delta", "Epsilon", "Gamma", "Zeta" }, testee.GenresList.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
        });
    }

    [TestMethod]
    public void OnDirectorsKeyUp_ControlV_DelegatesToClipboardHandler()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();

            // Act
            testee.TriggerDirectorsKeyUp(new KeyEventArgs(Keys.Control | Keys.V));

            // Assert
            Assert.AreEqual(testee.DirectorsList, testee.LastClipboardTargetListView);
        });
    }

    [TestMethod]
    public void OnCastKeyUp_ControlV_DelegatesToClipboardHandler()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();

            // Act
            testee.TriggerCastKeyUp(new KeyEventArgs(Keys.Control | Keys.V));

            // Assert
            Assert.AreEqual(testee.CastList, testee.LastClipboardTargetListView);
        });
    }

    [TestMethod]
    public void OnFilePathChanged_TextBoxChanged_UpdatesFilePath()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestDetailsForm(@"A:\Media\Example.mkv");
            testee.Show();
            testee.PathTextBox.Text = @"B:\Other\Changed.mkv";

            // Act
            testee.TriggerFilePathChanged();

            // Assert
            Assert.AreEqual(@"B:\Other\Changed.mkv", testee.FilePath);
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

    private sealed class TestDetailsForm(string filePath) : DetailsForm(filePath, NullLogger.Instance)
    {
        public Button InsertButton => m_BtnInsert;
        public Label AddGenreButton => m_AddGenreBtn;
        public ListView GenresList => m_GenresList;
        public ListView DirectorsList => m_DirectorsList;
        public ListView CastList => m_CastList;
        public TextBox TitleTextBox => m_TxtTitle;
        public TextBox PathTextBox => m_TxtPath;
        public TextBox VolumeTextBox => m_TxtVolume;
        public TextBox DescriptionTextBox => m_TxtDescription;
        public ListView? LastClipboardTargetListView { get; private set; }

        public void AddGenreForTest(string name)
        {
            AddGenre(name);
        }

        public void TriggerKeyDown(KeyEventArgs e)
        {
            typeof(DetailsForm).GetMethod("OnKeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(KeyEventArgs)], null)!.Invoke(this, [this, e]);
        }

        public void TriggerKeyUp(KeyEventArgs e)
        {
            typeof(DetailsForm).GetMethod("OnKeyUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(KeyEventArgs)], null)!.Invoke(this, [this, e]);
        }

        public void TriggerLoad(EventArgs e)
        {
            typeof(DetailsForm).GetMethod("OnLoad", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(EventArgs)], null)!.Invoke(this, [this, e]);
        }

        public void TriggerDescriptionPasteClick()
        {
            typeof(DetailsForm).GetMethod("OnDescriptionPasteClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(EventArgs)], null)!.Invoke(this, [this, EventArgs.Empty]);
        }

        public void TriggerGenrePasteClick()
        {
            typeof(DetailsForm).GetMethod("OnGenrePasteClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(EventArgs)], null)!.Invoke(this, [this, EventArgs.Empty]);
        }

        public void TriggerDirectorsKeyUp(KeyEventArgs e)
        {
            typeof(DetailsForm).GetMethod("OnDirectorsKeyUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(KeyEventArgs)], null)!.Invoke(this, [this, e]);
        }

        public void TriggerCastKeyUp(KeyEventArgs e)
        {
            typeof(DetailsForm).GetMethod("OnCastKeyUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(KeyEventArgs)], null)!.Invoke(this, [this, e]);
        }

        public void TriggerFilePathChanged()
        {
            typeof(DetailsForm).GetMethod("OnFilePathChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [typeof(object), typeof(EventArgs)], null)!.Invoke(this, [this, EventArgs.Empty]);
        }

        protected override List<string> GetGenres()
        {
            return ["Alpha", "Beta", "Gamma", "Delta", "Epsilon"];
        }

        protected override string GetGenreBySynonym(string name)
        {
            return name;
        }

        protected override Bitmap GetGenreImage(string name)
        {
            return new Bitmap(Resources.misc);
        }

        protected override void DoAddListViewItemFromClipboard(ListView listView, ImageList imageList)
        {
            LastClipboardTargetListView = listView;
        }
    }

    private sealed class TestStoreWorkflowDetailsForm(string filePath) : DetailsForm(filePath, NullLogger.Instance)
    {
        public bool StorePreEntryDataResult { get; set; } = true;
        public bool StoreMainEntryResult { get; set; } = true;
        public bool StorePostEntryDataResult { get; set; } = true;
        public bool ShouldStoreRelatedDataResult { get; set; } = true;
        public System.Collections.Generic.List<string> Calls { get; } = [];

        public bool RunStore()
        {
            return DoStore();
        }

        protected override void PrepareForStore()
        {
            Calls.Add("PrepareForStore");
        }

        protected override bool StorePreEntryData()
        {
            Calls.Add("StorePreEntryData");
            return StorePreEntryDataResult;
        }

        protected override bool StoreMainEntry()
        {
            Calls.Add("StoreMainEntry");
            return StoreMainEntryResult;
        }

        protected override bool StorePostEntryData()
        {
            Calls.Add("StorePostEntryData");
            return StorePostEntryDataResult;
        }

        protected override bool ShouldStoreRelatedData()
        {
            Calls.Add("ShouldStoreRelatedData");
            return ShouldStoreRelatedDataResult;
        }

        protected override void StoreRelatedData()
        {
            Calls.Add("StoreRelatedData");
        }
    }
}