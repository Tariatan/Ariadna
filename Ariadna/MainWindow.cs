#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ariadna.DatabaseStrategies;
using Ariadna.Properties;
using Ariadna.Storage;
using Ariadna.Themes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna;

public partial class MainWindow : Form
{
    private readonly Func<CatalogKind, MainPanel> createView;
    private readonly Icon? movieIcon;
    private readonly Dictionary<CatalogKind, MainPanel> views = [];
    private bool loaded;
    private CatalogKind? pendingCatalog;
    private FormWindowState restoreWindowState = FormWindowState.Normal;

    public MainWindow() : this(NullLogger.Instance) { }

    public MainWindow(ILogger logger, CatalogKind initialCatalog = CatalogKind.Movie)
        : this(initialCatalog, kind => CreateView(kind, logger)) { }

    internal MainWindow(CatalogKind initialCatalog, Func<CatalogKind, MainPanel> createView)
    {
        this.createView = createView;
        InitializeComponent();
        movieIcon = Icon;
        foreach (TabPage page in catalogTabs.TabPages)
        {
            var theme = Theme.Create((CatalogKind)page.Tag!);
            page.BackColor = theme.MainBackColor;
            page.ForeColor = theme.MainForeColor;
            page.Padding = Padding.Empty;
        }
        SelectCatalog(initialCatalog);
    }

    internal CatalogKind ActiveCatalog => (CatalogKind)catalogTabs.SelectedTab!.Tag!;
    internal MainPanel? ActiveView => views.GetValueOrDefault(ActiveCatalog);
    private static bool ModalDialogOpen => Application.OpenForms.Cast<Form>().Any(form => form.Modal);

    private static MainPanel CreateView(CatalogKind kind, ILogger logger)
    {
        AbstractDbStrategy strategy = kind switch
        {
            CatalogKind.Movie => new MoviesDbStrategy(logger),
            CatalogKind.Documentary => new DocumentariesDbStrategy(logger),
            CatalogKind.Game => new GamesDbStrategy(logger),
            CatalogKind.Library => new LibraryDbStrategy(logger),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        return new MainPanel(strategy, Theme.Create(kind));
    }

    internal void SelectCatalog(CatalogKind kind)
    {
        catalogTabs.SelectedTab = catalogTabs.TabPages.Cast<TabPage>().Single(page => (CatalogKind)page.Tag! == kind);
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
            view = createView(ActiveCatalog);
            view.Dock = DockStyle.Fill;
            views.Add(ActiveCatalog, view);
            catalogTabs.SelectedTab!.Controls.Add(view);
        }
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
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            Settings.Default.FormLocation = bounds.Location;
            Settings.Default.FormSize = bounds.Size;
            Settings.Default.Save();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Application.Idle -= OnActivationIdle;
        }
        base.Dispose(disposing);
    }
}
