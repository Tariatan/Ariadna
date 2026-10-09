using System.IO;
using MediaInfo;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class FileInspectionTests
{
    [TestMethod]
    public async Task InspectAsync_GameBelowOneGigabyte_UsesGigabytes()
    {
        // Arrange
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "media-two-seconds.avi");

        // Act
        var result = await FileInspection.InspectAsync(file, false, NullLogger.Instance, CancellationToken.None, true);

        // Assert
        Assert.AreEqual($"{new FileInfo(file).Length / (1024d * 1024 * 1024):N1} GB", result.Metrics.Single().Value);
    }

    [TestMethod]
    public async Task InspectAsync_ExistingVideo_ReadsBundledNativeDuration()
    {
        // Arrange
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "media-two-seconds.avi");
        // Act
        var result = await FileInspection.InspectAsync(file, true, NullLogger.Instance, CancellationToken.None);
        // Assert
        Assert.AreEqual(4, result.Metrics.Count);
        Assert.AreEqual("00:00:02", result.Metrics.ElementAt(1).Value);
        var info = new MediaInfoWrapper(file, NullLogger.Instance);
        Assert.AreEqual($"{info.VideoRate / 1000000d:N0} Mbps", result.Metrics.Last().Value);
    }

    [TestMethod]
    public async Task InspectAsync_CanceledRequest_DoesNotPublishInformation()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        // Act
        var request = FileInspection.InspectAsync(Path.GetTempPath(), false, NullLogger.Instance, cancellation.Token);
        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await request);
    }
}
