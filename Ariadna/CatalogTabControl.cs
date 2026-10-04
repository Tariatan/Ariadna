#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace Ariadna;

internal sealed class CatalogTabControl : TabControl
{
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var page = TabPages[e.Index];
        using var background = new SolidBrush(page.BackColor);
        e.Graphics.FillRectangle(background, e.Bounds);

        TextRenderer.DrawText(e.Graphics, page.Text, Font, e.Bounds, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
