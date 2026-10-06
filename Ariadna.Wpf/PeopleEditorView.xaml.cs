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
    public PeopleEditorView() => InitializeComponent();
    internal void Configure(EntryEditorModel model, CatalogActions catalogActions, bool cast)
    {
        editor = model;
        actions = catalogActions;
        actors = cast;
        People.ItemsSource = cast ? model.Actors : model.People;
        PanelLabel.Text = cast ? "Cast" : model.PeopleLabel;
    }

    private void Add(object sender, RoutedEventArgs e)
    {
        editor!.AddPlaceholderPerson(actors);
        People.SelectedItem = ((ObservableCollection<PersonEditorModel>)People.ItemsSource).Last();
        People.ScrollIntoView(People.SelectedItem);
    }

    private void Paste(object sender, RoutedEventArgs e) => Run(() => editor!.AddPeople(Clipboard.GetText(), actors));
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

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
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
            actions!.Report(Window.GetWindow(this), exception);
        }
    }
}