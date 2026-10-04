using System.IO;
using System.Xml.Linq;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.Wpf.Tests;
internal sealed class CatalogFixture : IDisposable
{
    internal string Root { get; } = Path.Combine(Path.GetTempPath(), "Ariadna-wpf-test-" + Guid.NewGuid().ToString("N"));
    internal CatalogConfiguration Configuration { get; }
    internal CatalogStore Store { get; }
    internal CatalogActions Actions { get; }

    internal CatalogFixture(string previewSuffix = "_preview")
    {
        Directory.CreateDirectory(Root);
        var databasePath = Path.Combine(Root, "catalog.sqlite");
        CatalogDatabase.Create(databasePath);
        Store = new CatalogStore(new CatalogDatabase(databasePath));
        var settings = new Dictionary<string, string>
        {
            ["MoviePostersRootPath"] = Path.Combine(Root, "movies"),
            ["GamePostersRootPath"] = Path.Combine(Root, "games"),
            ["LibraryPostersRootPath"] = Path.Combine(Root, "library"),
            ["DocumentaryPostersRootPath"] = Path.Combine(Root, "documentary"),
            ["DefaultMoviesPath"] = Path.Combine(Root, "MOVIES"),
            ["DefaultSeriesPath"] = Path.Combine(Root, "SERIES"),
            ["DefaultMoviesPathTMP2"] = Path.Combine(Root, "TMP"),
            ["DefaultGamesPath"] = Path.Combine(Root, "GAMES"),
            ["DefaultGamesPathVR"] = Path.Combine(Root, "VR"),
            ["DefaultDoocumentariesPath"] = Path.Combine(Root, "DOCUMENTARIES"),
            ["DefaultLibraryPath"] = Path.Combine(Root, "LIBRARY"),
            ["MaxGenresCount"] = "4",
            ["PosterWidth"] = "80",
            ["PosterHeight"] = "120",
            ["PreviewWidth"] = "120",
            ["PreviewHeight"] = "70",
            ["PreviewSuffix"] = previewSuffix,
        };
        foreach (var setting in settings.Where(setting => setting.Key.EndsWith("Path", StringComparison.Ordinal) || setting.Key.EndsWith("TMP2", StringComparison.Ordinal)))
        {
            Directory.CreateDirectory(setting.Value);
        }

        var config = Path.Combine(Root, "app.config");
        new XDocument(new XElement("configuration", new XElement("connectionStrings", new XElement("add", new XAttribute("name", "AriadnaCatalog"), new XAttribute("connectionString", $"Data Source={databasePath}"))), new XElement("applicationSettings", new XElement("Ariadna.Properties.Settings", settings.Select(setting => new XElement("setting", new XAttribute("name", setting.Key), new XElement("value", setting.Value))))))).Save(config);
        Configuration = new CatalogConfiguration(config, false);
        Actions = new CatalogActions(Store, Configuration, NullLogger.Instance);
    }

    internal CatalogEntry Add(CatalogKind kind, string title, bool? wish = null, bool? vr = null)
    {
        var entry = new CatalogEntry
        {
            Title = title,
            OriginalTitle = "Original " + title,
            Path = Path.Combine(Root, title),
            Year = 2000,
            Wanted = wish,
            Vr = vr,
            CreationDate = new DateOnly(2020, 1, 2),
        };
        entry.Id = Store.Save(kind, new CatalogDetails(entry, ["Genre"], [], []));
        return entry;
    }

    internal void AddMany(int count)
    {
        using var connection = new CatalogDatabase(Configuration.DatabasePath).Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO Movie(title,title_original,file_path,year,want_to_see,creation_time) VALUES($title,'',$path,2000,$wanted,'2020-01-02')";
        var title = command.Parameters.AddWithValue("$title", string.Empty);
        var path = command.Parameters.AddWithValue("$path", string.Empty);
        var wanted = command.Parameters.AddWithValue("$wanted", 0);
        foreach (var index in Enumerable.Range(0, count))
        {
            title.Value = $"{(char)('A' + index % 26)} Movie {index:D4}";
            path.Value = Path.Combine(Root, $"media-{index}.mkv");
            wanted.Value = index % 3 == 0;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void Dispose()
    {
        Directory.Delete(Root, true);
    }
}
