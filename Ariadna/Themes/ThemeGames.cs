using System.Drawing;

namespace Ariadna.Themes;

internal class ThemeGames : Theme
{
    public override void Init()
    {
        SplashScreenForeColor = Color.FromArgb(182, 57, 35);

        MainBackColor = Color.FromArgb(98, 35, 3);
        MainForeColor = Color.White;
        ControlsBackColor = Color.FromArgb(104, 32, 1);

        DetailsFormBackColor = Color.FromArgb(70, 35, 0);
        DetailsFormForeColor = Color.White;
        DetailsFormForeColorDimmed = Color.LightGray;
        DetailsFormConfirmBtnBackColor = Color.FromArgb(80, 0, 10);
        DetailsFormHighlightForeColor = Color.Gold;

        ListViewForeColor = Color.White;
        ListViewGradFromColor = MainBackColor;
        ListViewGradToColor = Color.Black;
        ListViewItemBgFromColor = Color.White;
        ListViewItemBgToColor = Color.FromArgb(36, 1, 0);
        ListViewItemBorderTickColor = Color.White;
        ListViewItemBorderTuckColor = Color.Gray;

        FloatingPanelBackColor = ControlsBackColor;
        FloatingPanelForeColor = Color.White;
    }
}