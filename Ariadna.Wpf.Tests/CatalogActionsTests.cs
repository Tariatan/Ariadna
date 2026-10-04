using System.IO;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class CatalogActionsTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Documentary)]
    public async Task DiscoverAsync_RegisteredAndIgnoredPaths_ReturnsNextCandidate(CatalogKind kind)
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var root = fixture.Configuration.DiscoveryRoot(kind);
        if (kind == CatalogKind.Documentary)
        {
            root = Path.Combine(root, "GROUP");
            Directory.CreateDirectory(root);
        }

        var first = Path.Combine(root, "01");
        var second = Path.Combine(root, "02");
        var third = Path.Combine(root, "03");
        foreach (var path in new[]
        {
            first,
            second,
            third
        }

        )
        {
            if (kind == CatalogKind.Game)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                File.WriteAllBytes(path, []);
            }
        }

        var entry = fixture.Add(kind, "Registered");
        entry.Path = first;
        fixture.Store.Save(kind, new CatalogDetails(entry, [], [], []));
        fixture.Store.Ignore(second);
        // Act
        var discovered = await fixture.Actions.DiscoverAsync(kind, CancellationToken.None);
        // Assert
        Assert.AreEqual(third, discovered);
    }

    [TestMethod]
    public void Remove_CatalogOnly_KeepsMediaAndOtherEntries()
    {
        // Arrange
        using var fixture = new CatalogFixture("_custom-preview");
        var first = fixture.Add(CatalogKind.Game, "First");
        var second = fixture.Add(CatalogKind.Game, "Second");
        Directory.CreateDirectory(first.Path);
        var root = fixture.Configuration.PosterRoot(CatalogKind.Game);
        foreach (var suffix in new[]
        {
            string.Empty,
            "_custom-preview1",
            "_custom-preview2",
            "_custom-preview3",
            "_custom-preview4"
        }

        )
        {
            File.WriteAllBytes(Path.Combine(root, $"{first.Id}{suffix}"), [1]);
        }

        // Act
        fixture.Actions.Remove(CatalogKind.Game, first, false);
        // Assert
        Assert.IsNull(fixture.Store.GetEntry(CatalogKind.Game, first.Id));
        Assert.IsNotNull(fixture.Store.GetEntry(CatalogKind.Game, second.Id));
        Assert.IsTrue(Directory.Exists(first.Path));
        Assert.IsEmpty(Directory.GetFiles(root));
    }
}
