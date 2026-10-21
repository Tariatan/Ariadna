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
        XxxCastEditor.Configure(editor, actions, true);
        AudioLanguagesSection.Visibility = editor.IsXxx ? Visibility.Collapsed : Visibility.Visible;
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
            var info = await FileInspection.InspectAsync(path, editor.Kind is CatalogKind.Movie or CatalogKind.Documentary or CatalogKind.Xxx, actions.Logger, lifetime.Token, editor.IsGame);
            if (!closed && editor.Path == path && inspectionRevision == revision)
            {
                FileInfo.Text = string.Empty;
                FileMetrics.ItemsSource = info.Metrics;
                AudioLanguages.ItemsSource = editor.IsXxx ? null : info.Languages;
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

    private void OnYearTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !ContainsOnlyDigits(e.Text);

    private void OnYearPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetData(DataFormats.UnicodeText) is not string text || !ContainsOnlyDigits(text.Trim()))
        {
            e.CancelCommand();
            return;
        }

        e.DataObject = new DataObject(DataFormats.UnicodeText, text.Trim());
        e.FormatToApply = DataFormats.UnicodeText;
    }

    private static bool ContainsOnlyDigits(string text) => text.All(char.IsAsciiDigit);

    private async void InspectChangedPath(object sender, KeyboardFocusChangedEventArgs e) => await InspectPathAsync(editor.Path);
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

        if (e.Key == Key.Enter && (Keyboard.FocusedElement == GenrePickerButton || Keyboard.FocusedElement == PasteGenresButton || Keyboard.FocusedElement == RefreshAllButton || Keyboard.FocusedElement == RefreshPosterButton || Keyboard.FocusedElement == RefreshDescriptionButton || PeopleEditor.IsRefreshFocused || CastEditor.IsRefreshFocused))
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
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.exif;*.webp;*.tiff|All files|*.*",
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

    private void ScreenshotDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            e.Handled = true;
            ReplaceImage(actions.Configuration.PreviewSuffix(1));
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
    private async void RefreshAll(object sender, RoutedEventArgs e) => await RefreshMetadataAsync(MetadataField.All, lifetime.Token);
    private async void RefreshPoster(object sender, RoutedEventArgs e) => await RefreshMetadataAsync(MetadataField.Poster, lifetime.Token);
    private async void RefreshDescription(object sender, RoutedEventArgs e) => await RefreshMetadataAsync(MetadataField.Description, lifetime.Token);

    private async void RefreshDirectors(object? sender, EventArgs e) => await RefreshMetadataAsync(MetadataField.Directors, lifetime.Token);
    private async void RefreshCast(object? sender, EventArgs e) => await RefreshMetadataAsync(MetadataField.Cast, lifetime.Token);

    private async Task RefreshMetadataAsync(MetadataField field, CancellationToken cancellationToken)
    {
        var activeButton = field switch
        {
            MetadataField.All => RefreshAllButton,
            MetadataField.Poster => RefreshPosterButton,
            MetadataField.Description => RefreshDescriptionButton,
            MetadataField.Directors => PeopleEditor.MetadataRefreshButton,
            MetadataField.Cast => CastEditor.MetadataRefreshButton,
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        Button[] activeButtons = field == MetadataField.All
            ? [RefreshAllButton, RefreshPosterButton, RefreshDescriptionButton, PeopleEditor.MetadataRefreshButton, CastEditor.MetadataRefreshButton]
            : [activeButton];
        foreach (var button in activeButtons)
        {
            RefreshIndicator.SetIsRefreshing(button, true);
        }
        RefreshAllButton.IsEnabled = false;
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
                string? failureSummary = null;
                if (field == MetadataField.All)
                {
                    failureSummary = await RefreshAllFieldsAsync(service, selected, cancellationToken);
                }
                else if (field is MetadataField.Directors or MetadataField.Cast)
                {
                    await service.RefreshPeopleAsync(selected, editor, field == MetadataField.Cast, cancellationToken);
                }
                else
                {
                    await service.RefreshAsync(selected, editor, field == MetadataField.Poster, cancellationToken);
                }

                // Keep the indicator active through queued bindings, layout and rendering.
                await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ContextIdle, cancellationToken);
                await RefreshIndicator.WaitForRenderAsync(cancellationToken);
                if (failureSummary != null && !closed)
                {
                    actions.Report(this, new InvalidDataException(failureSummary), editor.Kind);
                }
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
            foreach (var button in activeButtons)
            {
                RefreshIndicator.SetIsRefreshing(button, false);
            }
            if (!closed)
            {
                RefreshAllButton.IsEnabled = true;
                RefreshPosterButton.IsEnabled = true;
                RefreshDescriptionButton.IsEnabled = true;
                PeopleEditor.SetRefreshEnabled(true);
                CastEditor.SetRefreshEnabled(true);
            }
        }
    }

    private async Task<string?> RefreshAllFieldsAsync(MetadataService service, MetadataChoice choice, CancellationToken cancellationToken)
    {
        var revision = editor.Revision;
        var directors = editor.People.Select(person => (person.Name, person.Photo)).ToArray();
        var cast = editor.Actors.Select(person => (person.Name, person.Photo)).ToArray();
        var staged = new EntryEditorModel(actions, editor.Kind, editor.Path);
        var failures = await RefreshBatch.RunAsync(
        [
            (MetadataField.Translation, token => service.RefreshDetailsAsync(choice, staged, MetadataField.Translation, token)),
            (MetadataField.Year, token => service.RefreshDetailsAsync(choice, staged, MetadataField.Year, token)),
            (MetadataField.Genres, token => service.RefreshDetailsAsync(choice, staged, MetadataField.Genres, token)),
            (MetadataField.Poster, token => service.RefreshAsync(choice, staged, true, token)),
            (MetadataField.Description, token => service.RefreshAsync(choice, staged, false, token)),
            (MetadataField.Directors, token => service.RefreshPeopleAsync(choice, staged, false, token)),
            (MetadataField.Cast, token => service.RefreshPeopleAsync(choice, staged, true, token)),
        ], cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (closed || editor.Revision != revision ||
            !directors.SequenceEqual(editor.People.Select(person => (person.Name, person.Photo))) ||
            !cast.SequenceEqual(editor.Actors.Select(person => (person.Name, person.Photo))))
        {
            return null;
        }

        if (!failures.ContainsKey(MetadataField.Translation))
        {
            editor.Title = staged.Title;
        }

        if (!failures.ContainsKey(MetadataField.Year))
        {
            editor.Year = staged.Year;
        }

        if (!failures.ContainsKey(MetadataField.Genres))
        {
            editor.Genres.Clear();
            foreach (var genre in staged.Genres)
            {
                editor.Genres.Add(genre);
            }
        }

        if (!failures.ContainsKey(MetadataField.Poster))
        {
            editor.ReplaceImage(string.Empty, Images.Png(staged.Poster!));
        }

        if (!failures.ContainsKey(MetadataField.Description))
        {
            editor.Description = staged.Description;
        }

        if (!failures.ContainsKey(MetadataField.Directors))
        {
            editor.People.Clear();
            foreach (var person in staged.People)
            {
                editor.People.Add(person);
            }
        }

        if (!failures.ContainsKey(MetadataField.Cast))
        {
            editor.Actors.Clear();
            foreach (var person in staged.Actors)
            {
                editor.Actors.Add(person);
            }
        }

        return failures.Count == 0 ? null : "Refresh completed with unavailable fields:" + Environment.NewLine +
            string.Join(Environment.NewLine, failures.Select(failure => $"{failure.Key}: {failure.Value.Message}"));
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
