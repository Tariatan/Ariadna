using Ariadna.AuxiliaryPopups;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class FileInspectionServiceTests
{
    [TestMethod]
    public async Task GetSizeAsync_NestedDirectory_CountsAllFiles()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), "Ariadna-size-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(directory, "child"));
        File.WriteAllBytes(Path.Combine(directory, "first"), new byte[1024]);
        File.WriteAllBytes(Path.Combine(directory, "child", "second"), new byte[2048]);
        var testee = new FileInspectionService(NullLogger.Instance);
        try
        {
            // Act
            var size = await testee.GetSizeAsync(directory, CancellationToken.None, new Progress<long>());

            // Assert
            Assert.AreEqual(3072, size);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    public async Task GetVideoInfoAsync_ExistingVideo_ReadsMetadataAndReleasesFile()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".avi");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "media-two-seconds.avi"), path);
        var testee = new FileInspectionService(NullLogger.Instance);
        try
        {
            // Act
            var info = await testee.GetVideoInfoAsync(path, CancellationToken.None);

            // Assert
            Assert.IsNotNull(info);
            Assert.AreEqual(TimeSpan.FromSeconds(2), info.Duration);
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.IsTrue(exclusive.Length > 0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task GetSizeAsync_CancelledBeforeStart_ThrowsCancellation()
    {
        // Arrange
        var testee = new FileInspectionService(NullLogger.Instance);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var result = testee.GetSizeAsync("missing", cancellation.Token, new Progress<long>());

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await result);
    }
}
