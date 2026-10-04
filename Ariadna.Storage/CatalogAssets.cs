using System.Text.Json;

namespace Ariadna.Storage;

public sealed class CatalogAssets(string root, IReadOnlyDictionary<string, byte[]> images, string previewSuffix = "_preview")
{
    internal string Token { get; } = Guid.NewGuid().ToString("N");
    private string? recoveryDirectory;
    private AssetFile[] files = [];

    internal void Prepare(string catalogDirectory, int id)
    {
        if (!previewSuffix.StartsWith('_') || previewSuffix.Length < 2
            || previewSuffix.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('_' or '-')))
        {
            throw new ArgumentException("Preview suffix must start with an underscore and contain only letters, digits, underscores, or hyphens.");
        }
        var previewKeys = Enumerable.Range(1, 4).Select(index => $"{previewSuffix}{index}").ToHashSet(StringComparer.Ordinal);
        recoveryDirectory = Path.Combine(catalogDirectory, $".ariadna-save-{Token}");
        Directory.CreateDirectory(recoveryDirectory);
        files = images.Select(pair =>
        {
            if (pair.Key != string.Empty && !previewKeys.Contains(pair.Key))
            {
                throw new ArgumentException("Unknown image suffix.");
            }
            var destination = Path.Combine(root, $"{id}{pair.Key}");
            if (!Path.IsPathFullyQualified(destination))
            {
                throw new ArgumentException("Image paths must be absolute.");
            }
            return new AssetFile(destination, File.Exists(destination), pair.Key.Length == 0 ? "poster" : pair.Key);
        }).ToArray();

        foreach (var file in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file.Destination)!);
            var suffix = file.Key == "poster" ? string.Empty : file.Key;
            WriteDurable(Path.Combine(recoveryDirectory, file.Key + ".new"), images[suffix]);
            if (file.Existed)
            {
                using var source = File.OpenRead(file.Destination);
                using var backup = new FileStream(Path.Combine(recoveryDirectory, file.Key + ".old"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
                source.CopyTo(backup);
                backup.Flush(true);
            }
        }
        // Flush the journal before promoting any file. Recovery can replay rollback after a crash.
        using var journal = new FileStream(Path.Combine(recoveryDirectory, "journal.json"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
        JsonSerializer.Serialize(journal, files);
        journal.Flush(true);
    }

    internal void Promote()
    {
        foreach (var file in files)
        {
            var temporary = file.Destination + $".{Token}.tmp";
            WriteDurable(temporary, File.ReadAllBytes(Path.Combine(recoveryDirectory!, file.Key + ".new")));
            File.Move(temporary, file.Destination, true);
        }
    }

    internal void Rollback()
    {
        if (recoveryDirectory != null && File.Exists(Path.Combine(recoveryDirectory, "journal.json")))
        {
            Recover(recoveryDirectory, false);
        }
    }

    internal void Complete()
    {
        if (recoveryDirectory != null)
        {
            Recover(recoveryDirectory, true);
        }
    }

    internal static void Recover(string directory, bool committed)
    {
        // A read-only staging directory cannot be removed on Windows, even when empty.
        var attributes = File.GetAttributes(directory);
        if ((attributes & FileAttributes.ReadOnly) != 0)
        {
            File.SetAttributes(directory, attributes & ~FileAttributes.ReadOnly);
        }

        var journalPath = Path.Combine(directory, "journal.json");
        if (!File.Exists(journalPath))
        {
            // An interrupted staging operation has not promoted any images.
            Directory.Delete(directory, true);
            return;
        }
        var journalFiles = JsonSerializer.Deserialize<AssetFile[]>(File.ReadAllText(journalPath))
            ?? throw new InvalidDataException("Invalid image recovery journal.");
        var token = Path.GetFileName(directory)[".ariadna-save-".Length..];
        foreach (var file in journalFiles)
        {
            File.Delete(file.Destination + $".{token}.tmp");
        }
        if (!committed)
        {
            foreach (var file in journalFiles)
            {
                if (file.Existed)
                {
                    File.Copy(Path.Combine(directory, file.Key + ".old"), file.Destination, true);
                }
                else
                {
                    File.Delete(file.Destination);
                }
            }
        }
        foreach (var file in Directory.GetFiles(directory))
        {
            File.Delete(file);
        }
        Directory.Delete(directory);
    }

    private sealed record AssetFile(string Destination, bool Existed, string Key);

    private static void WriteDurable(string path, byte[] bytes)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
        stream.Write(bytes);
        stream.Flush(true);
    }
}
