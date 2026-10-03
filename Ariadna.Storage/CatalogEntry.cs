namespace Ariadna.Storage;

public sealed record CatalogEntry
{
    public int Id { get; set; } = -1;
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? CreationDate { get; set; }
    public bool? Wanted { get; set; }
    public bool? Vr { get; set; }
    public string? Version { get; set; }
}
