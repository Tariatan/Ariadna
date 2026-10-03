using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Ariadna.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Ariadna.Migration;

internal sealed class CatalogTransfer(ILogger logger)
{
    private static readonly string[] tableNames = ["Actor", "Author", "Director", "Genre", "GenreOfDocumentary", "GenreOfGame", "GenreOfLibrary", "Documentary", "Game", "Library", "Movie", "Ignore", "DocumentaryGenre", "GameGenre", "LibraryAuthor", "LibraryGenre", "MovieCast", "MovieDirector", "MovieGenre"];
    private static readonly string[] assetDirectories = ["movies", "documentary", "games", "library"];

    internal void Export(string connectionString, string assetRoot, string destination)
    {
        if (Directory.Exists(destination))
        {
            throw new IOException("The export destination already exists. Choose a new directory.");
        }
        Directory.CreateDirectory(destination);
        Directory.CreateDirectory(Path.Combine(destination, "tables"));
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
        using (var inventory = connection.CreateCommand())
        {
            inventory.Transaction = transaction;
            inventory.CommandText = "SELECT name FROM sys.tables WHERE is_ms_shipped=0";
            using var reader = inventory.ExecuteReader();
            var actual = new HashSet<string>();
            while (reader.Read())
            {
                actual.Add(reader.GetString(0));
            }
            if (!actual.SetEquals(tableNames))
            {
                throw new InvalidDataException("The source schema contains missing or unexpected tables. Migration must be reviewed before exporting.");
            }
        }
        var tables = new List<ExportTable>();
        foreach (var table in tableNames)
        {
            using var metadata = connection.CreateCommand();
            metadata.Transaction = transaction;
            metadata.CommandText = "SELECT c.name,TYPE_NAME(c.user_type_id),c.is_nullable,c.max_length FROM sys.columns c WHERE c.object_id=OBJECT_ID(@table) ORDER BY c.column_id";
            metadata.Parameters.AddWithValue("@table", $"dbo.{table}");
            var columns = new List<ExportColumn>();
            using (var reader = metadata.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(1);
                    if (type is not ("int" or "bit" or "nvarchar" or "image" or "date"))
                    {
                        throw new NotSupportedException($"Unsupported source type '{type}' in '{table}'.");
                    }
                    columns.Add(new ExportColumn(reader.GetString(0), type, reader.GetBoolean(2), reader.GetInt16(3)));
                }
            }
            if (columns.Count == 0)
            {
                throw new InvalidDataException($"Missing source table '{table}'.");
            }

            using var query = connection.CreateCommand();
            query.Transaction = transaction;
            query.CommandText = $"SELECT {string.Join(",", columns.Select(column => $"[{column.Name}]"))} FROM dbo.[{table}] ORDER BY Id";
            var file = Path.Combine(destination, "tables", table + ".jsonl");
            long rows = 0;
            using (var writer = new StreamWriter(file, false, new System.Text.UTF8Encoding(false)))
            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    var values = new object?[columns.Count];
                    foreach (var (column, index) in columns.Select((column, index) => (column, index)))
                    {
                        values[index] = reader.IsDBNull(index) ? null : column.Type switch
                        {
                            "int" => (long)reader.GetInt32(index),
                            "bit" => reader.GetBoolean(index) ? 1L : 0L,
                            "date" => reader.GetDateTime(index).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                            "image" => Convert.ToBase64String((byte[])reader.GetValue(index)),
                            _ => reader.GetString(index),
                        };
                    }
                    writer.WriteLine(JsonSerializer.Serialize(values));
                    rows++;
                }
            }

            metadata.Parameters.Clear();
            metadata.CommandText = "SELECT CONVERT(bigint,last_value) FROM sys.identity_columns WHERE object_id=OBJECT_ID(@table)";
            metadata.Parameters.AddWithValue("@table", $"dbo.{table}");
            var identity = metadata.ExecuteScalar() is long value ? value : 0;
            tables.Add(new ExportTable(table, columns.ToArray(), identity, rows, Hash(file)));
            logger.LogInformation("Exported '{Table}', rows: '{Rows}'", table, rows);
        }
        transaction.Commit();

        var assets = CopyAssets(assetRoot, destination);
        WriteManifest(destination, tables, assets);
    }

    internal void Snapshot(string catalog, string assetRoot, string destination)
    {
        if (System.Diagnostics.Process.GetProcessesByName("Ariadna").Length != 0)
        {
            throw new InvalidOperationException("Close all Ariadna windows before capturing a matched catalog and image backup.");
        }
        if (Directory.Exists(destination))
        {
            throw new IOException("Choose a new snapshot directory.");
        }
        Directory.CreateDirectory(destination);
        Directory.CreateDirectory(Path.Combine(destination, "tables"));
        var snapshot = Path.Combine(destination, "catalog.sqlite");
        var database = new CatalogDatabase(catalog);
        using var writeLock = database.AcquireWriteLock();
        database.RecoverAssetsWhileLocked();
        database.Backup(snapshot);
        var tables = new List<ExportTable>();
        using var connection = new CatalogDatabase(snapshot).Open(true);
        foreach (var table in tableNames)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info([{table}])";
            var columns = new List<ExportColumn>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader.GetString(1);
                    var type = reader.GetString(2) switch
                    {
                        "INTEGER" => name is "vr" or "want_to_see" or "want_to_play" ? "bit" : "int",
                        "BLOB" => "image",
                        _ => name == "creation_time" ? "date" : "nvarchar",
                    };
                    columns.Add(new ExportColumn(name, type, !reader.GetBoolean(3) && name != "Id", -1));
                }
            }
            command.CommandText = $"SELECT {string.Join(",", columns.Select(column => $"[{column.Name}]"))} FROM [{table}] ORDER BY Id";
            var file = Path.Combine(destination, "tables", table + ".jsonl");
            long rows = 0;
            using (var writer = new StreamWriter(file, false, new System.Text.UTF8Encoding(false)))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var values = columns.Select((column, index) => reader.IsDBNull(index) ? null : column.Type == "image" ? Convert.ToBase64String((byte[])reader.GetValue(index)) : reader.GetValue(index)).ToArray();
                    writer.WriteLine(JsonSerializer.Serialize(values));
                    rows++;
                }
            }
            command.CommandText = "SELECT seq FROM sqlite_sequence WHERE name=$table";
            command.Parameters.AddWithValue("$table", table);
            var identity = command.ExecuteScalar() is long value ? value : 0;
            tables.Add(new ExportTable(table, columns.ToArray(), identity, rows, Hash(file)));
        }
        var assets = CopyAssets(assetRoot, destination);
        WriteManifest(destination, tables, assets);
        Verify(destination, snapshot);
    }

    private static List<ExportAsset> CopyAssets(string assetRoot, string destination)
    {
        var assets = new List<ExportAsset>();
        foreach (var directory in assetDirectories)
        {
            foreach (var source in Directory.EnumerateFiles(Path.Combine(assetRoot, directory), "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(assetRoot, source);
                var target = Path.Combine(destination, "assets", relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target);
                var hash = Hash(source);
                if (Hash(target) != hash)
                {
                    throw new InvalidDataException($"Asset copy mismatch: '{relative}'.");
                }
                assets.Add(new ExportAsset(relative, new FileInfo(source).Length, hash));
            }
        }

        return assets;
    }

    private void WriteManifest(string destination, List<ExportTable> tables, List<ExportAsset> assets)
    {
        var manifest = new ExportManifest(1, DateTime.UtcNow, tables.ToArray(), assets.ToArray());
        File.WriteAllText(Path.Combine(destination, "manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        logger.LogInformation("Finished export, rows: '{Rows}', assets: '{Assets}'", tables.Sum(table => table.Rows), assets.Count);
    }

    internal void Import(string package, string destination)
    {
        if (!Path.IsPathFullyQualified(destination) || File.Exists(destination))
        {
            throw new IOException("Choose a new absolute catalog filename; existing catalogs are never replaced.");
        }
        var staging = destination + ".import-" + Guid.NewGuid().ToString("N");
        var manifest = ReadManifest(package);
        VerifyPackage(package, manifest);
        CatalogDatabase.Create(staging);
        var database = new CatalogDatabase(staging);
        using (var connection = database.Open())
        using (var transaction = connection.BeginTransaction())
        {
            foreach (var table in manifest.Tables)
            {
                VerifySchema(connection, transaction, table);
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"INSERT INTO [{table.Name}]({string.Join(",", table.Columns.Select(column => $"[{column.Name}]"))}) VALUES ({string.Join(",", table.Columns.Select((_, index) => $"$p{index}"))})";
                foreach (var (_, index) in table.Columns.Select((column, index) => (column, index)))
                {
                    command.Parameters.Add(new SqliteParameter($"$p{index}", DBNull.Value));
                }
                command.Prepare();
                foreach (var line in File.ReadLines(Path.Combine(package, "tables", table.Name + ".jsonl")))
                {
                    using var record = JsonDocument.Parse(line);
                    var values = record.RootElement.EnumerateArray().ToArray();
                    if (values.Length != table.Columns.Length)
                    {
                        throw new InvalidDataException($"Column count mismatch in '{table.Name}'.");
                    }
                    foreach (var (column, index) in table.Columns.Select((column, index) => (column, index)))
                    {
                        command.Parameters[index].Value = Decode(values[index], column.Type) ?? DBNull.Value;
                    }
                    command.ExecuteNonQuery();
                }
                command.Parameters.Clear();
                command.CommandText = "UPDATE sqlite_sequence SET seq=MAX(seq,$identity) WHERE name=$table";
                command.Parameters.AddWithValue("$identity", table.Identity);
                command.Parameters.AddWithValue("$table", table.Name);
                command.ExecuteNonQuery();
                if (table.Rows == 0)
                {
                    command.CommandText = "INSERT INTO sqlite_sequence(name,seq) SELECT $table,$identity WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name=$table)";
                    command.ExecuteNonQuery();
                }
            }
            transaction.Commit();
        }
        Verify(package, staging);
        File.Move(staging, destination);
    }

    internal void Verify(string package, string destination)
    {
        var manifest = ReadManifest(package);
        VerifyPackage(package, manifest);
        var database = new CatalogDatabase(destination);
        database.CheckIntegrity();
        using var connection = database.Open(true);
        foreach (var table in manifest.Tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT {string.Join(",", table.Columns.Select(column => $"[{column.Name}]"))} FROM [{table.Name}] ORDER BY Id";
            using var reader = command.ExecuteReader();
            long rows = 0;
            foreach (var line in File.ReadLines(Path.Combine(package, "tables", table.Name + ".jsonl")))
            {
                if (!reader.Read())
                {
                    throw new InvalidDataException($"Missing rows in '{table.Name}'.");
                }
                using var record = JsonDocument.Parse(line);
                var values = record.RootElement.EnumerateArray().ToArray();
                foreach (var (column, index) in table.Columns.Select((column, index) => (column, index)))
                {
                    var expected = Decode(values[index], column.Type);
                    var actual = reader.IsDBNull(index) ? null : reader.GetValue(index);
                    var equal = expected is byte[] bytes ? actual is byte[] actualBytes && bytes.AsSpan().SequenceEqual(actualBytes) : Equals(expected, actual);
                    if (!equal)
                    {
                        throw new InvalidDataException($"Value mismatch in '{table.Name}.{column.Name}', row '{rows + 1}'.");
                    }
                }
                rows++;
            }
            if (reader.Read() || rows != table.Rows)
            {
                throw new InvalidDataException($"Row count mismatch in '{table.Name}'.");
            }
            reader.Dispose();
            command.CommandText = "SELECT seq FROM sqlite_sequence WHERE name=$table";
            command.Parameters.AddWithValue("$table", table.Name);
            if (Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) < table.Identity)
            {
                throw new InvalidDataException($"Identity high-water mark mismatch in '{table.Name}'.");
            }
            logger.LogInformation("Verified every value in '{Table}', rows: '{Rows}'", table.Name, rows);
        }
        logger.LogInformation("Catalog verification passed, SQLite: '{Version}', rows: '{Rows}', assets: '{Assets}'", GetVersion(connection), manifest.Tables.Sum(table => table.Rows), manifest.Assets.Length);
    }

    private static object? Decode(JsonElement value, string type) => value.ValueKind == JsonValueKind.Null ? null : type switch
    {
        "int" or "bit" => value.GetInt64(),
        "image" => value.GetBytesFromBase64(),
        _ => value.GetString(),
    };

    private static ExportManifest ReadManifest(string package)
    {
        var manifest = JsonSerializer.Deserialize<ExportManifest>(File.ReadAllText(Path.Combine(package, "manifest.json")))
            ?? throw new InvalidDataException("Missing export manifest.");
        if (manifest.FormatVersion != 1 || !manifest.Tables.Select(table => table.Name).SequenceEqual(tableNames))
        {
            throw new InvalidDataException("Unsupported or incomplete export package.");
        }
        foreach (var table in manifest.Tables)
        {
            if (table.Columns.Any(column => column.Name.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_')))
            {
                throw new InvalidDataException("Invalid column identifier in export package.");
            }
        }
        return manifest;
    }

    private static void VerifyPackage(string package, ExportManifest manifest)
    {
        foreach (var table in manifest.Tables)
        {
            if (Hash(Path.Combine(package, "tables", table.Name + ".jsonl")) != table.Sha256)
            {
                throw new InvalidDataException($"Export table hash mismatch: '{table.Name}'.");
            }
        }
        foreach (var asset in manifest.Assets)
        {
            var root = Path.GetFullPath(Path.Combine(package, "assets")) + Path.DirectorySeparatorChar;
            var path = Path.GetFullPath(Path.Combine(root, asset.Path));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || new FileInfo(path).Length != asset.Bytes || Hash(path) != asset.Sha256)
            {
                throw new InvalidDataException("Export asset hash or path mismatch.");
            }
        }
    }

    private static void VerifySchema(SqliteConnection connection, SqliteTransaction transaction, ExportTable table)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"PRAGMA table_info([{table.Name}])";
        using var reader = command.ExecuteReader();
        var index = 0;
        while (reader.Read())
        {
            if (index >= table.Columns.Length || reader.GetString(1) != table.Columns[index].Name)
            {
                throw new InvalidDataException($"Source/target schema mismatch in '{table.Name}'.");
            }
            var source = table.Columns[index];
            var type = source.Type is "int" or "bit" ? "INTEGER" : source.Type == "image" ? "BLOB" : "TEXT";
            if (reader.GetString(2) != type || (source.Name != "Id" && reader.GetBoolean(3) == source.Nullable))
            {
                throw new InvalidDataException($"Type/nullability mismatch in '{table.Name}.{source.Name}'.");
            }
            index++;
        }
        if (index != table.Columns.Length)
        {
            throw new InvalidDataException($"Column count mismatch in '{table.Name}'.");
        }
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string GetVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sqlite_version()";
        return (string)command.ExecuteScalar()!;
    }
}
