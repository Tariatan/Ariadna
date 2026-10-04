using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace Ariadna.Wpf;
public partial class CatalogView : UserControl, IDisposable
{
    private const double MinimumPosterWidth = 155;
    private const double PosterRowHeight = 254;
    private readonly Action<CatalogView, string> edit;
    private double scrollOffset;
    private bool firstActivation = true;
    private readonly CancellationTokenSource lifetime = new();
    private CancellationTokenSource? discovery;
    internal CatalogViewModel Model { get; }
    internal CatalogActions Actions { get; }

    internal CatalogView(CatalogViewModel model, CatalogActions actions, Action<CatalogView, string> edit)
    {
        Model = model;
        Actions = actions;
        this.edit = edit;
        InitializeComponent();
        DataContext = Model;
    }

    internal async void Reload()
    {
        Actions.ThumbnailsInvalidated?.Invoke();
        await Model.RefreshAsync(CancellationToken.None);
        RevealSelection();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Model.SetColumns(Math.Max(1, (int)((PosterRows.ActualWidth - 22) / MinimumPosterWidth)));
        Dispatcher.InvokeAsync(() =>
        {
            if (firstActivation)
            {
                firstActivation = false;
                RevealSelection();
            }
            else
            {
                FindChild<ScrollViewer>(PosterRows)?.ScrollToVerticalOffset(scrollOffset);
            }
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        scrollOffset = FindChild<ScrollViewer>(PosterRows)?.VerticalOffset ?? 0;
        discovery?.Cancel();
    }

    private void OnGridSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var columns = Math.Max(1, (int)((e.NewSize.Width - 22) / MinimumPosterWidth));
        if (columns == Model.Columns)
        {
            return;
        }

        var scroll = FindChild<ScrollViewer>(PosterRows);
        var offset = scroll?.VerticalOffset ?? 0;
        var firstEntry = (int)(offset / PosterRowHeight) * Model.Columns;
        Model.SetColumns(columns);
        Dispatcher.InvokeAsync(() => scroll?.ScrollToVerticalOffset(firstEntry / columns * PosterRowHeight + offset % PosterRowHeight), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void SelectPoster(object sender, MouseButtonEventArgs e)
    {
        Model.Selected = (PosterItem)((FrameworkElement)sender).DataContext;
        PosterRows.Focus();
        if (e.ClickCount == 2)
        {
            Run(() => Actions.Execute(Model.Kind, Model.Selected.Entry.Path));
        }

        e.Handled = true;
    }

    private void EditPoster(object sender, MouseButtonEventArgs e)
    {
        Model.Selected = (PosterItem)((FrameworkElement)sender).DataContext;
        Details(sender, e);
        e.Handled = true;
    }

    private void OnGridKeyDown(object sender, KeyEventArgs e)
    {
        var page = Model.Columns * Math.Max(1, (int)(PosterRows.ActualHeight / PosterRowHeight));
        var delta = e.Key switch
        {
            Key.Left => -1,
            Key.Right => 1,
            Key.Up => -Model.Columns,
            Key.Down => Model.Columns,
            Key.PageUp => -page,
            Key.PageDown => page,
            _ => 0,
        };
        if (delta != 0)
        {
            Model.Move(delta);
            RevealSelection();
        }
        else if (e.Key == Key.Home)
        {
            Model.Selected = Model.Entries.FirstOrDefault();
            RevealSelection();
        }
        else if (e.Key == Key.End)
        {
            Model.Selected = Model.Entries.LastOrDefault();
            RevealSelection();
        }
        else if (e.Key == Key.Enter)
        {
            Execute(sender, e);
        }
        else if (e.Key == Key.F2)
        {
            Details(sender, e);
        }
        else if (e.Key == Key.Delete)
        {
            Remove(sender, e);
        }
        else
        {
            return;
        }

        e.Handled = true;
    }

    private void RevealSelection()
    {
        var row = Model.Rows.FirstOrDefault(value => value.Items.Contains(Model.Selected));
        if (row != null)
        {
            PosterRows.ScrollIntoView(row);
        }
    }

    private void QuickJump(object sender, RoutedEventArgs e)
    {
        Model.Jump((string)((Button)sender).Content);
        RevealSelection();
        PosterRows.Focus();
    }

    private void Details(object sender, RoutedEventArgs e)
    {
        if (Model.Selected != null)
        {
            Run(() => edit(this, Model.Selected.Entry.Path));
        }
    }

    private void Execute(object sender, RoutedEventArgs e)
    {
        if (Model.Selected != null)
        {
            Run(() => Actions.Execute(Model.Kind, Model.Selected.Entry.Path));
        }
    }

    private void AddFile(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = Actions.Configuration.DiscoveryRoot(Model.Kind),
            CheckFileExists = true
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            Run(() => edit(this, dialog.FileName));
        }
    }

    private void AddFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = Actions.Configuration.DiscoveryRoot(Model.Kind)
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            Run(() => edit(this, dialog.FolderName));
        }
    }

    private async void Discover(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;
        using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        discovery = request;
        try
        {
            var path = await Actions.DiscoverAsync(Model.Kind, request.Token);
            request.Token.ThrowIfCancellationRequested();
            if (IsLoaded)
            {
                if (path == null)
                {
                    MessageBox.Show(Window.GetWindow(this), "No unregistered entries were found.", "Ariadna");
                }
                else
                {
                    edit(this, path);
                }
            }
        }
        catch (OperationCanceledException)when (request.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (!request.IsCancellationRequested)
            {
                Actions.Report(Window.GetWindow(this), exception);
            }
        }
        finally
        {
            discovery = null;
            button.IsEnabled = true;
        }
    }

    private void Remove(object sender, RoutedEventArgs e)
    {
        if (Model.Selected == null)
        {
            return;
        }

        var deleteMedia = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        var prompt = deleteMedia ? "Remove this catalog entry AND permanently delete its media?" : "Remove this catalog entry and its poster images?";
        if (MessageBox.Show(Window.GetWindow(this), prompt, "Ariadna", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            Run(() => Actions.Remove(Model.Kind, Model.Selected.Entry, deleteMedia));
            Reload();
        }
    }

    private void Random(object sender, RoutedEventArgs e)
    {
        Model.Randomize();
        RevealSelection();
    }

    private void Refresh(object sender, RoutedEventArgs e) => Reload();
    private void ClearFilter(object sender, RoutedEventArgs e)
    {
        switch (((Button)sender).Tag)
        {
            case "Title":
                Model.Title = string.Empty;
                break;
            case "Person":
                Model.Person = string.Empty;
                break;
            case "Actor":
                Model.Actor = string.Empty;
                break;
            case "Genre":
                Model.Genre = string.Empty;
                break;
            case "Subgenre":
                Model.Subgenre = string.Empty;
                break;
        }
    }

    private void OnPeopleSuggestions(object sender, EventArgs e)
    {
        var combo = (ComboBox)sender;
        Run(() => combo.ItemsSource = Actions.Store.SuggestPeople((string)combo.Tag == "Actors", Model.IsLibrary, combo.Text, 30).Select(person => person.Name).ToArray());
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Actions.Report(Window.GetWindow(this), exception);
        }
    }

    internal static T? FindChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        foreach (var index in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(parent)))
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                return match;
            }

            if (FindChild<T>(child)is { } descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    public void Dispose()
    {
        lifetime.Cancel();
        Model.Dispose();
        lifetime.Dispose();
    }
}
