using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class PeopleEditorViewTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Scroll_PortraitRows_KeepsWholeRowsVisible(bool actors) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Row scrolling");
        var people = Enumerable.Range(0, 30).Select(index => new PersonPhoto($"Person {index}", null)).ToArray();
        fixture.Store.Save(CatalogKind.Movie, new CatalogDetails(entry, [], actors ? [] : people, actors ? people : []));
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Movie, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (PeopleEditorView)window.FindName(actors ? "CastEditor" : "PeopleEditor");
            var list = (ListBox)view.FindName("People");
            var scroll = CatalogView.FindChild<ScrollViewer>(list)!;
            var first = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);

            // Act
            list.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120)
            {
                RoutedEvent = Mouse.PreviewMouseWheelEvent,
            });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.AreEqual(first.ActualHeight + first.Margin.Top + first.Margin.Bottom, scroll.ViewportHeight, 0.01);
            Assert.AreEqual(scroll.ViewportHeight, scroll.VerticalOffset, 0.01);

            // Act
            scroll.ScrollToVerticalOffset(scroll.ViewportHeight * 2.4);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.AreEqual(scroll.ViewportHeight * 2, scroll.VerticalOffset, 0.01);

            // Act
            list.SelectedIndex = list.Items.Count - 1;
            list.ScrollIntoView(list.SelectedItem);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.AreEqual(scroll.ScrollableHeight, scroll.VerticalOffset, 0.01);
            Assert.AreEqual(0, scroll.VerticalOffset % scroll.ViewportHeight, 0.01);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie, false)]
    [DataRow(CatalogKind.Movie, true)]
    [DataRow(CatalogKind.Library, false)]
    public async Task Rename_F2_CommitsNameAndPreservesPhoto(CatalogKind kind, bool actors) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Rename target");
        var photo = EntryEditorModelTests.Png(Colors.SteelBlue);
        var person = new PersonPhoto("Augustin Popa", photo);
        fixture.Store.Save(kind, new CatalogDetails(entry, [], actors ? [] : [person], actors ? [person] : []));
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (PeopleEditorView)window.FindName(actors ? "CastEditor" : "PeopleEditor");
            var list = (ListBox)view.FindName("People");
            list.SelectedIndex = 0;
            list.Focus();

            // Act
            list.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(list), 0, Key.F2)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
            var input = CatalogView.FindChild<TextBox>(list)!;
            list.UpdateLayout();
            var label = ((Grid)input.Parent).Children.OfType<TextBlock>().Single();
            Assert.AreEqual(Visibility.Visible, input.Visibility);
            Assert.AreEqual(label.ActualWidth, input.ActualWidth, 0.01);
            Assert.AreEqual(label.ActualHeight, input.ActualHeight, 0.01);
            Assert.AreEqual(label.FontSize, input.FontSize);
            Assert.AreEqual(label.TextAlignment, input.TextAlignment);
            Assert.AreEqual(label.TextWrapping, input.TextWrapping);
            Assert.AreEqual(new Thickness(0), input.Padding);
            Assert.AreEqual(Color.FromRgb(51, 153, 255), ((SolidColorBrush)input.BorderBrush).Color);
            var textViewport = CatalogView.FindChild<ScrollViewer>(input)!;
            Assert.AreEqual(input.ActualWidth + 4, textViewport.ViewportWidth, 0.01);
            Assert.AreEqual(0, textViewport.HorizontalOffset, 0.01);
            Assert.AreEqual(label.TranslatePoint(new Point(), list), input.TranslatePoint(new Point(), list));
            if (kind == CatalogKind.Library)
            {
                Assert.AreEqual(0, input.GetRectFromCharacterIndex(0).Left, 0.01);
                Assert.AreEqual(0, input.GetRectFromCharacterIndex(0).Top, 0.01);
            }
            Assert.AreEqual("Augustin Popa", input.SelectedText);
            input.Text = "After";
            var enter = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), 0, Key.Enter)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            };
            input.RaiseEvent(enter);
            if (!enter.Handled)
            {
                enter.RoutedEvent = Keyboard.KeyDownEvent;
                input.RaiseEvent(enter);
            }
            Assert.AreEqual(Visibility.Collapsed, input.Visibility, "Enter must finish editing through the window's preview route.");
            list.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(list), 0, Key.F2)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent,
            });
            input.Text = "Canceled";
            input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), 0, Key.Escape)
            {
                RoutedEvent = Keyboard.KeyDownEvent,
            });
            ((EntryEditorModel)window.DataContext).Save();
            var saved = fixture.Store.GetDetails(kind, entry.Id)!;

            // Assert
            var result = (actors ? saved.Actors : saved.Directors).Single();
            Assert.AreEqual("After", result.Name);
            CollectionAssert.AreEqual(photo, result.Photo!);
            Assert.AreEqual(Visibility.Collapsed, input.Visibility);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie, false)]
    [DataRow(CatalogKind.Movie, true)]
    [DataRow(CatalogKind.Library, false)]
    public async Task Add_RepeatedClicks_PersistsPlaceholdersAndExistingRelationships(CatalogKind kind, bool actors) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Synthetic editor");
        var photo = EntryEditorModelTests.Png(Colors.SteelBlue);
        var person = new PersonPhoto("Before", photo);
        fixture.Store.Save(kind, new CatalogDetails(entry, ["Genre"], actors ? [] : [person], actors ? [person] : []));
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (PeopleEditorView)window.FindName(actors ? "CastEditor" : "PeopleEditor");
            var list = (ListBox)view.FindName("People");
            var add = (Button)view.FindName("AddButton");
            // Act
            add.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            add.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ((EntryEditorModel)window.DataContext).Save();
            var saved = fixture.Store.GetDetails(kind, entry.Id)!;
            // Assert
            var people = actors ? saved.Actors : saved.Directors;
            Assert.AreEqual(3, list.Items.Count);
            Assert.AreEqual(2, people.Count);
            Assert.AreEqual(1, people.Count(item => item.Name == "New Entry"));
            CollectionAssert.AreEqual(photo, people.Single(item => item.Name == "Before").Photo!);
            CollectionAssert.AreEqual(new[] { "Genre" }, saved.Genres.ToArray());
            Assert.AreSame(list.Items[2], list.SelectedItem);
            Assert.AreEqual(Visibility.Collapsed, CatalogView.FindChild<TextBox>(view)!.Visibility);
        }
        finally
        {
            window.Close();
        }
    });
    [TestMethod]
    [DataRow(CatalogKind.Movie, false)]
    [DataRow(CatalogKind.Movie, true)]
    [DataRow(CatalogKind.Library, false)]
    public async Task AddPeople_ClipboardNames_NormalizesAndReusesStoredPortraits(CatalogKind kind, bool actors) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var known = fixture.Add(kind, "Known person source");
        var photo = EntryEditorModelTests.Png(Colors.SteelBlue);
        var person = new PersonPhoto("Known Person", photo);
        fixture.Store.Save(kind, new CatalogDetails(known, [], actors ? [] : [person], actors ? [person] : []));
        var entry = fixture.Add(kind, "Paste target");
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        var model = (EntryEditorModel)window.DataContext;
        var view = (PeopleEditorView)window.FindName(actors ? "CastEditor" : "PeopleEditor");
        // Act
        var clipboard = Clipboard.GetDataObject();
        try
        {
            Clipboard.SetText(" known   Person, new person, Known Person, , ↓");
            ((Button)view.FindName("PasteButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        finally
        {
            if (clipboard is null)
            {
                Clipboard.Clear();
            }
            else
            {
                Clipboard.SetDataObject(clipboard, true);
            }
        }
        model.Save();
        var saved = fixture.Store.GetDetails(kind, entry.Id)!;
        // Assert
        Assert.AreEqual("↓", ((Button)view.FindName("PasteButton")).Content);
        var people = actors ? saved.Actors : saved.Directors;
        Assert.AreEqual(2, people.Count);
        Assert.IsTrue(people.Any(item => item.Name == "New Person"));
        CollectionAssert.AreEqual(photo, people.Single(item => item.Name == "Known Person").Photo!);
        return Task.CompletedTask;
    });
}
