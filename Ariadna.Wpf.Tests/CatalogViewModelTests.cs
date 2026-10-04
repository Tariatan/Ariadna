using System.IO;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class CatalogViewModelTests
{
    [TestMethod]
    public async Task RefreshAsync_RealProviderFiltersAndQuickJump_PreservesExpectedResults() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.Add(CatalogKind.Movie, "Alpha", true);
        fixture.Add(CatalogKind.Movie, "Яблоко", true);
        fixture.Add(CatalogKind.Movie, "Beta", false);
        using var model = Model(fixture, CatalogKind.Movie);
        // Act
        model.Wish = true;
        await model.RefreshAsync(CancellationToken.None);
        model.Jump("Я");
        // Assert
        Assert.HasCount(2, model.Entries);
        CollectionAssert.AreEqual(new[] { "A", "Я" }, model.Letters.ToArray());
        Assert.AreEqual("Яблоко", model.Selected!.Entry.Title);
        model.Title = "бло";
        await model.RefreshAsync(CancellationToken.None);
        Assert.HasCount(1, model.Entries);
        Assert.AreEqual("Яблоко", model.Entries[0].Entry.Title);
    });
    [TestMethod]
    public async Task RefreshAsync_ReplacedRequest_PublishesOnlyLatestFilter() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(3000);
        using var model = Model(fixture, CatalogKind.Movie);
        // Act
        model.Title = "A Movie";
        var first = model.RefreshAsync(CancellationToken.None, true);
        model.Title = "Z Movie";
        var second = model.RefreshAsync(CancellationToken.None);
        await Task.WhenAll(first, second);
        // Assert
        Assert.IsFalse(model.Busy);
        Assert.IsTrue(model.Entries.Count > 0);
        Assert.IsTrue(model.Entries.All(item => item.Entry.Title.StartsWith("Z Movie", StringComparison.Ordinal)));
    });
    [TestMethod]
    public async Task SetColumns_ResizedGrid_KeepsSelectionAndEntryOrder() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.AddMany(31);
        using var model = Model(fixture, CatalogKind.Movie);
        await model.RefreshAsync(CancellationToken.None);
        model.Selected = model.Entries[20];
        var selected = model.Selected;
        // Act
        model.SetColumns(3);
        // Assert
        Assert.AreSame(selected, model.Selected);
        Assert.HasCount(11, model.Rows);
        CollectionAssert.AreEqual(model.Entries.ToArray(), model.Rows.SelectMany(row => row.Items).ToArray());
        Assert.IsTrue(model.Rows.All(row => row.Items.Count <= 3));
    });
    [TestMethod]
    public async Task Vr_MutuallyExclusiveFilters_PreservesOtherFilters() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.Add(CatalogKind.Game, "VR", true, true);
        fixture.Add(CatalogKind.Game, "Flat", true, false);
        using var model = Model(fixture, CatalogKind.Game);
        // Act
        model.Wish = true;
        model.NonVr = true;
        model.Vr = true;
        await model.RefreshAsync(CancellationToken.None);
        // Assert
        Assert.IsFalse(model.NonVr);
        Assert.IsTrue(model.Wish);
        Assert.HasCount(1, model.Entries);
        Assert.AreEqual("VR", model.Entries[0].Entry.Title);
    });
    internal static CatalogViewModel Model(CatalogFixture fixture, CatalogKind kind) => new(kind, fixture.Store, fixture.Configuration, new ThumbnailCache(NullLogger.Instance), NullLogger.Instance);
}
