using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class CatalogViewTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie, "movies", "Wishlist|Recent|New|Series|Movies")]
    [DataRow(CatalogKind.Game, "games", "Wishlist|Recent|New|VR|Non-VR")]
    [DataRow(CatalogKind.Library, "library", "Wishlist|Recent|New")]
    [DataRow(CatalogKind.Documentary, "documentaries", "Wishlist|Recent|New")]
    public async Task Show_CollectionFilters_OnlyShowsApplicableControls(CatalogKind kind, string argument, string expectedFlags) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.Add(kind, "Entry");
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var subgenre = Filter(view, "Subgenre");
            var initiallyVisible = subgenre.IsVisible;

            // Act
            view.Model.Genre = "Programming";
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            CollectionAssert.AreEqual(expectedFlags.Split('|'), Descendants<CheckBox>(view).Where(control => control.IsVisible).Select(control => (string)control.Content).ToArray());
            Assert.AreEqual(kind is CatalogKind.Movie or CatalogKind.Library, ((ComboBox)view.FindName("PersonSearch")).IsVisible);
            Assert.AreEqual(kind == CatalogKind.Movie, ((ComboBox)view.FindName("ActorSearch")).IsVisible);
            Assert.IsTrue(Filter(view, "Genre").IsVisible);
            Assert.IsFalse(initiallyVisible);
            Assert.AreEqual(kind == CatalogKind.Library, subgenre.IsVisible);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow("Programming", "C#")]
    [DataRow("Literature", "Fantasy")]
    [DataRow("Misc", "Misc")]
    [DataRow("Custom genre", "Misc")]
    public async Task ClearFilter_LibraryGenreCleared_HidesAndResetsSubgenreWhilePreservingOtherFilters(string genre, string selectedSubgenre) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Library, "Entry", true);
        fixture.Store.Save(CatalogKind.Library, new CatalogDetails(entry, [genre, selectedSubgenre], [], []));
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, "library");
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            view.Model.Title = "Entry";
            view.Model.Wish = true;
            view.Model.Genre = genre;
            view.Model.Subgenre = selectedSubgenre;
            await view.Model.RefreshAsync(CancellationToken.None);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var subgenre = Filter(view, "Subgenre");
            var visibleBeforeClear = subgenre.IsVisible;

            // Act
            window.ActivateCatalog("movies");
            window.ActivateCatalog("library");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var visibleAfterReturn = subgenre.IsVisible;
            var retainedSubgenre = view.Model.Subgenre;
            Descendants<Button>(view).Single(button => (string?)button.Tag == "Genre").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await view.Model.RefreshAsync(CancellationToken.None);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsTrue(visibleBeforeClear);
            Assert.IsTrue(visibleAfterReturn);
            Assert.AreEqual(selectedSubgenre, retainedSubgenre);
            Assert.IsFalse(subgenre.IsVisible);
            Assert.IsFalse(Descendants<Label>(view).Single(label => (string?)label.Content == "Subgenre").IsVisible);
            Assert.IsFalse(Descendants<Button>(view).Single(button => (string?)button.Tag == "Subgenre").IsVisible);
            Assert.AreEqual(string.Empty, view.Model.Genre);
            Assert.AreEqual(string.Empty, view.Model.Subgenre);
            Assert.AreEqual("Entry", view.Model.Title);
            Assert.IsTrue(view.Model.Wish);
            Assert.HasCount(1, view.Model.Entries);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie, "movies")]
    [DataRow(CatalogKind.Game, "games")]
    [DataRow(CatalogKind.Library, "library")]
    [DataRow(CatalogKind.Documentary, "documentaries")]
    public async Task OnGridMouseWheel_PartialRowVisible_AlignsToAdjacentRowWithoutChangingSelection(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var index in Enumerable.Range(0, 40))
        {
            fixture.Add(kind, $"Entry {index:D2}");
        }

        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var rowHeight = CatalogView.FindChild<ListBoxItem>(grid)!.ActualHeight;
            var selected = view.Model.Selected;
            scroll.ScrollToVerticalOffset(rowHeight / 2);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            var down = Wheel(CatalogView.FindChild<PosterCard>(grid)!, -120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var downOffset = scroll.VerticalOffset;
            scroll.ScrollToVerticalOffset(rowHeight / 2);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var up = Wheel(CatalogView.FindChild<PosterCard>(grid)!, 120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsTrue(down.Handled);
            Assert.IsTrue(up.Handled);
            Assert.AreEqual(rowHeight, downOffset, 0.01);
            Assert.AreEqual(0, scroll.VerticalOffset, 0.01);
            Assert.AreSame(selected, view.Model.Selected);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task OnGridMouseWheel_SmallAndRapidDeltas_AccumulatesWholeRowsAndClampsAtEnds() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(120);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, "movies");
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var rowHeight = CatalogView.FindChild<ListBoxItem>(grid)!.ActualHeight;
            scroll.ScrollToTop();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            var partial = Wheel(grid, -60);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var partialOffset = scroll.VerticalOffset;
            Wheel(grid, -60);
            Wheel(grid, -120);
            Wheel(grid, -240);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var burstOffset = scroll.VerticalOffset;
            Wheel(grid, 12000);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var topOffset = scroll.VerticalOffset;
            Wheel(grid, -12000);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var bottomOffset = scroll.VerticalOffset;
            Wheel(grid, -120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsTrue(partial.Handled);
            Assert.AreEqual(0, partialOffset, 0.01);
            Assert.AreEqual(rowHeight * 4, burstOffset, 0.01);
            Assert.AreEqual(0, topOffset, 0.01);
            Assert.AreEqual(scroll.ScrollableHeight, bottomOffset, 0.01);
            Assert.AreEqual(bottomOffset, scroll.VerticalOffset, 0.01);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task OnGridKeyDown_PageDown_UsesRenderedRowHeight() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(120);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, "movies")
        {
            Height = 1000,
        };
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var rowHeight = CatalogView.FindChild<ListBoxItem>(grid)!.ActualHeight;
            var rowsPerPage = Math.Max(1, (int)(grid.ActualHeight / rowHeight));
            view.Model.Selected = view.Model.Entries[0];

            // Act
            grid.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(grid), 0, Key.PageDown)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });

            // Assert
            Assert.AreSame(view.Model.Entries[view.Model.Columns * rowsPerPage], view.Model.Selected);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task OnGridSizeChanged_ColumnCountChanges_PreservesFirstVisibleEntryAndPartialRowOffset() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(120);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, "movies")
        {
            Width = 900,
        };
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var rowHeight = CatalogView.FindChild<ListBoxItem>(grid)!.ActualHeight;
            var oldColumns = view.Model.Columns;
            var selected = view.Model.Selected;
            var partialOffset = rowHeight / 2;
            scroll.ScrollToVerticalOffset(5 * rowHeight + partialOffset);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            window.Width += 300;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsGreaterThan(oldColumns, view.Model.Columns);
            Assert.AreEqual(5 * oldColumns / view.Model.Columns * rowHeight + partialOffset, scroll.VerticalOffset, 0.01);
            Assert.AreSame(selected, view.Model.Selected);
        }
        finally
        {
            window.Close();
        }
    });

    private static ComboBox Filter(CatalogView view, string property) => Descendants<ComboBox>(view)
        .Single(control => BindingOperations.GetBinding(control, ComboBox.TextProperty)?.Path.Path == property);

    private static IEnumerable<T> Descendants<T>(DependencyObject parent)
        where T : DependencyObject
    {
        foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
        {
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

    private static MouseWheelEventArgs Wheel(UIElement target, int delta)
    {
        var eventArgs = new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta)
        {
            RoutedEvent = Mouse.PreviewMouseWheelEvent,
        };
        target.RaiseEvent(eventArgs);
        return eventArgs;
    }
}
