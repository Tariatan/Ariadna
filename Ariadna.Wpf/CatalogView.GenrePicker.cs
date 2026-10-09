using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ariadna.Wpf;

public partial class CatalogView
{
    private bool pickingSubgenre;

    private void OpenGenrePicker(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBox { IsMouseCaptureWithin: true })
        {
            // Handling mouse-up bypasses TextBox's normal selection-capture release.
            Mouse.Capture(null);
        }

        ShowGenrePicker((string?)((FrameworkElement)sender).Tag == "Subgenre");
        e.Handled = true;
    }

    private void OnGenreFieldKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Space or Key.Down)
        {
            ShowGenrePicker(sender == SubgenreSearch);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            GenrePopup.IsOpen = false;
            e.Handled = true;
        }
    }

    private void ShowGenrePicker(bool subgenre)
    {
        ClosePeopleSuggestions();
        GenrePopup.IsOpen = false;
        pickingSubgenre = subgenre;
        var values = subgenre ? Model.Subgenres : Model.IsLibrary ? Model.Genres : Actions.Store.GetGenres(Model.Kind);
        GenreSuggestions.ItemsSource = values.Where(value => !string.IsNullOrWhiteSpace(value)).Order().ToArray();
        GenreSuggestions.SelectedIndex = -1;
        GenrePopupSurface.Width = Math.Max(1, ActualWidth - 24);
        GenrePopup.IsOpen = true;
    }

    private void OnGenrePopupOpened(object? sender, EventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (GenrePopup.IsOpen)
            {
                GenreSuggestions.UpdateLayout();
                GenreSuggestions.Focus();
            }
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void OnGenreSuggestionKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmGenre();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            GenrePopup.IsOpen = false;
            (pickingSubgenre ? SubgenreSearch : GenreSearch).Focus();
            e.Handled = true;
        }
    }

    private void OnGenreSuggestionMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && ItemsControl.ContainerFromElement(GenreSuggestions, e.OriginalSource as DependencyObject) is ListBoxItem item)
        {
            GenreSuggestions.SelectedItem = item.Content;
            ConfirmGenre();
            e.Handled = true;
        }
    }

    private void ConfirmGenre()
    {
        if (GenreSuggestions.SelectedItem is not string genre)
        {
            return;
        }

        if (pickingSubgenre)
        {
            Model.Subgenre = genre;
        }
        else
        {
            Model.Genre = genre;
        }

        GenrePopup.IsOpen = false;
        (pickingSubgenre ? SubgenreSearch : GenreSearch).Focus();
    }
}
