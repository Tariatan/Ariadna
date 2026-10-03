namespace Ariadna.Storage;

public sealed record CatalogQuery
{
    public string? Name { get; init; }
    public string? Director { get; init; }
    public string? Actor { get; init; }
    public string? Genre { get; init; }
    public bool Wish { get; init; }
    public bool Recent { get; init; }
    public bool New { get; init; }
    public bool Vr { get; init; }
    public bool NonVr { get; init; }
    public bool Series { get; init; }
    public bool Movies { get; init; }
    public int RecentMonths { get; init; } = 3;
    public string SeriesDrive { get; init; } = string.Empty;
    public string MoviesDrive { get; init; } = string.Empty;
    public string TemporaryMoviesPath { get; init; } = string.Empty;
    public DateTime Now { get; init; } = DateTime.Now;
}
