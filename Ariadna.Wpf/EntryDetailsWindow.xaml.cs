using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    private int selectedPreview = 1;
    internal EntryDetailsWindow(CatalogActions actions, CatalogKind kind, string path)
    {
        this.actions = actions;
        editor = new EntryEditorModel(actions, kind, path);
        InitializeComponent();
        DataContext = editor;
        PeopleEditor.Configure(editor, actions, false, lifetime.Token);
        CastEditor.Configure(editor, actions, true, lifetime.Token);
        SelectedPreview.Source = editor.Previews[0];
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleText.Focus();
        var path = editor.Path;
        if (editor.IsMovie && editor.StoredId < 0 && !string.IsNullOrWhiteSpace(actions.Configuration.Get("TmdbApiKey")))
        {
            _ = FindMetadataAsync(lifetime.Token);
        }

        try
        {
            var info = await FileInspection.InspectAsync(path, editor.Kind is CatalogKind.Movie or CatalogKind.Documentary, actions.Logger, lifetime.Token);
            if (!closed && editor.Path == path)
            {
                FileInfo.Text = info;
            }
        }
        catch (OperationCanceledException)when (closed)
        {
        }
        catch (Exception exception)
        {
            if (!closed)
            {
                FileInfo.Text = "File information unavailable";
                actions.Logger.LogWarning("File inspection failed, error type '{ErrorType}'", exception.GetType().Name);
            }
        }
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
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
            PeopleEditor.Commit();
            CastEditor.Commit();
            editor.Save();
            DialogResult = true;
        }
        catch (Exception exception)
        {
            actions.Report(this, exception);
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
            actions.Report(this, exception);
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.OriginalSource is not TextBox { AcceptsReturn: true } && !HasAncestor<DataGrid>(e.OriginalSource as DependencyObject) && !HasAncestor<ComboBox>(e.OriginalSource as DependencyObject))
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

    private void ReplaceImage(object sender, RoutedEventArgs e)
    {
        var suffix = ImageSuffix((Button)sender);
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

    private void PasteImage(object sender, RoutedEventArgs e)
    {
        Run(() =>
        {
            if (Clipboard.GetImage()is { } image)
            {
                editor.ReplaceImage(ImageSuffix((Button)sender), Images.Png(image));
                UpdatePreview();
            }
        });
    }

    private void PosterDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ReplaceImage(new Button { Tag = string.Empty }, e);
        }
    }

    private void SelectPreview(object sender, RoutedEventArgs e)
    {
        selectedPreview = int.Parse((string)((Button)sender).Tag, CultureInfo.InvariantCulture);
        PreviewReplace.Tag = selectedPreview.ToString(CultureInfo.InvariantCulture);
        PreviewPaste.Tag = selectedPreview.ToString(CultureInfo.InvariantCulture);
        UpdatePreview();
    }

    private string ImageSuffix(Button button) => (string)button.Tag == string.Empty ? string.Empty : actions.Configuration.PreviewSuffix(int.Parse((string)button.Tag, CultureInfo.InvariantCulture));
    private void UpdatePreview() => SelectedPreview.Source = editor.Previews[selectedPreview - 1];
    private void AddGenre(object sender, RoutedEventArgs e) => editor.AddGenre(GenreName.Text);
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
    private async void FindMetadata(object sender, RoutedEventArgs e) => await FindMetadataAsync(lifetime.Token);
    private async Task FindMetadataAsync(CancellationToken cancellationToken)
    {
        MetadataButton.IsEnabled = false;
        var revision = editor.Revision;
        try
        {
            using var service = new MetadataService(actions.Configuration);
            var choices = await service.SearchAsync(editor.Title, Directory.Exists(editor.Path), cancellationToken);
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
                actions.Report(this, exception);
            }
        }
        finally
        {
            MetadataButton.IsEnabled = true;
        }
    }

    private MetadataChoice? ChooseMetadata(IReadOnlyCollection<MetadataChoice> choices)
    {
        if (choices.Count == 0)
        {
            MessageBox.Show(this, "No matching movies or series were found.", "Ariadna");
            return null;
        }

        var list = new ListBox
        {
            ItemsSource = choices,
            Margin = new Thickness(12),
            SelectedIndex = 0
        };
        var confirm = new Button
        {
            Content = "Choose",
            IsDefault = true,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var cancel = new Button
        {
            Content = "Cancel",
            IsCancel = true,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        buttons.Children.Add(cancel);
        buttons.Children.Add(confirm);
        var panel = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Bottom);
        panel.Children.Add(buttons);
        panel.Children.Add(list);
        var window = new Window
        {
            Owner = this,
            Title = "Choose TMDb match",
            Width = 700,
            Height = 430,
            Content = panel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        confirm.Click += (_, _) => window.DialogResult = true;
        list.MouseDoubleClick += (_, _) => window.DialogResult = true;
        return window.ShowDialog() == true ? list.SelectedItem as MetadataChoice : null;
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            actions.Report(this, exception);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        closed = true;
        lifetime.Cancel();
        lifetime.Dispose();
    }
}
