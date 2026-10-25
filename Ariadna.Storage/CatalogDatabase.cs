using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Ariadna.Storage;

public sealed class CatalogDatabase
{
    public static readonly int ApplicationId = 0x41524941;
    public static readonly int SchemaVersion = 2;
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
        AddXxxTables(connection);
    }

    // An additive schema-2 extension: older frontends can still use their four catalogs.
    // Existing assets are untouched, so the verified database backup uses those same files.
    public void EnsureXxxCollection()
    {
        using var writeLock = AcquireWriteLock();
        RecoverAssetsWhileLocked();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Xxx'";
        if ((long)command.ExecuteScalar()! != 0)
        {
            return;
        }

        CheckIntegrity();
        var backup = $"{path}.before-xxx-{Guid.NewGuid():N}.sqlite";
        Backup(backup);
        AddXxxTables(connection);
        CheckIntegrity();
    }

    public void RenamePostapocalypseGenre()
    {
        using var writeLock = AcquireWriteLock();
        RecoverAssetsWhileLocked();
        using var connection = Open();
        using var command = connection.CreateCommand();
        string[] tables = ["Genre", "GenreOfGame", "GenreOfDocumentary", "GenreOfLibrary"];
        command.CommandText = string.Join(" UNION ALL ", tables.Select(table => $"SELECT 1 FROM [{table}] WHERE [name]='Postapocalypse'")) + " LIMIT 1";
        if (command.ExecuteScalar() is null)
        {
            return;
        }

        CheckIntegrity();
        Backup($"{path}.before-dystopian-{Guid.NewGuid():N}.sqlite");
        using var transaction = connection.BeginTransaction();
        command.Transaction = transaction;
        foreach (var table in tables)
        {
            command.CommandText = $"UPDATE [{table}] SET [name]='Dystopian' WHERE [name]='Postapocalypse'";
            command.ExecuteNonQuery();
        }
        transaction.Commit();
        CheckIntegrity();
    }

    private static void AddXxxTables(SqliteConnection connection)
    {
        using var stream = typeof(CatalogDatabase).Assembly.GetManifestResourceStream("Ariadna.Storage.XxxSchema.sql")
            ?? throw new InvalidOperationException("The XXX schema resource is missing.");
        using var reader = new StreamReader(stream);
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = reader.ReadToEnd();
        command.ExecuteNonQuery();
        transaction.Commit();
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

    public void ResetPortraitStorage(string backupPath)
    {
        if (!File.Exists(backupPath) || System.IO.Path.GetFullPath(backupPath).Equals(path, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A separate verified pre-reset catalog backup is required.", nameof(backupPath));
        }
        using var writeLock = AcquireWriteLock();
        using var connection = Connect(path, SqliteOpenMode.ReadWrite);
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA application_id";
        if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != ApplicationId)
        {
            throw new InvalidDataException("This file is not an Ariadna catalog.");
        }
        command.CommandText = "PRAGMA user_version";
        if (Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) != 1)
        {
            throw new InvalidDataException("Portrait reset requires a schema-1 catalog and can run only once.");
        }
        command.CommandText = "SELECT COUNT(*) FROM CatalogAssetCommit";
        if ((long)command.ExecuteScalar()! != 0 || Directory.GetDirectories(DirectoryPath, ".ariadna-save-*").Length != 0)
        {
            throw new InvalidOperationException("Recover outstanding image saves with the previous application before changing the schema.");
        }

        command.Parameters.AddWithValue("$backup", backupPath);
        command.CommandText = "ATTACH DATABASE $backup AS baseline";
        command.ExecuteNonQuery();
        command.Parameters.Clear();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
        var tables = new List<string>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
        }
        foreach (var table in tables.Append("sqlite_sequence"))
        {
            VerifyResetTable(command, table, "*");
        }

        command.CommandText = "PRAGMA foreign_keys=OFF";
        command.ExecuteNonQuery();
        using var transaction = connection.BeginTransaction();
        command.Transaction = transaction;
        command.CommandText = "CREATE TABLE Person (Id INTEGER PRIMARY KEY AUTOINCREMENT, tmdbId INTEGER UNIQUE CHECK (tmdbId > 0))";
        command.ExecuteNonQuery();
        long offset = 0;
        foreach (var table in (string[])["Actor", "Director", "Author"])
        {
            command.CommandText = $"SELECT COALESCE(MAX(Id),-1) FROM [{table}]";
            var maximumId = (long)command.ExecuteScalar()!;
            command.CommandText = $"SELECT seq FROM sqlite_sequence WHERE name='{table}'";
            var sequence = command.ExecuteScalar() is long value ? value : 0;
            command.CommandText = $"""
                INSERT INTO Person(Id) SELECT Id+{offset} FROM [{table}] ORDER BY Id;
                CREATE TABLE [{table}_new] (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL COLLATE ARIADNA CHECK (length(name) <= 50),
                    personId INTEGER NOT NULL REFERENCES Person(Id));
                INSERT INTO [{table}_new](Id,name,personId) SELECT Id,name,Id+{offset} FROM [{table}] ORDER BY Id;
                DROP TABLE [{table}];
                ALTER TABLE [{table}_new] RENAME TO [{table}];
                CREATE INDEX [IX_{table}_Id] ON [{table}](Id);
                CREATE INDEX [IX_{table}_name] ON [{table}](name);
                CREATE INDEX [IX_{table}_personId] ON [{table}](personId);
                UPDATE sqlite_sequence SET seq=MAX(seq,{sequence}) WHERE name='{table}';
                INSERT INTO sqlite_sequence(name,seq) SELECT '{table}',{sequence} WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name='{table}');
                """;
            command.ExecuteNonQuery();
            offset += maximumId + 1;
        }
        command.CommandText = "PRAGMA foreign_key_check";
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                throw new InvalidDataException("Portrait reset would break catalog relationships.");
            }
        }
        command.CommandText = "PRAGMA user_version=2";
        command.ExecuteNonQuery();
        foreach (var table in tables)
        {
            VerifyResetTable(command, table, table is "Actor" or "Author" or "Director" ? "Id,name" : "*");
        }
        command.CommandText = "SELECT EXISTS(SELECT name,seq FROM baseline.sqlite_sequence EXCEPT SELECT name,seq FROM sqlite_sequence)";
        if ((long)command.ExecuteScalar()! != 0)
        {
            throw new InvalidDataException("Portrait reset changed an existing identity counter.");
        }
        transaction.Commit();
        command.Transaction = null;
        // Rebuild the file so removed portrait bytes do not remain in free pages.
        command.CommandText = "VACUUM";
        command.ExecuteNonQuery();
        command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE)";
        if ((long)command.ExecuteScalar()! != 0)
        {
            throw new IOException("Portrait schema reset committed, but WAL cleanup was blocked. Close catalog readers and checkpoint before continuing.");
        }
        CheckIntegrity();
    }

    private static void VerifyResetTable(SqliteCommand command, string table, string columns)
    {
        command.CommandText = $"SELECT EXISTS(SELECT {columns} FROM [{table}] EXCEPT SELECT {columns} FROM baseline.[{table}]) OR EXISTS(SELECT {columns} FROM baseline.[{table}] EXCEPT SELECT {columns} FROM [{table}])";
        if ((long)command.ExecuteScalar()! != 0)
        {
            throw new InvalidDataException($"Portrait reset comparison failed for '{table}'.");
        }
        command.CommandText = $"SELECT (SELECT COUNT(*) FROM [{table}])=(SELECT COUNT(*) FROM baseline.[{table}])";
        if ((long)command.ExecuteScalar()! == 0)
        {
            throw new InvalidDataException($"Portrait reset row count changed for '{table}'.");
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
