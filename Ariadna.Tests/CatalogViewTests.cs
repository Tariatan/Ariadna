using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public sealed class CatalogViewTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie, "movies")]
    [DataRow(CatalogKind.Game, "games")]
    [DataRow(CatalogKind.Documentary, "documentaries")]
    [DataRow(CatalogKind.Library, "library")]
    public async Task Remove_ConfirmedDeletion_SelectsPreviousThenNextOrEmpty(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var title in new[] { "A entry", "B entry", "C entry", "D entry" })
        {
            fixture.Add(kind, title);
        }

        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var originalIds = view.Model.Entries.Select(item => item.Entry.Id).ToArray();
            (int Index, int? ExpectedId)[] removals = [(2, originalIds[1]), (2, originalIds[1]), (0, originalIds[1]), (0, null)];
            foreach (var (index, expectedId) in removals)
            {
                view.Model.Selected = view.Model.Entries[index];
                var removedId = view.Model.Selected.Entry.Id;
                // Act: confirmation is a user interaction; exercise the confirmed action directly.
                await view.RemoveConfirmedAsync(false);
                while (view.Model.Busy)
                {
                    await Task.Delay(10);
                }

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

                // Assert
                Assert.IsFalse(view.Model.Entries.Any(item => item.Entry.Id == removedId));
                Assert.AreEqual(expectedId, view.Model.Selected?.Entry.Id);
                if (expectedId != null)
                {
                    Assert.IsNotNull(view.Model.Selected);
                }
            }
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
    public async Task Reload_SavedEntry_SelectsEntryInPosterRows(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var index in Enumerable.Range(0, 40))
        {
            fixture.Add(kind, $"A Entry {index:D2}");
        }

        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            view.Model.Selected = view.Model.Entries[0];
            scroll.ScrollToTop();
            var added = fixture.Add(kind, "Z New entry");

            // Act
            await view.Reload(added.Id);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.AreEqual(added.Id, view.Model.Selected!.Entry.Id);
            Assert.IsTrue(view.Model.Rows.Any(row => row.Items.Contains(view.Model.Selected)));
            Assert.IsTrue(view.Model.Selected.Selected);
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
    public async Task ScrollBar_ThumbDragged_UpdatesContentDuringDragAndKeepsRowsAligned(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var index in Enumerable.Range(0, 120))
        {
            fixture.Add(kind, $"Entry {index:D3}");
        }

        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var bar = CatalogView.FindChild<ScrollBar>(grid)!;
            var thumb = bar.Track.Thumb;
            var selected = view.Model.Selected;
            scroll.ScrollToTop();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Act
            thumb.RaiseEvent(new DragStartedEventArgs(0, 0) { RoutedEvent = Thumb.DragStartedEvent });
            thumb.RaiseEvent(new DragDeltaEventArgs(0, 80) { RoutedEvent = Thumb.DragDeltaEvent });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var firstValue = bar.Value;
            var contentWhileDragging = scroll.ContentVerticalOffset;
            thumb.RaiseEvent(new DragDeltaEventArgs(0, 1) { RoutedEvent = Thumb.DragDeltaEvent });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var secondValue = bar.Value;
            thumb.RaiseEvent(new DragCompletedEventArgs(0, 81, false) { RoutedEvent = Thumb.DragCompletedEvent });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.AreEqual(firstValue, contentWhileDragging, 0.01, "Posters must scroll before the mouse is released.");
            Assert.IsTrue(firstValue > 0);
            Assert.IsTrue(secondValue > firstValue && secondValue - firstValue < 1, $"A small mouse movement must advance the thumb continuously: {firstValue} -> {secondValue}.");
            Assert.IsTrue(scroll.ContentVerticalOffset > 0);
            Assert.AreEqual(Math.Floor(secondValue), Math.Floor(scroll.VerticalOffset), 0.01);
            Assert.IsTrue(thumb.ActualHeight >= 50, "The visible thumb must be at least 50 DIPs high.");
            Assert.IsTrue(BindingOperations.IsDataBound(bar, ScrollBar.ValueProperty), "Release must restore normal scrollbar synchronization.");
            AssertFirstRowStartsAtTop(grid);
            Assert.AreSame(selected, view.Model.Selected);
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
    public async Task Layout_InitialSelectionOutsideViewport_RetainsSelectedPosterRow(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var index in Enumerable.Range(0, 60))
        {
            fixture.Add(kind, $"Entry {index:D2}");
        }

        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var selected = view.Model.Entries[40];
            view.Model.Selected = selected;

            // Act
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var grid = (ListBox)view.FindName("PosterRows");

            // Assert
            AssertFirstRowStartsAtTop(grid);
            Assert.AreSame(selected, view.Model.Selected);
            var selectedRow = view.Model.Rows.Single(row => row.Items.Contains(selected));
            Assert.IsTrue(selectedRow.Items.Contains(selected));
            Assert.IsTrue(grid.Items.Contains(selectedRow));
        }
        finally
        {
            window.Close();
        }
    });

    private static void AssertFirstRowStartsAtTop(ListBox grid)
    {
        var viewport = CatalogView.FindChild<ScrollContentPresenter>(grid)!;
        var first = Enumerable.Range(0, grid.Items.Count)
            .Select(index => grid.ItemContainerGenerator.ContainerFromIndex(index))
            .OfType<ListBoxItem>()
            .Select(row => new { Top = row.TranslatePoint(new Point(), viewport).Y, row.ActualHeight })
            .Where(row => row.Top + row.ActualHeight > 0.01 && row.Top < viewport.ActualHeight)
            .Min(row => row.Top);
        Assert.AreEqual(0, first, 0.01, "The first visible row must start at the viewport top.");
    }

    [TestMethod]
    [DataRow(700, 500)]
    [DataRow(1120, 780)]
    [DataRow(1600, 1000)]
    public async Task OnGridKeyDown_EndRequested_SelectsLastPosterInFinalRow(int width, int height) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(120);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, "movies")
        {
            Width = width,
            Height = height,
        };
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");

            // Act
            grid.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, HeadlessLayout.Source, 0, Key.End)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            AssertFirstRowStartsAtTop(grid);
            Assert.AreSame(view.Model.Entries.Last(), view.Model.Selected);
            Assert.IsTrue(view.Model.Rows.Last().Items.Contains(view.Model.Selected));
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
    public async Task QuickJump_EntryOutsideViewport_RetainsSelectionAcrossTabs(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var index in Enumerable.Range(0, 60))
        {
            var entry = fixture.Add(kind, $"{(index < 30 ? "A" : "Z")} Entry {index:D2}");
            entry.OriginalTitle = entry.Title;
            fixture.Store.Save(kind, new CatalogDetails(entry, ["Genre"], [], []));
        }

        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var quickList = Descendants<ItemsControl>(view).Single(control => ReferenceEquals(control.ItemsSource, view.Model.Letters));
            var letter = quickList.ItemContainerGenerator.ContainerFromItem("Z");

            // Act
            CatalogView.FindChild<Button>(letter)!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var offset = scroll.VerticalOffset;
            HeadlessLayout.SelectCatalog(window, argument == "movies" ? "games" : "movies");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            HeadlessLayout.SelectCatalog(window, argument);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            AssertFirstRowStartsAtTop(grid);
            Assert.AreEqual("Z Entry 30", view.Model.Selected!.Entry.Title);
            Assert.AreEqual(offset, scroll.VerticalOffset, 0.01);
        }
        finally
        {
            window.Close();
        }
    });

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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var subgenre = Filter(view, "Subgenre");
            var initiallyVisible = HeadlessLayout.IsVisible(subgenre);

            // Act
            view.Model.Genre = "Programming";
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            CollectionAssert.AreEqual(expectedFlags.Split('|'), Descendants<CheckBox>(view).Where(control => HeadlessLayout.IsVisible(control)).Select(control => (string)control.Content).ToArray());
            Assert.AreEqual(kind is CatalogKind.Movie or CatalogKind.Library, HeadlessLayout.IsVisible((TextBox)view.FindName("PersonSearch")));
            Assert.AreEqual(kind == CatalogKind.Movie, HeadlessLayout.IsVisible((TextBox)view.FindName("ActorSearch")));
            Assert.IsTrue(HeadlessLayout.IsVisible(Filter(view, "Genre")));
            Assert.IsFalse(initiallyVisible);
            Assert.AreEqual(kind == CatalogKind.Library, HeadlessLayout.IsVisible(subgenre));
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            view.Model.Title = "Entry";
            view.Model.Wish = true;
            view.Model.Genre = genre;
            view.Model.Subgenre = selectedSubgenre;
            await view.Model.RefreshAsync(CancellationToken.None);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var subgenre = Filter(view, "Subgenre");
            var visibleBeforeClear = HeadlessLayout.IsVisible(subgenre);

            // Act
            HeadlessLayout.SelectCatalog(window, "movies");
            HeadlessLayout.SelectCatalog(window, "library");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var visibleAfterReturn = HeadlessLayout.IsVisible(subgenre);
            var retainedSubgenre = view.Model.Subgenre;
            Descendants<Button>(view).Single(button => (string?)button.Tag == "Genre").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await view.Model.RefreshAsync(CancellationToken.None);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.IsTrue(visibleBeforeClear);
            Assert.IsTrue(visibleAfterReturn);
            Assert.AreEqual(selectedSubgenre, retainedSubgenre);
            Assert.IsFalse(HeadlessLayout.IsVisible(subgenre));
            Assert.IsFalse(HeadlessLayout.IsVisible(Descendants<Label>(view).Single(label => (string?)label.Content == "Subgenre")));
            Assert.IsFalse(HeadlessLayout.IsVisible(Descendants<Button>(view).Single(button => (string?)button.Tag == "Subgenre")));
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
    public async Task OnGridMouseWheel_FractionalRowRequested_KeepsWholeRowStepsWithoutChangingSelection(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var selected = view.Model.Selected;
            scroll.ScrollToVerticalOffset(2.5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            AssertFirstRowStartsAtTop(grid);

            // Act
            var down = Wheel(CatalogView.FindChild<PosterCard>(grid)!, -120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var downOffset = scroll.VerticalOffset;
            scroll.ScrollToVerticalOffset(2.5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var up = Wheel(CatalogView.FindChild<PosterCard>(grid)!, 120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.IsTrue(down.Handled);
            Assert.IsTrue(up.Handled);
            Assert.AreEqual(3, downOffset, 0.01);
            Assert.AreEqual(1, scroll.VerticalOffset, 0.01);
            AssertFirstRowStartsAtTop(grid);
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            scroll.ScrollToTop();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Act
            var partial = Wheel(grid, -60);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var partialOffset = scroll.VerticalOffset;
            Wheel(grid, -60);
            Wheel(grid, -120);
            Wheel(grid, -240);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var burstOffset = scroll.VerticalOffset;
            Wheel(grid, 12000);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var topOffset = scroll.VerticalOffset;
            Wheel(grid, -12000);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var bottomOffset = scroll.VerticalOffset;
            Wheel(grid, -120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.IsTrue(partial.Handled);
            Assert.AreEqual(0, partialOffset, 0.01);
            Assert.AreEqual(4, burstOffset, 0.01);
            Assert.AreEqual(0, topOffset, 0.01);
            Assert.AreEqual(scroll.ScrollableHeight, bottomOffset, 0.01);
            Assert.AreEqual(bottomOffset, scroll.VerticalOffset, 0.01);
            AssertFirstRowStartsAtTop(grid);
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var rowHeight = CatalogView.FindChild<ListBoxItem>(grid)!.ActualHeight;
            var rowsPerPage = Math.Max(1, (int)(grid.ActualHeight / rowHeight));
            view.Model.Selected = view.Model.Entries[0];

            // Act
            grid.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, HeadlessLayout.Source, 0, Key.PageDown)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.AreSame(view.Model.Entries[view.Model.Columns * rowsPerPage], view.Model.Selected);
            AssertFirstRowStartsAtTop(grid);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task OnGridSizeChanged_ColumnCountChanges_PreservesFirstVisibleEntryAndWholeRowAlignment() => await WpfThread.RunAsync(async () =>
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var oldColumns = view.Model.Columns;
            var selected = view.Model.Selected;
            scroll.ScrollToVerticalOffset(5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Act
            window.Width += 300;
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.IsGreaterThan(oldColumns, view.Model.Columns);
            Assert.AreEqual(5 * oldColumns / view.Model.Columns, scroll.VerticalOffset, 0.01);
            AssertFirstRowStartsAtTop(grid);
            Assert.AreSame(selected, view.Model.Selected);
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
    public async Task OnGridKeyDown_EscapeAfterScrolling_PreservesViewportAndSelection(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
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
            HeadlessLayout.Arrange(window);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            view.Model.Selected = view.Model.Entries[0];
            scroll.ScrollToVerticalOffset(5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);
            var offset = scroll.VerticalOffset;
            var selected = view.Model.Selected;
            var escape = new KeyEventArgs(Keyboard.PrimaryDevice, HeadlessLayout.Source, 0, Key.Escape)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            };

            // Act
            grid.RaiseEvent(escape);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            HeadlessLayout.Arrange(window);

            // Assert
            Assert.IsTrue(escape.Handled);
            Assert.AreEqual(offset, scroll.VerticalOffset, 0.01);
            Assert.AreSame(selected, view.Model.Selected);
        }
        finally
        {
            window.Close();
        }
    });
    private static TextBox Filter(CatalogView view, string property) => Descendants<TextBox>(view)
        .Single(control => BindingOperations.GetBinding(control, TextBox.TextProperty)?.Path.Path == property);

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
