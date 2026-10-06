using System.Globalization;
using System.Windows.Media.Imaging;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class GenreIconConverterTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Library)]
    public async Task Convert_CatalogGenres_LoadsPackagedLegacyIcons(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var converter = new GenreIconConverter();
        // Act
        var images = GenreCatalog.For(kind).Select(genre => new BitmapImage(new Uri((string)converter.Convert(genre, typeof(string), null!, CultureInfo.InvariantCulture)))).ToArray();
        // Assert
        Assert.IsTrue(images.All(image => image.PixelWidth > 0));
        return Task.CompletedTask;
    });
}