using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace Ariadna.Wpf;
public partial class CatalogView : UserControl, IDisposable
{
    private const double MinimumPosterWidth = 250;
    private readonly Action<CatalogView, string> edit;
    private readonly System.Windows.Threading.DispatcherTimer peopleTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private Window? owner;
    private TextBox? peopleField;
    private bool applyingPerson;
    private double scrollOffset;
    private int mouseWheelDelta;
    private bool firstActivation = true;
    private readonly CancellationTokenSource lifetime = new();
    private CancellationTokenSource? discovery;
    internal CatalogViewModel Model { get; }
    internal CatalogActions Actions { get; }
    private double PosterRowHeight => FindChild<ListBoxItem>(PosterRows)?.ActualHeight ?? 0;

    internal CatalogView(CatalogViewModel model, CatalogActions actions, Action<CatalogView, string> edit)
    {
        Model = model;
        Actions = actions;
        this.edit = edit;
        InitializeComponent();
        DataContext = Model;
        peopleTimer.Tick += OnPeopleTimer;
        PreviewMouseDown += (_, _) =>
        {
            if (!PeopleSuggestions.IsMouseOver)
            {
                ClosePeopleSuggestions();
            }
        };
    }

    internal async Task Reload(int? selectedEntryId = null)
    {
        Actions.ThumbnailsInvalidated?.Invoke();
        await Model.RefreshAsync(lifetime.Token, selectedEntryId: selectedEntryId);
        RevealSelection();
        if (selectedEntryId != null && IsLoaded)
        {
            PosterRows.Focus();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        owner = Window.GetWindow(this);
        if (owner != null)
        {
            owner.Deactivated += OnOwnerDeactivated;
            owner.LocationChanged += OnOwnerDeactivated;
            owner.SizeChanged += OnOwnerDeactivated;
        }
        Model.SetColumns(Math.Max(1, (int)((PosterRows.ActualWidth - 22) / MinimumPosterWidth)));
        Dispatcher.InvokeAsync(() =>
        {
            if (!IsLoaded)
            {
                return;
            }

            if (firstActivation)
            {
                firstActivation = false;
                RevealSelection();
            }
            else
            {
                FindChild<ScrollViewer>(PosterRows)?.ScrollToVerticalOffset(scrollOffset);
            }

            PosterRows.Focus();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ClosePeopleSuggestions();
        GenrePopup.IsOpen = false;
        DetachOwner();
        scrollOffset = FindChild<ScrollViewer>(PosterRows)?.VerticalOffset ?? 0;
        mouseWheelDelta = 0;
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
        var firstEntry = (int)(scroll?.VerticalOffset ?? 0) * Model.Columns;
        Model.SetColumns(columns);
        Dispatcher.InvokeAsync(() => scroll?.ScrollToVerticalOffset(firstEntry / columns), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnGridMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scroll = FindChild<ScrollViewer>(PosterRows);
        if (scroll == null)
        {
            return;
        }

        e.Handled = true;
        mouseWheelDelta += e.Delta;
        var rows = mouseWheelDelta / Mouse.MouseWheelDeltaForOneLine;
        mouseWheelDelta %= Mouse.MouseWheelDeltaForOneLine;
        if (rows == 0)
        {
            return;
        }

        scroll.ScrollToVerticalOffset(Math.Clamp(Math.Floor(scroll.VerticalOffset) - rows, 0, scroll.ScrollableHeight));
        // Apply the queued offset before another wheel event calculates its next row.
        scroll.UpdateLayout();
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

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
        if (e.Text == "+")
        {
            AddEntry(e);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Add)
        {
            AddEntry(e);
        }
    }

    private void AddEntry(InputEventArgs e)
    {
        if (e.OriginalSource is TextBoxBase or PasswordBox ||
            (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) != 0 ||
            !AddEntryButton.IsEnabled)
        {
            return;
        }

        e.Handled = true;
        AddEntryButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private void OnGridKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            // Prevent ListBox from restoring focus to an off-screen row.
            e.Handled = true;
            return;
        }

        var rowHeight = PosterRowHeight;
        var page = Model.Columns * (rowHeight > 0 ? Math.Max(1, (int)(PosterRows.ActualHeight / rowHeight)) : 1);
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
                    CatalogDialog.ShowInformation(Window.GetWindow(this), Model.Kind, "Discovery complete", "No unregistered entries were found.");
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
                Actions.Report(Window.GetWindow(this), exception, Model.Kind);
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
        var selected = Model.Selected;
        if (selected == null)
        {
            return;
        }

        var deleteMedia = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if (CatalogDialog.ConfirmRemoval(Window.GetWindow(this), Model.Kind, selected.Caption, selected.Entry.Path, deleteMedia))
        {
            var index = Model.Entries.TakeWhile(entry => entry != selected).Count();
            var neighbour = index > 0 ? Model.Entries[index - 1] : Model.Entries.ElementAtOrDefault(index + 1);
            Run(() =>
            {
                Actions.Remove(Model.Kind, selected.Entry, deleteMedia);
                _ = Reload(neighbour?.Entry.Id);
            });
        }
    }

    private void Random(object sender, RoutedEventArgs e)
    {
        Model.Randomize();
        RevealSelection();
    }

    private void Refresh(object sender, RoutedEventArgs e) => _ = Reload();
    private void ClearFilter(object sender, RoutedEventArgs e)
    {
        GenrePopup.IsOpen = false;
        switch (((Button)sender).Tag)
        {
            case "Title":
                Model.Title = string.Empty;
                break;
            case "Person":
                ClosePeopleSuggestions();
                Model.Person = string.Empty;
                _ = Reload();
                break;
            case "Actor":
                ClosePeopleSuggestions();
                Model.Actor = string.Empty;
                _ = Reload();
                break;
            case "Genre":
                Model.Genre = string.Empty;
                break;
            case "Subgenre":
                Model.Subgenre = string.Empty;
                break;
        }
    }

    private void OnOwnerDeactivated(object? sender, EventArgs e)
    {
        ClosePeopleSuggestions();
        GenrePopup.IsOpen = false;
    }

    private void DetachOwner()
    {
        if (owner != null)
        {
            owner.Deactivated -= OnOwnerDeactivated;
            owner.LocationChanged -= OnOwnerDeactivated;
            owner.SizeChanged -= OnOwnerDeactivated;
            owner = null;
        }
    }

    private void ClosePeopleSuggestions()
    {
        peopleTimer.Stop();
        PeoplePopup.IsOpen = false;
    }

    private void OnPeopleTextChanged(object sender, TextChangedEventArgs e)
    {
        if (applyingPerson || !IsLoaded)
        {
            return;
        }

        ClosePeopleSuggestions();
        GenrePopup.IsOpen = false;
        peopleField = (TextBox)sender;
        if (peopleField.Text.Length > 0)
        {
            peopleTimer.Start();
        }
    }

    private void OnPeopleTimer(object? sender, EventArgs e)
    {
        peopleTimer.Stop();
        if (!IsLoaded || peopleField == null || !peopleField.IsKeyboardFocusWithin)
        {
            return;
        }

        Run(() =>
        {
            PeopleSuggestions.ItemsSource = Actions.Store.SuggestPeople(peopleField == ActorSearch, Model.IsLibrary, peopleField.Text, 200)
                .Select(person => new PersonEditorModel(person)).ToArray();
            PeoplePopup.IsOpen = true;
        });
    }

    private void OnPeopleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ClosePeopleSuggestions();
            e.Handled = true;
        }
        else if (e.Key == Key.Down && PeoplePopup.IsOpen && PeopleSuggestions.Items.Count > 0)
        {
            PeopleSuggestions.SelectedIndex = 0;
            ((ListBoxItem)PeopleSuggestions.ItemContainerGenerator.ContainerFromIndex(0))?.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ClosePeopleSuggestions();
            _ = Reload();
            e.Handled = true;
        }
    }

    private void OnSuggestionKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmPerson();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            ClosePeopleSuggestions();
            peopleField?.Focus();
            e.Handled = true;
        }
    }

    private void OnSuggestionDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(PeopleSuggestions, e.OriginalSource as DependencyObject) is ListBoxItem)
        {
            ConfirmPerson();
            e.Handled = true;
        }
    }

    private void ConfirmPerson()
    {
        if (PeopleSuggestions.SelectedItem is not PersonEditorModel person || peopleField == null)
        {
            return;
        }

        applyingPerson = true;
        try
        {
            peopleField.Text = person.Name;
            peopleField.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
        finally
        {
            applyingPerson = false;
        }

        ClosePeopleSuggestions();
        peopleField.Focus();
        _ = Reload();
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Actions.Report(Window.GetWindow(this), exception, Model.Kind);
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
        ClosePeopleSuggestions();
        GenrePopup.IsOpen = false;
        peopleTimer.Tick -= OnPeopleTimer;
        DetachOwner();
        lifetime.Cancel();
        Model.Dispose();
        lifetime.Dispose();
    }
}
