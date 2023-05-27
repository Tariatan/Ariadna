using System.Drawing;

namespace Ariadna.Themes
{
    public abstract class Theme
    {
        public static Color SplashScreenForeColor;

        public static Color MainBackColor;
        public static Color MainForeColor;
        public static Color ControlsBackColor;

        public static Color DetailsFormBackColor;
        public static Color DetailsFormForeColor;
        public static Color DetailsFormForeColorDimmed;
        public static Color DetailsFormConfirmBtnBackColor;
        public static Color DetailsFormHighlightForeColor;

        public static Color ListViewForeColor;
        public static Color ListViewGradFromColor;
        public static Color ListViewGradToColor;
        public static Color ListViewItemBgFromColor;
        public static Color ListViewItemBgToColor;
        public static Color ListViewItemBorderTickColor;
        public static Color ListViewItemBorderTuckColor;

        public static Color FloatingPanelBackColor;
        public static Color FloatingPanelForeColor;

        public abstract void Init();
    }
}
