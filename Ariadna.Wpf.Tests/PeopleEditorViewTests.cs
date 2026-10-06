using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            Assert.IsNull(CatalogView.FindChild<TextBox>(view));
        }
        finally
        {
            window.Close();
        }
    });
}