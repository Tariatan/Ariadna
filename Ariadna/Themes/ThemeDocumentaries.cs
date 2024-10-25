using System.Drawing;

namespace Ariadna.Themes;

internal class ThemeDocumentaries : Theme
{
    public override void Init()
    {
        SplashScreenForeColor = Color.SteelBlue;

        MainBackColor = Color.SteelBlue;
        MainForeColor = Color.White;
        ControlsBackColor = Color.SteelBlue;

        DetailsFormBackColor = Color.FromArgb(64, 0, 64);
        DetailsFormForeColor = Color.White;
        DetailsFormForeColorDimmed = Color.LightGray;
        DetailsFormConfirmBtnBackColor = Color.FromArgb(64, 0, 64);
        DetailsFormHighlightForeColor = Color.Gold;

        ListViewForeColor = Color.White;
        ListViewGradFromColor = Color.SteelBlue;
        ListViewGradToColor = Color.Black;
        ListViewItemBgFromColor = Color.White;
        ListViewItemBgToColor = Color.FromArgb(16, 16, 16);
        ListViewItemBorderTickColor = Color.White;
        ListViewItemBorderTuckColor = Color.Gray;

        FloatingPanelBackColor = Color.SteelBlue;
        FloatingPanelForeColor = Color.White;
    }
}