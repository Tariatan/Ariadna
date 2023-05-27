using Ariadna.Themes;
using Manina.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Ariadna
{
    public class ImageListViewAriadnaRenderer : ImageListView.ImageListViewRenderer
    {
        private readonly int padW = 20;
        private readonly int padTop = 12;
        private readonly int padBtm = 6;
        private readonly Brush textBrush = new SolidBrush(Theme.ListViewForeColor);
        private readonly StringFormat stringFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
        };

        private System.Windows.Forms.Timer mBlinkTimer = new System.Windows.Forms.Timer();
        private enum EBlinkState { None = 0, TICK, TUCK, }
        private EBlinkState mBlinkState = EBlinkState.None;
        private const int BLINK_COUNT = 5;
        private int mBlinkCount = BLINK_COUNT;
        private const int BLINK_INTERVAL_MS = 70;

        public ImageListViewAriadnaRenderer()
        {
            mBlinkTimer.Tick += new System.EventHandler(Blink);
            mBlinkTimer.Interval = BLINK_INTERVAL_MS;
        }
        public override void Dispose()
        {
            base.Dispose();

            // Dispose local resources
            textBrush.Dispose();
            stringFormat.Dispose();
            mBlinkTimer.Dispose();
        }
        public override void DrawBackground(Graphics g, Rectangle bounds)
        {
            using (Brush brush = new LinearGradientBrush(bounds, Theme.ListViewGradFromColor, Theme.ListViewGradToColor, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, bounds);
            }
        }
        public override Size MeasureItem(Manina.Windows.Forms.View view)
        {
            Size itemSize = ImageListView.ThumbnailSize;
            itemSize.Width += 2 * padW;
            itemSize.Height += padTop + padBtm + 2 * ImageListView.Font.Height;
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
            using (Brush brush = new SolidBrush(Color.Green))
            {
                g.FillRectangle(brush, bounds);
            }
        }
        private void DrawItemText(Graphics g, ImageListViewItem item, Rectangle bounds)
        {
            // Draw item text
            RectangleF rt = new RectangleF(bounds.Left,
                bounds.Top + ImageListView.ThumbnailSize.Height + padTop + padBtm,
                ImageListView.ThumbnailSize.Width + 2 * padW,
                ImageListView.Font.Height * 2);

            g.DrawString(item.Text, ImageListView.Font, textBrush, rt, stringFormat);
        }
        private void DrawItemBackground(Graphics g, ItemState state, Rectangle pos)
        {
            bool isSelected = ((state & ItemState.Selected) == ItemState.Selected);
            bool isHovered = ((state & ItemState.Hovered) == ItemState.Hovered);

            if (!isSelected && !isHovered)
            {
                return;
            }

            Color from = Color.FromArgb(isSelected ? 110 : 65, Theme.ListViewItemBgFromColor);
            Color to = Theme.ListViewItemBgToColor;

            using (Brush brush = new LinearGradientBrush(new Point(pos.X - padW, pos.Y - padTop), new Point(pos.X - padW, pos.Y + pos.Height + padTop + padBtm), from, to))
            {
                g.FillRectangle(brush, pos.X - padW, pos.Y - padTop, pos.Width + 2 * padW, pos.Height + padTop + padBtm);
            }
        }
        private void DrawItemBorder(Graphics g, ItemState state, Rectangle pos)
        {
            bool isSelected = ((state & ItemState.Selected) == ItemState.Selected);
            bool isHovered = ((state & ItemState.Hovered) == ItemState.Hovered);

            if (!isSelected && !isHovered)
            {
                return;
            }

            using (SolidBrush brush = new SolidBrush((mBlinkState == EBlinkState.TICK) ? Theme.ListViewItemBorderTickColor : Theme.ListViewItemBorderTuckColor))
            using (Pen pen = new Pen(brush))
            {
                g.DrawRectangle(pen, pos.X + 1, pos.Y + 1, pos.Width - 2, pos.Height - 2);
            }
        }
        private void DrawImage(Graphics g, Image img, Rectangle bounds)
        {
            if (img != null)
            {
                g.DrawImage(img, bounds.X + padW, bounds.Y + padTop, ImageListView.ThumbnailSize.Width, ImageListView.ThumbnailSize.Height);
            }
        }
        public void Blink()
        {
            mBlinkTimer.Stop();
            mBlinkState = EBlinkState.TICK;
            mBlinkCount = BLINK_COUNT;
            mBlinkTimer.Start();
        }
        private void Blink(object sender, System.EventArgs e)
        {
            mBlinkState = (mBlinkState == EBlinkState.TICK) ? EBlinkState.TUCK : EBlinkState.TICK;
            ImageListView.Refresh();
            if (--mBlinkCount < 0)
            {
                mBlinkTimer.Stop();
                mBlinkState = EBlinkState.None;
            }
        }
    }
}
