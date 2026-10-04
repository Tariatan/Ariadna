using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal sealed class ThumbnailCache(ILogger logger)
{
    private readonly SemaphoreSlim workers = new(4);
    private readonly object gate = new();
    private readonly Dictionary<string, BitmapSource?> images = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<string> order = new();
    private int generation;
    internal int Count
    {
        get
        {
            lock (gate)
            {
                return images.Count;
            }
        }
    }

    internal async Task<BitmapSource?> LoadAsync(string path, CancellationToken cancellationToken)
    {
        int version;
        lock (gate)
        {
            version = generation;
            if (images.TryGetValue(path, out var cached))
            {
                return cached;
            }
        }

        await workers.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (gate)
            {
                if (images.TryGetValue(path, out var cached))
                {
                    return cached;
                }
            }

            var image = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return File.Exists(path) ? Images.Decode(File.ReadAllBytes(path), 240) : null;
                }
                catch (Exception exception)when (exception is IOException or UnauthorizedAccessException or NotSupportedException or System.IO.FileFormatException)
                {
                    logger.LogWarning("Poster decoding failed, error type '{ErrorType}'", exception.GetType().Name);
                    return null;
                }
            }, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            lock (gate)
            {
                if (version == generation && !images.ContainsKey(path))
                {
                    images.Add(path, image);
                    order.Enqueue(path);
                    while (order.Count > 256)
                    {
                        images.Remove(order.Dequeue());
                    }
                }
            }

            return image;
        }
        finally
        {
            workers.Release();
        }
    }

    internal void Clear()
    {
        lock (gate)
        {
            images.Clear();
            order.Clear();
            generation++;
        }
    }
}
