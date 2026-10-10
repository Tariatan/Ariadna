using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
public partial class EntryDetailsWindow : Window
{
    private readonly CatalogActions actions;
    private readonly EntryEditorModel editor;
    private readonly CancellationTokenSource lifetime = new();
    private bool closed;
    private bool saving;
    private bool shiftPressed;
    private int selectedPreview = 1;
    private string? inspectedPath;
    private int inspectionRevision;
    internal int SavedEntryId => editor.StoredId;
    internal EntryDetailsWindow(CatalogActions actions, CatalogKind kind, string path)
    {
        this.actions = actions;
        editor = new EntryEditorModel(actions, kind, path);
        InitializeComponent();
        DataContext = editor;
        PeopleEditor.Configure(editor, actions, false);
        CastEditor.Configure(editor, actions, true);
        PeopleEditor.RefreshRequested += RefreshDirectors;
        CastEditor.RefreshRequested += RefreshCast;
        SelectedPreview.Source = editor.Previews[0];
        if (kind == CatalogKind.Library)
        {
            DescriptionText.Height = 300;
        }

        if (!editor.IsMovie)
        {
            PeopleColumn.Width = new GridLength(1, GridUnitType.Star);
            CastColumn.Width = new GridLength(0);
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleText.Focus();
        var path = editor.Path;
        if (editor.IsMovie && editor.StoredId < 0 && !string.IsNullOrWhiteSpace(actions.Configuration.Get("TmdbApiKey")))
        {
            _ = FindMetadataAsync(lifetime.Token);
        }

        await InspectPathAsync(path);
    }

    private async Task InspectPathAsync(string path)
    {
        if (closed || inspectedPath == path)
        {
            return;
        }

        inspectedPath = path;
        var revision = ++inspectionRevision;
        FileMetrics.ItemsSource = null;
        AudioLanguages.ItemsSource = null;
        FileInfo.Text = "Reading file information…";
        try
        {
            var info = await FileInspection.InspectAsync(path, editor.Kind is CatalogKind.Movie or CatalogKind.Documentary, actions.Logger, lifetime.Token, editor.IsGame);
            if (!closed && editor.Path == path && inspectionRevision == revision)
            {
                FileInfo.Text = string.Empty;
                FileMetrics.ItemsSource = info.Metrics;
                AudioLanguages.ItemsSource = info.Languages;
            }
        }
        catch (OperationCanceledException)when (closed)
        {
        }
        catch (Exception exception)
        {
            if (!closed && editor.Path == path && inspectionRevision == revision)
            {
                inspectedPath = null;
                FileInfo.Text = "File information unavailable";
                actions.Logger.LogWarning("File inspection failed, error type '{ErrorType}'", exception.GetType().Name);
            }
        }
    }

    private async void InspectChangedPath(object sender, KeyboardFocusChangedEventArgs e) => await InspectPathAsync(editor.Path);
    private async void BrowsePath(object sender, RoutedEventArgs e)
    {
        string? path = null;
        if (editor.IsGame || Directory.Exists(editor.Path))
        {
            var dialog = new OpenFolderDialog { Title = "Choose media folder" };
            if (dialog.ShowDialog(this) == true)
            {
                path = dialog.FolderName;
            }
        }
        else
        {
            var dialog = new OpenFileDialog { Title = "Choose media file", Filter = "All files|*.*" };
            if (dialog.ShowDialog(this) == true)
            {
                path = dialog.FileName;
            }
        }

        if (path != null)
        {
            editor.Path = path;
            await InspectPathAsync(path);
        }
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        if (shiftPressed || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            Ignore(sender, e);
            return;
        }

        if (saving)
        {
            return;
        }

        saving = true;
        SaveButton.IsEnabled = false;
        try
        {
            editor.Save();
            DialogResult = true;
        }
        catch (Exception exception)
        {
            actions.Report(this, exception, editor.Kind);
        }
        finally
        {
            saving = false;
            SaveButton.IsEnabled = true;
        }
    }

    private void Ignore(object sender, RoutedEventArgs e)
    {
        try
        {
            actions.Store.Ignore(editor.Path);
            DialogResult = false;
        }
        catch (Exception exception)
        {
            actions.Report(this, exception, editor.Kind);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (GenrePicker.IsOpen && (e.Key is Key.Enter or Key.Escape))
        {
            OnGenrePickerKeyDown(sender, e);
            return;
        }

        if (e.Key == Key.Enter && (Keyboard.FocusedElement == GenrePickerButton || Keyboard.FocusedElement == PasteGenresButton || Keyboard.FocusedElement == RefreshPosterButton || Keyboard.FocusedElement == RefreshDescriptionButton || PeopleEditor.IsRefreshFocused || CastEditor.IsRefreshFocused))
        {
            return;
        }

        if (e.OriginalSource is TextBox && HasAncestor<PeopleEditorView>(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
            return;
        }

        if (HasAncestor<ListBox>(e.OriginalSource as DependencyObject) && e.Key == Key.Enter)
        {
            e.Handled = true;
            return;
        }

        UpdateSaveAction(e.Key is Key.LeftShift or Key.RightShift || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        if (e.Key == Key.Enter && e.OriginalSource is not TextBox { AcceptsReturn: true } && !HasAncestor<PeopleEditorView>(e.OriginalSource as DependencyObject) && !HasAncestor<ComboBox>(e.OriginalSource as DependencyObject))
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                Ignore(sender, e);
            }
            else
            {
                Save(sender, e);
            }

            e.Handled = true;
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e) => UpdateSaveAction(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
    private void OnActivated(object? sender, EventArgs e) => UpdateSaveAction(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
    private void OnDeactivated(object? sender, EventArgs e) => UpdateSaveAction(false);
    private void UpdateSaveAction(bool ignore)
    {
        shiftPressed = ignore;
        if (SaveButton != null)
        {
            SaveButton.Content = ignore ? "Ignore" : "Save";
            SaveButton.Foreground = ignore ? Brushes.Gold : Brushes.White;
        }
    }

    private static bool HasAncestor<T>(DependencyObject? element)
        where T : DependencyObject
    {
        while (element != null)
        {
            if (element is T)
            {
                return true;
            }

            element = System.Windows.Media.VisualTreeHelper.GetParent(element);
        }

        return false;
    }

    private void ReplaceImage(string suffix)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = actions.Configuration.Get("BitmapInitialSearchDir"),
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
        };
        if (dialog.ShowDialog(this) == true)
        {
            Run(() =>
            {
                editor.ReplaceImage(suffix, File.ReadAllBytes(dialog.FileName));
                UpdatePreview();
            });
        }
    }

    private void PosterDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ReplaceImage(string.Empty);
        }
    }

    private void SelectPreview(object sender, RoutedEventArgs e)
    {
        selectedPreview = int.Parse((string)((Button)sender).Tag, CultureInfo.InvariantCulture);
        UpdatePreview();
    }

    private void PreviewDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            SelectPreview(sender, e);
            e.Handled = true;
            ReplaceImage(actions.Configuration.PreviewSuffix(selectedPreview));
        }
    }

    private void UpdatePreview() => SelectedPreview.Source = editor.Previews[selectedPreview - 1];
    private void ToggleGenrePicker(object sender, RoutedEventArgs e)
    {
        if (GenrePicker.IsOpen)
        {
            GenrePicker.IsOpen = false;
            return;
        }

        if (editor.Genres.Count >= actions.Configuration.GetInt("MaxGenresCount", 5))
        {
            return;
        }

        GenreChoices.ItemsSource = editor.AvailableGenres.Where(name => !editor.Genres.Contains(name, StringComparer.OrdinalIgnoreCase)).ToArray();
        GenrePicker.IsOpen = true;
        GenreChoices.SelectedIndex = 0;
        GenreChoices.Focus();
    }

    private void ConfirmGenreChoice(object sender, RoutedEventArgs e)
    {
        if (GenreChoices.SelectedItem is string genre)
        {
            editor.AddGenre(genre);
            GenrePicker.IsOpen = false;
            e.Handled = true;
        }
    }

    private void OnGenrePickerKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmGenreChoice(sender, e);
        }
        else if (e.Key == Key.Escape)
        {
            GenrePicker.IsOpen = false;
            e.Handled = true;
        }
    }

    private void OnGenrePickerClosed(object sender, EventArgs e)
    {
        if (!closed)
        {
            GenreList.Focus();
        }
    }

    private void OnGenresKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            RemoveGenre(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            PasteGenres(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
        }
    }

    private void RemoveGenre(object sender, RoutedEventArgs e)
    {
        if (GenreList.SelectedItem is string genre)
        {
            editor.Genres.Remove(genre);
        }
    }

    private void PasteGenres(object sender, RoutedEventArgs e) => Run(() =>
    {
        foreach (var genre in Clipboard.GetText().Split(['\r', '\n', ';', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            editor.AddGenre(genre);
        }
    });
    private async void RefreshPoster(object sender, RoutedEventArgs e) => await RefreshMetadataAsync(MetadataField.Poster, lifetime.Token);
    private async void RefreshDescription(object sender, RoutedEventArgs e) => await RefreshMetadataAsync(MetadataField.Description, lifetime.Token);

    private async void RefreshDirectors(object? sender, EventArgs e) => await RefreshMetadataAsync(MetadataField.Directors, lifetime.Token);
    private async void RefreshCast(object? sender, EventArgs e) => await RefreshMetadataAsync(MetadataField.Cast, lifetime.Token);

    private async Task RefreshMetadataAsync(MetadataField field, CancellationToken cancellationToken)
    {
        var activeButton = field switch
        {
            MetadataField.Poster => RefreshPosterButton,
            MetadataField.Description => RefreshDescriptionButton,
            MetadataField.Directors => PeopleEditor.MetadataRefreshButton,
            MetadataField.Cast => CastEditor.MetadataRefreshButton,
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        RefreshIndicator.SetIsRefreshing(activeButton, true);
        RefreshPosterButton.IsEnabled = false;
        RefreshDescriptionButton.IsEnabled = false;
        PeopleEditor.SetRefreshEnabled(false);
        CastEditor.SetRefreshEnabled(false);
        try
        {
            var revision = editor.Revision;
            using var service = new MetadataService(actions.Configuration);
            var choices = await service.SearchAsync(editor.DisplayTitle, Directory.Exists(editor.Path), cancellationToken);
            if (closed || editor.Revision != revision)
            {
                return;
            }

            if (choices.Count == 0)
            {
                throw new InvalidDataException("No matching movie or series was found.");
            }

            var selected = choices.Count == 1 ? choices.First() : ChooseMetadata(choices);
            if (selected != null && !closed)
            {
                if (field is MetadataField.Directors or MetadataField.Cast)
                {
                    await service.RefreshPeopleAsync(selected, editor, field == MetadataField.Cast, cancellationToken);
                }
                else
                {
                    await service.RefreshAsync(selected, editor, field == MetadataField.Poster, cancellationToken);
                }

                // Keep the indicator active through queued bindings, layout and rendering.
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (closed)
        {
        }
        catch (Exception exception)
        {
            if (!closed)
            {
                actions.Report(this, exception, editor.Kind);
            }
        }
        finally
        {
            RefreshIndicator.SetIsRefreshing(activeButton, false);
            if (!closed)
            {
                RefreshPosterButton.IsEnabled = true;
                RefreshDescriptionButton.IsEnabled = true;
                PeopleEditor.SetRefreshEnabled(true);
                CastEditor.SetRefreshEnabled(true);
            }
        }
    }

    private async Task FindMetadataAsync(CancellationToken cancellationToken)
    {
        var revision = editor.Revision;
        try
        {
            using var service = new MetadataService(actions.Configuration);
            var choices = await service.SearchAsync(editor.DisplayTitle, Directory.Exists(editor.Path), cancellationToken);
            if (closed || editor.Revision != revision)
            {
                return;
            }

            MetadataChoice? selected = choices.Count == 1 ? choices.First() : ChooseMetadata(choices);
            if (selected != null)
            {
                await service.ApplyAsync(selected, editor, cancellationToken);
            }
        }
        catch (OperationCanceledException)when (closed)
        {
        }
        catch (Exception exception)
        {
            if (!closed)
            {
                actions.Report(this, exception, editor.Kind);
            }
        }
    }

    private MetadataChoice? ChooseMetadata(IReadOnlyCollection<MetadataChoice> choices)
    {
        if (choices.Count == 0)
        {
            return null;
        }

        var window = new MetadataChoiceWindow(this, editor.Theme, choices);
        return window.ShowDialog() == true ? window.SelectedChoice : null;
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            actions.Report(this, exception, editor.Kind);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        closed = true;
        GenrePicker.IsOpen = false;
        lifetime.Cancel();
        lifetime.Dispose();
    }
}