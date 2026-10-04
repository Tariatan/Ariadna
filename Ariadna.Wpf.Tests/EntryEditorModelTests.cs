using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class EntryEditorModelTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    public async Task Save_UnchangedLegacyValues_PreservesDataAndImages(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
            var entry = fixture.Add(kind, " Я Entry ");
        entry.Description = null;
        entry.Version = null;
        entry.CreationDate = null;
        var portrait = Png(Colors.Red);
        var people = kind is CatalogKind.Movie or CatalogKind.Library ? new[]
        {
            new PersonPhoto("Person", portrait)
        }

        : [];
        var actors = kind == CatalogKind.Movie ? new[]
        {
            new PersonPhoto("Actor", portrait)
        }

        : [];
        fixture.Store.Save(kind, new CatalogDetails(entry, ["Genre", "Unknown saved genre", "Five", "Six", "Seven"], people, actors));
        var root = fixture.Configuration.PosterRoot(kind);
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, entry.Id.ToString());
        File.WriteAllBytes(file, portrait);
        var editor = new EntryEditorModel(fixture.Actions, kind, entry.Path);
        // Act
        editor.Save();
        var saved = fixture.Store.GetDetails(kind, entry.Id)!;
        // Assert
        Assert.AreEqual(entry.Id, saved.Entry.Id);
            Assert.AreEqual(entry.Title, saved.Entry.Title);
            Assert.AreEqual(entry.OriginalTitle, saved.Entry.OriginalTitle);
            Assert.AreEqual(entry.Path, saved.Entry.Path);
        Assert.IsNull(saved.Entry.Wanted);
        Assert.IsNull(saved.Entry.CreationDate);
        if (kind == CatalogKind.Game)
        {
            Assert.IsNull(saved.Entry.Vr);
            Assert.IsNull(saved.Entry.Version);
        }
        else
        {
            Assert.IsNull(saved.Entry.Description);
        }

        CollectionAssert.AreEqual(new[] { "Genre", "Unknown saved genre", "Five", "Six", "Seven" }, saved.Genres.ToArray());
        CollectionAssert.AreEqual(portrait, File.ReadAllBytes(file));
        CollectionAssert.AreEqual(people.Select(person => person.Name).ToArray(), saved.Directors.Select(person => person.Name).ToArray());
        foreach (var person in saved.Directors.Concat(saved.Actors))
        {
            CollectionAssert.AreEqual(portrait, person.Photo!);
        }

        return Task.CompletedTask;
    });
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    public async Task Save_EditedFieldsAndPoster_ReopensCompleteEntry(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture("_custom-preview");
        var entry = fixture.Add(kind, "Before");
        var editor = new EntryEditorModel(fixture.Actions, kind, entry.Path)
        {
            Title = "After",
            Description = "Raw\n\ttext",
            Wanted = true,
            Version = "1.2",
            Vr = true,
        };
        editor.Genres.Clear();
        editor.AddGenre("New genre");
        editor.ReplaceImage(string.Empty, Png(Colors.Blue));
        if (kind == CatalogKind.Game)
        {
            foreach (var index in Enumerable.Range(1, 4))
            {
                editor.ReplaceImage(fixture.Configuration.PreviewSuffix(index), Png(Colors.Green));
            }
        }

        // Act
        editor.Save();
        var reopened = new EntryEditorModel(fixture.Actions, kind, entry.Path);
        // Assert
        Assert.AreEqual(entry.Id, reopened.StoredId);
        Assert.AreEqual("After", reopened.Title);
        Assert.IsTrue(reopened.Wanted);
        Assert.IsNotNull(reopened.Poster);
        CollectionAssert.AreEqual(new[] { "New genre" }, reopened.Genres.ToArray());
        if (kind == CatalogKind.Game)
        {
            Assert.IsTrue(reopened.Previews.All(image => image != null));
            Assert.AreEqual("1.2", reopened.Version);
            Assert.IsTrue(reopened.Vr);
        }
        else
        {
            Assert.AreEqual("Raw\n\ttext", reopened.Description);
        }

        new CatalogDatabase(fixture.Configuration.DatabasePath).CheckIntegrity();
        new CatalogDatabase(fixture.Configuration.DatabasePath).RecoverAssets();
        Assert.IsEmpty(Directory.GetDirectories(fixture.Root, ".ariadna-save-*"));
        return Task.CompletedTask;
    });
    [TestMethod]
    public async Task BuildDetails_CanceledImageAndFieldEdits_LeavesStoredDataUntouched() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Before");
        var editor = new EntryEditorModel(fixture.Actions, CatalogKind.Movie, entry.Path);
        // Act
        editor.Title = "Unsaved";
        editor.ReplaceImage(string.Empty, Png(Colors.Red));
        editor.BuildDetails();
        // Assert
        Assert.AreEqual("Before", fixture.Store.GetEntry(CatalogKind.Movie, entry.Id)!.Title);
        Assert.IsEmpty(Directory.GetFiles(fixture.Configuration.PosterRoot(CatalogKind.Movie)));
        return Task.CompletedTask;
    });
    [TestMethod]
    public async Task Save_ClearedPeopleAndGenres_RemovesRelationships() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Entry");
        fixture.Store.Save(CatalogKind.Movie, new CatalogDetails(entry, ["Genre"], [new("Director", null)], [new("Actor", null)]));
        var editor = new EntryEditorModel(fixture.Actions, CatalogKind.Movie, entry.Path);
        // Act
        editor.Genres.Clear();
        editor.People.Clear();
        editor.Actors.Clear();
        editor.Save();
        // Assert
        var details = fixture.Store.GetDetails(CatalogKind.Movie, entry.Id)!;
        Assert.IsEmpty(details.Genres);
        Assert.IsEmpty(details.Directors);
        Assert.IsEmpty(details.Actors);
        return Task.CompletedTask;
    });
    internal static byte[] Png(Color color)
    {
        var pixels = new byte[40 * 60 * 4];
        for (var index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = color.B;
            pixels[index + 1] = color.G;
            pixels[index + 2] = color.R;
            pixels[index + 3] = 255;
        }

        return Images.Png(BitmapSource.Create(40, 60, 96, 96, PixelFormats.Bgra32, null, pixels, 160));
    }
}
