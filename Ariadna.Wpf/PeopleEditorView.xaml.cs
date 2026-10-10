using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Ariadna.Wpf;
public partial class PeopleEditorView : UserControl
{
    private EntryEditorModel? editor;
    private CatalogActions? actions;
    private bool actors;
    private const double PortraitRowHeight = 156;
    public PeopleEditorView() => InitializeComponent();
    internal void Configure(EntryEditorModel model, CatalogActions catalogActions, bool cast)
    {
        editor = model;
        actions = catalogActions;
        actors = cast;
        RefreshButton.Visibility = model.IsMovie ? Visibility.Visible : Visibility.Collapsed;
        People.ItemsSource = cast ? model.Actors : model.People;
        PanelLabel.Text = cast ? "Cast" : model.PeopleLabel;
        if (model.Kind == Ariadna.Storage.CatalogKind.Library)
        {
            People.ItemTemplate = (DataTemplate)FindResource("AuthorName");
            People.Height = double.NaN;
            People.MaxHeight = 90;
        }
    }

    internal event EventHandler? RefreshRequested;
    internal void SetRefreshEnabled(bool enabled) => RefreshButton.IsEnabled = enabled;
    internal Button MetadataRefreshButton => RefreshButton;
    internal bool IsRefreshFocused => RefreshButton.IsKeyboardFocused;
    private void Refresh(object sender, RoutedEventArgs e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

    private void Add(object sender, RoutedEventArgs e)
    {
        editor!.AddPlaceholderPerson(actors);
        People.SelectedItem = ((ObservableCollection<PersonEditorModel>)People.ItemsSource).Last();
        People.ScrollIntoView(People.SelectedItem);
    }

    private void Paste(object sender, RoutedEventArgs e) => Run(() => editor!.AddPeople(Clipboard.GetText(), actors));

    private void OnPeopleMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (editor?.Kind == Ariadna.Storage.CatalogKind.Library || e.Delta == 0)
        {
            return;
        }

        if (CatalogView.FindChild<ScrollViewer>(People) is { } scroll)
        {
            scroll.ScrollToVerticalOffset(scroll.VerticalOffset - Math.Sign(e.Delta) * PortraitRowHeight);
            e.Handled = true;
        }
    }

    private void OnPeopleScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (editor?.Kind == Ariadna.Storage.CatalogKind.Library || e.OriginalSource is not ScrollViewer scroll || e.VerticalChange == 0)
        {
            return;
        }

        var offset = Math.Round(scroll.VerticalOffset / PortraitRowHeight) * PortraitRowHeight;
        if (Math.Abs(offset - scroll.VerticalOffset) > 0.01)
        {
            scroll.ScrollToVerticalOffset(offset);
        }
    }
    private void Remove(object sender, RoutedEventArgs e)
    {
        if (People.SelectedItem is PersonEditorModel person)
        {
            ((ObservableCollection<PersonEditorModel>)People.ItemsSource).Remove(person);
        }
    }

    private void ReplacePhoto(object sender, RoutedEventArgs e)
    {
        if (People.SelectedItem is not PersonEditorModel person)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*"
        };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            Run(() => person.SetPhoto(Images.Resize(File.ReadAllBytes(dialog.FileName), actions!.Configuration.GetInt("PortraitWidth", 100), actions.Configuration.GetInt("PortraitHeight", 150))));
        }
    }

    private void OnPhotoDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            People.SelectedItem = ((FrameworkElement)sender).DataContext;
            ReplacePhoto(sender, e);
        }
    }

    private void OnNameDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            People.SelectedItem = ((FrameworkElement)sender).DataContext;
            BeginRename();
            e.Handled = true;
        }
    }

    private void BeginRename()
    {
        if (People.SelectedItem is not PersonEditorModel person)
        {
            return;
        }

        People.ScrollIntoView(person);
        People.UpdateLayout();
        if (People.ItemContainerGenerator.ContainerFromItem(person) is ListBoxItem item &&
            CatalogView.FindChild<TextBox>(item) is { } input)
        {
            input.Text = person.Name;
            input.Visibility = Visibility.Visible;
            ((Grid)input.Parent).Children.OfType<TextBlock>().Single().Visibility = Visibility.Hidden;
            input.Focus();
            input.SelectAll();
        }
    }

    private void FinishRename(TextBox input, bool commit)
    {
        if (input.Visibility != Visibility.Visible)
        {
            return;
        }

        if (commit && input.DataContext is PersonEditorModel person && !string.IsNullOrWhiteSpace(input.Text))
        {
            person.Name = input.Text.Trim();
        }

        input.Visibility = Visibility.Collapsed;
        ((Grid)input.Parent).Children.OfType<TextBlock>().Single().Visibility = Visibility.Visible;
    }

    private void OnNameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Escape)
        {
            FinishRename((TextBox)sender, e.Key == Key.Enter);
            People.Focus();
            e.Handled = true;
        }
    }

    private void OnNameLostFocus(object sender, KeyboardFocusChangedEventArgs e) => FinishRename((TextBox)sender, true);

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.OriginalSource is TextBox)
        {
            return;
        }

        if (e.Key == Key.F2)
        {
            BeginRename();
            e.Handled = true;
        }

        if (e.Key == Key.Delete)
        {
            Remove(sender, e);
            e.Handled = true;
        }

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
        }

        if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.OriginalSource is not TextBox)
        {
            Paste(sender, e);
            e.Handled = true;
        }
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            actions!.Report(Window.GetWindow(this), exception, editor!.Kind);
        }
    }
}
