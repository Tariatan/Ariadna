using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;
[TestClass]
public sealed class EntryDetailsWindowTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie, false)]
    [DataRow(CatalogKind.Movie, true)]
    [DataRow(CatalogKind.Game, false)]
    [DataRow(CatalogKind.Game, true)]
    [DataRow(CatalogKind.Documentary, false)]
    [DataRow(CatalogKind.Documentary, true)]
    [DataRow(CatalogKind.Library, false)]
    [DataRow(CatalogKind.Library, true)]
    [DataRow(CatalogKind.Xxx, false)]
    [DataRow(CatalogKind.Xxx, true)]
    public async Task YearInput_AddOrEdit_AcceptsOnlyAsciiDigits(CatalogKind kind, bool existing) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var path = existing ? fixture.Add(kind, "Year input").Path : System.IO.Path.Combine(fixture.Root, "New entry");
        var window = new EntryDetailsWindow(fixture.Actions, kind, path);
        try
        {
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var year = (TextBox)window.FindName(kind == CatalogKind.Xxx ? "XxxYearText" : "YearText");

            foreach (var text in new[] { "2026", "0", "abc", "20a6", "-1", "+1", "20.26", " 2026", "2026 ", " 2026 ", "2026\r\n", "20 26", "٢٠٢٦", "２０２６" })
            {
                // Act
                var input = new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, year, text))
                {
                    RoutedEvent = TextCompositionManager.PreviewTextInputEvent,
                };
                year.RaiseEvent(input);
                var paste = new DataObjectPastingEventArgs(new DataObject(DataFormats.UnicodeText, text), false, DataFormats.UnicodeText);
                year.RaiseEvent(paste);

                // Assert
                var valid = text is "2026" or "0";
                Assert.AreEqual(!valid, input.Handled, $"Typing: {text}");
                var validPaste = valid || text is " 2026" or "2026 " or " 2026 " or "2026\r\n";
                Assert.AreEqual(!validPaste, paste.CommandCancelled, $"Pasting: {text}");
                if (validPaste)
                {
                    Assert.AreEqual(text.Trim(), paste.DataObject.GetData(DataFormats.UnicodeText));
                }
            }

            // Act
            year.Text = "2000";
            year.Select(2, 2);
            year.SelectedText = "26";

            // Assert
            Assert.AreEqual("2026", ((EntryEditorModel)window.DataContext).Year);
            Assert.IsFalse(year.AllowDrop);
            Assert.IsFalse(InputMethod.GetIsInputMethodEnabled(year));

            // Act
            year.SelectAll();
            year.SelectedText = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, ((EntryEditorModel)window.DataContext).Year);

            // Act
            var model = (EntryEditorModel)window.DataContext;
            model.Year = " 2020 ";
            var id = model.Save();

            // Assert
            Assert.AreEqual(2020, fixture.Store.GetDetails(kind, id)!.Entry.Year);
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
    public async Task Layout_RefreshButtons_VisibleOnlyForMovies(CatalogKind kind) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Refresh test");
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            // Act
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var all = (Button)window.FindName("RefreshAllButton");
            var poster = (Button)window.FindName("RefreshPosterButton");
            var description = (Button)window.FindName("RefreshDescriptionButton");

            // Assert
            var expected = kind == CatalogKind.Movie ? Visibility.Visible : Visibility.Collapsed;
            Assert.AreEqual(expected, all.Visibility);
            Assert.AreEqual("↻  all", all.Content);
            Assert.AreEqual(HorizontalAlignment.Left, all.HorizontalAlignment);
            Assert.AreEqual(1, Grid.GetRow(all));
            Assert.AreEqual(expected, poster.Visibility);
            Assert.AreEqual(expected, description.Visibility);
            Assert.AreEqual(1, Grid.GetRow(poster));
            Assert.AreEqual(Dock.Right, DockPanel.GetDock(description));
            foreach (var name in new[] { "PeopleEditor", "CastEditor" })
            {
                var view = (PeopleEditorView)window.FindName(name);
                var refresh = (Button)view.FindName("RefreshButton");
                Assert.AreEqual(expected, refresh.Visibility);
                Assert.AreEqual("↻", refresh.Content);
                Assert.IsTrue(((StackPanel)refresh.Parent).Children.IndexOf(refresh) < ((StackPanel)refresh.Parent).Children.IndexOf((Button)view.FindName("PasteButton")));
            }
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task Save_MovieTitleFields_PreservesOriginalAndTranslationMapping() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Translated title");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Movie, entry.Path);
        try
        {
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var title = (TextBox)window.FindName("TitleText");
            var translation = (TextBox)window.FindName("SecondaryTitleText");
            Assert.AreEqual(entry.OriginalTitle, title.Text);
            Assert.AreEqual(entry.Title, translation.Text);

            // Act
            title.Text = "Edited original";
            translation.Text = "Edited translation";
            ((EntryEditorModel)window.DataContext).Save();
            var saved = fixture.Store.GetDetails(CatalogKind.Movie, entry.Id)!;

            // Assert
            Assert.AreEqual("Edited original", saved.Entry.OriginalTitle);
            Assert.AreEqual("Edited translation", saved.Entry.Title);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task Layout_Game_PreviewSelectionAndReplacementRefreshImages() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture("_custom");
        var entry = fixture.Add(CatalogKind.Game, "Preview layout");
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Game, entry.Path);
        try
        {
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var model = (EntryEditorModel)window.DataContext;
            var thumbnails = FindButtons(HeadlessLayout.Root(window)).Where(button => button.Tag is string tag && int.TryParse(tag, out _)).ToArray();
            byte[] pixels = [255, 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255];
            var image = System.Windows.Media.Imaging.BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null, pixels, 8);

            // Act
            model.ReplaceImage(fixture.Configuration.PreviewSuffix(3), Images.Png(image));
            thumbnails[2].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.AreEqual(4, thumbnails.Length);
            Assert.AreSame(model.Previews[2], ((Image)window.FindName("SelectedPreview")).Source);
            Assert.IsNotNull(FindImages(thumbnails[2]).Single().Source);
            var year = (TextBox)window.FindName("YearText");
            var version = (TextBox)window.FindName("VersionText");
            Assert.AreEqual(80, year.ActualWidth, 0.1);
            Assert.AreEqual(120, version.ActualWidth, 0.1);
            Assert.IsTrue(version.TranslatePoint(new Point(), HeadlessLayout.Root(window)).Y > year.TranslatePoint(new Point(), HeadlessLayout.Root(window)).Y);
            Assert.AreEqual(year.TranslatePoint(new Point(), HeadlessLayout.Root(window)).X, version.TranslatePoint(new Point(), HeadlessLayout.Root(window)).X, 0.1);
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            HeadlessLayout.Arrange(window);
            var description = (TextBox)window.FindName("DescriptionText");
            var content = (Grid)description.Parent;

            // Assert
            Assert.IsTrue(description.ActualHeight > 125);
            Assert.AreEqual(content.ActualHeight - description.Margin.Bottom,
                description.TranslatePoint(new Point(0, description.ActualHeight), content).Y, 1);
            Assert.IsTrue(description.TranslatePoint(new Point(0, description.ActualHeight), HeadlessLayout.Root(window)).Y <
                ((FrameworkElement)window.FindName("FooterBar")).TranslatePoint(new Point(), HeadlessLayout.Root(window)).Y);
        }
        finally
        {
            window.Close();
        }
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            ((ItemsControl)window.FindName("FileMetrics")).ItemsSource = new[] { new MediaMetric("17.6 GB", MediaMetric.StorageIcon), new MediaMetric("01:56:14", MediaMetric.ClockIcon), new MediaMetric("1920×802", MediaMetric.ScreenIcon), new MediaMetric("19 Mbps", MediaMetric.BitrateIcon) };
            ((ItemsControl)window.FindName("AudioLanguages")).ItemsSource = new[] { new AudioLanguage("Russian"), new AudioLanguage("English") };
            // Act
            HeadlessLayout.Arrange(window);
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
                Assert.AreEqual(wishlist.TranslatePoint(new Point(0, wishlist.ActualHeight / 2), HeadlessLayout.Root(window)).Y,
                    button.TranslatePoint(new Point(0, button.ActualHeight / 2), HeadlessLayout.Root(window)).Y, 1, buttonName);
            }
            foreach (var name in new[] { "MediaPath", "FileMetrics", "AudioLanguages", "Wishlist", "SaveButton" })
            {
                var control = (FrameworkElement)window.FindName(name);
                var point = control.TranslatePoint(new Point(), HeadlessLayout.Root(window));
                Assert.IsTrue(point.X >= 0 && point.Y >= 0, name);
                Assert.IsTrue(point.X + control.ActualWidth <= HeadlessLayout.Root(window).ActualWidth, name);
                Assert.IsTrue(point.Y + control.ActualHeight <= HeadlessLayout.Root(window).ActualHeight, name);
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var source = HeadlessLayout.Source;
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

}
