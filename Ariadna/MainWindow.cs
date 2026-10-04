#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Ariadna.Data;
using Ariadna.Properties;
using Ariadna.Storage;
using Ariadna.Themes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna;

public partial class MainWindow : Form
{
    private readonly Func<CatalogKind, MainPanel> createView;
    private readonly Func<CatalogKind, CancellationToken, Task<MainPanel>> prepareViewAsync;
    private readonly ILogger logger;
    private readonly CancellationTokenSource preloadLifetime = new();
    private readonly HashSet<CatalogKind> preloadingCatalogs = [];
    private readonly Icon? movieIcon;
    private readonly Dictionary<CatalogKind, MainPanel> views = [];
    private readonly Dictionary<CatalogKind, TabPage> catalogPages;
    private bool loaded;
    private bool preloadingStarted;
    private bool resourcesDisposed;
    private CatalogKind? pendingCatalog;
    private FormWindowState restoreWindowState = FormWindowState.Normal;

    public MainWindow() : this(NullLogger.Instance) { }

    public MainWindow(ILogger logger, CatalogKind initialCatalog = CatalogKind.Movie)
        : this(initialCatalog, kind => CreateView(kind, logger),
            (kind, cancellationToken) => CreateViewAsync(kind, logger, cancellationToken), logger) { }

    internal MainWindow(CatalogKind initialCatalog, Func<CatalogKind, MainPanel> createView,
        Func<CatalogKind, CancellationToken, Task<MainPanel>>? prepareViewAsync = null, ILogger? logger = null)
    {
        this.createView = createView;
        this.prepareViewAsync = prepareViewAsync ?? ((kind, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(createView(kind));
        });
        this.logger = logger ?? NullLogger.Instance;

        CreateCatalog();

        InitializeComponent();
        catalogPages = new Dictionary<CatalogKind, TabPage>
        {
            [CatalogKind.Movie] = moviesPage,
            [CatalogKind.Game] = gamesPage,
            [CatalogKind.Library] = libraryPage,
            [CatalogKind.Documentary] = documentariesPage,
        };
        movieIcon = Icon;
        foreach (var (kind, page) in catalogPages)
        {
            var theme = Theme.Create(kind);
            page.BackColor = theme.MainBackColor;
            page.ForeColor = theme.MainForeColor;
            page.Padding = Padding.Empty;
        }
        SelectCatalog(initialCatalog);
    }

    internal CatalogKind ActiveCatalog => catalogPages.Single(pair => pair.Value == catalogTabs.SelectedTab).Key;
    internal MainPanel? ActiveView => views.GetValueOrDefault(ActiveCatalog);
    internal Task PreloadCompletion { get; private set; } = Task.CompletedTask;
    private static bool ModalDialogOpen => Application.OpenForms.Cast<Form>().Any(form => form.Modal);

    private static AbstractDbStrategy CreateStrategy(CatalogKind kind, ILogger logger)
        => kind switch
        {
            CatalogKind.Movie => new MoviesDbStrategy(logger),
            CatalogKind.Documentary => new DocumentariesDbStrategy(logger),
            CatalogKind.Game => new GamesDbStrategy(logger),
            CatalogKind.Library => new LibraryDbStrategy(logger),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

    private static MainPanel CreateView(CatalogKind kind, ILogger logger)
        => new(CreateStrategy(kind, logger), Theme.Create(kind));

    private static async Task<MainPanel> CreateViewAsync(CatalogKind kind, ILogger logger, CancellationToken cancellationToken)
    {
        var strategy = CreateStrategy(kind, logger);
        // Capture configuration before dispatch so a pending read owns its database and query.
        var store = CatalogServices.CreateStore();
        var query = CatalogServices.CreateQuery(new AbstractDbStrategy.QueryParams { Subgenre = Utilities.EmptyDots },
            library: kind == CatalogKind.Library);
        var entries = await Task.Run(() => store.Query(kind, query)
            .Select(entry => new EntryDto { Id = entry.Id, Title = entry.Title, Path = entry.Path }).ToList(), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new MainPanel(strategy, Theme.Create(kind), entries);
    }

    private void CreateCatalog()
    {
        catalogTabs = new CatalogTabControl();

        moviesPage = new TabPage();
        gamesPage = new TabPage();
        libraryPage = new TabPage();
        documentariesPage = new TabPage();

        catalogTabs.Controls.Add(moviesPage);
        catalogTabs.Controls.Add(gamesPage);
        catalogTabs.Controls.Add(libraryPage);
        catalogTabs.Controls.Add(documentariesPage);
        catalogTabs.Dock = DockStyle.Fill;
        catalogTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        catalogTabs.ItemSize = new Size(200, 36);
        catalogTabs.Location = new Point(0, 0);
        catalogTabs.Name = "catalogTabs";
        catalogTabs.Padding = new Point(12, 4);
        catalogTabs.SelectedIndex = 0;
        catalogTabs.Size = new Size(1676, 900);
        catalogTabs.SizeMode = TabSizeMode.Fixed;
        catalogTabs.TabIndex = 0;
        catalogTabs.Selecting += OnCatalogSelecting;
        catalogTabs.SelectedIndexChanged += OnCatalogSelected;
        Controls.Add(catalogTabs);
    }

    internal void SelectCatalog(CatalogKind kind)
    {
        catalogTabs.SelectedTab = catalogPages[kind];
    }

    internal void ActivateExisting(CatalogKind? kind)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = restoreWindowState;
        }
        if (kind is { } requested)
        {
            if (!ModalDialogOpen)
            {
                pendingCatalog = null;
                Application.Idle -= OnActivationIdle;
                SelectCatalog(requested);
            }
            else
            {
                pendingCatalog = requested;
                Application.Idle -= OnActivationIdle;
                Application.Idle += OnActivationIdle;
            }
        }
    }

