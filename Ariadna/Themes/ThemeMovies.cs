using System.Drawing;

namespace Ariadna.Themes;

internal class ThemeMovies : Theme
{
    public override void Init()
    {
        SplashScreenForeColor = Color.DarkMagenta;

        MainBackColor = Color.Purple;
        MainForeColor = Color.White;
        ControlsBackColor = Color.DarkMagenta;

        DetailsFormBackColor = Color.FromArgb(64, 0, 64);
        DetailsFormForeColor = Color.White;
        DetailsFormForeColorDimmed = Color.LightGray;
        DetailsFormConfirmBtnBackColor = Color.FromArgb(75, 0, 75);
        DetailsFormHighlightForeColor = Color.Gold;

        ListViewForeColor = Color.White;
        ListViewGradFromColor = Color.Purple;
        ListViewGradToColor = Color.Black;
        ListViewItemBgFromColor = Color.White;
        ListViewItemBgToColor = Color.FromArgb(16, 16, 16);
        ListViewItemBorderTickColor = Color.White;
        ListViewItemBorderTuckColor = Color.Gray;

        FloatingPanelBackColor = Color.DarkMagenta;
        FloatingPanelForeColor = Color.White;
    }
}