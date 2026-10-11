using System.IO;
using System.Windows.Media;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class PortraitStorageTests
{
    [TestMethod]
    public async Task Save_EditedPortrait_Writes200By300FileAndReopensWithIds() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Movie");
        var editor = new EntryEditorModel(fixture.Actions, CatalogKind.Movie, entry.Path);
        var person = new PersonEditorModel(new PersonPhoto("Person", null) { TmdbId = 123 });
        person.SetPhoto(Images.Resize(EntryEditorModelTests.Png(Colors.Red), 200, 300));
        editor.Actors.Add(person);

        // Act
        editor.Save();
        var reopened = new EntryEditorModel(fixture.Actions, CatalogKind.Movie, entry.Path);
        var saved = reopened.Actors.Single().ToPerson();

        // Assert
        Assert.IsTrue(saved.Id >= 0);
        Assert.IsTrue(saved.PersonId >= 0);
        Assert.AreEqual(123, saved.TmdbId);
        var file = Path.Combine(fixture.Configuration.PeopleRoot, $"{saved.PersonId}.png");
        Assert.IsTrue(File.Exists(file));
        var image = Images.Decode(File.ReadAllBytes(file));
        Assert.AreEqual(200, image.PixelWidth);
        Assert.AreEqual(300, image.PixelHeight);
        Assert.IsNotNull(reopened.Actors.Single().Portrait);
        return Task.CompletedTask;
    });

    [TestMethod]
    public async Task BuildDetails_CancelledPortraitEdit_LeavesStoredPortraitAndIdentityUntouched() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Movie");
        var original = Images.Resize(EntryEditorModelTests.Png(Colors.Red), 200, 300);
        fixture.Store.Save(CatalogKind.Movie, new CatalogDetails(entry, [], [], [new("Person", original) { TmdbId = 123 }]));
        var editor = new EntryEditorModel(fixture.Actions, CatalogKind.Movie, entry.Path);
        var before = editor.Actors.Single().ToPerson();

        // Act
        editor.Actors.Single().SetPhoto(Images.Resize(EntryEditorModelTests.Png(Colors.Blue), 200, 300));
        editor.BuildDetails();

        // Assert
        var stored = fixture.Store.GetDetails(CatalogKind.Movie, entry.Id)!.Actors.Single();
        Assert.AreEqual(before.PersonId, stored.PersonId);
        CollectionAssert.AreEqual(original, stored.Photo!);
        Assert.HasCount(1, Directory.GetFiles(fixture.Configuration.PeopleRoot));
        return Task.CompletedTask;
    });
}
