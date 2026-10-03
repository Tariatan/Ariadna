using System;
using System.Drawing;
using Ariadna.Storage;

namespace Ariadna.Themes;

public abstract class Theme
{
    public Color SplashScreenForeColor { get; protected set; }

    public Color MainBackColor { get; protected set; }
    public Color MainForeColor { get; protected set; }
    public Color ControlsBackColor { get; protected set; }

    public Color DetailsFormBackColor { get; protected set; }
    public Color DetailsFormForeColor { get; protected set; }
    public Color DetailsFormForeColorDimmed { get; protected set; }
    public Color DetailsFormConfirmBtnBackColor { get; protected set; }
    public Color DetailsFormHighlightForeColor { get; protected set; }

    public Color ListViewForeColor { get; protected set; }
    public Color ListViewGradFromColor { get; protected set; }
    public Color ListViewGradToColor { get; protected set; }
    public Color ListViewItemBgFromColor { get; protected set; }
    public Color ListViewItemBgToColor { get; protected set; }
    public Color ListViewItemBorderTickColor { get; protected set; }
    public Color ListViewItemBorderTuckColor { get; protected set; }

    public Color FloatingPanelBackColor { get; protected set; }
    public Color FloatingPanelForeColor { get; protected set; }

    public static Theme Create(CatalogKind kind) => kind switch
    {
        CatalogKind.Movie => new ThemeMovies(),
        CatalogKind.Documentary => new ThemeDocumentaries(),
        CatalogKind.Game => new ThemeGames(),
        CatalogKind.Library => new ThemeLibrary(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
