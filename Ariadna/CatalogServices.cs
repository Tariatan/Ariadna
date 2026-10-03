using System;
using System.Configuration;
using System.IO;
using Ariadna.Storage;
using Ariadna.DatabaseStrategies;
using Ariadna.Properties;

namespace Ariadna;

internal static class CatalogServices
{
    internal static CatalogDatabase CreateDatabase()
    {
        var path = Environment.GetEnvironmentVariable("ARIADNA_CATALOG_PATH");
        if (string.IsNullOrEmpty(path))
        {
            var connection = ConfigurationManager.ConnectionStrings["AriadnaCatalog"]
                ?? throw new InvalidDataException("The AriadnaCatalog connection setting is missing.");
            path = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connection.ConnectionString).DataSource;
        }
        return new CatalogDatabase(path);
    }

    internal static CatalogStore CreateStore() => new(CreateDatabase());

    internal static string GetPosterRoot(CatalogKind kind)
    {
        var root = Environment.GetEnvironmentVariable("ARIADNA_ASSET_ROOT");
        var directory = kind switch
        {
            CatalogKind.Movie => "movies",
            CatalogKind.Documentary => "documentary",
            CatalogKind.Game => "games",
            CatalogKind.Library => "library",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        if (!string.IsNullOrEmpty(root))
        {
            return Path.Combine(root, directory) + Path.DirectorySeparatorChar;
        }
        return kind switch
        {
            CatalogKind.Movie => Settings.Default.MoviePostersRootPath,
            CatalogKind.Documentary => Settings.Default.DocumentaryPostersRootPath,
            CatalogKind.Game => Settings.Default.GamePostersRootPath,
            CatalogKind.Library => Settings.Default.LibraryPostersRootPath,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    internal static CatalogQuery CreateQuery(AbstractDbStrategy.QueryParams values, bool library = false) => new()
    {
        Name = values.Name,
        Director = values.Director,
        Actor = values.Actor,
        Genre = library && !string.IsNullOrEmpty(values.Subgenre) && values.Subgenre != Utilities.EmptyDots ? values.Subgenre : values.Genre,
        Wish = values.IsWish,
        Recent = values.IsRecent,
        New = values.IsNew,
        Vr = values.IsVr,
        NonVr = values.IsNonVr,
        Series = values.IsSeries,
        Movies = values.IsMovies,
        RecentMonths = Settings.Default.RecentInMonth,
        SeriesDrive = Settings.Default.DefaultSeriesPath[..1],
        MoviesDrive = Settings.Default.DefaultMoviesPath[..1],
        TemporaryMoviesPath = Settings.Default.DefaultMoviesPathTMP2,
    };
}
