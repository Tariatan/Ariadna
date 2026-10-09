using System.IO;
using MediaInfo;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal static class FileInspection
{
    internal static Task<FileInspectionResult> InspectAsync(string path, bool video, ILogger logger, CancellationToken cancellationToken, bool sizeInGigabytes = false) => Task.Run(() =>
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
        var sizeText = sizeInGigabytes || size >= 1024L * 1024 * 1024 ? $"{size / (1024d * 1024 * 1024):N1} GB" : $"{size / (1024d * 1024):N1} MB";
        var metrics = new List<MediaMetric> { new(sizeText, MediaMetric.StorageIcon) };
        IReadOnlyCollection<AudioLanguage> languages = [];
        if (video && candidate != null)
        {
            var info = new MediaInfoWrapper(candidate, logger);
            cancellationToken.ThrowIfCancellationRequested();
            if (info.Success)
            {
                var duration = $"{TimeSpan.FromMilliseconds(info.Duration):hh\\:mm\\:ss}";
                var resolution = $"{info.Width}×{info.Height}";
                metrics.Add(new MediaMetric(duration, MediaMetric.ClockIcon));
                metrics.Add(new MediaMetric(resolution, MediaMetric.ScreenIcon));
                metrics.Add(new MediaMetric($"{info.VideoRate / 1000000d:N0} Mbps", MediaMetric.BitrateIcon));
                languages = info.AudioStreams.Select(stream => new AudioLanguage(string.IsNullOrWhiteSpace(stream.Language) ? "Unknown" : stream.Language)).ToArray();
            }
        }

        return new FileInspectionResult(languages, metrics);
    }, cancellationToken);
}
