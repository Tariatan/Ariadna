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
            foreach (var buttonName in new[] { "CancelButton", "SaveButton" })
            {
                var button = (FrameworkElement)window.FindName(buttonName);
                Assert.AreEqual(wishlist.TranslatePoint(new Point(0, wishlist.ActualHeight / 2), window).Y,
                    button.TranslatePoint(new Point(0, button.ActualHeight / 2), window).Y, 1, buttonName);
            }
            foreach (var name in new[] { "MediaPath", "FileMetrics", "AudioLanguages", "Wishlist", "CancelButton", "SaveButton" })
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

}