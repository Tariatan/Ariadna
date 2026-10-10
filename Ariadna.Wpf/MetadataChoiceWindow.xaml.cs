using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ariadna.Wpf;
public partial class MetadataChoiceWindow : Window
{
    internal MetadataChoice? SelectedChoice => Matches.SelectedItem as MetadataChoice;

    internal MetadataChoiceWindow(Window owner, CatalogTheme theme, IReadOnlyCollection<MetadataChoice> choices)
    {
        InitializeComponent();
        Owner = owner;
        Background = theme.EditorBackground;
        ChooseButton.Background = theme.Background;
        Matches.ItemsSource = choices;
        Matches.SelectedIndex = choices.Count > 0 ? 0 : -1;
        ChooseButton.IsEnabled = choices.Count > 0;
        Loaded += (_, _) => ((ListBoxItem?)Matches.ItemContainerGenerator.ContainerFromIndex(0))?.Focus();
    }

    private void Choose(object sender, RoutedEventArgs e)
    {
        if (SelectedChoice != null)
        {
            DialogResult = true;
        }
    }

    private void ChooseOnDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(Matches, e.OriginalSource as DependencyObject) is ListBoxItem)
        {
            Choose(sender, e);
        }
    }

    private void Dismiss(object sender, RoutedEventArgs e) => DialogResult = false;
}