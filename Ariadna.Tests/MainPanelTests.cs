using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.DatabaseStrategies;
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using Manina.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public class MainPanelTests
{
    [TestMethod]
    public void SelectRandomEntry_WithAvailableEntries_SelectsAndFocusesOneEntry()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries =
                [
                    new EntryDto { Id = 1, Title = "Alien", Path = @"A:\Media\Alien.mkv" },
                    new EntryDto { Id = 2, Title = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" },
                    new EntryDto { Id = 3, Title = "Terminator", Path = @"A:\Media\Terminator.mkv" },
                ],
            };
            using var testee = CreatePanel(strategy);

            // Act
            InvokeMainPanelParameterlessMethod(testee, "SelectRandomEntry");

            // Assert
            Assert.AreEqual(1, GetImageListView(testee).Items.Cast<ImageListViewItem>().Count(item => item.Selected));
            Assert.IsNotNull(GetImageListView(testee).Items.FocusedItem);
        });
    }

    [TestMethod]
    public void Constructor_WithEntries_FillsQuickListUsingFilteredFirstCharacters()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries =
                [
                    new EntryDto { Id = 1, Title = "Alien", Path = @"A:\Media\Alien.mkv" },
                    new EntryDto { Id = 2, Title = "Beta", Path = @"A:\Media\Beta.mkv" },
                    new EntryDto { Id = 3, Title = "1 Thing", Path = @"A:\Media\OneThing.mkv" },
                    new EntryDto { Id = 4, Title = "#Hash", Path = @"A:\Media\Hash.mkv" },
                ],
                QuickListFilterValues = ["1", "#"],
            };

            // Act
            using var testee = CreatePanel(strategy);

            // Assert
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, GetQuickListFlow(testee).Controls.Cast<Button>().Select(button => button.Text).ToArray());
        });
    }

    [TestMethod]
    public void ToolStripCheckboxedFilterClicked_UncheckedButton_ChecksButtonAndQueriesWithUpdatedParams()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            var wishlistButton = GetToolStripButton(testee, "m_ToolStrip_WishlistBtn");
            wishlistButton.Checked = false;

            // Act
            InvokeMainPanelMethod(testee, "ToolStrip_CheckboxedFilter_Clicked", wishlistButton, EventArgs.Empty);

            // Assert
            Assert.IsTrue(wishlistButton.Checked);
            Assert.IsNotNull(wishlistButton.Image);
            Assert.AreEqual(Resources.icon_checked.Size, wishlistButton.Image.Size);
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.IsTrue(strategy.LastQueryParams!.IsWish);
        });
    }

    [TestMethod]
    public void ToolStripVrBtnClicked_WhenNonVrChecked_UnchecksNonVrAndQueriesVr()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            var vrButton = GetToolStripButton(testee, "m_ToolStrip_VRBtn");
            var nonVrButton = GetToolStripButton(testee, "m_ToolStrip_nonVRBtn");
            vrButton.Checked = false;
            nonVrButton.Checked = true;

            // Act
            InvokeMainPanelMethod(testee, "ToolStrip_ToolStrip_VRBtn_Clicked", vrButton, EventArgs.Empty);

            // Assert
            Assert.IsTrue(vrButton.Checked);
            Assert.IsFalse(nonVrButton.Checked);
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.IsTrue(strategy.LastQueryParams!.IsVr);
            Assert.IsFalse(strategy.LastQueryParams.IsNonVr);
        });
    }

    [TestMethod]
    public void OnFloatingPanelClosed_DirectorSelection_UpdatesDirectorTextAndQueries()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            var floatingPanel = GetFloatingPanel(testee);
            floatingPanel.PanelContentType = FloatingPanel.EPanelContentType.DIRECTORS;
            floatingPanel.EntryNames.Add("Ridley Scott");

            // Act
            InvokeMainPanelMethod(testee, "OnFloatingPanelClosed", floatingPanel, EventArgs.Empty);

            // Assert
            Assert.AreEqual("Ridley Scott", testee.m_ToolStrip_DirectorName.Text);
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.AreEqual("Ridley Scott", strategy.LastQueryParams!.Director);
        });
    }

    [TestMethod]
    public void OnFloatingPanelItemSelected_WhenPanelVisible_UpdatesGenreTextAndQueries()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            testee.Show();
            var floatingPanel = GetFloatingPanel(testee);
            floatingPanel.EntryNames.AddRange(["Sci-Fi", "Thriller"]);
            floatingPanel.Show(testee.FindForm());
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "OnFloatingPanelItemSelected", floatingPanel, EventArgs.Empty);

            // Assert
            Assert.AreEqual("Sci-Fi Thriller", testee.m_ToolStrip_GenreName.Text);
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.AreEqual("Sci-Fi Thriller", strategy.LastQueryParams!.Genre);
        });
    }

    [TestMethod]
    public void OnTypeTimer_TitleField_QueriesEntries()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            testee.Show();
            GetToolStripTextBox(testee, "m_ToolStrip_EntryName").Text = "Alien";
            InvokeMainPanelMethod(testee, "OnEntryNameTextChanged", testee, EventArgs.Empty);
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "OnTypeTimer", testee, EventArgs.Empty);

            // Assert
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.AreEqual("Alien", strategy.LastQueryParams!.Name);
        });
    }

    [TestMethod]
    public void OnTypeTimer_DirectorFieldWithText_LoadsDirectorsIntoFloatingPanel()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Directors = ImmutableSortedDictionary<string, Bitmap>.Empty.Add("Ridley Scott", new Bitmap(10, 10)),
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            testee.m_ToolStrip_DirectorName.Text = "ridley";
            InvokeMainPanelMethod(testee, "OnDirectorNameTextChanged", testee, EventArgs.Empty);

            // Act
            InvokeMainPanelMethod(testee, "OnTypeTimer", testee, EventArgs.Empty);

            // Assert
            Assert.AreEqual("RIDLEY", strategy.LastDirectorLookupName);
            Assert.AreEqual(200, strategy.LastDirectorLookupLimit);
            Assert.IsTrue(GetFloatingPanel(testee).Visible);
            Assert.AreEqual(FloatingPanel.EPanelContentType.DIRECTORS, GetFloatingPanel(testee).PanelContentType);
        });
    }

    [TestMethod]
    public void MainPanelKeyPress_PlusWhenAutomaticFindSucceeds_DoesNotInvokeManualFind()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                FindNextEntryAutomaticallyResult = true,
            };
            using var testee = CreatePanel(strategy);
            testee.Show();

            // Act
            InvokeMainPanelMethod(testee, "MainPanel_KeyPress", testee, new KeyPressEventArgs('+'));

            // Assert
            Assert.AreEqual(1, strategy.FindNextEntryAutomaticallyCallCount);
            Assert.AreEqual(0, strategy.FindNextEntryManuallyCallCount);
        });
    }

    [TestMethod]
    public void MainPanelKeyPress_PlusWhenAutomaticFindFails_InvokesManualFind()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                FindNextEntryAutomaticallyResult = false,
            };
            using var testee = CreatePanel(strategy);
            testee.Show();

            // Act
            InvokeMainPanelMethod(testee, "MainPanel_KeyPress", testee, new KeyPressEventArgs('+'));

            // Assert
            Assert.AreEqual(1, strategy.FindNextEntryAutomaticallyCallCount);
            Assert.AreEqual(1, strategy.FindNextEntryManuallyCallCount);
        });
    }

    [TestMethod]
    public void ToolStripAddBtnMouseUp_LeftClickWhenAutomaticFindFails_InvokesManualFind()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                FindNextEntryAutomaticallyResult = false,
            };
            using var testee = CreatePanel(strategy);
            testee.Show();

            // Act
            InvokeMainPanelMethod(testee, "ToolStrip_AddBtn_MouseUp", testee, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));

            // Assert
            Assert.AreEqual(1, strategy.FindNextEntryAutomaticallyCallCount);
            Assert.AreEqual(1, strategy.FindNextEntryManuallyCallCount);
        });
    }

    [TestMethod]
    public void ToolStripAddBtnMouseUp_RightClick_InvokesManualFindWithoutAutomaticFind()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            testee.Show();

            // Act
            InvokeMainPanelMethod(testee, "ToolStrip_AddBtn_MouseUp", testee, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));

            // Assert
            Assert.AreEqual(0, strategy.FindNextEntryAutomaticallyCallCount);
            Assert.AreEqual(1, strategy.FindNextEntryManuallyCallCount);
        });
    }

    [TestMethod]
    public void OnQuickListClicked_WhenMatchingEntryExists_SelectsMatchingItem()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries =
                [
                    new EntryDto { Id = 1, Title = "Alien", Path = @"A:\Media\Alien.mkv" },
                    new EntryDto { Id = 2, Title = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" },
                ],
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var quickListButton = new Button { Text = "B" };

            // Act
            InvokeMainPanelMethod(testee, "OnQuickListClicked", quickListButton, EventArgs.Empty);

            // Assert
            var selectedItems = GetImageListView(testee).Items.Cast<ImageListViewItem>().Where(item => item.Selected).ToArray();
            Assert.AreEqual(1, selectedItems.Length);
            Assert.AreEqual("Blade Runner", selectedItems[0].Text);
        });
    }

    [TestMethod]
    public void ListViewMouseClicked_RightClickWithFocusedItem_ShowsEntryDetails()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 5, Title = "Alien", Path = @"A:\Media\Alien.mkv" }],
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];

            // Act
            InvokeMainPanelMethod(testee, "ListView_MouseClicked", listView, new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));

            // Assert
            Assert.AreEqual(5, strategy.LastShownEntryDetailsId);
        });
    }

    [TestMethod]
    public void ListViewMouseDoubleClick_WithFocusedItem_ExecutesEntry()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 7, Title = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" }],
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];

            // Act
            InvokeMainPanelMethod(testee, "ListView_MouseDoubleClick", listView, new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));

            // Assert
            Assert.AreEqual(7, strategy.LastExecutedEntryId);
        });
    }

    [TestMethod]
    public void OnEntryNameConfirmed_EnterPressed_QueriesEntries()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy();
            using var testee = CreatePanel(strategy);
            testee.Show();
            GetToolStripTextBox(testee, "m_ToolStrip_EntryName").Text = "Alien";
            strategy.ResetTracking();
            var keyArgs = new KeyEventArgs(Keys.Enter);

            // Act
            InvokeMainPanelMethod(testee, "OnEntryNameConfirmed", testee, keyArgs);

            // Assert
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.AreEqual("Alien", strategy.LastQueryParams!.Name);
            Assert.IsTrue(keyArgs.SuppressKeyPress);
        });
    }

    [TestMethod]
    public void EntryInserted_WhenMatchingEntryExists_QueriesAndSelectsInsertedEntry()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries =
                [
                    new EntryDto { Id = 1, Title = "Alien", Path = @"A:\Media\Alien.mkv" },
                    new EntryDto { Id = 2, Title = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" },
                ],
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            strategy.ResetTracking();

            // Act
            strategy.RaiseEntryInserted(2);

            // Assert
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.AreEqual("Blade Runner", GetImageListView(testee).Items.Cast<ImageListViewItem>().Single(item => item.Selected).Text);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenConfirmationIsCanceled_DoesNotRemoveEntryOrQuery()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 5, Title = "Alien", Path = @"A:\Media\Alien.mkv" }],
                EntryInfo = new EntryInfo { Title = "Alien", TitleOrig = "Alien", Path = @"A:\Media\Alien.mkv" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            testee.RemoveEntryConfirmationResult = DialogResult.Cancel;
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", false);

            // Assert
            Assert.AreEqual(0, strategy.RemoveEntryCallCount);
            Assert.AreEqual(0, strategy.QueryEntriesCallCount);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenConfirmedWithoutFileDeletion_RemovesEntryAndRefreshesList()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 5, Title = "Alien", Path = @"A:\Media\Alien.mkv" }],
                EntryInfo = new EntryInfo { Title = "Alien", TitleOrig = "Alien", Path = @"A:\Media\Alien.mkv" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", false);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            Assert.AreEqual(5, strategy.LastRemovedEntryId);
            Assert.AreEqual(1, strategy.QueryEntriesCallCount);
            Assert.AreEqual(0, testee.DeletedFiles.Count);
            Assert.AreEqual(0, testee.DeletedDirectories.Count);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenPreviousEntryExists_SelectsPreviousEntryAfterRefresh()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries =
                [
                    new EntryDto { Id = 1, Title = "Alien", Path = @"A:\Media\Alien.mkv" },
                    new EntryDto { Id = 2, Title = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" },
                    new EntryDto { Id = 3, Title = "Terminator", Path = @"A:\Media\Terminator.mkv" },
                ],
                EntryInfo = new EntryInfo { Title = "Blade Runner", TitleOrig = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[1];
            listView.Items[1].Selected = true;
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", false);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            Assert.AreEqual("Alien", listView.Items.FocusedItem.Text);
            Assert.AreEqual("Alien", listView.Items.Cast<ImageListViewItem>().Single(item => item.Selected).Text);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenPreviousEntryDoesNotExist_SelectsNextEntryAfterRefresh()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries =
                [
                    new EntryDto { Id = 1, Title = "Alien", Path = @"A:\Media\Alien.mkv" },
                    new EntryDto { Id = 2, Title = "Blade Runner", Path = @"A:\Media\BladeRunner.mkv" },
                ],
                EntryInfo = new EntryInfo { Title = "Alien", TitleOrig = "Alien", Path = @"A:\Media\Alien.mkv" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            listView.Items[0].Selected = true;
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", false);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            Assert.AreEqual("Blade Runner", listView.Items.FocusedItem.Text);
            Assert.AreEqual("Blade Runner", listView.Items.Cast<ImageListViewItem>().Single(item => item.Selected).Text);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenFileExistsAndDeleteFileRequested_DeletesFile()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 5, Title = "Alien", Path = @"A:\Media\Alien.mkv" }],
                EntryInfo = new EntryInfo { Title = "Alien", TitleOrig = "Alien", Path = @"A:\Media\Alien.mkv" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            testee.ExistingFilePaths.Add(@"A:\Media\Alien.mkv");
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", true);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            CollectionAssert.AreEqual(new[] { @"A:\Media\Alien.mkv" }, testee.DeletedFiles.ToArray());
            Assert.AreEqual(0, testee.ShownMessages.Count);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenDirectoryExistsAndDeleteFileRequested_DeletesDirectory()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 7, Title = "Game", Path = @"A:\Games\Game" }],
                EntryInfo = new EntryInfo { Title = "Game", TitleOrig = "Game", Path = @"A:\Games\Game" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            testee.ExistingDirectoryPaths.Add(@"A:\Games\Game");
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", true);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            CollectionAssert.AreEqual(new[] { @"A:\Games\Game" }, testee.DeletedDirectories.ToArray());
            Assert.AreEqual(0, testee.ShownMessages.Count);
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenDirectoryDeletionThrows_ShowsExceptionMessage()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 7, Title = "Game", Path = @"A:\Games\Game" }],
                EntryInfo = new EntryInfo { Title = "Game", TitleOrig = "Game", Path = @"A:\Games\Game" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            testee.ExistingDirectoryPaths.Add(@"A:\Games\Game");
            testee.DeleteDirectoryException = new IOException("Locked");
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", true);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            CollectionAssert.AreEqual(new[] { "Locked" }, testee.ShownMessages.ToArray());
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenDirectoryContainsReadOnlyFile_DeletesDirectory()
    {
        RunInSta(() =>
        {
            // Arrange
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
            var childFilePath = Path.Combine(tempDirectory, "file.txt");
            File.WriteAllText(childFilePath, "content");
            File.SetAttributes(childFilePath, File.GetAttributes(childFilePath) | FileAttributes.ReadOnly);

            try
            {
                var strategy = new TestMainPanelDbStrategy
                {
                    Entries = [new EntryDto { Id = 8, Title = "Game", Path = tempDirectory }],
                    EntryInfo = new EntryInfo { Title = "Game", TitleOrig = "Game", Path = tempDirectory },
                };
                using var testee = CreatePanel(strategy);
                testee.Show();
                var listView = GetImageListView(testee);
                listView.Items.FocusedItem = listView.Items[0];
                testee.RemoveEntryConfirmationResult = DialogResult.Yes;
                testee.UseRealFileSystem = true;
                strategy.ResetTracking();

                // Act
                InvokeMainPanelMethod(testee, "RemoveEntry", true);

                // Assert
                Assert.AreEqual(1, strategy.RemoveEntryCallCount);
                Assert.IsFalse(Directory.Exists(tempDirectory));
                Assert.AreEqual(0, testee.ShownMessages.Count);
            }
            finally
            {
                if (File.Exists(childFilePath))
                {
                    File.SetAttributes(childFilePath, FileAttributes.Normal);
                }

                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        });
    }

    [TestMethod]
    public void RemoveEntry_WhenPathDoesNotExist_ShowsPathNotFoundMessage()
    {
        RunInSta(() =>
        {
            // Arrange
            var strategy = new TestMainPanelDbStrategy
            {
                Entries = [new EntryDto { Id = 9, Title = "Missing", Path = @"A:\Missing\File.mkv" }],
                EntryInfo = new EntryInfo { Title = "Missing", TitleOrig = "Missing", Path = @"A:\Missing\File.mkv" },
            };
            using var testee = CreatePanel(strategy);
            testee.Show();
            var listView = GetImageListView(testee);
            listView.Items.FocusedItem = listView.Items[0];
            testee.RemoveEntryConfirmationResult = DialogResult.Yes;
            strategy.ResetTracking();

            // Act
            InvokeMainPanelMethod(testee, "RemoveEntry", true);

            // Assert
            Assert.AreEqual(1, strategy.RemoveEntryCallCount);
            CollectionAssert.AreEqual(new[] { @"A:\Missing\File.mkv" }, testee.PathNotFoundMessages.ToArray());
        });
    }

    private static TestMainPanel CreatePanel(TestMainPanelDbStrategy strategy)
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        return new TestMainPanel(strategy);
    }

    private static ImageListView GetImageListView(MainPanel panel)
    {
        return (ImageListView)typeof(MainPanel).GetField("m_ImageListView", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static FlowLayoutPanel GetQuickListFlow(MainPanel panel)
    {
        return (FlowLayoutPanel)typeof(MainPanel).GetField("m_QuickListFlow", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static FloatingPanel GetFloatingPanel(MainPanel panel)
    {
        return (FloatingPanel)typeof(MainPanel).GetField("m_FloatingPanel", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static ToolStripButton GetToolStripButton(MainPanel panel, string fieldName)
    {
        return (ToolStripButton)typeof(MainPanel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(panel)!;
    }

    private static ToolStripTextBox GetToolStripTextBox(MainPanel panel, string fieldName)
    {
        return (ToolStripTextBox)typeof(MainPanel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(panel)!;
    }

    private static void InvokeMainPanelMethod(MainPanel panel, string methodName, params object[] arguments)
    {
        var parameterTypes = arguments.Select(argument => argument.GetType()).ToArray();
        typeof(MainPanel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, parameterTypes, null)!.Invoke(panel, arguments);
    }

    private static void InvokeMainPanelParameterlessMethod(MainPanel panel, string methodName)
    {
        typeof(MainPanel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)!.Invoke(panel, null);
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

    private sealed class TestMainPanelDbStrategy : AbstractDbStrategy
    {
        private readonly PosterFromFileAdaptor m_Adaptor = new();

        public List<EntryDto> Entries { get; set; } = [];
        public string[] QuickListFilterValues { get; set; } = [];
        public ImmutableSortedDictionary<string, Bitmap> Directors { get; set; } = ImmutableSortedDictionary<string, Bitmap>.Empty;
        public ImmutableSortedDictionary<string, Bitmap> Actors { get; set; } = ImmutableSortedDictionary<string, Bitmap>.Empty;
        public QueryParams? LastQueryParams { get; private set; }
        public int QueryEntriesCallCount { get; private set; }
        public string? LastDirectorLookupName { get; private set; }
        public int LastDirectorLookupLimit { get; private set; }
        public bool FindNextEntryAutomaticallyResult { get; set; }
        public int FindNextEntryAutomaticallyCallCount { get; private set; }
        public int FindNextEntryManuallyCallCount { get; private set; }
        public int? LastShownEntryDetailsId { get; private set; }
        public int? LastExecutedEntryId { get; private set; }
        public EntryInfo EntryInfo { get; set; } = new();
        public int RemoveEntryCallCount { get; private set; }
        public int? LastRemovedEntryId { get; private set; }

        public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_Adaptor;

        public override List<EntryDto> GetEntries() => Entries;

        public override List<EntryDto> QueryEntries(QueryParams values)
        {
            QueryEntriesCallCount++;
            LastQueryParams = values;
            return Entries;
        }

        public override EntryInfo GetEntryInfo(int id) => EntryInfo;
        public override void ShowEntryDetails(int id)
        {
            LastShownEntryDetailsId = id;
        }

        public override void ExecuteEntry(int id)
        {
            LastExecutedEntryId = id;
        }

        public override void RemoveEntry(int id)
        {
            RemoveEntryCallCount++;
            LastRemovedEntryId = id;
            Entries = Entries.Where(entry => entry.Id != id).ToList();
        }
        public override bool FindNextEntryAutomatically()
        {
            FindNextEntryAutomaticallyCallCount++;
            return FindNextEntryAutomaticallyResult;
        }

        public override void FindNextEntryManually()
        {
            FindNextEntryManuallyCallCount++;
        }

        public override void UpdateSubgenre(MainPanel panel) {}
        public override string[] QuickListFilter() => QuickListFilterValues;

        public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit)
        {
            LastDirectorLookupName = name;
            LastDirectorLookupLimit = limit;
            return Directors;
        }

        public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => Actors;
        public override ImmutableSortedDictionary<string, Bitmap> GetGenres() => ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override void FilterControls(MainPanel panel) {}

        public void ResetTracking()
        {
            LastQueryParams = null;
            QueryEntriesCallCount = 0;
            LastDirectorLookupName = null;
            LastDirectorLookupLimit = 0;
            FindNextEntryAutomaticallyCallCount = 0;
            FindNextEntryManuallyCallCount = 0;
            LastShownEntryDetailsId = null;
            LastExecutedEntryId = null;
            RemoveEntryCallCount = 0;
            LastRemovedEntryId = null;
        }

        public void RaiseEntryInserted(int id)
        {
            OnEntryInserted(new EntryInsertedEventArgs(id));
        }
    }

    private sealed class TestMainPanel(AbstractDbStrategy strategy) : MainPanel(strategy)
    {
        private Form? host;

        public new void Show()
        {
            if (host == null)
            {
                host = new Form { ClientSize = Size };
                Dock = DockStyle.Fill;
                host.Controls.Add(this);
            }
            host.Show();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && host != null)
            {
                var window = host;
                host = null;
                window.Controls.Remove(this);
                window.Dispose();
            }
            base.Dispose(disposing);
        }

        public DialogResult RemoveEntryConfirmationResult { get; set; } = DialogResult.Yes;
        public bool UseRealFileSystem { get; set; }
        public HashSet<string> ExistingFilePaths { get; } = [];
        public HashSet<string> ExistingDirectoryPaths { get; } = [];
        public List<string> DeletedFiles { get; } = [];
        public List<string> DeletedDirectories { get; } = [];
        public List<string> ShownMessages { get; } = [];
        public List<string> PathNotFoundMessages { get; } = [];
        public IOException? DeleteDirectoryException { get; set; }

        protected override DialogResult ShowRemoveEntryConfirmation(string message, string caption, bool deleteFile)
        {
            return RemoveEntryConfirmationResult;
        }

        protected override void ShowMessage(string message)
        {
            ShownMessages.Add(message);
        }

        protected override void ShowPathNotFoundMessage(string path)
        {
            PathNotFoundMessages.Add(path);
        }

        protected override bool FileExists(string path)
        {
            if (UseRealFileSystem)
            {
                return base.FileExists(path);
            }

            return ExistingFilePaths.Contains(path);
        }

        protected override bool DirectoryExists(string path)
        {
            if (UseRealFileSystem)
            {
                return base.DirectoryExists(path);
            }

            return ExistingDirectoryPaths.Contains(path);
        }

        protected override void DeleteFile(string path)
        {
            DeletedFiles.Add(path);
        }

        protected override void DeleteDirectory(string path)
        {
            if (UseRealFileSystem)
            {
                base.DeleteDirectory(path);
                return;
            }

            if (DeleteDirectoryException != null)
            {
                throw DeleteDirectoryException;
            }

            DeletedDirectories.Add(path);
        }
    }
}
