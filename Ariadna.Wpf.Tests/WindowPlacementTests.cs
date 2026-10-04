using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

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
}