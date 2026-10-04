using System.Windows;
using System.Windows.Controls;
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
