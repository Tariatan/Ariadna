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
        StringAssert.Contains(result, "00:00:02");
        StringAssert.Contains(result, "Duration:");
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
