using System.Drawing;

namespace Ariadna.Themes;

internal class ThemeLibrary : Theme
{
    public override void Init()
    {
        SplashScreenForeColor = Color.Olive;

        MainBackColor = Color.Olive;
        MainForeColor = Color.White;
        ControlsBackColor = Color.Olive;

        DetailsFormBackColor = Color.FromArgb(70, 35, 0);
        DetailsFormForeColor = Color.White;
        DetailsFormForeColorDimmed = Color.LightGray;
        DetailsFormConfirmBtnBackColor = Color.FromArgb(70, 35, 0);
        DetailsFormHighlightForeColor = Color.Gold;

        ListViewForeColor = Color.White;
        ListViewGradFromColor = Color.Olive;
        ListViewGradToColor = Color.Black;
        ListViewItemBgFromColor = Color.White;
        ListViewItemBgToColor = Color.FromArgb(16, 16, 16);
        ListViewItemBorderTickColor = Color.White;
        ListViewItemBorderTuckColor = Color.Gray;

        FloatingPanelBackColor = Color.FromArgb(70, 35, 0);
        FloatingPanelForeColor = Color.White;
    }
}