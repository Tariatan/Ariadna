using System.IO;
using System.Windows.Media;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class ThumbnailCacheTests
{
    [TestMethod]
    public async Task LoadAsync_ManyPosters_BoundsCacheAndReleasesFileHandles() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var cache = new ThumbnailCache(NullLogger.Instance);
        var png = EntryEditorModelTests.Png(Colors.Red);
        var files = Enumerable.Range(0, 270).Select(index => Path.Combine(fixture.Root, index.ToString())).ToArray();
        foreach (var file in files)
        {
            File.WriteAllBytes(file, png);
        }

        // Act
        var images = await Task.WhenAll(files.Select(file => cache.LoadAsync(file, CancellationToken.None)));
        var cachedCount = cache.Count;
        File.WriteAllBytes(files[0], EntryEditorModelTests.Png(Colors.Blue));
        cache.Clear();
        var replaced = await cache.LoadAsync(files[0], CancellationToken.None);
        // Assert
        Assert.IsTrue(images.All(image => image is { IsFrozen: true }));
        Assert.AreEqual(256, cachedCount);
        Assert.AreEqual(1, cache.Count);
        Assert.AreNotSame(images[0], replaced);
        Assert.IsNotNull(replaced);
    });
    [TestMethod]
    public async Task LoadAsync_CorruptOrMissingPoster_UsesEmptyFallback() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var cache = new ThumbnailCache(NullLogger.Instance);
        var corrupt = Path.Combine(fixture.Root, "corrupt");
        File.WriteAllText(corrupt, "invalid image");
        // Act
        var invalid = await cache.LoadAsync(corrupt, CancellationToken.None);
        var missing = await cache.LoadAsync(Path.Combine(fixture.Root, "missing"), CancellationToken.None);
        // Assert
        Assert.IsNull(invalid);
        Assert.IsNull(missing);
    });
}
