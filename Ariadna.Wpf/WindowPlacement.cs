using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal sealed class WindowPlacement(string path, ILogger logger)
{
    internal void Restore(Window window)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            var placement = JsonSerializer.Deserialize<Placement>(File.ReadAllText(path));
            if (placement == null || !double.IsFinite(placement.Left) || !double.IsFinite(placement.Top) || !double.IsFinite(placement.Width) || !double.IsFinite(placement.Height) || placement.Width < window.MinWidth || placement.Height < window.MinHeight)
            {
                return;
            }

            var desktop = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            var rectangle = new Rect(placement.Left, placement.Top, placement.Width, placement.Height);
            if (!desktop.IntersectsWith(rectangle))
            {
                return;
            }

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = placement.Left;
            window.Top = placement.Top;
            window.Width = Math.Min(placement.Width, desktop.Width);
            window.Height = Math.Min(placement.Height, desktop.Height);
            window.WindowState = placement.Maximized ? WindowState.Maximized : WindowState.Normal;
        }
        catch (Exception exception)when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            logger.LogWarning("Window placement could not be restored, error type '{ErrorType}'", exception.GetType().Name);
        }
    }

    internal void Save(Window window)
    {
        try
        {
            var rectangle = window.RestoreBounds;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new Placement(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, window.WindowState == WindowState.Maximized)));
        }
        catch (Exception exception)when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning("Window placement could not be saved, error type '{ErrorType}'", exception.GetType().Name);
        }
    }

    private sealed record Placement(double Left, double Top, double Width, double Height, bool Maximized);
}
