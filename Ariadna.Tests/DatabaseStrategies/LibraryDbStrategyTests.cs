using Ariadna.DatabaseStrategies;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class LibraryDbStrategyTests
{
    [TestMethod]
    public void QuickListFilter_Always_ReturnsExpectedFilterCharacters()
    {
        // Arrange
        var testee = new LibraryDbStrategy(NullLogger.Instance);

        // Act
        var result = testee.QuickListFilter();

        // Assert
        CollectionAssert.AreEqual(new[] { "}", "«", "(", "9" }, result);
    }

    [TestMethod]
    public void GetSubgenres_GenreContainsEnglish_ReturnsLanguageGenres()
    {
        // Arrange
        var testee = new LibraryDbStrategy(NullLogger.Instance);

        // Act
        var result = testee.GetSubgenres("English");

        // Assert
        CollectionAssert.AreEquivalent(Utilities.LibraryLanguagesGenres.Keys.ToArray(), result.Keys.ToArray());
    }

    [TestMethod]
    public void GetSubgenres_GenreContainsLiterature_ReturnsLiteratureGenres()
    {
        // Arrange
        var testee = new LibraryDbStrategy(NullLogger.Instance);

        // Act
        var result = testee.GetSubgenres("Literature");

        // Assert
        CollectionAssert.AreEquivalent(Utilities.LibraryLiteratureGenres.Keys.ToArray(), result.Keys.ToArray());
    }

    [TestMethod]
    public void GetSubgenres_GenreContainsProgramming_ReturnsProgrammingGenres()
    {
        // Arrange
        var testee = new LibraryDbStrategy(NullLogger.Instance);

        // Act
        var result = testee.GetSubgenres("Programming");

        // Assert
        CollectionAssert.AreEquivalent(Utilities.LibraryProgrammingGenres.Keys.ToArray(), result.Keys.ToArray());
    }

    [TestMethod]
    public void GetSubgenres_GenreDoesNotMatchKnownGroups_ReturnsMiscGenres()
    {
        // Arrange
        var testee = new LibraryDbStrategy(NullLogger.Instance);

        // Act
        var result = testee.GetSubgenres("Unknown");

        // Assert
        CollectionAssert.AreEquivalent(Utilities.LibraryMiscGenres.Keys.ToArray(), result.Keys.ToArray());
    }
}
