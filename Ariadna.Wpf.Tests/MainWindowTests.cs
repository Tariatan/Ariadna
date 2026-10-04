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
