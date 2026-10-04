using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
public partial class MainWindow : Window
{
    private readonly List<CatalogView> views = [];
    private readonly CancellationTokenSource lifetime = new();
    private bool modal;
    private bool pendingActivation;
    private string? pendingArgument;
    private readonly WindowPlacement? placement;
    internal MainWindow(CatalogStore store, CatalogConfiguration configuration, ILogger logger, string? argument, WindowPlacement? placement = null)
    {
        InitializeComponent();
        this.placement = placement;
        placement?.Restore(this);
        var thumbnails = new ThumbnailCache(logger);
        var actions = new CatalogActions(store, configuration, logger)
        {
            ThumbnailsInvalidated = thumbnails.Clear
        };
        CatalogKind[] kinds = [CatalogKind.Movie, CatalogKind.Game, CatalogKind.Library, CatalogKind.Documentary];
        foreach (var kind in kinds)
        {
            var model = new CatalogViewModel(kind, store, configuration, thumbnails, logger);
            var view = new CatalogView(model, actions, Edit);
            views.Add(view);
            CatalogTabs.Items.Add(new TabItem
            {
                Header = model.Theme.Caption,
                Content = view,
                Background = model.Theme.Background,
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(20, 8, 20, 8),
            });
        }

        CatalogTabs.SelectedIndex = IndexFor(argument);
    }

    internal Task PrepareAsync(CancellationToken cancellationToken) => views[CatalogTabs.SelectedIndex].Model.RefreshAsync(cancellationToken);
    internal async Task PreloadAsync(CancellationToken cancellationToken)
    {
        using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken);
        await Task.WhenAll(views.Where(view => view.Model.Entries.Count == 0).Select(view => view.Model.RefreshAsync(request.Token)));
    }

    private void Edit(CatalogView view, string path)
    {
        modal = true;
        try
        {
            var editor = new EntryDetailsWindow(view.Actions, view.Model.Kind, path)
            {
                Owner = this
            };
            if (editor.ShowDialog() == true)
            {
                view.Reload();
            }
        }
        finally
        {
            modal = false;
            if (pendingActivation)
            {
                pendingActivation = false;
                ActivateCatalog(pendingArgument);
            }
        }
    }

    internal void ActivateCatalog(string? argument)
    {
        if (modal)
        {
            pendingActivation = true;
            pendingArgument = argument;
            return;
        }

        if (argument != null)
        {
            CatalogTabs.SelectedIndex = IndexFor(argument);
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    private static int IndexFor(string? argument) => argument?.ToLowerInvariant() switch
    {
        "games" => 1,
        "library" => 2,
        "documentaries" => 3,
        _ => 0,
    };
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
        {
            return;
        }

        if (e.Key is >= Key.D1 and <= Key.D4)
        {
            CatalogTabs.SelectedIndex = e.Key - Key.D1;
            e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            var step = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? -1 : 1;
            CatalogTabs.SelectedIndex = (CatalogTabs.SelectedIndex + step + 4) % 4;
            e.Handled = true;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!e.Cancel)
        {
            placement?.Save(this);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        lifetime.Cancel();
        foreach (var view in views)
        {
            view.Dispose();
        }

        lifetime.Dispose();
    }
}