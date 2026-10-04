using System.IO;
using MediaInfo;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal static class FileInspection
{
    internal static Task<string> InspectAsync(string path, bool video, ILogger logger, CancellationToken cancellationToken) => Task.Run(() =>
    {
        long size = 0;
        var candidate = File.Exists(path) ? path : null;
        if (candidate != null)
        {
            size = new FileInfo(candidate).Length;
        }
        else if (Directory.Exists(path))
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint
            };
            foreach (var file in Directory.EnumerateFiles(path, "*", options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                size += new FileInfo(file).Length;
                candidate ??= file;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = $"Size: {size / (1024d * 1024):N1} MB";
        if (video && candidate != null)
        {
            var info = new MediaInfoWrapper(candidate, logger);
            cancellationToken.ThrowIfCancellationRequested();
            if (info.Success)
            {
                result += $"   Duration: {TimeSpan.FromMilliseconds(info.Duration):hh\\:mm\\:ss}   {info.Width}×{info.Height}   {info.VideoRate} kbps\n";
                result += string.Join(", ", info.AudioStreams.Select(stream => stream.Language));
            }
        }

        return result;
    }, cancellationToken);
}
