using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;
[TestClass]
public sealed class MainWindowTests
{

    [TestMethod]
    [DataRow(700)]
    [DataRow(1120)]
    [DataRow(1600)]
    public async Task Layout_WindowWidths_DisplaysAssemblyVersionBesideAllTabs(int width) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        string[] arguments = ["games", "library", "documentaries", "xxx", "movies"];
        using var fixture = new CatalogFixture();
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null)
        {
            Width = width,
        };
        try
        {
            // Act
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var tabs = (TabControl)window.FindName("CatalogTabs");
            var label = (TextBlock)tabs.Template.FindName("AppVersionLabel", tabs);
            var binding = BindingOperations.GetBinding(label, TextBlock.TextProperty)!;
            Assert.AreEqual("ApplicationVersion", binding.Path.Path);
            Assert.AreEqual(typeof(Window), binding.RelativeSource.AncestorType);
            // The detached tree has no Window ancestor. Evaluate the same property explicitly.
            label.SetBinding(TextBlock.TextProperty, new Binding("ApplicationVersion") { Source = window });
            HeadlessLayout.Arrange(window);
            var labelPosition = label.TranslatePoint(new Point(), tabs);

            // Assert
            Assert.AreEqual(typeof(MainWindow).Assembly.GetName().Version!.ToString(3), label.Text);
            Assert.AreEqual(Colors.Black, ((SolidColorBrush)label.Foreground).Color);
            Assert.AreEqual(0.5, label.Opacity);
            Assert.IsFalse(label.Focusable);
            Assert.IsFalse(label.IsHitTestVisible);
            Assert.AreEqual(tabs.ActualWidth - 12, labelPosition.X + label.ActualWidth, 1);
            Assert.HasCount(5, tabs.Items);
            foreach (TabItem tab in tabs.Items)
            {
                Assert.IsTrue(tab.TranslatePoint(new Point(tab.ActualWidth, 0), tabs).X <= labelPosition.X);
                Assert.IsTrue(labelPosition.Y >= 0 && labelPosition.Y + label.ActualHeight <= tab.ActualHeight);
            }

            foreach (var argument in arguments)
            {
                HeadlessLayout.SelectCatalog(window, argument);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
                Assert.IsTrue(HeadlessLayout.IsVisible(label));
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
    public async Task Layout_LargeCatalog_VirtualizesRowsAndRetainsTabState() => await WpfThread.RunAsync(async () =>
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var tabs = (TabControl)window.FindName("CatalogTabs");
            var movies = (CatalogView)((TabItem)tabs.Items[0]).Content;
            var grid = (ListBox)movies.FindName("PosterRows");
            var realized = Descendants<ListBoxItem>(grid).Count();
            movies.Model.Jump("Z");
            grid.ScrollIntoView(movies.Model.Rows.Last());
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var offset = CatalogView.FindChild<ScrollViewer>(grid)!.VerticalOffset;
            var selection = movies.Model.Selected;
            HeadlessLayout.SelectCatalog(window, "games");
            HeadlessLayout.SelectCatalog(window, "movies");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            // Assert
            Assert.IsTrue(realized > 0 && realized < 25, $"Realized rows: {realized}");
            Assert.AreSame(selection, movies.Model.Selected);
            Assert.AreEqual(offset, CatalogView.FindChild<ScrollViewer>(grid)!.VerticalOffset, 1);
            Assert.HasCount(5, tabs.Items);
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
    public async Task LayoutDetails_AllCollections_LoadsFieldsWithoutShowing(CatalogKind kind) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Synthetic entry");
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            // Act
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            // Assert
            var model = (EntryEditorModel)window.DataContext;
            Assert.AreEqual(entry.Id, model.StoredId);
            Assert.AreEqual(kind == CatalogKind.Movie ? "Original Synthetic entry" : "Synthetic entry", ((TextBox)window.FindName("TitleText")).Text);
            Assert.IsFalse(window.IsVisible);
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
