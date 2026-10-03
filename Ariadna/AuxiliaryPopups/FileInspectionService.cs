#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

internal sealed class FileInspectionService(ILogger logger) : IFileInspectionService
{
    public Task<long> GetSizeAsync(string path, CancellationToken cancellationToken, IProgress<long> progress)
        => Task.Run(() => CalculateSize(path, cancellationToken, progress), cancellationToken);

    private static long CalculateSize(string path, CancellationToken cancellationToken, IProgress<long> progress)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (File.Exists(path))
        {
            return new FileInfo(path).Length;
        }
        if (!Directory.Exists(path))
        {
            return 0;
        }

        var pending = new Stack<string>();
        pending.Push(path);
        long size = 0;
        var lastUpdate = Environment.TickCount64;
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = pending.Pop();
            foreach (var file in EnumerateSafe(directory, false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    size += new FileInfo(file).Length;
                }
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }
                if (Environment.TickCount64 - lastUpdate >= 200)
                {
                    progress.Report(size);
                    lastUpdate = Environment.TickCount64;
                }
            }
            foreach (var child in EnumerateSafe(directory, true))
            {
                // Do not follow junctions/symlinks back into an ancestor.
                if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) == 0)
                {
                    pending.Push(child);
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return size;
    }

    private static string[] EnumerateSafe(string path, bool directories)
    {
        try
        {
            return directories ? Directory.GetDirectories(path) : Directory.GetFiles(path);
        }
        catch (IOException) { return []; }
        catch (UnauthorizedAccessException) { return []; }
    }

    public Task<VideoInfoSnapshot?> GetVideoInfoAsync(string path, CancellationToken cancellationToken)
        => Task.Run(() => ReadVideoInfo(path, cancellationToken), cancellationToken);

    private VideoInfoSnapshot? ReadVideoInfo(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Directory.Exists(path))
        {
            var first = EnumerateSafe(path, false).FirstOrDefault();
            var child = EnumerateSafe(path, true).FirstOrDefault();
            path = first ?? (child == null ? string.Empty : EnumerateSafe(child, false).FirstOrDefault() ?? string.Empty);
        }
        if (!File.Exists(path))
        {
            return null;
        }
        var info = new MediaInfoWrapper(path, logger);
        cancellationToken.ThrowIfCancellationRequested();
        var result = new VideoInfoSnapshot(Utilities.GetVideoDuration(path), info.Width, info.Height, info.VideoRate,
            info.AudioStreams.Select(stream => stream.Language).ToArray());
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
