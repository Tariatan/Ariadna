using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public sealed class WindowPlacementTests
{
    [TestMethod]
    public async Task Save_WindowNeverShown_PreservesPreviousPlacement() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), "Ariadna-placement-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "window.json");
        const string saved = "{\"Left\":50,\"Top\":50,\"Width\":820,\"Height\":620,\"Maximized\":false}";
        File.WriteAllText(path, saved);
        var testee = new WindowPlacement(path, NullLogger.Instance);
        var window = new Window();
        try
        {
            // Act
            testee.Save(window);

            // Assert
            Assert.AreEqual(saved, File.ReadAllText(path));
        }
        finally
        {
            window.Close();
            Directory.Delete(directory, true);
        }
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Restore_ValidSavedPlacement_RestoresBoundsAndStateWithoutShowing(bool maximized) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var path = Path.Combine(fixture.Root, "placement.json");
        var left = SystemParameters.WorkArea.Left + 30;
        var top = SystemParameters.WorkArea.Top + 30;
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new { Left = left, Top = top, Width = 820, Height = 620, Maximized = maximized }));
        var window = new Window { MinWidth = 700, MinHeight = 500 };
        try
        {
            // Act
            new WindowPlacement(path, NullLogger.Instance).Restore(window);

            // Assert
            Assert.AreEqual(WindowStartupLocation.Manual, window.WindowStartupLocation);
            Assert.AreEqual(left, window.Left);
            Assert.AreEqual(top, window.Top);
            Assert.AreEqual(Math.Min(820, SystemParameters.VirtualScreenWidth), window.Width);
            Assert.AreEqual(Math.Min(620, SystemParameters.VirtualScreenHeight), window.Height);
            Assert.AreEqual(maximized ? WindowState.Maximized : WindowState.Normal, window.WindowState);
            Assert.IsFalse(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow("not json")]
    [DataRow("null")]
    [DataRow("{\"Left\":10,\"Top\":10,\"Width\":1,\"Height\":1}")]
    [DataRow("{\"Left\":1000000,\"Top\":1000000,\"Width\":820,\"Height\":620}")]
    public async Task Restore_InvalidSavedPlacement_PreservesDefaults(string json) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var path = Path.Combine(fixture.Root, "placement.json");
        File.WriteAllText(path, json);
        var window = new Window { Width = 900, Height = 700, MinWidth = 700, MinHeight = 500 };
        try
        {
            // Act
            new WindowPlacement(path, NullLogger.Instance).Restore(window);

            // Assert
            Assert.AreEqual(900, window.Width);
            Assert.AreEqual(700, window.Height);
            Assert.IsFalse(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
        return Task.CompletedTask;
    });
}
