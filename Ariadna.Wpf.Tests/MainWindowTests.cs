using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class MainWindowTests
{
    [TestMethod]
    [DataRow(700)]
    [DataRow(1120)]
    [DataRow(1600)]
    public async Task Show_WindowWidths_DisplaysAssemblyVersionBesideAllTabs(int width) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        string[] arguments = ["games", "library", "documentaries", "movies"];
        using var fixture = new CatalogFixture();
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null)
        {
            Width = width,
        };
        try
        {
            // Act
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var tabs = (TabControl)window.FindName("CatalogTabs");
            var label = (TextBlock)tabs.Template.FindName("AppVersionLabel", tabs);
            var labelPosition = label.TranslatePoint(new Point(), tabs);

            // Assert
            Assert.AreEqual(typeof(MainWindow).Assembly.GetName().Version!.ToString(3), label.Text);
            Assert.AreEqual(Colors.Black, ((SolidColorBrush)label.Foreground).Color);
            Assert.AreEqual(0.5, label.Opacity);
            Assert.IsFalse(label.Focusable);
            Assert.IsFalse(label.IsHitTestVisible);
            Assert.AreEqual(tabs.ActualWidth - 12, labelPosition.X + label.ActualWidth, 1);
            Assert.HasCount(4, tabs.Items);
            foreach (TabItem tab in tabs.Items)
            {
                Assert.IsTrue(tab.TranslatePoint(new Point(tab.ActualWidth, 0), tabs).X <= labelPosition.X);
                Assert.IsTrue(labelPosition.Y >= 0 && labelPosition.Y + label.ActualHeight <= tab.ActualHeight);
            }

            foreach (var argument in arguments)
            {
                window.ActivateCatalog(argument);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                Assert.IsTrue(label.IsVisible);
                var contentHost = (ContentPresenter)tabs.Template.FindName("PART_SelectedContentHost", tabs);
                Assert.AreSame(((TabItem)tabs.SelectedItem).Content, contentHost.Content);
            }
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(WindowState.Normal)]
    [DataRow(WindowState.Minimized)]
    [DataRow(WindowState.Maximized)]
    public async Task Close_WindowMovedAndResized_RestoresNormalBoundsOnNextLaunch(WindowState state) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var path = System.IO.Path.Combine(fixture.Root, "window.json");
        var placement = new WindowPlacement(path, NullLogger.Instance);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null, placement);
        window.Show();
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = SystemParameters.WorkArea.Left + 30;
        window.Top = SystemParameters.WorkArea.Top + 30;
        window.Width = 820;
        window.Height = 620;
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        var bounds = window.RestoreBounds;
        window.WindowState = state;
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

        // Act
        window.Close();
        var reopened = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null, placement);
        try
        {
            reopened.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsTrue(System.IO.File.Exists(path));
            Assert.AreEqual(WindowStartupLocation.Manual, reopened.WindowStartupLocation);
            Assert.AreEqual(bounds.Left, reopened.RestoreBounds.Left, 1);
            Assert.AreEqual(bounds.Top, reopened.RestoreBounds.Top, 1);
            Assert.AreEqual(bounds.Width, reopened.RestoreBounds.Width, 1);
            Assert.AreEqual(bounds.Height, reopened.RestoreBounds.Height, 1);
            Assert.AreEqual(state == WindowState.Maximized ? WindowState.Maximized : WindowState.Normal, reopened.WindowState);
        }
        finally
        {
            reopened.Close();
        }
    });

    [TestMethod]
    public async Task Close_ClosureCanceled_DoesNotOverwriteSavedPlacement() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var path = System.IO.Path.Combine(fixture.Root, "window.json");
        var placement = new WindowPlacement(path, NullLogger.Instance);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null, placement);
        window.Show();
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        placement.Save(window);
        var saved = System.IO.File.ReadAllText(path);
        window.Width += 80;
        System.ComponentModel.CancelEventHandler cancel = (_, eventArgs) => eventArgs.Cancel = true;
        window.Closing += cancel;
        try
        {
            // Act
            window.Close();

            // Assert
            Assert.IsTrue(window.IsVisible);
            Assert.AreEqual(saved, System.IO.File.ReadAllText(path));
        }
        finally
        {
            window.Closing -= cancel;
            window.Close();
        }
    });

    [TestMethod]
    public async Task Show_LargeCatalog_VirtualizesRowsAndRetainsTabState() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(3000);
        fixture.Add(CatalogKind.Game, "Game");
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            // Act
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var tabs = (TabControl)window.FindName("CatalogTabs");
            var movies = (CatalogView)((TabItem)tabs.Items[0]).Content;
            var grid = (ListBox)movies.FindName("PosterRows");
            var realized = Descendants<ListBoxItem>(grid).Count();
            movies.Model.Jump("Z");
            grid.ScrollIntoView(movies.Model.Rows.Last());
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var offset = CatalogView.FindChild<ScrollViewer>(grid)!.VerticalOffset;
            var selection = movies.Model.Selected;
            window.ActivateCatalog("games");
            window.ActivateCatalog("movies");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            // Assert
            Assert.IsTrue(realized > 0 && realized < 25, $"Realized rows: {realized}");
            Assert.AreSame(selection, movies.Model.Selected);
            Assert.AreEqual(offset, CatalogView.FindChild<ScrollViewer>(grid)!.VerticalOffset, 1);
            Assert.HasCount(4, tabs.Items);
            Assert.HasCount(3000, movies.Model.Entries);
        }
        finally
        {
            window.Close();
        }
    });
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    public async Task ShowDetails_AllCollections_LoadsNativeWpfLayout(CatalogKind kind) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Synthetic entry");
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            // Act
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            // Assert
            var model = (EntryEditorModel)window.DataContext;
            Assert.AreEqual(entry.Id, model.StoredId);
            Assert.AreEqual("Synthetic entry", ((TextBox)window.FindName("TitleText")).Text);
            Assert.IsTrue(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
    });
    private static IEnumerable<T> Descendants<T>(DependencyObject parent)
        where T : DependencyObject
    {
        foreach (var index in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(parent)))
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in Descendants<T>(child))
            {
                yield return descendant;
            }
        }
    }
}