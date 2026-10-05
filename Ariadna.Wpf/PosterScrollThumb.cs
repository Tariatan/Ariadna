using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Ariadna.Wpf;

public sealed class PosterScrollThumb : Thumb
{
    private ScrollBar? scrollBar;
    private BindingBase? valueBinding;

    public PosterScrollThumb()
    {
        DragStarted += OnDragStarted;
        DragDelta += OnDragDelta;
        DragCompleted += OnDragCompleted;
        Unloaded += OnUnloaded;
    }

    private void OnDragStarted(object sender, DragStartedEventArgs e)
    {
        RestoreBinding();
        for (DependencyObject? parent = VisualTreeHelper.GetParent(this); parent != null; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is ScrollBar bar)
            {
                valueBinding = BindingOperations.GetBindingBase(bar, ScrollBar.ValueProperty);
                if (valueBinding != null)
                {
                    scrollBar = bar;
                    var value = bar.Value;
                    // Keep fractional thumb movement independent of row-offset coercion.
                    BindingOperations.ClearBinding(bar, ScrollBar.ValueProperty);
                    bar.Value = value;
                }

                return;
            }
        }
    }

    private void OnDragCompleted(object sender, DragCompletedEventArgs e) => RestoreBinding();

    private void OnUnloaded(object sender, RoutedEventArgs e) => RestoreBinding();

    private void OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (scrollBar != null)
        {
            var value = Math.Clamp(scrollBar.Value + scrollBar.Track.ValueFromDistance(e.HorizontalChange, e.VerticalChange), scrollBar.Minimum, scrollBar.Maximum);
            scrollBar.Value = value;
            ScrollBar.ScrollToVerticalOffsetCommand.Execute(value, scrollBar);
            scrollBar.RaiseEvent(new ScrollEventArgs(ScrollEventType.ThumbTrack, value) { RoutedEvent = ScrollBar.ScrollEvent });
            e.Handled = true;
        }
    }

    private void RestoreBinding()
    {
        if (scrollBar != null && valueBinding != null)
        {
            BindingOperations.SetBinding(scrollBar, ScrollBar.ValueProperty, valueBinding);
        }

        scrollBar = null;
        valueBinding = null;
    }
}