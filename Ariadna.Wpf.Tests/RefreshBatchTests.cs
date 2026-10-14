using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class RefreshBatchTests
{
    [TestMethod]
    [DataRow((int)MetadataField.Poster)]
    [DataRow((int)MetadataField.Description)]
    [DataRow((int)MetadataField.Directors)]
    [DataRow((int)MetadataField.Cast)]
    [DataRow((int)MetadataField.Translation)]
    [DataRow((int)MetadataField.Year)]
    [DataRow((int)MetadataField.Genres)]
    public async Task RunAsync_FieldFails_ContinuesRemainingActionsAndReportsFailure(int failedFieldValue)
    {
        // Arrange
        var failedField = (MetadataField)failedFieldValue;
        MetadataField[] fields = [MetadataField.Translation, MetadataField.Year, MetadataField.Genres, MetadataField.Poster, MetadataField.Description, MetadataField.Directors, MetadataField.Cast];
        var attempted = new List<MetadataField>();
        var actions = fields.Select(field => (field, (Func<CancellationToken, Task>)(_ =>
        {
            attempted.Add(field);
            return field == failedField ? Task.FromException(new InvalidOperationException("Unavailable")) : Task.CompletedTask;
        }))).ToArray();

        // Act
        var failures = await RefreshBatch.RunAsync(actions, CancellationToken.None);

        // Assert
        CollectionAssert.AreEqual(fields, attempted.ToArray());
        Assert.AreEqual(1, failures.Count);
        Assert.AreEqual("Unavailable", failures[failedField].Message);
    }

    [TestMethod]
    public async Task RunAsync_Cancelled_StopsRemainingActions()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var laterActionRan = false;
        (MetadataField, Func<CancellationToken, Task>)[] actions =
        [
            (MetadataField.Poster, token =>
            {
                cancellation.Cancel();
                return Task.FromCanceled(token);
            }),
            (MetadataField.Cast, _ =>
            {
                laterActionRan = true;
                return Task.CompletedTask;
            }),
        ];

        // Act / Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await RefreshBatch.RunAsync(actions, cancellation.Token));
        Assert.IsFalse(laterActionRan);
    }
}
