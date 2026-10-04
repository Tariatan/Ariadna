#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace Ariadna;

internal sealed class CatalogTabControl : TabControl
{
    private const int WmPaint = 0x000F;
    private const int WmPrintClient = 0x0318;

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg is not (WmPaint or WmPrintClient) || TabCount == 0)
        {
            return;
        }

        var lastHeader = GetTabRect(TabCount - 1);
        var unusedHeader = Rectangle.FromLTRB(lastHeader.Right + 2, 0, ClientSize.Width, lastHeader.Bottom);
        if (unusedHeader.Width <= 0 || (m.Msg == WmPrintClient && m.WParam == 0))
        {
            return;
        }

        using var graphics = m.Msg == WmPrintClient
            ? Graphics.FromHdc(m.WParam)
            : Graphics.FromHwnd(Handle);
        graphics.FillRectangle(Brushes.Gray, unusedHeader);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var page = TabPages[e.Index];
        var backgroundBounds = e.Bounds;
        if (e.Index == SelectedIndex)
        {
            // The selected header expands into the native frame; keep its bottom joined to the page.
            backgroundBounds.Inflate(-4, -4);
            backgroundBounds.Height += 4;
        }
        using var background = new SolidBrush(page.BackColor);
        e.Graphics.FillRectangle(background, backgroundBounds);

        TextRenderer.DrawText(e.Graphics, page.Text, Font, e.Bounds, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
