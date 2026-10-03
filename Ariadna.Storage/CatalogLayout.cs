namespace Ariadna.Storage;

internal sealed record CatalogLayout(string Table, string GenreTable, string GenreRelation, string EntryKey, string WantedColumn)
{
    internal static CatalogLayout For(CatalogKind kind) => kind switch
    {
        CatalogKind.Movie => new("Movie", "Genre", "MovieGenre", "movieId", "want_to_see"),
        CatalogKind.Documentary => new("Documentary", "GenreOfDocumentary", "DocumentaryGenre", "documentaryId", "want_to_see"),
        CatalogKind.Game => new("Game", "GenreOfGame", "GameGenre", "gameId", "want_to_play"),
        CatalogKind.Library => new("Library", "GenreOfLibrary", "LibraryGenre", "libraryId", "want_to_see"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
