using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class EntryDetailsWindowTests
{
    [TestMethod]
    public async Task Layout_Game_PreviewSelectionAndReplacementRefreshImages() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture("_custom");
        var entry = fixture.Add(CatalogKind.Game, "Preview layout");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Game, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var model = (EntryEditorModel)window.DataContext;
            var thumbnails = FindButtons(window).Where(button => button.Tag is string tag && int.TryParse(tag, out _)).ToArray();
            byte[] pixels = [255, 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255];
            var image = System.Windows.Media.Imaging.BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null, pixels, 8);

            // Act
            model.ReplaceImage(fixture.Configuration.PreviewSuffix(3), Images.Png(image));
            thumbnails[2].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            window.UpdateLayout();

            // Assert
            Assert.AreEqual(4, thumbnails.Length);
            Assert.AreSame(model.Previews[2], ((Image)window.FindName("SelectedPreview")).Source);
            Assert.IsNotNull(FindImages(thumbnails[2]).Single().Source);
            var year = (TextBox)window.FindName("YearText");
            var version = (TextBox)window.FindName("VersionText");
            Assert.AreEqual(year.ActualWidth, version.ActualWidth, 0.1);
            Assert.IsTrue(version.TranslatePoint(new Point(), window).Y > year.TranslatePoint(new Point(), window).Y);
            Assert.AreEqual(year.TranslatePoint(new Point(), window).X, version.TranslatePoint(new Point(), window).X, 0.1);
            Assert.IsNull(window.FindName("PreviewReplace"));
            Assert.IsNull(window.FindName("PreviewPaste"));
        }
        finally
        {
            window.Close();
        }
    });

    private static IEnumerable<Button> FindButtons(DependencyObject root) => Descendants(root).OfType<Button>();
    private static IEnumerable<Image> FindImages(DependencyObject root) => Descendants(root).OfType<Image>();
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    [TestMethod]
    [DataRow(1000d, 640d)]
    [DataRow(1400d, 950d)]
    public async Task Layout_Documentary_DescriptionFillsRemainingHeight(double width, double height) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Documentary, "Documentary layout");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Documentary, entry.Path)
        {
            Width = width,
            Height = height,
        };
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            window.UpdateLayout();
            var description = (TextBox)window.FindName("DescriptionText");
            var content = (Grid)description.Parent;

            // Assert
            Assert.IsTrue(description.ActualHeight > 125);
            Assert.AreEqual(content.ActualHeight - description.Margin.Bottom,
                description.TranslatePoint(new Point(0, description.ActualHeight), content).Y, 1);
            Assert.IsTrue(description.TranslatePoint(new Point(0, description.ActualHeight), window).Y <
                ((FrameworkElement)window.FindName("FooterBar")).TranslatePoint(new Point(), window).Y);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Library)]
    public async Task OnKeyDown_Escape_CancelsWithoutSaving(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Original title");
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        window.Loaded += (_, _) => window.Dispatcher.BeginInvoke(() =>
        {
            var title = (TextBox)window.FindName("TitleText");
            title.Text = "Unsaved title";
            title.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.Escape)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
        }, DispatcherPriority.ApplicationIdle);

        // Act
        var result = window.ShowDialog();

        // Assert
        Assert.AreEqual(false, result);
        Assert.IsNull(window.FindName("CancelButton"));
        Assert.AreEqual(entry.Title, fixture.Store.GetDetails(kind, entry.Id)!.Entry.Title);
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Save_ShiftPressed_IgnoresOnlyOnConfirmation(bool confirm) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var path = System.IO.Path.Combine(fixture.Root, "new-media.mkv");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Movie, path);
        window.Loaded += (_, _) => window.Dispatcher.BeginInvoke(() =>
        {
            window.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.LeftShift) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            var save = (Button)window.FindName("SaveButton");
            Assert.AreEqual("Ignore", save.Content);
            Assert.AreEqual(Colors.Gold, ((SolidColorBrush)save.Foreground).Color);
            if (confirm)
            {
                ((Button)window.FindName("SaveButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else
            {
                window.DialogResult = false;
            }
        }, DispatcherPriority.ApplicationIdle);
        // Act
        var result = window.ShowDialog();
        // Assert
        Assert.AreEqual(false, result);
        Assert.AreEqual(confirm, fixture.Store.GetRegisteredPaths(CatalogKind.Movie).Contains(path));
        Assert.AreEqual(-1, fixture.Store.FindId(CatalogKind.Movie, path));
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow(1000d, 640d)]
    [DataRow(1400d, 950d)]
    public async Task Layout_WindowSize_KeepsInformationLeftAndActionsRight(double width, double height) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Minimum layout");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Movie, entry.Path) { Width = width, Height = height };
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            ((ItemsControl)window.FindName("FileMetrics")).ItemsSource = new[] { new MediaMetric("17.6 GB", MediaMetric.StorageIcon), new MediaMetric("01:56:14", MediaMetric.ClockIcon), new MediaMetric("1920×802", MediaMetric.ScreenIcon), new MediaMetric("19 Mbps", MediaMetric.BitrateIcon) };
            ((ItemsControl)window.FindName("AudioLanguages")).ItemsSource = new[] { new AudioLanguage("Russian"), new AudioLanguage("English") };
            // Act
            window.UpdateLayout();
            // Assert
            Assert.IsFalse(window.ShowInTaskbar);
            var footer = (FrameworkElement)window.FindName("FooterBar");
            var save = (FrameworkElement)window.FindName("SaveButton");
            var path = (FrameworkElement)window.FindName("MediaPath");
            Assert.AreEqual(footer.ActualWidth - 3, save.TranslatePoint(new Point(save.ActualWidth, 0), footer).X, 1);
            Assert.AreEqual(1, path.TranslatePoint(new Point(), footer).X, 1);
            var wishlist = (FrameworkElement)window.FindName("Wishlist");
            foreach (var buttonName in new[] { "SaveButton" })
            {
                var button = (FrameworkElement)window.FindName(buttonName);
                Assert.AreEqual(wishlist.TranslatePoint(new Point(0, wishlist.ActualHeight / 2), window).Y,
                    button.TranslatePoint(new Point(0, button.ActualHeight / 2), window).Y, 1, buttonName);
            }
            foreach (var name in new[] { "MediaPath", "FileMetrics", "AudioLanguages", "Wishlist", "SaveButton" })
            {
                var control = (FrameworkElement)window.FindName(name);
                var point = control.TranslatePoint(new Point(), window);
                Assert.IsTrue(point.X >= 0 && point.Y >= 0, name);
                Assert.IsTrue(point.X + control.ActualWidth <= window.ActualWidth, name);
                Assert.IsTrue(point.Y + control.ActualHeight <= window.ActualHeight, name);
            }
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task OnKeyUp_ShiftReleased_RestoresSaveAppearance() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Shift release");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Movie, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var source = PresentationSource.FromVisual(window);
            var save = (Button)window.FindName("SaveButton");
            window.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.RightShift) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            Assert.AreEqual("Ignore", save.Content);
            // Act
            window.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.RightShift) { RoutedEvent = Keyboard.PreviewKeyUpEvent });
            // Assert
            Assert.AreEqual("Save", save.Content);
            Assert.AreEqual(Colors.White, ((SolidColorBrush)save.Foreground).Color);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Library)]
    public async Task ToggleGenrePicker_ChooseAndDelete_PreservesMetadataAndGenreLimit(CatalogKind kind) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Genre picker target");
        fixture.Store.Save(kind, new CatalogDetails(entry, ["Custom genre"], [], []));
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var picker = (System.Windows.Controls.Primitives.Popup)window.FindName("GenrePicker");
            var choices = (ListBox)window.FindName("GenreChoices");
            var selected = (ListBox)window.FindName("GenreList");
            var button = (Button)window.FindName("GenrePickerButton");
            var model = (EntryEditorModel)window.DataContext;
            // Act
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.IsTrue(picker.IsOpen);
            Assert.IsFalse(choices.Items.Contains("Custom genre"));
            var chosen = (string)choices.Items[0];
            choices.SelectedItem = chosen;
            choices.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(choices), 0, Key.Enter) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            Assert.IsFalse(picker.IsOpen);
            Assert.IsTrue(model.Genres.Contains(chosen));
            selected.SelectedItem = chosen;
            selected.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.Delete) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            choices.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(choices), 0, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            Assert.IsFalse(picker.IsOpen);
            model.AddGenre("Second custom");
            model.AddGenre("Third custom");
            model.AddGenre("Fourth custom");
            model.AddGenre("Fifth custom");
            model.AddGenre("Sixth custom");
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            model.Save();
            // Assert
            Assert.IsFalse(picker.IsOpen);
            Assert.IsFalse(model.Genres.Contains(chosen));
            Assert.AreEqual(5, model.Genres.Count);
            Assert.IsFalse(model.Genres.Contains("Sixth custom"));
            var saved = fixture.Store.GetDetails(kind, entry.Id)!;
            Assert.AreEqual(entry.Title, saved.Entry.Title);
            CollectionAssert.AreEqual(model.Genres.ToArray(), saved.Genres.ToArray());
            Assert.IsNull(window.FindName("GenreName"));
        }
        finally
        {
            window.Close();
        }
    });
}
