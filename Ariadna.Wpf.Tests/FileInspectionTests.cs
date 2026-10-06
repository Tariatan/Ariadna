using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;
[TestClass]
public sealed class FileInspectionTests
{
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
        StringAssert.EndsWith(result.Metrics.Last().Value, "Mbps");
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