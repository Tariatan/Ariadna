using Ariadna.DatabaseStrategies;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class GamesDbStrategyTests
{
    [TestMethod]
    public void QuickListFilter_Always_ReturnsExpectedFilterCharacters()
    {
        // Arrange
        var testee = new GamesDbStrategy(NullLogger.Instance);

        // Act
        var result = testee.QuickListFilter();

        // Assert
        CollectionAssert.AreEqual(new[] { "}", "«" }, result);
    }
}
