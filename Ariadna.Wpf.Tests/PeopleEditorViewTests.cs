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
    [DataRow(CatalogKind.Movie, false)]
    [DataRow(CatalogKind.Movie, true)]
    [DataRow(CatalogKind.Library, false)]
    public async Task Rename_F2_CommitsNameAndPreservesPhoto(CatalogKind kind, bool actors) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Rename target");
        var photo = EntryEditorModelTests.Png(Colors.SteelBlue);
        var person = new PersonPhoto("Before", photo);
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
            Assert.AreEqual(Visibility.Visible, input.Visibility);
            Assert.AreEqual("Before", input.SelectedText);
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
