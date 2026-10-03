using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.DatabaseStrategies;
using Ariadna.ImageListHelpers;
using Ariadna.Storage;
using Ariadna.Tests.AuxiliaryPopups;
using Ariadna.Themes;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
[DoNotParallelize]
public sealed class MainWindowTests
{
    private string directory = string.Empty;
    private string? previousCatalog;
    private string? previousAssets;

    [TestInitialize]
    public void Initialize()
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        directory = Path.Combine(Path.GetTempPath(), "Ariadna-tabs-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var catalog = Path.Combine(directory, "catalog.sqlite");
        CatalogDatabase.Create(catalog);
        previousCatalog = Environment.GetEnvironmentVariable("ARIADNA_CATALOG_PATH");
        previousAssets = Environment.GetEnvironmentVariable("ARIADNA_ASSET_ROOT");
        Environment.SetEnvironmentVariable("ARIADNA_CATALOG_PATH", catalog);
        Environment.SetEnvironmentVariable("ARIADNA_ASSET_ROOT", directory);
        var store = CatalogServices.CreateStore();
        foreach (var kind in Enum.GetValues<CatalogKind>())
        {
            for (var index = 0; index < 12; index++)
            {
                store.Save(kind, new CatalogDetails(new CatalogEntry
                {
                    Title = $"{kind} {index:D2}",
                    Path = $"Synthetic-{kind}-{index}",
                    Wanted = index % 2 == 0,
                }, [], [], []));
            }
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable("ARIADNA_CATALOG_PATH", previousCatalog);
        Environment.SetEnvironmentVariable("ARIADNA_ASSET_ROOT", previousAssets);
        Directory.Delete(directory, true);
    }

    [TestMethod]
    public void Show_FourPermanentTabs_LoadsOnlyTheInitialCatalog()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var created = new List<CatalogKind>();
            using var testee = new MainWindow(CatalogKind.Game, kind =>
            {
                created.Add(kind);
                return new MainPanel(CreateStrategy(kind), Theme.Create(kind));
            });

            // Act
            testee.Show();
            Application.DoEvents();

            // Assert
            var tabs = UiTest.Field<TabControl>(testee, "catalogTabs");
            CollectionAssert.AreEqual((string[])["Movies", "Documentaries", "Games", "Library"], tabs.TabPages.Cast<TabPage>().Select(page => page.Text).ToArray());
            CollectionAssert.AreEqual((CatalogKind[])[CatalogKind.Game], created);
            Assert.AreEqual(CatalogKind.Game, testee.ActiveCatalog);
            Assert.HasCount(12, Grid(testee.ActiveView!).Items);
        });
    }

    [TestMethod]
    public void SelectCatalog_ReturningToLoadedTab_PreservesFiltersSelectionScrollAndPalette()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            testee.Show();
            Application.DoEvents();
            var movies = testee.ActiveView!;
            var search = UiTest.Field<ToolStripTextBox>(movies, "m_ToolStrip_EntryName");
            var wishlist = UiTest.Field<ToolStripButton>(movies, "m_ToolStrip_WishlistBtn");
            wishlist.PerformClick();
            search.Text = "Movie";
            typeof(MainPanel).GetMethod("OnEntryNameConfirmed", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(movies, [search, new KeyEventArgs(Keys.Enter)]);
            var grid = Grid(movies);
            grid.EnsureVisible(grid.Items.Count - 1);
            grid.Items.FocusedItem = grid.Items[^1];
            grid.Items[^1].Selected = true;
            var selected = grid.Items.FocusedItem;
            var scroll = grid.ViewOffset;
            var itemCount = grid.Items.Count;

            // Act
            foreach (var kind in (CatalogKind[])[CatalogKind.Documentary, CatalogKind.Game, CatalogKind.Library, CatalogKind.Movie])
            {
                testee.SelectCatalog(kind);
                Application.DoEvents();
                Assert.AreEqual(Theme.Create(kind).MainBackColor, testee.ActiveView!.BackColor);
                Assert.AreEqual(Theme.Create(kind).MainBackColor, Grid(testee.ActiveView!).vScrollBar.BackColor);
            }

            // Assert
            Assert.AreSame(movies, testee.ActiveView);
            Assert.AreEqual("Movie", search.Text);
            Assert.IsTrue(wishlist.Checked);
            Assert.AreSame(selected, grid.Items.FocusedItem);
            Assert.AreEqual(scroll, grid.ViewOffset);
            Assert.HasCount(itemCount, grid.Items);
            using var canvas = new Bitmap(20, 20);
            using var graphics = Graphics.FromImage(canvas);
            UiTest.Field<ImageListViewAriadnaRenderer>(movies, "m_ListViewRenderer").DrawBackground(graphics, new Rectangle(0, 0, 20, 20));
            Assert.IsTrue(canvas.GetPixel(10, 1).R > canvas.GetPixel(10, 1).G);
        });
    }

    [TestMethod]
    public void SelectCatalog_PendingSearchAndOpenPicker_CommitsSearchAndClosesOldPicker()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            testee.Show();
            Application.DoEvents();
            var movies = testee.ActiveView!;
            UiTest.Field<ToolStripTextBox>(movies, "m_ToolStrip_EntryName").Text = "Movie 00";
            var picker = UiTest.Field<FloatingPanel>(movies, "m_FloatingPanel");
            picker.Show(testee);

            // Act
            testee.SelectCatalog(CatalogKind.Library);
            testee.SelectCatalog(CatalogKind.Movie);
            Application.DoEvents();

            // Assert
            Assert.IsFalse(picker.Visible);
            Assert.HasCount(1, Grid(movies).Items);
            Assert.AreEqual("Movie 00", Grid(movies).Items[0].Text);
            Assert.IsFalse(UiTest.Field<System.Windows.Forms.Timer>(movies, "m_TypeTimer").Enabled);
        });
    }

    [TestMethod]
    public void ProcessCmdKey_BrowserTabShortcuts_SelectsAndWrapsCatalogTabs()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            testee.Show();
            Application.DoEvents();

            // Act
            SendCommandKey(testee, Keys.Control | Keys.Shift | Keys.Tab);
            Assert.AreEqual(CatalogKind.Library, testee.ActiveCatalog);
            SendCommandKey(testee, Keys.Control | Keys.Tab);
            Assert.AreEqual(CatalogKind.Movie, testee.ActiveCatalog);
            SendCommandKey(testee, Keys.Control | Keys.D3);

            // Assert
            Assert.AreEqual(CatalogKind.Game, testee.ActiveCatalog);
        });
    }

    [TestMethod]
    public void ActivateExisting_NoArgumentAndRequestedCatalog_PreservesOrSelectsTheExistingTab()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance, CatalogKind.Library);
            testee.Show();
            Application.DoEvents();
            var library = testee.ActiveView;

            // Act
            testee.ActivateExisting(null);
            Assert.AreSame(library, testee.ActiveView);
            testee.ActivateExisting(CatalogKind.Game);

            // Assert
            Assert.AreEqual(CatalogKind.Game, testee.ActiveCatalog);
            Assert.IsFalse(testee.ActiveView!.m_ToolStrip_ActorName.Visible);
            Assert.IsTrue(testee.ActiveView.m_ToolStrip_VRBtn.Visible);
            Assert.IsNotNull(testee.Icon);
        });
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ActivateExisting_ModalEditorOpen_AppliesLatestTabRequestAfterEditorCloses(bool newerRequestAfterClose)
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            using var editor = new MovieDetailsForm();
            using var completionTimer = new System.Windows.Forms.Timer { Interval = 25 };
            var timeout = System.Diagnostics.Stopwatch.StartNew();
            CatalogKind? completedCatalog = null;
            var expectedCatalog = newerRequestAfterClose ? CatalogKind.Library : CatalogKind.Game;
            completionTimer.Tick += (_, _) =>
            {
                if (testee.ActiveCatalog == expectedCatalog || timeout.Elapsed > TimeSpan.FromSeconds(5))
                {
                    completedCatalog = testee.ActiveCatalog;
                    completionTimer.Stop();
                    Application.ExitThread();
                }
            };
            editor.Shown += (_, _) =>
            {
                // Act
                testee.ActivateExisting(CatalogKind.Game);
                Assert.AreEqual(CatalogKind.Movie, testee.ActiveCatalog);
                editor.Close();
            };
            testee.Shown += (_, _) =>
            {
                editor.ShowDialog(testee);
                if (newerRequestAfterClose)
                {
                    testee.ActivateExisting(CatalogKind.Library);
                }
                completionTimer.Start();
            };

            Application.Run(testee);

            // Assert
            Assert.AreEqual(expectedCatalog, completedCatalog);
        });
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void ShowEntryDetails_EditorOwnedByTabbedWindow_SavesAndRefreshesOnlyItsCatalog(CatalogKind kind)
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance, kind);
            testee.Show();
            Application.DoEvents();
            var view = testee.ActiveView!;
            var strategy = UiTest.Field<AbstractDbStrategy>(view, "m_DbStrategy");
            var entry = CatalogServices.CreateStore().Query(kind, new CatalogQuery()).First();
            using var editTimer = new System.Windows.Forms.Timer { Interval = 25 };
            var editorOpened = false;
            editTimer.Tick += (_, _) =>
            {
                var editor = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form is IEntryDetailsDialog);
                if (editor == null)
                {
                    return;
                }
                editTimer.Stop();
                editorOpened = true;
                Assert.AreEqual(Theme.Create(kind).DetailsFormBackColor, editor.BackColor);
                UiTest.Field<TextBox>(editor, "titleText").Text = "Edited in tab";
                UiTest.Field<Button>(editor, "saveButton").PerformClick();
            };

            // Act
            editTimer.Start();
            strategy.ShowEntryDetails(entry.Id);
            Application.DoEvents();

            // Assert
            Assert.IsTrue(editorOpened);
            Assert.AreEqual(kind, testee.ActiveCatalog);
            Assert.AreEqual("Edited in tab", CatalogServices.CreateStore().GetDetails(kind, entry.Id)!.Entry.Title);
            Assert.IsTrue(Grid(view).Items.Any(item => item.Text == "Edited in tab"));
            foreach (var otherKind in Enum.GetValues<CatalogKind>().Where(other => other != kind))
            {
                testee.SelectCatalog(otherKind);
                Assert.IsFalse(Grid(testee.ActiveView!).Items.Any(item => item.Text == "Edited in tab"));
            }
        });
    }

    [TestMethod]
    public void Dispose_LoadedTabs_DisposesPickersTimersAndStrategySubscriptions()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            testee.Show();
            var panels = new List<MainPanel>();
            foreach (var kind in Enum.GetValues<CatalogKind>())
            {
                testee.SelectCatalog(kind);
                panels.Add(testee.ActiveView!);
            }

            // Act
            testee.Dispose();

            // Assert
            foreach (var panel in panels)
            {
                Assert.IsTrue(panel.IsDisposed);
                Assert.IsTrue(UiTest.Field<FloatingPanel>(panel, "m_FloatingPanel").IsDisposed);
                Assert.IsFalse(UiTest.Field<System.Windows.Forms.Timer>(panel, "m_TypeTimer").Enabled);
                var strategy = UiTest.Field<AbstractDbStrategy>(panel, "m_DbStrategy");
                Assert.IsNull(typeof(AbstractDbStrategy).GetField("EntryInserted", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(strategy));
            }
        });
    }

    private static ImageListView Grid(MainPanel panel) => UiTest.Field<ImageListView>(panel, "m_ImageListView");

    private static void SendCommandKey(MainWindow window, Keys key)
    {
        var message = new Message();
        Assert.IsTrue((bool)typeof(MainWindow).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [message, key])!);
    }

    private static AbstractDbStrategy CreateStrategy(CatalogKind kind) => kind switch
    {
        CatalogKind.Movie => new MoviesDbStrategy(NullLogger.Instance),
        CatalogKind.Documentary => new DocumentariesDbStrategy(NullLogger.Instance),
        CatalogKind.Game => new GamesDbStrategy(NullLogger.Instance),
        CatalogKind.Library => new LibraryDbStrategy(NullLogger.Instance),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
