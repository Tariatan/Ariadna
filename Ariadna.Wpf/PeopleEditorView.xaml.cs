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
    private CancellationToken cancellationToken;
    public PeopleEditorView() => InitializeComponent();
    internal void Configure(EntryEditorModel model, CatalogActions catalogActions, bool cast, CancellationToken lifetime)
    {
        editor = model;
        actions = catalogActions;
        actors = cast;
        cancellationToken = lifetime;
        People.ItemsSource = cast ? model.Actors : model.People;
        DownloadButton.Visibility = model.IsMovie ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void Commit()
    {
        People.CommitEdit(DataGridEditingUnit.Cell, true);
        People.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private void Add(object sender, RoutedEventArgs e)
    {
        Run(() => editor!.AddPeople(NewName.Text, actors));
        NewName.Clear();
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

    private async void DownloadPhoto(object sender, RoutedEventArgs e)
    {
        if (People.SelectedItem is not PersonEditorModel person)
        {
            return;
        }

        var name = person.Name;
        var previous = person.Photo;
        DownloadButton.IsEnabled = false;
        try
        {
            using var service = new MetadataService(actions!.Configuration);
            var photo = await service.PortraitAsync(name, cancellationToken);
            if (photo != null && !cancellationToken.IsCancellationRequested && person.Name == name && person.Photo == previous)
            {
                person.SetPhoto(Images.Resize(photo, actions.Configuration.GetInt("PortraitWidth", 100), actions.Configuration.GetInt("PortraitHeight", 150)));
            }
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                actions!.Report(Window.GetWindow(this), exception);
            }
        }
        finally
        {
            DownloadButton.IsEnabled = true;
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && People.CurrentCell.IsValid)
        {
            Commit();
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
