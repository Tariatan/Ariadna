using System.Windows;
using System.Windows.Media;

namespace Ariadna.Wpf;

internal static class RefreshIndicator
{
    internal static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.RegisterAttached(
        "IsRefreshing", typeof(bool), typeof(RefreshIndicator), new PropertyMetadata(false));

    internal static async Task WaitForRenderAsync(CancellationToken cancellationToken)
    {
        var rendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var frames = 0;
        EventHandler onRendering = (_, _) =>
        {
            // The second frame follows composition of the updated visual tree.
            if (++frames == 2)
            {
                rendered.TrySetResult();
            }
        };
        CompositionTarget.Rendering += onRendering;
        try
        {
            await rendered.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            CompositionTarget.Rendering -= onRendering;
        }
    }

    public static bool GetIsRefreshing(DependencyObject element) => (bool)element.GetValue(IsRefreshingProperty);
    public static void SetIsRefreshing(DependencyObject element, bool value) => element.SetValue(IsRefreshingProperty, value);
}
