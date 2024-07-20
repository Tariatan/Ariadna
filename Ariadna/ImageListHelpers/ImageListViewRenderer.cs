using System.Drawing;
using System.Drawing.Drawing2D;
using Ariadna.Themes;
using Manina.Windows.Forms;

namespace Ariadna.ImageListHelpers;

internal class ImageListViewAriadnaRenderer : ImageListView.ImageListViewRenderer
{
    private const int PAD_W = 20;
    private const int PAD_TOP = 12;
    private const int PAD_BTM = 6;
    private readonly Brush m_TextBrush = new SolidBrush(Theme.ListViewForeColor);
    private readonly StringFormat m_StringFormat = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
    };

    private readonly System.Windows.Forms.Timer m_BlinkTimer = new();
    private enum EBlinkState { NONE = 0, TICK, TUCK, }
    private EBlinkState m_BlinkState = EBlinkState.NONE;
    private const int BLINK_COUNT = 5;
    private int m_BlinkCount = BLINK_COUNT;
    private const int BLINK_INTERVAL_MS = 70;

    public ImageListViewAriadnaRenderer()
    {
        m_BlinkTimer.Tick += Blink;
        m_BlinkTimer.Interval = BLINK_INTERVAL_MS;
    }
    public override void Dispose()
    {
        base.Dispose();

        // Dispose local resources
        m_TextBrush.Dispose();
        m_StringFormat.Dispose();
        m_BlinkTimer.Dispose();
    }
    public override void DrawBackground(Graphics g, Rectangle bounds)
    {
        using var brush = new LinearGradientBrush(bounds, Theme.ListViewGradFromColor, Theme.ListViewGradToColor, LinearGradientMode.Vertical);
        g.FillRectangle(brush, bounds);
    }
    public override Size MeasureItem(View view)
    {
        var itemSize = ImageListView.ThumbnailSize;
        itemSize.Width += 2 * PAD_W;
        itemSize.Height += PAD_TOP + PAD_BTM + 2 * ImageListView.Font.Height;
        return itemSize;
    }
    public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
    {
        //FillItemBackground(g, bounds);

        DrawItemBackground(g, state, bounds);

        DrawItemBorder(g, state, bounds);

        DrawImage(g, item.GetCachedImage(CachedImageType.Thumbnail), bounds);

        DrawItemText(g, item, bounds);
    }
    private void FillItemBackground(Graphics g, Rectangle bounds)
    {
        using Brush brush = new SolidBrush(Color.Green);
        g.FillRectangle(brush, bounds);
    }
    private void DrawItemText(Graphics g, ImageListViewItem item, Rectangle bounds)
    {
        // Draw item text
        var rt = new RectangleF(bounds.Left,
            bounds.Top + ImageListView.ThumbnailSize.Height + PAD_TOP + PAD_BTM,
            ImageListView.ThumbnailSize.Width + 2 * PAD_W,
            ImageListView.Font.Height * 2);

        g.DrawString(item.Text, ImageListView.Font, m_TextBrush, rt, m_StringFormat);
    }
    private void DrawItemBackground(Graphics g, ItemState state, Rectangle pos)
    {
        var isSelected = ((state & ItemState.Selected) == ItemState.Selected);
        var isHovered = ((state & ItemState.Hovered) == ItemState.Hovered);

        if (!isSelected && !isHovered)
        {
            return;
        }

        var from = Color.FromArgb(isSelected ? 110 : 65, Theme.ListViewItemBgFromColor);
        var to = Theme.ListViewItemBgToColor;

        using Brush brush = new LinearGradientBrush(new Point(pos.X - PAD_W, pos.Y - PAD_TOP), new Point(pos.X - PAD_W, pos.Y + pos.Height + PAD_TOP + PAD_BTM), from, to);
        g.FillRectangle(brush, pos.X - PAD_W, pos.Y - PAD_TOP, pos.Width + 2 * PAD_W, pos.Height + PAD_TOP + PAD_BTM);
    }
    private void DrawItemBorder(Graphics g, ItemState state, Rectangle pos)
    {
        var isSelected = ((state & ItemState.Selected) == ItemState.Selected);
        var isHovered = ((state & ItemState.Hovered) == ItemState.Hovered);

        if (!isSelected && !isHovered)
        {
            return;
        }

        using var brush = new SolidBrush((m_BlinkState == EBlinkState.TICK) ? Theme.ListViewItemBorderTickColor : Theme.ListViewItemBorderTuckColor);
        using var pen = new Pen(brush);
        g.DrawRectangle(pen, pos.X + 1, pos.Y + 1, pos.Width - 2, pos.Height - 2);
    }
    private void DrawImage(Graphics g, Image img, Rectangle bounds)
    {
        if (img != null)
        {
            g.DrawImage(img, bounds.X + PAD_W, bounds.Y + PAD_TOP, ImageListView.ThumbnailSize.Width, ImageListView.ThumbnailSize.Height);
        }
    }
    public void Blink()
    {
        m_BlinkTimer.Stop();
        m_BlinkState = EBlinkState.TICK;
        m_BlinkCount = BLINK_COUNT;
        m_BlinkTimer.Start();
    }
    private void Blink(object sender, System.EventArgs e)
    {
        m_BlinkState = (m_BlinkState == EBlinkState.TICK) ? EBlinkState.TUCK : EBlinkState.TICK;
        ImageListView.Refresh();
        if (--m_BlinkCount < 0)
        {
            m_BlinkTimer.Stop();
            m_BlinkState = EBlinkState.NONE;
        }
    }
}