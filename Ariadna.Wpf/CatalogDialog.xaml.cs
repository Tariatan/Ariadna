using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Ariadna.Storage;

namespace Ariadna.Wpf;
public partial class CatalogDialog : Window
{
    private readonly bool confirmation;

    internal CatalogDialog(Window? owner, CatalogKind kind, string heading, string message, string? confirmLabel = null, bool error = false)
    {
        var theme = CatalogTheme.For(kind);
        confirmation = confirmLabel != null;
        InitializeComponent();
        Background = theme.EditorBackground;
        Owner = owner;
        WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
        Heading.Text = heading;
        Message.Text = message;
        PrimaryAction.Content = confirmLabel ?? "OK";
        PrimaryAction.Background = confirmation
            ? new SolidColorBrush(Color.FromRgb(155, 35, 55))
            : theme.Background;
        CancelAction.Visibility = confirmation ? Visibility.Visible : Visibility.Collapsed;
        CancelAction.IsDefault = confirmation;
        PrimaryAction.IsDefault = !confirmation;
        PrimaryAction.IsCancel = !confirmation;
        if (confirmation)
        {
            WarningIcon.Visibility = Visibility.Visible;
            CircleIcon.Visibility = Visibility.Collapsed;
            IconGlyph.Visibility = Visibility.Collapsed;
        }
        else if (error)
        {
            IconGlyph.Text = "!";
            CircleIcon.Stroke = new SolidColorBrush(Color.FromRgb(255, 227, 177));
            IconGlyph.Foreground = CircleIcon.Stroke;
        }
    }

    internal static bool ConfirmRemoval(Window? owner, CatalogKind kind, string title, string path, bool deleteMedia)
    {
        var dialog = new CatalogDialog(owner, kind,
            deleteMedia ? "Delete entry and media?" : "Remove catalog entry?",
            string.Empty,
            deleteMedia ? "Delete permanently" : "Remove entry");
        dialog.Message.Inlines.Add(new Run("Remove "));
        dialog.Message.Inlines.Add(new Run(title)
        {
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.Gold,
        });
        dialog.Message.Inlines.Add(new Run(deleteMedia
            ? $" from the catalog AND permanently delete its media?\n\n{path}"
            : " from the catalog and delete its poster images?"));
        return dialog.ShowDialog() == true;
    }

    internal static void ShowInformation(Window? owner, CatalogKind kind, string heading, string message) =>
        new CatalogDialog(owner, kind, heading, message).ShowDialog();

    internal static void ShowError(Window? owner, CatalogKind kind, string heading, string message) =>
        new CatalogDialog(owner, kind, heading, message, error: true).ShowDialog();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (confirmation)
        {
            CancelAction.Focus();
        }
        else
        {
            PrimaryAction.Focus();
        }
    }

    private void Accept(object sender, RoutedEventArgs e) => DialogResult = true;
    private void Dismiss(object sender, RoutedEventArgs e) => DialogResult = false;
}