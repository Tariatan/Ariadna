namespace Ariadna.Wpf;

internal static class RefreshBatch
{
    internal static async Task<IReadOnlyDictionary<MetadataField, Exception>> RunAsync(
        IReadOnlyCollection<(MetadataField Field, Func<CancellationToken, Task> Action)> actions,
        CancellationToken cancellationToken)
    {
        var failures = new Dictionary<MetadataField, Exception>();
        foreach (var (field, action) in actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await action(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Add(field, exception);
            }
        }

        return failures;
    }
}
