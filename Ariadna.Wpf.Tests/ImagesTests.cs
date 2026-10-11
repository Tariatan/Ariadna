using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class ImagesTests
{
    [TestMethod]
    public async Task Resize_FineCheckerboardToPosterSize_AveragesPixelsWithoutAliasing() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        const int width = 1200;
        const int height = 1800;
        var pixels = new byte[width * height];
        for (var index = 0; index < pixels.Length; index++)
        {
            pixels[index] = (byte)((index % width + index / width) % 2 == 0 ? 0 : 255);
        }

        var source = Images.Png(BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width));

        // Act
        var resized = Images.Decode(Images.Resize(source, 600, 900));

        // Assert
        Assert.AreEqual(600, resized.PixelWidth);
        Assert.AreEqual(900, resized.PixelHeight);
        var gray = new FormatConvertedBitmap(resized, PixelFormats.Gray8, null, 0);
        var actualPixels = new byte[600 * 900];
        gray.CopyPixels(actualPixels, 600, 0);
        Assert.IsTrue(actualPixels.All(value => value is >= 120 and <= 135),
            "Fine alternating pixels should average to gray instead of producing jagged patterns.");
        return Task.CompletedTask;
    });
}
