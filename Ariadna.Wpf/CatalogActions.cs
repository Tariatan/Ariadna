using System.Diagnostics;
using System.IO;
using System.Windows;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal sealed class CatalogActions(CatalogStore store, CatalogConfiguration configuration, ILogger logger)
{
    internal CatalogStore Store { get; } = store;
    internal CatalogConfiguration Configuration { get; } = configuration;
    internal ILogger Logger { get; } = logger;
    internal Action? ThumbnailsInvalidated { get; set; }

    internal void Report(Window owner, Exception exception)
    {
        Logger.LogError("Catalog action failed, error type '{ErrorType}'", exception.GetType().Name);
        MessageBox.Show(owner, exception.Message, "Ariadna", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal void Execute(CatalogKind kind, string path)
    {
        if (kind is CatalogKind.Movie or CatalogKind.Documentary && File.Exists(path))
        {
            Start(Configuration.Get("MediaPlayerPath"), [path]);
            return;
        }

        if (kind == CatalogKind.Library || Directory.Exists(path))
        {
            Start(Configuration.Get("TotalCommanderPath"), ["/O", $"/L={path}"]);
            return;
        }

        throw new FileNotFoundException("The entry's media path is unavailable.");
    }

    private static void Start(string executable, IEnumerable<string> arguments)
    {
        if (!File.Exists(executable))
        {
            throw new FileNotFoundException("The configured player or file manager is unavailable.");
        }

        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = Path.GetDirectoryName(executable),
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        Process.Start(start)?.Dispose();
    }

    internal Task<string?> DiscoverAsync(CatalogKind kind, CancellationToken cancellationToken) => Task.Run(() =>
    {
        var registered = Store.GetRegisteredPaths(kind);
        foreach (var path in DiscoveryCandidates(kind))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!registered.Any(stored => CatalogStore.PathsEqual(stored, path)))
            {
                return path;
            }
        }

        return null;
    }, cancellationToken);
    internal IEnumerable<string> DiscoveryCandidates(CatalogKind kind)
    {
        var root = Configuration.DiscoveryRoot(kind);
        if (kind == CatalogKind.Movie)
        {
            return Files(root).Concat(Files(Configuration.Get("DefaultMoviesPathTMP2"))).Concat(Directories(Configuration.Get("DefaultSeriesPath")));
        }

        if (kind == CatalogKind.Game)
        {
            return Directories(root).Concat(Directories(Configuration.Get("DefaultGamesPathVR")));
        }

        if (kind == CatalogKind.Documentary)
        {
            return Directories(root).SelectMany(directory => Directories(directory).Concat(Files(directory)));
        }

        return Directories(root).SelectMany(directory => LibraryCandidates(directory).Concat(Files(directory)));
    }

    private static IEnumerable<string> LibraryCandidates(string root)
    {
        // Preserve the legacy uppercase grouping / lowercase entry-directory convention.
        foreach (var directory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            if (Path.GetDirectoryName(directory)!.Any(char.IsLower))
            {
                continue;
            }

            if (Path.GetFileName(directory).Any(char.IsLower))
            {
                yield return directory;
            }
            else
            {
                foreach (var file in Files(directory))
                {
                    yield return file;
                }
            }
        }
    }

    private static IEnumerable<string> Files(string path) => Directory.Exists(path) ? Directory.EnumerateFiles(path) : [];
    private static IEnumerable<string> Directories(string path) => Directory.Exists(path) ? Directory.EnumerateDirectories(path) : [];
    internal void Remove(CatalogKind kind, CatalogEntry entry, bool deleteMedia)
    {
        if (!Store.Delete(kind, entry.Id))
        {
            return;
        }

        var root = Configuration.PosterRoot(kind);
        string[] suffixes = kind == CatalogKind.Game ? [string.Empty, ..Enumerable.Range(1, 4).Select(Configuration.PreviewSuffix)] : [string.Empty];
        foreach (var suffix in suffixes)
        {
            File.Delete(Path.Combine(root, $"{entry.Id}{suffix}"));
        }

        if (deleteMedia)
        {
            if (File.Exists(entry.Path))
            {
                File.SetAttributes(entry.Path, File.GetAttributes(entry.Path) & ~FileAttributes.ReadOnly);
                File.Delete(entry.Path);
            }
            else if (Directory.Exists(entry.Path))
            {
                Directory.Delete(entry.Path, true);
            }
        }
    }
}