    private void OnActivationIdle(object? sender, EventArgs e)
    {
        if (ModalDialogOpen || pendingCatalog == null)
        {
            return;
        }
        var requested = pendingCatalog.Value;
        pendingCatalog = null;
        Application.Idle -= OnActivationIdle;
        SelectCatalog(requested);
        Activate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }
        RestoreWindowBounds();
        loaded = true;
        EnsureSelectedView();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ActiveView?.FocusCatalog();
        if (!IsDisposed && !preloadingStarted && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
        {
            preloadingStarted = true;
            // Let the initial catalog paint before preparing the other pages.
            BeginInvoke(new Action(StartPreloading));
        }
    }

    private void StartPreloading()
    {
        if (IsDisposed)
        {
            return;
        }
        var cancellationToken = preloadLifetime.Token;
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        var remaining = catalogPages.Keys.Where(kind => !views.ContainsKey(kind)).ToArray();
        preloadingCatalogs.UnionWith(remaining);
        PreloadCompletion = Task.WhenAll(remaining.Select(kind => PreloadCatalogAsync(kind, cancellationToken)));
    }

    private async Task PreloadCatalogAsync(CatalogKind kind, CancellationToken cancellationToken)
    {
        MainPanel? preparedView = null;
        try
        {
            preparedView = await prepareViewAsync(kind, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var view = preparedView;
            AttachView(kind, preparedView);
            preparedView = null;
            view.PreloadVisiblePosters();
            if (ActiveCatalog == kind)
            {
                ActiveView?.FocusCatalog();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Closing the window cancels preload and discards any unattached control.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to preload catalog '{Catalog}'", kind);
            if (!cancellationToken.IsCancellationRequested && ActiveCatalog == kind)
            {
                ShowLoadingMessage(kind, "Unable to load catalog. Switch tabs to retry.");
            }
        }
        finally
        {
            preparedView?.Dispose();
            preloadingCatalogs.Remove(kind);
        }
    }

    private void OnCatalogSelecting(object? sender, TabControlCancelEventArgs e) => ActiveView?.SuspendCatalog();

    private void OnCatalogSelected(object? sender, EventArgs e)
    {
        if (loaded)
        {
            EnsureSelectedView();
            ActiveView?.FocusCatalog();
        }
    }

    private void EnsureSelectedView()
    {
        Icon = ActiveCatalog switch
        {
            CatalogKind.Documentary => Resources.AriadnaDocumentaries,
            CatalogKind.Game => Resources.AriadnaGames,
            CatalogKind.Library => Resources.AriadnaLibrary,
            _ => movieIcon,
        };
        if (!views.TryGetValue(ActiveCatalog, out var view))
        {
            if (preloadingCatalogs.Contains(ActiveCatalog))
            {
                ShowLoadingMessage(ActiveCatalog, "Loading catalog...");
                return;
            }
            AttachView(ActiveCatalog, createView(ActiveCatalog));
        }
    }

    private void AttachView(CatalogKind kind, MainPanel view)
    {
        var page = catalogPages[kind];
        page.Size = catalogTabs.DisplayRectangle.Size;
        view.Visible = false;
        view.Dock = DockStyle.Fill;
        view.Size = page.ClientSize;
        view.InitializeCatalog();
        foreach (var placeholder in page.Controls.Cast<Control>().ToArray())
        {
            placeholder.Dispose();
        }
        views.Add(kind, view);
        page.Controls.Add(view);
        view.Visible = true;
    }

    private void ShowLoadingMessage(CatalogKind kind, string message)
    {
        var page = catalogPages[kind];
        if (page.Controls.Count == 0)
        {
            page.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = page.ForeColor,
            });
        }
        page.Controls[0].Text = message;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData is (Keys.Control | Keys.Tab) or (Keys.Control | Keys.Shift | Keys.Tab))
        {
            var direction = keyData.HasFlag(Keys.Shift) ? -1 : 1;
            catalogTabs.SelectedIndex = (catalogTabs.SelectedIndex + direction + catalogTabs.TabCount) % catalogTabs.TabCount;
            return true;
        }
        if ((keyData & Keys.Modifiers) == Keys.Control && (keyData & Keys.KeyCode) is >= Keys.D1 and <= Keys.D4)
        {
            catalogTabs.SelectedIndex = (keyData & Keys.KeyCode) - Keys.D1;
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        ActiveView?.HandleKeyDown(e);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        ActiveView?.HandleKeyPress(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState != FormWindowState.Minimized)
        {
            restoreWindowState = WindowState;
        }
        if (loaded)
        {
            ActiveView?.SuspendCatalog();
        }
    }

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        if (loaded)
        {
            ActiveView?.SuspendCatalog();
        }
    }

    private void RestoreWindowBounds()
    {
        var saved = new Rectangle(Settings.Default.FormLocation, Settings.Default.FormSize);
        if (saved.Width > 800 && saved.Height > 600 && Screen.AllScreens.Any(screen => Rectangle.Intersect(screen.WorkingArea, saved) is { Width: > 100, Height: > 100 }))
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = saved;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (!e.Cancel)
        {
            preloadLifetime.Cancel();
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            Settings.Default.FormLocation = bounds.Location;
            Settings.Default.FormSize = bounds.Size;
            Settings.Default.Save();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !resourcesDisposed)
        {
            resourcesDisposed = true;
            preloadLifetime.Cancel();
            preloadLifetime.Dispose();
            Application.Idle -= OnActivationIdle;
        }
        base.Dispose(disposing);
    }
}