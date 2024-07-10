using System.Drawing;

namespace Ariadna.Themes
{
    class ThemeDocumentaries : Theme
    {
        public override void Init()
        {
            SplashScreenForeColor = Color.BlueViolet;

            MainBackColor = Color.BlueViolet;
            MainForeColor = Color.White;
            ControlsBackColor = Color.BlueViolet;

            DetailsFormBackColor = Color.FromArgb(64, 0, 64);
            DetailsFormForeColor = Color.White;
            DetailsFormForeColorDimmed = Color.LightGray;
            DetailsFormConfirmBtnBackColor = Color.FromArgb(64, 0, 64);
            DetailsFormHighlightForeColor = Color.Gold;

            ListViewForeColor = Color.White;
            ListViewGradFromColor = Color.BlueViolet;
            ListViewGradToColor = Color.Black;
            ListViewItemBgFromColor = Color.White;
            ListViewItemBgToColor = Color.FromArgb(16, 16, 16);
            ListViewItemBorderTickColor = Color.White;
            ListViewItemBorderTuckColor = Color.Gray;

            FloatingPanelBackColor = Color.BlueViolet;
            FloatingPanelForeColor = Color.White;

        }
    }
}
