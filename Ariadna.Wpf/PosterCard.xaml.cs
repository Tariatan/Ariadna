using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.Wpf;
public partial class PosterCard : UserControl
{
    private CancellationTokenSource? loading;
    public PosterCard() => InitializeComponent();
    private void OnLoaded(object sender, RoutedEventArgs e) => LoadPoster();
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Cancel();
        Poster.Source = null;
        if (IsLoaded)
        {
            LoadPoster();
        }
    }

    private async void LoadPoster()
    {
        Cancel();
        if (DataContext is not PosterItem item)
        {
            return;
        }

        using var request = new CancellationTokenSource();
        loading = request;
        try
        {
            var image = await item.Cache.LoadAsync(item.ImagePath, request.Token);
            if (!request.IsCancellationRequested && DataContext == item)
            {
                Poster.Source = image;
            }
        }
        catch (OperationCanceledException)when (request.IsCancellationRequested)
        {
        }
        finally
        {
            if (loading == request)
            {
                loading = null;
            }
        }
    }

    private void Cancel() => loading?.Cancel();
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Cancel();
        Poster.Source = null;
    }
}
