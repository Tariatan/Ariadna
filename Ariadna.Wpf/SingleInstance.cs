using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal sealed class SingleInstance : IDisposable
{
    private readonly Mutex mutex;
    private readonly string pipe;
    private readonly ILogger logger;
    private readonly CancellationTokenSource lifetime = new();
    internal SingleInstance(string catalogPath, ILogger logger)
    {
        this.logger = logger;
        var identity = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(catalogPath).ToUpperInvariant())));
        pipe = $"AriadnaWpf_{identity}";
        mutex = new Mutex(false, $"Local\\{pipe}");
        try
        {
            IsPrimary = mutex.WaitOne(0);
        }
        catch (AbandonedMutexException)
        {
            IsPrimary = true;
        }
    }

    internal bool IsPrimary { get; }

    internal async Task ActivateAsync(string? argument, CancellationToken cancellationToken)
    {
        using var client = new NamedPipeClientStream(".", pipe, PipeDirection.Out, PipeOptions.Asynchronous);
        await client.ConnectAsync(10000, cancellationToken);
        await JsonSerializer.SerializeAsync(client, argument, cancellationToken: cancellationToken);
        await client.FlushAsync(cancellationToken);
    }

    internal void StartListening(Action<string?> activate) => _ = ListenAsync(activate, lifetime.Token);
    private async Task ListenAsync(Action<string?> activate, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(pipe, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await server.WaitForConnectionAsync(cancellationToken);
                var argument = await JsonSerializer.DeserializeAsync<string>(server, cancellationToken: cancellationToken);
                activate(argument);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (IOException exception)
            {
                logger.LogWarning("Launch request failed, error type '{ErrorType}'", exception.GetType().Name);
            }
            catch (JsonException exception)
            {
                logger.LogWarning("Invalid launch request, error type '{ErrorType}'", exception.GetType().Name);
            }
        }
    }

    public void Dispose()
    {
        try
        {
            lifetime.Cancel();
        }
        finally
        {
            if (IsPrimary)
            {
                mutex.ReleaseMutex();
            }

            mutex.Dispose();
            lifetime.Dispose();
        }
    }
}
