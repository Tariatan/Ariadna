using System.Windows;
using System.Windows.Controls;
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
    public async Task Commit_EditedPortraitName_PersistsWithoutLosingRelationships(CatalogKind kind, bool actors) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Synthetic editor");
        var photo = EntryEditorModelTests.Png(System.Windows.Media.Colors.SteelBlue);
        var person = new PersonPhoto("Before", photo);
        fixture.Store.Save(kind, new CatalogDetails(entry, ["Genre"], actors ? [] : [person], actors ? [person] : []));
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        try
        {
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var view = (PeopleEditorView)window.FindName(actors ? "CastEditor" : "PeopleEditor");
            var list = (ListBox)view.FindName("People");
            var container = list.ItemContainerGenerator.ContainerFromIndex(0);
            var name = CatalogView.FindChild<TextBox>(container)!;
            name.Focus();
            // Act
            name.SetCurrentValue(TextBox.TextProperty, "After");
            view.Commit();
            ((EntryEditorModel)window.DataContext).Save();
            var saved = fixture.Store.GetDetails(kind, entry.Id)!;
            // Assert
            var savedPerson = (actors ? saved.Actors : saved.Directors).Single();
            Assert.AreEqual("After", savedPerson.Name);
            CollectionAssert.AreEqual(photo, savedPerson.Photo!);
            CollectionAssert.AreEqual(new[] { "Genre" }, saved.Genres.ToArray());
            Assert.AreSame(list.Items[0], list.SelectedItem);
        }
        finally
        {
            window.Close();
        }
    });
}