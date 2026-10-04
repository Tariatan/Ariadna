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
                var id = store.Save(kind, new CatalogDetails(new CatalogEntry
                {
                    Title = $"{kind} {index:D2}",
                    Path = $"Synthetic-{kind}-{index}",
                    Wanted = index % 2 == 0,
                }, [], [], []));
                var posters = CatalogServices.GetPosterRoot(kind);
                Directory.CreateDirectory(posters);
                using var poster = new Bitmap(24, 36);
                using var graphics = Graphics.FromImage(poster);
                graphics.Clear(Theme.Create(kind).MainBackColor);
                poster.Save(Path.Combine(posters, id.ToString()), System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable("ARIADNA_CATALOG_PATH", previousCatalog);
        Environment.SetEnvironmentVariable("ARIADNA_ASSET_ROOT", previousAssets);
        // A canceled synchronous SQLite read can finish releasing its connection after the UI closes.
        var timeout = System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            try
            {
                Directory.Delete(directory, true);
                break;
            }
            catch (IOException) when (timeout.Elapsed < TimeSpan.FromSeconds(5))
            {
                Thread.Sleep(10);
            }
        }
    }

    [TestMethod]
    public void Show_FourPermanentTabs_LoadsInitialCatalogThenPreloadsRemainingCatalogs()
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
            CollectionAssert.AreEqual((CatalogKind[])[CatalogKind.Game], created);
            UiTest.PumpUntil(() => created.Count == 4);

            // Assert
            var tabs = UiTest.Field<TabControl>(testee, "catalogTabs");
            CollectionAssert.AreEqual((string[])["Movies", "Games", "Library", "Documentaries"], tabs.TabPages.Cast<TabPage>().Select(page => page.Text).ToArray());
            CollectionAssert.AreEquivalent(Enum.GetValues<CatalogKind>(), created);
            Assert.AreEqual(CatalogKind.Game, testee.ActiveCatalog);
            Assert.HasCount(12, Grid(testee.ActiveView!).Items);
        });
    }

    [TestMethod]
    public void Show_BackgroundCatalogsReady_PreservesActivePageAndInitializesHiddenViews()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            testee.Show();
            Application.DoEvents();
            var movies = testee.ActiveView!;
            var search = UiTest.Field<ToolStripTextBox>(movies, "m_ToolStrip_EntryName");
            search.Text = "Movie 00";
            typeof(MainPanel).GetMethod("OnEntryNameConfirmed", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(movies, [search, new KeyEventArgs(Keys.Enter)]);
            var selected = Grid(movies).Items.FocusedItem;
            var scroll = Grid(movies).ViewOffset;
            search.Focus();
            var focusedControl = movies.ActiveControl;

            // Act
            var views = UiTest.Field<Dictionary<CatalogKind, MainPanel>>(testee, "views");
            UiTest.PumpUntil(() => views.Count == 4);

            // Assert
            Assert.AreSame(movies, testee.ActiveView);
            Assert.AreEqual(CatalogKind.Movie, testee.ActiveCatalog);
            Assert.AreEqual("Movie 00", search.Text);
            Assert.AreSame(focusedControl, movies.ActiveControl);
            Assert.AreSame(selected, Grid(movies).Items.FocusedItem);
            Assert.AreEqual(scroll, Grid(movies).ViewOffset);
            foreach (var (kind, view) in views.Where(pair => pair.Key != CatalogKind.Movie))
            {
                Assert.HasCount(12, Grid(view).Items);
                Assert.HasCount(1, Grid(view).SelectedItems);
                Assert.IsNotNull(Grid(view).Items.FocusedItem);
                Assert.IsFalse(view.Visible);
                Assert.AreEqual(Theme.Create(kind).MainBackColor, view.BackColor);
                Assert.IsTrue(UiTest.Field<bool>(view, "loaded"));
            }
        });
    }

    [TestMethod]
    public void SelectCatalog_HiddenCatalogPreloaded_UsesPreparedGridAndCachedPostersWithoutReloading()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(NullLogger.Instance);
            testee.Show();
            var views = UiTest.Field<Dictionary<CatalogKind, MainPanel>>(testee, "views");
            UiTest.PumpUntil(() => views.Count == 4 && testee.PreloadCompletion.IsCompleted);
            var games = views[CatalogKind.Game];
            var grid = Grid(games);
            var selected = grid.Items.FocusedItem;
            UiTest.PumpUntil(() => selected!.ThumbnailCacheState == CacheState.Cached);
            CatalogServices.CreateStore().Save(CatalogKind.Game, new CatalogDetails(new CatalogEntry
            {
                Title = "Added after preload",
                Path = "Synthetic-new-game",
            }, [], [], []));

            // Act
            testee.SelectCatalog(CatalogKind.Game);
            Application.DoEvents();

            // Assert
            Assert.AreSame(games, testee.ActiveView);
            Assert.HasCount(12, grid.Items);
            Assert.AreSame(selected, grid.Items.FocusedItem);
            Assert.AreEqual(CacheState.Cached, selected!.ThumbnailCacheState);
            Assert.HasCount(1, grid.SelectedItems);
        });
    }

    [TestMethod]
    public void SelectCatalog_PreloadInProgress_ReusesPendingViewWithoutBlockingOrDuplicateCreation()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var ready = new TaskCompletionSource<MainPanel>(TaskCreationOptions.RunContinuationsAsynchronously);
            var created = new List<CatalogKind>();
            using var testee = new MainWindow(CatalogKind.Movie, kind =>
            {
                created.Add(kind);
                return new MainPanel(CreateStrategy(kind), Theme.Create(kind));
            }, (kind, cancellationToken) => kind == CatalogKind.Game
                ? ready.Task
                : Task.FromResult(new MainPanel(CreateStrategy(kind), Theme.Create(kind))));
            testee.Show();
            UiTest.PumpUntil(() => UiTest.Field<HashSet<CatalogKind>>(testee, "preloadingCatalogs").Contains(CatalogKind.Game));

            // Act
            testee.SelectCatalog(CatalogKind.Game);
            Assert.IsNull(testee.ActiveView);
            var page = UiTest.Field<TabControl>(testee, "catalogTabs").SelectedTab!;
            Assert.AreEqual("Loading catalog...", page.Controls[0].Text);
            testee.SelectCatalog(CatalogKind.Movie);
            testee.SelectCatalog(CatalogKind.Game);
            var strategy = CreateStrategy(CatalogKind.Game);
            var prepared = new MainPanel(strategy, Theme.Create(CatalogKind.Game), strategy.GetEntries());
            ready.SetResult(prepared);
            UiTest.PumpUntil(() => testee.ActiveView != null);

            // Assert
            CollectionAssert.AreEqual((CatalogKind[])[CatalogKind.Movie], created);
            Assert.AreSame(prepared, testee.ActiveView);
            Assert.HasCount(1, page.Controls);
            Assert.HasCount(12, Grid(prepared).Items);
            Assert.HasCount(1, Grid(prepared).SelectedItems);
        });
    }

    [TestMethod]
    public void Dispose_PreloadCompletesAfterShutdown_CancelsLifetimeAndDisposesUnattachedView()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var ready = new TaskCompletionSource<MainPanel>(TaskCreationOptions.RunContinuationsAsynchronously);
            var lifetime = CancellationToken.None;
            using var testee = new MainWindow(CatalogKind.Movie,
                kind => new MainPanel(CreateStrategy(kind), Theme.Create(kind)),
                (kind, cancellationToken) =>
                {
                    lifetime = cancellationToken;
                    return kind == CatalogKind.Game ? ready.Task
                        : Task.FromResult(new MainPanel(CreateStrategy(kind), Theme.Create(kind)));
                });
            testee.Show();
            UiTest.PumpUntil(() => lifetime.CanBeCanceled);
            var strategy = CreateStrategy(CatalogKind.Game);
            var prepared = new MainPanel(strategy, Theme.Create(CatalogKind.Game), strategy.GetEntries());

            // Act
            testee.Dispose();
            ready.SetResult(prepared);
            UiTest.PumpUntil(() => prepared.IsDisposed);

            // Assert
            Assert.IsTrue(lifetime.IsCancellationRequested);
            Assert.IsTrue(prepared.IsDisposed);
            Assert.IsTrue(UiTest.Field<FloatingPanel>(prepared, "m_FloatingPanel").IsDisposed);
        });
    }

    [TestMethod]
    public void Show_OnePreloadFails_LoadsOtherCatalogsAndRetriesFailedTabOnSelection()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new MainWindow(CatalogKind.Movie,
                kind => new MainPanel(CreateStrategy(kind), Theme.Create(kind)),
                (kind, cancellationToken) => kind == CatalogKind.Game
                    ? Task.FromException<MainPanel>(new IOException("Synthetic preload failure"))
                    : Task.FromResult(new MainPanel(CreateStrategy(kind), Theme.Create(kind))));
            testee.Show();
            var views = UiTest.Field<Dictionary<CatalogKind, MainPanel>>(testee, "views");

            // Act
            UiTest.PumpUntil(() => views.Count == 3);
            Assert.AreEqual(CatalogKind.Movie, testee.ActiveCatalog);
            testee.SelectCatalog(CatalogKind.Game);

            // Assert
            Assert.HasCount(4, views);
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
                SelectReadyCatalog(testee, kind);
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
            SelectReadyCatalog(testee, CatalogKind.Library);
            SelectReadyCatalog(testee, CatalogKind.Movie);
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
            Assert.AreEqual(CatalogKind.Documentary, testee.ActiveCatalog);
            SendCommandKey(testee, Keys.Control | Keys.Tab);
            Assert.AreEqual(CatalogKind.Movie, testee.ActiveCatalog);
            SendCommandKey(testee, Keys.Control | Keys.D3);

            // Assert
            Assert.AreEqual(CatalogKind.Library, testee.ActiveCatalog);
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
            UiTest.PumpUntil(() => testee.ActiveView != null);

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
                if ((testee.ActiveCatalog == expectedCatalog && testee.PreloadCompletion.IsCompleted)
                    || timeout.Elapsed > TimeSpan.FromSeconds(5))
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
                SelectReadyCatalog(testee, otherKind);
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
                SelectReadyCatalog(testee, kind);
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

    private static void SelectReadyCatalog(MainWindow window, CatalogKind kind)
    {
        window.SelectCatalog(kind);
        UiTest.PumpUntil(() => window.ActiveView != null);
    }

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