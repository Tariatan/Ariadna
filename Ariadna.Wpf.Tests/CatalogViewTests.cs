using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    [DataRow(CatalogKind.Movie, "movies", false)]
    [DataRow(CatalogKind.Movie, "movies", true)]
    [DataRow(CatalogKind.Library, "library", false)]
    public async Task PeopleSearch_TypedName_ShowsPortraitResultsAndConfirmsWithEnter(CatalogKind kind, string argument, bool actor) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Matching entry");
        fixture.Store.Save(kind, new CatalogDetails(entry, [], actor ? [] : [new PersonPhoto("\u0420\u043e\u043b\u0430\u043d\u0434 Test", null)], actor ? [new PersonPhoto("\u0420\u043e\u043b\u0430\u043d\u0434 Test", null)] : []));
        fixture.Add(kind, "Other entry");
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            await window.PreloadAsync(CancellationToken.None);
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var field = (TextBox)view.FindName(actor ? "ActorSearch" : "PersonSearch");
            var popup = (Popup)view.FindName("PeoplePopup");
            var suggestions = (ListBox)view.FindName("PeopleSuggestions");

            // Act
            field.Focus();
            field.Text = "\u0440\u043e\u043b";
            await Task.Delay(350);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsTrue(popup.IsOpen);
            Assert.HasCount(1, suggestions.Items);
            Assert.AreEqual("\u0420\u043e\u043b\u0430\u043d\u0434 Test", ((PersonEditorModel)suggestions.Items[0]).Name);
            Assert.HasCount(2, view.Model.Entries);

            // Act
            suggestions.SelectedIndex = 0;
            suggestions.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(suggestions), 0, Key.Enter)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
            await view.Model.RefreshAsync(CancellationToken.None);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsFalse(popup.IsOpen);
            Assert.AreEqual("\u0420\u043e\u043b\u0430\u043d\u0434 Test", field.Text);
            Assert.HasCount(1, view.Model.Entries);
            Assert.AreEqual(entry.Id, view.Model.Entries[0].Entry.Id);

            // Act
            field.Text = "\u0440\u043e\u043b";
            await Task.Delay(350);
            field.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(field), 0, Key.Escape)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });

            // Assert
            Assert.IsFalse(popup.IsOpen);
            Assert.AreEqual("\u0440\u043e\u043b", field.Text);

            // Act
            Descendants<Button>(view).Single(button => (string?)button.Tag == (actor ? "Actor" : "Person")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await view.Model.RefreshAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(string.Empty, field.Text);
            Assert.HasCount(2, view.Model.Entries);
        }
        finally
        {
            window.Close();
            await Task.Delay(300);
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie, "movies")]
    [DataRow(CatalogKind.Game, "games")]
    [DataRow(CatalogKind.Library, "library")]
    [DataRow(CatalogKind.Documentary, "documentaries")]
    public async Task Reload_SavedEntryOutsideViewport_SelectsRevealsAndFocusesEntry(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            view.Model.Selected = view.Model.Entries[0];
            scroll.ScrollToTop();
            ((Button)view.FindName("AddEntryButton")).Focus();
            var added = fixture.Add(kind, "Z New entry");

            // Act
            await view.Reload(added.Id);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.AreEqual(added.Id, view.Model.Selected!.Entry.Id);
            Assert.AreSame(grid, Keyboard.FocusedElement);
            Assert.IsTrue(scroll.VerticalOffset > 0);
            var selectedRow = view.Model.Rows.Single(row => row.Items.Contains(view.Model.Selected));
            var container = (ListBoxItem)grid.ItemContainerGenerator.ContainerFromItem(selectedRow);
            Assert.IsNotNull(container);
            var top = container.TranslatePoint(new Point(), grid).Y;
            Assert.IsTrue(top >= 0 && top < grid.ActualHeight);
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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var bar = CatalogView.FindChild<ScrollBar>(grid)!;
            var thumb = bar.Track.Thumb;
            var selected = view.Model.Selected;
            scroll.ScrollToTop();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            thumb.RaiseEvent(new DragStartedEventArgs(0, 0) { RoutedEvent = Thumb.DragStartedEvent });
            thumb.RaiseEvent(new DragDeltaEventArgs(0, 80) { RoutedEvent = Thumb.DragDeltaEvent });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var firstValue = bar.Value;
            var contentWhileDragging = scroll.ContentVerticalOffset;
            thumb.RaiseEvent(new DragDeltaEventArgs(0, 1) { RoutedEvent = Thumb.DragDeltaEvent });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var secondValue = bar.Value;
            thumb.RaiseEvent(new DragCompletedEventArgs(0, 81, false) { RoutedEvent = Thumb.DragCompletedEvent });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

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
    [DataRow(CatalogKind.Movie, false)]
    [DataRow(CatalogKind.Game, false)]
    [DataRow(CatalogKind.Library, false)]
    [DataRow(CatalogKind.Documentary, false)]
    [DataRow(CatalogKind.Movie, true)]
    [DataRow(CatalogKind.Game, true)]
    [DataRow(CatalogKind.Library, true)]
    [DataRow(CatalogKind.Documentary, true)]
    public async Task AddEntry_PlusWhileBrowsing_DiscoversEntryInActiveCatalog(CatalogKind kind, bool numpad) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var root = fixture.Configuration.DiscoveryRoot(kind);
        var path = System.IO.Path.Combine(root, "Synthetic entry");
        if (kind == CatalogKind.Game)
        {
            System.IO.Directory.CreateDirectory(path);
        }
        else
        {
            if (kind != CatalogKind.Movie)
            {
                root = System.IO.Directory.CreateDirectory(System.IO.Path.Combine(root, "GROUP")).FullName;
            }

            path = System.IO.Path.Combine(root, "Synthetic entry.mkv");
            System.IO.File.WriteAllText(path, "Synthetic media");
        }

        var opened = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thumbnails = new ThumbnailCache(NullLogger.Instance);
        using var view = new CatalogView(new CatalogViewModel(kind, fixture.Store, fixture.Configuration, thumbnails, NullLogger.Instance), fixture.Actions,
            (_, entryPath) => opened.TrySetResult(entryPath));
        var window = new Window { Content = view };
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var grid = (ListBox)view.FindName("PosterRows");
            var button = (Button)view.FindName("AddEntryButton");
            InputEventArgs input = numpad ? new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(grid), 0, Key.Add)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            } : new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, grid, "+"))
            {
                RoutedEvent = TextCompositionManager.PreviewTextInputEvent,
            };

            // Act
            grid.RaiseEvent(input);
            var discovered = await opened.Task.WaitAsync(TimeSpan.FromSeconds(10));

            // Assert
            Assert.IsTrue(input.Handled);
            Assert.AreEqual(path, discovered);
            Assert.IsTrue(button.IsEnabled);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow("TitleSearch", true, "+", false)]
    [DataRow("GenreSearch", true, "+", false)]
    [DataRow("PosterRows", false, "+", false)]
    [DataRow("PosterRows", true, "=", false)]
    [DataRow("TitleSearch", true, "+", true)]
    [DataRow("GenreSearch", true, "+", true)]
    [DataRow("PosterRows", false, "+", true)]
    public async Task AddEntry_EditingOrUnavailableShortcut_DoesNotInvokeAddEntry(string controlName, bool enabled, string text, bool numpad) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var thumbnails = new ThumbnailCache(NullLogger.Instance);
        using var view = new CatalogView(new CatalogViewModel(CatalogKind.Movie, fixture.Store, fixture.Configuration, thumbnails, NullLogger.Instance), fixture.Actions,
            (_, _) => Assert.Fail("The shortcut must not open an editor."));
        var window = new Window { Content = view };
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var button = (Button)view.FindName("AddEntryButton");
            button.IsEnabled = enabled;
            var control = (UIElement)view.FindName(controlName);
            if (control is ComboBox combo)
            {
                control = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
            }

            InputEventArgs input = numpad ? new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(control), 0, Key.Add)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            } : new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, control, text))
            {
                RoutedEvent = TextCompositionManager.PreviewTextInputEvent,
            };

            // Act
            control.RaiseEvent(input);

            // Assert
            Assert.IsFalse(input.Handled);
            Assert.AreEqual(enabled, button.IsEnabled);
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
    public async Task Show_InitialSelectionOutsideViewport_StartsFirstVisibleRowAtTop(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var grid = (ListBox)view.FindName("PosterRows");

            // Assert
            AssertFirstRowStartsAtTop(grid);
            Assert.AreSame(selected, view.Model.Selected);
            var selectedRow = (ListBoxItem)grid.ItemContainerGenerator.ContainerFromItem(view.Model.Rows.Single(row => row.Items.Contains(selected)));
            var viewport = CatalogView.FindChild<ScrollContentPresenter>(grid)!;
            Assert.IsTrue(selectedRow.TranslatePoint(new Point(), viewport).Y >= -0.01);
            Assert.IsTrue(selectedRow.TranslatePoint(new Point(0, selectedRow.ActualHeight), viewport).Y <= viewport.ActualHeight + 0.01);
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
    public async Task OnGridKeyDown_EndRequested_KeepsTopRowWholeAndLastEntryReachable(int width, int height) => await WpfThread.RunAsync(async () =>
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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");

            // Act
            grid.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(grid), 0, Key.End)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            AssertFirstRowStartsAtTop(grid);
            Assert.AreSame(view.Model.Entries.Last(), view.Model.Selected);
            var lastRow = (ListBoxItem)grid.ItemContainerGenerator.ContainerFromIndex(grid.Items.Count - 1);
            var viewport = CatalogView.FindChild<ScrollContentPresenter>(grid)!;
            var lastTop = lastRow.TranslatePoint(new Point(), viewport).Y;
            Assert.IsTrue(lastTop >= -0.01 && lastTop < viewport.ActualHeight, "The final row must remain reachable.");
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
    public async Task QuickJump_EntryOutsideViewport_KeepsRowsAlignedAndRetainsPositionAcrossTabs(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        foreach (var index in Enumerable.Range(0, 60))
        {
            fixture.Add(kind, $"{(index < 30 ? "A" : "Z")} Entry {index:D2}");
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
            var quickList = Descendants<ItemsControl>(view).Single(control => ReferenceEquals(control.ItemsSource, view.Model.Letters));
            var letter = quickList.ItemContainerGenerator.ContainerFromItem("Z");

            // Act
            CatalogView.FindChild<Button>(letter)!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var offset = scroll.VerticalOffset;
            window.ActivateCatalog(argument == "movies" ? "games" : "movies");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            window.ActivateCatalog(argument);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            AssertFirstRowStartsAtTop(grid);
            Assert.AreEqual("Z Entry 30", view.Model.Selected!.Entry.Title);
            Assert.IsTrue(offset > 0);
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
            Assert.AreEqual(kind is CatalogKind.Movie or CatalogKind.Library, ((TextBox)view.FindName("PersonSearch")).IsVisible);
            Assert.AreEqual(kind == CatalogKind.Movie, ((TextBox)view.FindName("ActorSearch")).IsVisible);
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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var selected = view.Model.Selected;
            scroll.ScrollToVerticalOffset(2.5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            AssertFirstRowStartsAtTop(grid);

            // Act
            var down = Wheel(CatalogView.FindChild<PosterCard>(grid)!, -120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var downOffset = scroll.VerticalOffset;
            scroll.ScrollToVerticalOffset(2.5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var up = Wheel(CatalogView.FindChild<PosterCard>(grid)!, 120);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
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
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

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
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            var grid = (ListBox)view.FindName("PosterRows");
            var scroll = CatalogView.FindChild<ScrollViewer>(grid)!;
            var oldColumns = view.Model.Columns;
            var selected = view.Model.Selected;
            scroll.ScrollToVerticalOffset(5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Act
            window.Width += 300;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

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
    public async Task OnGridKeyDown_EscapeAfterScrolling_PreservesViewportSelectionAndFocus(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
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
            view.Model.Selected = view.Model.Entries[0];
            grid.Focus();
            scroll.ScrollToVerticalOffset(5);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var offset = scroll.VerticalOffset;
            var selected = view.Model.Selected;
            var focus = Keyboard.FocusedElement;
            var escape = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(grid), 0, Key.Escape)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            };

            // Act
            grid.RaiseEvent(escape);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsTrue(offset > 0);
            Assert.IsTrue(escape.Handled);
            Assert.AreEqual(offset, scroll.VerticalOffset, 0.01);
            Assert.AreSame(selected, view.Model.Selected);
            Assert.AreSame(focus, Keyboard.FocusedElement);
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
