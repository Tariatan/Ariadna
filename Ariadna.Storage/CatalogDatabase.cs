using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Ariadna.Storage;

public sealed class CatalogDatabase
{
    public static readonly int ApplicationId = 0x41524941;
    public static readonly int SchemaVersion = 1;
    private static readonly CompareInfo comparison = CultureInfo.GetCultureInfo("en-US").CompareInfo;
    private readonly string path;
    internal string DirectoryPath => System.IO.Path.GetDirectoryName(path)!;

    internal IDisposable AcquireWriteLock()
    {
        var name = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path.ToUpperInvariant())));
        var mutex = new Mutex(false, $"Local\\AriadnaCatalog_{name}");
        try
        {
            try
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(10)))
                {
                    throw new TimeoutException("Another Ariadna window is saving. Try again when it finishes.");
                }
            }
            catch (AbandonedMutexException)
            {
                // The prior writer crashed; the acquired lock protects journal recovery.
            }
            return new WriteLock(mutex);
        }
        catch
        {
            mutex.Dispose();
            throw;
        }
    }

    public void RecoverAssets()
    {
        using var writeLock = AcquireWriteLock();
        RecoverAssetsWhileLocked();
    }

    internal void RecoverAssetsWhileLocked()
    {
        using var connection = Open();
        foreach (var directory in Directory.GetDirectories(DirectoryPath, ".ariadna-save-*"))
        {
            var token = System.IO.Path.GetFileName(directory)[".ariadna-save-".Length..];
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM CatalogAssetCommit WHERE token=$token";
            command.Parameters.AddWithValue("$token", token);
            CatalogAssets.Recover(directory, (long)command.ExecuteScalar()! != 0);
            command.CommandText = "DELETE FROM CatalogAssetCommit WHERE token=$token";
            command.ExecuteNonQuery();
        }
    }

    public CatalogDatabase(string path)
    {
        if (!System.IO.Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("The catalog path must be absolute.", nameof(path));
        }

        this.path = System.IO.Path.GetFullPath(path);
    }

    public SqliteConnection Open(bool readOnly = false)
    {
        var connection = Connect(path, readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite);
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA application_id";
            if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != ApplicationId)
            {
                throw new InvalidDataException("This file is not an Ariadna catalog. Restore or import a verified catalog.");
            }

            command.CommandText = "PRAGMA user_version";
            if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != SchemaVersion)
            {
                throw new InvalidDataException("Unsupported Ariadna catalog schema. Use a compatible application or migration tool.");
            }

            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    public static void Create(string path)
    {
        if (!System.IO.Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("The catalog path must be absolute.", nameof(path));
        }

        // CreateNew reserves the destination, so retries never overwrite another catalog.
        using (var reservation = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        {
        }

        using var connection = Connect(path, SqliteOpenMode.ReadWrite);
        using var stream = typeof(CatalogDatabase).Assembly.GetManifestResourceStream("Ariadna.Storage.Schema.sql")
            ?? throw new InvalidOperationException("The catalog schema resource is missing.");
        using var reader = new StreamReader(stream);
        using var command = connection.CreateCommand();
        command.CommandText = reader.ReadToEnd();
        command.ExecuteNonQuery();
    }

    public void CheckIntegrity()
    {
        using var connection = Open(true);
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check";
        if (!Equals(command.ExecuteScalar(), "ok"))
        {
            throw new InvalidDataException("SQLite integrity check failed.");
        }

        command.CommandText = "PRAGMA foreign_key_check";
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            throw new InvalidDataException("SQLite foreign-key check failed.");
        }
    }

    public void Backup(string destination)
    {
        using var reservation = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
        reservation.Dispose();
        using var source = Open();
        using var target = Connect(destination, SqliteOpenMode.ReadWrite);
        source.BackupDatabase(target);
        new CatalogDatabase(destination).CheckIntegrity();
    }

    private static SqliteConnection Connect(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = mode,
            ForeignKeys = true,
            DefaultTimeout = 5,
            Pooling = false,
        }.ToString());

        try
        {
            connection.Open();
            connection.CreateCollation("ARIADNA", Compare);
            connection.CreateFunction<string?, string?, bool>("contains_text", (text, value) =>
                text != null && value != null && comparison.IndexOf(text, value, CompareOptions.IgnoreCase) >= 0, true);
            connection.CreateFunction<string?, string?, bool>("starts_text", (text, value) =>
                text != null && value != null && comparison.IsPrefix(text, value, CompareOptions.IgnoreCase), true);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT sqlite_version()";
            var version = Version.Parse((string)command.ExecuteScalar()!);
            if (version < new Version(3, 51, 3))
            {
                throw new NotSupportedException("SQLite 3.51.3 or newer is required for safe concurrent WAL use.");
            }

            command.CommandText = "PRAGMA synchronous=FULL";
            command.ExecuteNonQuery();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    internal static int Compare(string? left, string? right) => comparison.Compare(left?.TrimEnd(' '), right?.TrimEnd(' '), CompareOptions.IgnoreCase);

    private sealed class WriteLock(Mutex mutex) : IDisposable
    {
        public void Dispose()
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }
    }
}
