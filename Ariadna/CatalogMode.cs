#nullable enable
using Ariadna.Storage;

namespace Ariadna;

internal static class CatalogMode
{
    internal static CatalogKind? Parse(string? argument) => argument?.ToLowerInvariant() switch
    {
        "movies" => CatalogKind.Movie,
        "documentaries" => CatalogKind.Documentary,
        "games" => CatalogKind.Game,
        "library" => CatalogKind.Library,
        _ => null,
    };
}
