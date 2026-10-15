using System.Windows.Media;
using Ariadna.Storage;

namespace Ariadna.Wpf;
internal sealed record CatalogTheme(string Caption, Brush Background, Brush EditorBackground)
{
    internal static CatalogTheme For(CatalogKind kind) => kind switch
    {
        CatalogKind.Movie => new("Movies", Brushes.Purple, new SolidColorBrush(Color.FromRgb(64, 0, 64))),
        CatalogKind.Game => new("Games", new SolidColorBrush(Color.FromRgb(98, 35, 3)), new SolidColorBrush(Color.FromRgb(70, 35, 0))),
        CatalogKind.Library => new("Library", Brushes.Olive, new SolidColorBrush(Color.FromRgb(64, 64, 0))),
        CatalogKind.Documentary => new("Documentaries", Brushes.SteelBlue, new SolidColorBrush(Color.FromRgb(35, 65, 90))),
        CatalogKind.Xxx => new("XXX", new SolidColorBrush(Color.FromRgb(112, 0, 57)), new SolidColorBrush(Color.FromRgb(28, 0, 14))),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
