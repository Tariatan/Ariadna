using System.Drawing;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class InspectionControlTests
{
    [TestMethod]
    public void LoadPathAsync_ProgressQueuedAfterCompletion_DoesNotRestoreProgressState()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var form = new Form();
            using var testee = new FileSizeControl();
            var inspection = new ControlledInspection();
            testee.Inspection = inspection;
            form.Controls.Add(testee);
            form.Show();
            var task = testee.LoadPathAsync("sample");

            // Act
            inspection.Size.SetResult(3 * 1024 * 1024);
            UiTest.PumpUntil(() => task.IsCompleted);
            inspection.Progress!.Report(1024 * 1024);
            Application.DoEvents();

            // Assert
            Assert.AreEqual("3 Mb", testee.Controls[0].Text);
            Assert.AreNotEqual(Color.Yellow, testee.Controls[0].ForeColor);
        });
    }

    [TestMethod]
    public void Dispose_PendingSizeCalculation_CancelsAndIgnoresLateResult()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var form = new Form();
            var testee = new FileSizeControl();
            var inspection = new ControlledInspection();
            testee.Inspection = inspection;
            form.Controls.Add(testee);
            form.Show();
            var task = testee.LoadPathAsync("sample");

            // Act
            testee.Dispose();
            inspection.Size.SetResult(12);
            UiTest.PumpUntil(() => task.IsCompleted);

            // Assert
            Assert.IsTrue(inspection.Token.IsCancellationRequested);
            Assert.IsTrue(task.IsCompletedSuccessfully);
        });
    }

    [TestMethod]
    public void LoadPathAsync_PathReplaced_IgnoresOldVideoResult()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var form = new Form();
            using var testee = new VideoInfoControl();
            var first = new ControlledInspection();
            var second = new ControlledInspection();
            testee.Inspection = first;
            form.Controls.Add(testee);
            form.Show();
            var firstTask = testee.LoadPathAsync("first");
            testee.Inspection = second;
            var secondTask = testee.LoadPathAsync("second");

            // Act
            second.Video.SetResult(new VideoInfoSnapshot(TimeSpan.FromSeconds(2), 1920, 1080, 8000000, ["English"]));
            UiTest.PumpUntil(() => secondTask.IsCompleted);
            first.Video.SetResult(new VideoInfoSnapshot(TimeSpan.FromSeconds(1), 640, 480, 1000000, []));
            UiTest.PumpUntil(() => firstTask.IsCompleted);

            // Assert
            Assert.AreEqual("1920x1080", testee.Controls.Find("dimensions", true).Single().Text);
            Assert.IsTrue(first.Token.IsCancellationRequested);
        });
    }

    [TestMethod]
    public void Dispose_PendingVideoInspection_CancelsAndIgnoresLateResult()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var form = new Form();
            var testee = new VideoInfoControl();
            var inspection = new ControlledInspection();
            testee.Inspection = inspection;
            form.Controls.Add(testee);
            form.Show();
            var task = testee.LoadPathAsync("sample");

            // Act
            testee.Dispose();
            inspection.Video.SetResult(new VideoInfoSnapshot(TimeSpan.Zero, 1, 1, 0, []));
            UiTest.PumpUntil(() => task.IsCompleted);

            // Assert
            Assert.IsTrue(inspection.Token.IsCancellationRequested);
            Assert.IsTrue(task.IsCompletedSuccessfully);
        });
    }

    private sealed class ControlledInspection : IFileInspectionService
    {
        internal TaskCompletionSource<long> Size { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<VideoInfoSnapshot?> Video { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal IProgress<long>? Progress { get; private set; }
        internal CancellationToken Token { get; private set; }

        public Task<long> GetSizeAsync(string path, CancellationToken cancellationToken, IProgress<long> progress)
        {
            Token = cancellationToken;
            Progress = progress;
            return Size.Task;
        }

        public Task<VideoInfoSnapshot?> GetVideoInfoAsync(string path, CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            return Video.Task;
        }
    }
}
