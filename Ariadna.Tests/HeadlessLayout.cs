using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Ariadna.Tests;

internal static class HeadlessLayout
{
    private static readonly ConditionalWeakTable<Window, FrameworkElement> roots = new();

    internal static FrameworkElement Root(Window window) => roots.GetValue(window, owner =>
    {
        var root = (FrameworkElement)owner.Content;
        owner.Content = null;
        root.DataContext = owner.DataContext;
        root.Resources.MergedDictionaries.Add(owner.Resources);
        return root;
    });

    internal static void Arrange(Window window)
    {
        var root = Root(window);
        var width = double.IsNaN(window.Width) ? 1120 : window.Width;
        var height = double.IsNaN(window.Height) ? 800 : window.Height;
        root.Measure(new Size(width, height));
        root.Arrange(new Rect(0, 0, width, height));
        root.UpdateLayout();
    }

    internal static void SelectCatalog(Window window, string argument)
    {
        var tabs = (System.Windows.Controls.TabControl)window.FindName("CatalogTabs");
        string[] arguments = ["movies", "games", "library", "documentaries", "xxx"];
        tabs.SelectedIndex = Array.IndexOf(arguments, argument);
        Arrange(window);
    }

    internal static bool IsVisible(DependencyObject element)
    {
        for (DependencyObject? current = element; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is UIElement control && control.Visibility != Visibility.Visible)
            {
                return false;
            }
        }

        return true;
    }

    // Routed input can be tested without connecting a visual to a desktop HWND.
    internal static PresentationSource Source { get; } = new DetachedSource();

    private sealed class DetachedSource : PresentationSource
    {
        public override Visual RootVisual { get; set; } = null!;
        public override bool IsDisposed => false;
        protected override CompositionTarget GetCompositionTargetCore() => null!;
    }
}
