using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMDbLib.Objects.General;
using TMDbLib.Objects.TvShows;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class MetadataServiceTests
{
    [TestMethod]
    public async Task ApplyDetails_AvailableFields_ReplacesTranslationYearAndGenresPreservingOriginalTitle() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(Ariadna.Storage.CatalogKind.Movie, "Old translation");
        var editor = new EntryEditorModel(fixture.Actions, Ariadna.Storage.CatalogKind.Movie, entry.Path);
        var originalTitle = editor.OriginalTitle;
        editor.Genres.Add("Old genre");

        // Act
        MetadataService.ApplyDetails(editor, MetadataField.Translation, "Русское название", 2026, ["Drama"]);
        MetadataService.ApplyDetails(editor, MetadataField.Year, null, 2026, []);
        MetadataService.ApplyDetails(editor, MetadataField.Genres, null, 0, ["Drama", "Drama", "Comedy"]);
        editor.Save();
        var saved = fixture.Store.GetDetails(Ariadna.Storage.CatalogKind.Movie, entry.Id)!;

        // Assert
        Assert.AreEqual("Русское название", saved.Entry.Title);
        Assert.AreEqual(originalTitle, saved.Entry.OriginalTitle);
        Assert.AreEqual(2026, saved.Entry.Year);
        CollectionAssert.AreEqual((string[])["Drama", "Comedy"], saved.Genres.ToArray());
        return Task.CompletedTask;
    });

    [TestMethod]
    public async Task ApplyDetails_UnavailableFields_PreservesExistingValues() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(Ariadna.Storage.CatalogKind.Movie, "Existing translation");
        var editor = new EntryEditorModel(fixture.Actions, Ariadna.Storage.CatalogKind.Movie, entry.Path) { Year = "2001" };
        editor.Genres.Add("Existing genre");
        var genres = editor.Genres.ToArray();

        // Act / Assert
        Assert.Throws<System.IO.InvalidDataException>(() => MetadataService.ApplyDetails(editor, MetadataField.Translation, " ", 0, []));
        Assert.Throws<System.IO.InvalidDataException>(() => MetadataService.ApplyDetails(editor, MetadataField.Year, null, 0, []));
        Assert.Throws<System.IO.InvalidDataException>(() => MetadataService.ApplyDetails(editor, MetadataField.Genres, null, 0, []));
        Assert.AreEqual("Existing translation", editor.Title);
        Assert.AreEqual("2001", editor.Year);
        CollectionAssert.AreEqual(genres, editor.Genres.ToArray());
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow("Брат", "ru")]
    [DataRow("Ёлки", "ru")]
    [DataRow("ё", "ru")]
    [DataRow("Iron Man / Железный человек", "ru")]
    [DataRow("The Matrix", "en")]
    [DataRow("Amélie", "en")]
    [DataRow("七人の侍", "en")]
    [DataRow("123 — !", "en")]
    [DataRow("", "en")]
    [DataRow(null, "en")]
    public void GetLanguage_OriginalTitle_SelectsRussianLettersOrEnglishFallback(string? originalTitle, string expected)
    {
        // Arrange
        // The title may be absent before automatic discovery finishes.

        // Act
        var language = MetadataService.GetLanguage(originalTitle);

        // Assert
        Assert.AreEqual(expected, language);
    }

    [TestMethod]
    public async Task GetSeriesDirectorsAsync_DirectorsAvailable_DoesNotLoadCreators()
    {
        // Arrange
        var credits = new Credits
        {
            Crew = [new Crew { Id = 1, Name = "Director", Job = "Director", ProfilePath = "/director.jpg" }],
        };

        // Act
        var result = await MetadataService.GetSeriesDirectorsAsync(credits,
            () => throw new AssertFailedException("Creators should not be requested when directors exist."));

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual((1, "Director", "/director.jpg"), result[0]);
    }

    [TestMethod]
    public async Task GetSeriesDirectorsAsync_NoNamedDirectors_ReturnsDistinctNamedCreatorsWithPortraits()
    {
        // Arrange
        var credits = new Credits
        {
            Crew =
            [
                new Crew { Id = 1, Name = " ", Job = "Director" },
                new Crew { Id = 2, Name = "Producer", Job = "Producer" },
            ],
        };
        var show = new TvShow
        {
            CreatedBy =
            [
                new CreatedBy { Id = 3, Name = "Creator", ProfilePath = "/creator.jpg" },
                new CreatedBy { Id = 3, Name = "Creator duplicate" },
                new CreatedBy { Id = 4, Name = " " },
                new CreatedBy { Id = 5, Name = "Another creator" },
            ],
        };

        // Act
        var result = await MetadataService.GetSeriesDirectorsAsync(credits, () => Task.FromResult(show));

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual((3, "Creator", "/creator.jpg"), result[0]);
        Assert.AreEqual(5, result[1].Id);
        Assert.IsNull(result[1].ProfilePath);
    }

    [TestMethod]
    public async Task GetSeriesDirectorsAsync_NoDirectorsOrCreators_ReturnsEmpty()
    {
        // Arrange
        var credits = new Credits();

        // Act
        var result = await MetadataService.GetSeriesDirectorsAsync(credits, () => Task.FromResult(new TvShow()));

        // Assert
        Assert.AreEqual(0, result.Length);
    }
}
