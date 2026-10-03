using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Manina.Windows.Forms;

internal sealed class AriadnaVScrollBar : Control
{
    private const int MIN_THUMB_HEIGHT = 28;
    private const int TRACK_PADDING = 2;
    private const int THUMB_HORIZONTAL_INSET = 2;
    private const int TRACK_HORIZONTAL_INSET = 2;

    private static readonly Color ThumbBackColor = Color.FromArgb(244, 236, 249);
    private static readonly Color ThumbBorderColor = Color.FromArgb(214, 166, 231);
    private static readonly Color ThumbHoverBackColor = Color.FromArgb(255, 245, 255);
    private static readonly Color ThumbDragBackColor = Color.FromArgb(255, 255, 255);

    private int m_Minimum;
    private int m_Maximum;
    private int m_LargeChange = 10;
    private int m_SmallChange = 1;
    private int m_Value;
    private bool m_IsHoveringThumb;
    private bool m_IsDraggingThumb;
    private int m_ThumbDragOffset;

    public AriadnaVScrollBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint, true);

        TabStop = false;
        Width = 14;
    }

    [DefaultValue(0)]
    public int Minimum
    {
        get => m_Minimum;
        set
        {
            if (m_Minimum == value)
            {
                return;
            }

            m_Minimum = value;
            if (m_Maximum < m_Minimum)
            {
                m_Maximum = m_Minimum;
            }

            ClampValue();
            Invalidate();
        }
    }

    [DefaultValue(0)]
    public int Maximum
    {
        get => m_Maximum;
        set
        {
            if (m_Maximum == value)
            {
                return;
            }

            m_Maximum = Math.Max(m_Minimum, value);
            ClampValue();
            Invalidate();
        }
    }

    [DefaultValue(10)]
    public int LargeChange
    {
        get => m_LargeChange;
        set
        {
            var newValue = Math.Max(1, value);
            if (m_LargeChange == newValue)
            {
                return;
            }

            m_LargeChange = newValue;
            ClampValue();
            Invalidate();
        }
    }

    [DefaultValue(1)]
    public int SmallChange
    {
        get => m_SmallChange;
        set => m_SmallChange = Math.Max(1, value);
    }

    [DefaultValue(0)]
    public int Value
    {
        get => m_Value;
        set
        {
            var newValue = Clamp(value);
            if (m_Value == newValue)
            {
                return;
            }

            m_Value = newValue;
            Invalidate();
        }
    }

    public event ScrollEventHandler Scroll;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var trackColor = BackColor;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(trackColor);

        var trackRectangle = GetTrackRectangle();
        using var trackBrush = new SolidBrush(trackColor);
        using (var trackPath = CreateRoundedRectanglePath(trackRectangle, trackRectangle.Width))
        {
            e.Graphics.FillPath(trackBrush, trackPath);
        }

        var thumbRectangle = GetThumbRectangle();
        if (thumbRectangle.Height <= 0)
        {
            return;
        }

        var thumbBackColor = m_IsDraggingThumb
            ? ThumbDragBackColor
            : (m_IsHoveringThumb ? ThumbHoverBackColor : ThumbBackColor);

        using var thumbBrush = new SolidBrush(thumbBackColor);
        using var thumbBorderPen = new Pen(ThumbBorderColor);
        using var thumbPath = CreateRoundedRectanglePath(thumbRectangle, Math.Min(thumbRectangle.Width, thumbRectangle.Height));
        e.Graphics.FillPath(thumbBrush, thumbPath);
        e.Graphics.DrawPath(thumbBorderPen, thumbPath);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var thumbRectangle = GetThumbRectangle();
        if (thumbRectangle.Contains(e.Location))
        {
            m_IsDraggingThumb = true;
            m_ThumbDragOffset = e.Y - thumbRectangle.Top;
            Capture = true;
            Invalidate();
            return;
        }

        if (e.Y < thumbRectangle.Top)
        {
            ChangeValue(Value - LargeChange, ScrollEventType.LargeDecrement);
            return;
        }

        if (e.Y > thumbRectangle.Bottom)
        {
            ChangeValue(Value + LargeChange, ScrollEventType.LargeIncrement);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var thumbRectangle = GetThumbRectangle();
        var isHoveringThumb = thumbRectangle.Contains(e.Location);
        if (m_IsHoveringThumb != isHoveringThumb)
        {
            m_IsHoveringThumb = isHoveringThumb;
            Invalidate();
        }

        if (!m_IsDraggingThumb)
        {
            return;
        }

        var trackRectangle = GetTrackRectangle();
        var maxTop = trackRectangle.Bottom - thumbRectangle.Height;
        var thumbTop = Math.Max(trackRectangle.Top, Math.Min(maxTop, e.Y - m_ThumbDragOffset));
        var newValue = ThumbTopToValue(thumbTop, thumbRectangle.Height);
        ChangeValue(newValue, ScrollEventType.ThumbTrack);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!m_IsDraggingThumb)
        {
            return;
        }

        m_IsDraggingThumb = false;
        Capture = false;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (m_IsDraggingThumb || !m_IsHoveringThumb)
        {
            return;
        }

        m_IsHoveringThumb = false;
        Invalidate();
    }

    private void ChangeValue(int newValue, ScrollEventType type)
    {
        var oldValue = m_Value;
        Value = newValue;
        if (oldValue == m_Value)
        {
            return;
        }

        Scroll?.Invoke(this, new ScrollEventArgs(type, oldValue, m_Value, ScrollOrientation.VerticalScroll));
    }

    private void ClampValue()
    {
        m_Value = Clamp(m_Value);
    }

    private int Clamp(int value)
    {
        return Math.Max(m_Minimum, Math.Min(GetValueUpperBound(), value));
    }

    private Rectangle GetTrackRectangle()
    {
        var width = Math.Max(4, Width - (2 * TRACK_HORIZONTAL_INSET));
        return new Rectangle(TRACK_HORIZONTAL_INSET, TRACK_PADDING, width, Math.Max(0, Height - (2 * TRACK_PADDING)));
    }

    private Rectangle GetThumbRectangle()
    {
        var trackRectangle = GetTrackRectangle();
        if (trackRectangle.Height <= 0)
        {
            return Rectangle.Empty;
        }

        var thumbHeight = GetThumbHeight(trackRectangle.Height);
        var scrollableRange = GetValueUpperBound() - m_Minimum;
        var travelRange = Math.Max(0, trackRectangle.Height - thumbHeight);
        var thumbTop = trackRectangle.Top;

        if (scrollableRange > 0 && travelRange > 0)
        {
            var ratio = (m_Value - m_Minimum) / (double)scrollableRange;
            thumbTop += (int)Math.Round(ratio * travelRange);
        }

        return new Rectangle(
            THUMB_HORIZONTAL_INSET,
            thumbTop,
            Math.Max(4, Width - (2 * THUMB_HORIZONTAL_INSET)),
            thumbHeight);
    }

    private int GetThumbHeight(int trackHeight)
    {
        var scrollRange = Math.Max(1, (m_Maximum - m_Minimum) + 1);
        var largeChange = Math.Max(1, m_LargeChange);
        var thumbHeight = (int)Math.Round(trackHeight * (largeChange / (double)scrollRange));
        return Math.Max(MIN_THUMB_HEIGHT, Math.Min(trackHeight, thumbHeight));
    }

    private int GetValueUpperBound()
    {
        return Math.Max(m_Minimum, m_Maximum - Math.Max(1, m_LargeChange) + 1);
    }

    private int ThumbTopToValue(int thumbTop, int thumbHeight)
    {
        var trackRectangle = GetTrackRectangle();
        var travelRange = Math.Max(1, trackRectangle.Height - thumbHeight);
        var scrollableRange = GetValueUpperBound() - m_Minimum;
        if (scrollableRange <= 0)
        {
            return m_Minimum;
        }

        var offset = Math.Max(0, thumbTop - trackRectangle.Top);
        var ratio = offset / (double)travelRange;
        return m_Minimum + (int)Math.Round(ratio * scrollableRange);
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int diameter)
    {
        var path = new GraphicsPath();
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return path;
        }

        var normalizedDiameter = Math.Max(1, Math.Min(diameter, Math.Min(bounds.Width, bounds.Height)));
        var radiusRectangle = new Rectangle(bounds.Location, new Size(normalizedDiameter, normalizedDiameter));

        path.AddArc(radiusRectangle, 180, 90);
        radiusRectangle.X = bounds.Right - normalizedDiameter;
        path.AddArc(radiusRectangle, 270, 90);
        radiusRectangle.Y = bounds.Bottom - normalizedDiameter;
        path.AddArc(radiusRectangle, 0, 90);
        radiusRectangle.X = bounds.Left;
        path.AddArc(radiusRectangle, 90, 90);
        path.CloseFigure();

        return path;
    }
}
