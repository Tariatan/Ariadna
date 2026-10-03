namespace Ariadna.Storage;

public sealed record CatalogDetails(
    CatalogEntry Entry,
    IReadOnlyCollection<string> Genres,
    IReadOnlyCollection<PersonPhoto> Directors,
    IReadOnlyCollection<PersonPhoto> Actors);
