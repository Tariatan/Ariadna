using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMDbLib.Objects.General;
using TMDbLib.Objects.TvShows;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class MetadataServiceTests
{
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
