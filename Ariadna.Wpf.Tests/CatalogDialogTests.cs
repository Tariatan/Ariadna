using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class CatalogDialogTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    public async Task ShowDialog_RemovalCancelFocused_ReturnsFalse(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var dialog = new CatalogDialog(null, kind, "Delete entry and media?", "Remove this catalog entry AND permanently delete its media?", "Delete permanently");
        var cancel = (Button)dialog.FindName("CancelAction");
        var primary = (Button)dialog.FindName("PrimaryAction");
        bool? result = null;
        var focusedCancel = false;
        dialog.ContentRendered += (_, _) => dialog.Dispatcher.BeginInvoke(() =>
        {
            focusedCancel = Keyboard.FocusedElement == cancel;
            ((IInvokeProvider)new ButtonAutomationPeer(cancel).GetPattern(PatternInterface.Invoke)).Invoke();
        }, DispatcherPriority.ApplicationIdle);

        // Act
        result = dialog.ShowDialog();

        // Assert
        Assert.IsFalse(result);
        Assert.IsTrue(focusedCancel);
        Assert.IsTrue(cancel.IsDefault);
        Assert.IsFalse(primary.IsDefault);
        Assert.AreEqual(CatalogTheme.For(kind).EditorBackground.ToString(), dialog.Background.ToString());
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowDialog_ExplicitActionOrClose_OnlyActionConfirms(bool accept) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var dialog = new CatalogDialog(null, CatalogKind.Movie, "Remove catalog entry?", "Remove this catalog entry and its poster images?", "Remove entry");
        dialog.ContentRendered += (_, _) => dialog.Dispatcher.BeginInvoke(() =>
        {
            if (accept)
            {
                var button = (Button)dialog.FindName("PrimaryAction");
                ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
            }
            else
            {
                dialog.Close();
            }
        }, DispatcherPriority.ApplicationIdle);

        // Act
        var result = dialog.ShowDialog();

        // Assert
        Assert.AreEqual(accept, result);
        return Task.CompletedTask;
    });
}