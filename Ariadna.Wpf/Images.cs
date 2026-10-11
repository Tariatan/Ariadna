using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ariadna.Wpf;
internal static class Images
{
    internal static BitmapSource Decode(byte[] bytes, int width = 0)
    {
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = width;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    internal static byte[] Png(BitmapSource image)
    {
        using var stream = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        encoder.Save(stream);
        return stream.ToArray();
    }

    internal static byte[] Resize(byte[] bytes, int width, int height)
    {
        var image = Decode(bytes);
        var scaled = new TransformedBitmap(image, new ScaleTransform(
            (double)width / image.PixelWidth,
            (double)height / image.PixelHeight));
        return Png(scaled);
    }

    internal static byte[] ResizePortrait(byte[] bytes) => Resize(bytes, 200, 300);
}
