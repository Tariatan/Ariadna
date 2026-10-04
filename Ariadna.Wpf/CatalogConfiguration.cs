using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Ariadna.Storage;
using Microsoft.Data.Sqlite;

namespace Ariadna.Wpf;
internal sealed class CatalogConfiguration
{
    private readonly Dictionary<string, string> settings;
    internal CatalogConfiguration(string file, bool useEnvironment = true)
    {
        var document = XDocument.Load(file);
        settings = document.Descendants("setting").Where(setting => setting.Attribute("name") != null).ToDictionary(setting => (string)setting.Attribute("name")!, setting => setting.Element("value")?.Value ?? string.Empty);
        var connection = document.Descendants("connectionStrings").Elements("add").SingleOrDefault(element => (string? )element.Attribute("name") == "AriadnaCatalog") ?? throw new InvalidDataException("The AriadnaCatalog connection setting is missing.");
        DatabasePath = (useEnvironment ? Environment.GetEnvironmentVariable("ARIADNA_CATALOG_PATH") : null) ?? new SqliteConnectionStringBuilder((string? )connection.Attribute("connectionString")).DataSource;
        AssetRoot = useEnvironment ? Environment.GetEnvironmentVariable("ARIADNA_ASSET_ROOT") : null;
    }

    internal string DatabasePath { get; }
    internal string? AssetRoot { get; }
    internal string PreviewPrefix => Get("PreviewSuffix")is { Length: > 0 } prefix ? prefix : "_preview";

    internal string Get(string name) => settings.TryGetValue(name, out var value) ? value : string.Empty;
    internal int GetInt(string name, int fallback) => int.TryParse(Get(name), CultureInfo.InvariantCulture, out var value) ? value : fallback;
    internal string PreviewSuffix(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 4);
        return $"{PreviewPrefix}{index}";
    }

    internal string PosterRoot(CatalogKind kind)
    {
        var(directory, setting) = kind switch
        {
            CatalogKind.Movie => ("movies", "MoviePostersRootPath"),
            CatalogKind.Game => ("games", "GamePostersRootPath"),
            CatalogKind.Library => ("library", "LibraryPostersRootPath"),
            CatalogKind.Documentary => ("documentary", "DocumentaryPostersRootPath"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        return AssetRoot == null ? Get(setting) : Path.Combine(AssetRoot, directory);
    }

    internal string DiscoveryRoot(CatalogKind kind) => Get(kind switch
    {
        CatalogKind.Movie => "DefaultMoviesPath",
        CatalogKind.Game => "DefaultGamesPath",
        CatalogKind.Library => "DefaultLibraryPath",
        CatalogKind.Documentary => "DefaultDoocumentariesPath",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    });
    internal CatalogQuery Query(CatalogQuery query) => query with
    {
        RecentMonths = GetInt("RecentInMonth", 3),
        SeriesDrive = Get("DefaultSeriesPath").Length > 0 ? Get("DefaultSeriesPath")[..1] : string.Empty,
        MoviesDrive = Get("DefaultMoviesPath").Length > 0 ? Get("DefaultMoviesPath")[..1] : string.Empty,
        TemporaryMoviesPath = Get("DefaultMoviesPathTMP2"),
    };
}
