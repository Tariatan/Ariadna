#nullable enable
using System.Windows.Forms;
using Ariadna.Themes;

namespace Ariadna.AuxiliaryPopups;

internal static class DetailTheme
{
    internal static void Apply(Control control, Theme theme)
    {
        control.BackColor = theme.DetailsFormBackColor;
        control.ForeColor = theme.DetailsFormForeColor;
        if (control is Button)
        {
            control.BackColor = theme.DetailsFormConfirmBtnBackColor;
        }
        else if (control is CheckBox)
        {
            control.ForeColor = theme.DetailsFormHighlightForeColor;
        }
        if (control is GenreSelectionControl genres)
        {
            genres.ApplyPickerTheme(theme);
        }
        foreach (Control child in control.Controls)
        {
            Apply(child, theme);
        }
    }
}
