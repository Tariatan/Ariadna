namespace Ariadna.Storage;

public sealed record PersonPhoto(string Name, byte[]? Photo)
{
    public int Id { get; init; } = -1;
    public int PersonId { get; init; } = -1;
    public int? TmdbId { get; init; }
}
