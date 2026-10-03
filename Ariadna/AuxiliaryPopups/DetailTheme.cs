#nullable enable
using System.Windows.Forms;
using Ariadna.Themes;

namespace Ariadna.AuxiliaryPopups;

internal static class DetailTheme
{
    internal static void Apply(Control control)
    {
        control.BackColor = Theme.DetailsFormBackColor;
        control.ForeColor = Theme.DetailsFormForeColor;
        if (control is Button)
        {
            control.BackColor = Theme.DetailsFormConfirmBtnBackColor;
        }
        else if (control is CheckBox)
        {
            control.ForeColor = Theme.DetailsFormHighlightForeColor;
        }
        foreach (Control child in control.Controls)
        {
            Apply(child);
        }
    }
}
