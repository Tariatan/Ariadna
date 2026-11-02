using System.Windows;
using System.Windows.Controls;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public sealed class CatalogDialogTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Xxx)]
    public async Task Constructor_Confirmation_DefaultsToCancel(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var dialog = new CatalogDialog(null, kind, "Delete entry and media?", "Synthetic confirmation", "Delete permanently");
        try
        {
            // Act
            HeadlessLayout.Arrange(dialog);
            var cancel = (Button)dialog.FindName("CancelAction");
            var primary = (Button)dialog.FindName("PrimaryAction");

            // Assert
            Assert.IsTrue(cancel.IsDefault);
            Assert.IsFalse(primary.IsDefault);
            Assert.AreEqual(Visibility.Visible, cancel.Visibility);
            Assert.AreEqual("Delete permanently", primary.Content);
            Assert.AreEqual(CatalogTheme.For(kind).EditorBackground.ToString(), dialog.Background.ToString());
            Assert.IsNull(dialog.DialogResult);
        }
        finally
        {
            dialog.Close();
        }
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Constructor_InformationOrError_HasSingleDismissAction(bool error) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var dialog = new CatalogDialog(null, CatalogKind.Movie, "Synthetic heading", "Synthetic message", error: error);
        try
        {
            // Act
            HeadlessLayout.Arrange(dialog);
            var primary = (Button)dialog.FindName("PrimaryAction");

            // Assert
            Assert.IsTrue(primary.IsDefault);
            Assert.IsTrue(primary.IsCancel);
            Assert.AreEqual("OK", primary.Content);
            Assert.AreEqual(Visibility.Collapsed, ((Button)dialog.FindName("CancelAction")).Visibility);
            Assert.AreEqual("Synthetic heading", ((TextBlock)dialog.FindName("Heading")).Text);
            Assert.AreEqual("Synthetic message", ((TextBlock)dialog.FindName("Message")).Text);
        }
        finally
        {
            dialog.Close();
        }
        return Task.CompletedTask;
    });
}
