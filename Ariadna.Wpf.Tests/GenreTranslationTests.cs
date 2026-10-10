using System.Globalization;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class GenreTranslationTests
{
    [TestMethod]
    [DataRow("Боевик", "Action")]
    [DataRow("приключения", "Adventure")]
    [DataRow("Мультфильм", "Animation")]
    [DataRow("Анимация", "Animation")]
    [DataRow("Мелодрама", "Drama")]
    [DataRow("Фантастика", "Sci-Fi")]
    [DataRow("Новогодний", "New Year")]
    public async Task AddGenre_RussianMovieGenre_PersistsEnglishWithMatchingIcon(string russian, string english) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(CatalogKind.Movie, "Translation sample");
        var editor = new EntryEditorModel(fixture.Actions, CatalogKind.Movie, entry.Path);
        editor.Genres.Clear();
        var converter = new GenreIconConverter();

        // Act
        editor.AddGenre(russian);
        editor.AddGenre(english);
        fixture.Store.Save(CatalogKind.Movie, editor.BuildDetails());
        var loaded = fixture.Store.GetDetails(CatalogKind.Movie, entry.Id);

        // Assert
        CollectionAssert.AreEqual((string[])[english], loaded!.Genres.ToArray());
        var icon = converter.Convert(english, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.IsNotNull(icon);
        Assert.AreEqual(icon, converter.Convert(russian, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.AreEqual(1, fixture.Store.Query(CatalogKind.Movie, new CatalogQuery { Genre = english, RequireGenreMatch = true }).Count);
        return Task.CompletedTask;
    });

    [TestMethod]
    public void Normalize_CustomAndOtherCollectionGenres_PreservesNames()
    {
        // Arrange
        const string custom = "Custom movie genre";

        // Act
        var movie = GenreCatalog.Normalize(CatalogKind.Movie, custom);
        var game = GenreCatalog.Normalize(CatalogKind.Game, "Боевик");

        // Assert
        Assert.AreEqual(custom, movie);
        Assert.AreEqual("Боевик", game);
    }
}
