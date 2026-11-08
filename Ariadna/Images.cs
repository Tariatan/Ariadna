using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace Ariadna;
internal static class Images
{
    internal static BitmapSource Decode(byte[] bytes, int width = 0)
    {
        using var stream = new MemoryStream(bytes);
        if (bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
        {
            return DecodeWebP(stream, width);
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = width;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    private static BitmapSource DecodeWebP(Stream stream, int width)
    {
        using var codec = SKCodec.Create(stream)
            ?? throw new FileFormatException("Invalid WebP image.");
        var source = codec.Info;
        var dimensions = width > 0 && width < source.Width
            ? codec.GetScaledDimensions((float)width / source.Width)
            : new SKSizeI(source.Width, source.Height);
        using var colorSpace = SKColorSpace.CreateSrgb();
        var info = new SKImageInfo(dimensions.Width, dimensions.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul, colorSpace);
        var pixels = new byte[info.BytesSize];
        if (codec.GetPixels(info, pixels) != SKCodecResult.Success)
        {
            throw new FileFormatException("Could not decode WebP image.");
        }

        var image = BitmapSource.Create(info.Width, info.Height, 96, 96, PixelFormats.Bgra32, null, pixels, info.RowBytes);
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
