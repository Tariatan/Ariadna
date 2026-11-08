using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public sealed class ImagesTests
{
    [TestMethod]
    [DataRow("UklGRiIAAABXRUJQVlA4TBYAAAAvA0ABEAdQmSJXoYABgCD8b5uI6H8I", 128)]
    [DataRow("UklGRjwAAABXRUJQVlA4IDAAAADQAQCdASoEAAYAAUAmJaACdLoB+AADsAD+8Bvf/zsGxLteOf/3OB7OB7OB/EgAAAA=", 255)]
    public async Task Decode_WebP_ProducesFrozenBitmapAndCanSaveAsPng(string encodedImage, int alpha) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var source = Convert.FromBase64String(encodedImage);

        // Act
        var thumbnail = Images.Decode(source, 2);
        var saved = Images.Decode(Images.ResizePortrait(source));

        // Assert
        Assert.IsTrue(thumbnail.IsFrozen);
        Assert.AreEqual(2, thumbnail.PixelWidth);
        Assert.AreEqual(3, thumbnail.PixelHeight);
        Assert.AreEqual(200, saved.PixelWidth);
        Assert.AreEqual(300, saved.PixelHeight);
        var bitmap = new FormatConvertedBitmap(saved, PixelFormats.Bgra32, null, 0);
        var pixel = new byte[4];
        bitmap.CopyPixels(new System.Windows.Int32Rect(0, 0, 1, 1), pixel, 4, 0);
        Assert.IsTrue(pixel[2] is >= 195 and <= 205);
        Assert.AreEqual((byte)alpha, pixel[3]);
        return Task.CompletedTask;
    });

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
