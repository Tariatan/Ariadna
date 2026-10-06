using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class AudioLanguageTests
{
    [TestMethod]
    [DataRow("Russian", "ru_flag")]
    [DataRow("Русский", "ru_flag")]
    [DataRow("eng", "en_flag")]
    [DataRow("French", "fr_flag")]
    [DataRow("deu", "de_flag")]
    [DataRow("Ukrainian", "ua_flag")]
    [DataRow("Japanese", "unknown")]
    public async Task Icon_DetectedLanguage_LoadsFlagAndRetainsLabel(string language, string expectedFlag) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        var track = new AudioLanguage(language);
        // Act
        var image = new BitmapImage(new Uri(track.Icon));
        // Assert
        StringAssert.EndsWith(track.Icon, $"/{expectedFlag}.png");
        Assert.AreEqual(language, track.Name);
        Assert.IsTrue(image.PixelWidth > 0);
        return Task.CompletedTask;
    });
}