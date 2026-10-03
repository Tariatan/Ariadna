#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace Ariadna;

internal sealed class CatalogTabControl : TabControl
{
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var page = TabPages[e.Index];
        var selected = e.Index == SelectedIndex;
        using var background = new SolidBrush(selected ? page.BackColor : Color.FromArgb(48, 48, 48));
        e.Graphics.FillRectangle(background, e.Bounds);
        var indicator = new Rectangle(e.Bounds.X, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
        using var accent = new SolidBrush(page.BackColor);
        e.Graphics.FillRectangle(accent, indicator);
        TextRenderer.DrawText(e.Graphics, page.Text, Font, e.Bounds, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if ((e.State & DrawItemState.Focus) != 0)
        {
            ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(e.Bounds, -6, -6), Color.White, background.Color);
        }
    }
}
