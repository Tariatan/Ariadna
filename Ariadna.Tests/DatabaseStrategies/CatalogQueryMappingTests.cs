using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class CatalogQueryMappingTests
{
    [TestMethod]
    public void CreateQuery_LibrarySubgenreSelected_PrefersSubgenre()
    {
        // Arrange
        var values = new Ariadna.DatabaseStrategies.AbstractDbStrategy.QueryParams { Genre = "Programming", Subgenre = "C#" };

        // Act
        var result = CatalogServices.CreateQuery(values, library: true);

        // Assert
        Assert.AreEqual("C#", result.Genre);
    }

    [TestMethod]
    public void CreateQuery_LibrarySubgenreEmpty_UsesSelectedGenre()
    {
        // Arrange
        var values = new Ariadna.DatabaseStrategies.AbstractDbStrategy.QueryParams { Genre = "Programming", Subgenre = Utilities.EmptyDots };

        // Act
        var result = CatalogServices.CreateQuery(values, library: true);

        // Assert
        Assert.AreEqual("Programming", result.Genre);
    }
}
