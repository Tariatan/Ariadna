using Ariadna.Storage;

namespace Ariadna.Wpf;
internal sealed class PosterItem(CatalogEntry entry, CatalogKind kind, string imagePath, ThumbnailCache cache) : ObservableObject
{
    private bool selected;
    public CatalogEntry Entry { get; } = entry;
    public string ImagePath { get; } = imagePath;
    internal ThumbnailCache Cache { get; } = cache;
    public string Caption => kind == CatalogKind.Movie && !string.IsNullOrWhiteSpace(Entry.OriginalTitle) ? Entry.OriginalTitle : Entry.Title;
    public bool Selected { get => selected; set => Set(ref selected, value); }
}
