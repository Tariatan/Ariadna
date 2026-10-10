using System.Windows;

namespace Ariadna.Wpf;

internal static class RefreshIndicator
{
    internal static readonly DependencyProperty IsRefreshingProperty = DependencyProperty.RegisterAttached(
        "IsRefreshing", typeof(bool), typeof(RefreshIndicator), new PropertyMetadata(false));

    public static bool GetIsRefreshing(DependencyObject element) => (bool)element.GetValue(IsRefreshingProperty);
    public static void SetIsRefreshing(DependencyObject element, bool value) => element.SetValue(IsRefreshingProperty, value);
}
