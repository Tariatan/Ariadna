namespace Ariadna.Wpf;
internal sealed record MetadataChoice(int Id, bool Series, string Title, string OriginalTitle, int Year)
{
    public override string ToString() => $"{Title} / {OriginalTitle} ({Year})";
}
