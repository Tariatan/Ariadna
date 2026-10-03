using System.Diagnostics;
using System.Text.Json;
using Ariadna.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Ariadna.Migration;

internal sealed class CatalogComparison(ILogger logger)
{
    internal void Run(string sourceConnection, string catalog, string reportPath)
    {
        var store = new CatalogStore(new CatalogDatabase(catalog));
        using var source = new SqlConnection(sourceConnection);
        source.Open();
        var results = new List<object>();
        foreach (var kind in Enum.GetValues<CatalogKind>())
        {
            var table = kind.ToString();
            var wanted = kind == CatalogKind.Game ? "want_to_play" : "want_to_see";
            var now = DateTime.Now;
            Check(kind, "all", new CatalogQuery(), "1=1", []);
            foreach (var term in new[] { "Р°", "Рђ", "The", "Р·РІС‘Р·Рґ", "_", "%", "Г©" })
            {
                Check(kind, "name:" + term, new CatalogQuery { Name = term }, "(CHARINDEX(UPPER(@name),UPPER(title))>0 OR CHARINDEX(UPPER(@name),UPPER(title_original))>0 OR CHARINDEX(UPPER(@name),UPPER(file_path))>0)", [("@name", term)]);
            }
            Check(kind, "wanted", new CatalogQuery { Wish = true }, $"{wanted}=1", []);
            Check(kind, "recent", new CatalogQuery { Recent = true, Now = now }, "creation_time>@recent", [("@recent", now.AddMonths(kind == CatalogKind.Game ? -6 : -3))]);
            Check(kind, "new", new CatalogQuery { New = true, Now = now }, "year IN (@year,@previousYear)", [("@year", now.Year), ("@previousYear", now.Year - 1)]);
            if (kind == CatalogKind.Game)
            {
                Check(kind, "vr", new CatalogQuery { Vr = true }, "vr=1", []);
                Check(kind, "non-vr", new CatalogQuery { NonVr = true }, "vr=0", []);
            }

            var genreTable = kind switch
            {
                CatalogKind.Movie => "Genre",
                CatalogKind.Documentary => "GenreOfDocumentary",
                CatalogKind.Game => "GenreOfGame",
                _ => "GenreOfLibrary",
            };
            var relation = table + "Genre";
            var key = table.ToLowerInvariant() + "Id";
            using var genreCommand = source.CreateCommand();
            genreCommand.CommandText = $"SELECT TOP(1) g.name FROM dbo.[{genreTable}] g JOIN dbo.[{relation}] r ON r.genreId=g.Id GROUP BY g.Id,g.name ORDER BY COUNT(*) DESC,g.Id";
            if (genreCommand.ExecuteScalar() is string genre)
            {
                Check(kind, "genre", new CatalogQuery { Genre = genre }, $"EXISTS(SELECT 1 FROM [{relation}] r JOIN [{genreTable}] g ON g.Id=r.genreId WHERE r.[{key}]=e.Id AND g.name=@genre)", [("@genre", genre)]);
            }

            void Check(CatalogKind category, string label, CatalogQuery query, string condition, (string Name, object Value)[] parameters)
            {
                using var command = source.CreateCommand();
                command.CommandText = $"SELECT Id FROM dbo.[{table}] e WHERE {condition} ORDER BY title,Id";
                foreach (var (name, value) in parameters)
                {
                    command.Parameters.AddWithValue(name, value);
                }
                var stopwatch = Stopwatch.StartNew();
                var expected = new List<int>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expected.Add(reader.GetInt32(0));
                    }
                }
                var sqlMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();
                var actual = store.Query(category, query).Select(entry => entry.Id).ToArray();
                var sqliteMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                if (!expected.Order().SequenceEqual(actual.Order()))
                {
                    throw new InvalidDataException($"Query membership mismatch: '{category}', '{label}'.");
                }
                var orderDifferences = label == "all" ? expected.Where((id, index) => actual[index] != id).Count() : (int?)null;
                results.Add(new { Collection = category.ToString(), Query = label, Rows = actual.Length, SqlMilliseconds = sqlMilliseconds, SqliteMilliseconds = sqliteMilliseconds, AlphabeticalPositionDifferences = orderDifferences });
            }
        }
        File.WriteAllText(reportPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        logger.LogInformation("Compared '{Count}' real-data queries; all result memberships match SQL Server", results.Count);
    }
}
