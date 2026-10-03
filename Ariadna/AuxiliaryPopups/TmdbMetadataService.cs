#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ariadna.Extension;
using Ariadna.Properties;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;
using TMDbLib.Objects.TvShows;
using TMDbLib.Utilities.Serializer;

namespace Ariadna.AuxiliaryPopups;

internal sealed class TmdbMetadataService(ILogger logger) : ITmdbMetadataService
{
    private readonly TMDbClient client = new(Settings.Default.TmdbApiKey);
    private readonly SemaphoreSlim configGate = new(1, 1);
    private bool configured;

    private async Task EnsureConfigAsync(CancellationToken cancellationToken)
    {
        await configGate.WaitAsync(cancellationToken);
        try
        {
            if (configured)
            {
                return;
            }
            var cache = new FileInfo("config.json");
            if (cache.Exists && cache.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-10))
            {
                var json = await File.ReadAllTextAsync(cache.FullName, Encoding.UTF8, cancellationToken);
                client.SetConfig(TMDbJsonSerializer.Instance.DeserializeFromString<TMDbLib.Objects.General.TMDbConfig>(json)
                    ?? throw new InvalidDataException("Invalid TMDb configuration cache."));
            }
            else
            {
                // This library's configuration endpoint has no token overload.
                var config = await client.GetConfigAsync().WaitAsync(cancellationToken);
                await File.WriteAllTextAsync(cache.FullName, TMDbJsonSerializer.Instance.SerializeToString(config), Encoding.UTF8, cancellationToken);
            }
            configured = true;
        }
        finally
        {
            configGate.Release();
        }
    }

    public async Task<TmdbMetadata?> LoadAsync(int movieId, int tvShowId, CancellationToken cancellationToken)
    {
        if (movieId < 0 && tvShowId < 0)
        {
            return null;
        }
        if (!await RetryAsync(() => EnsureConfigAsync(cancellationToken), cancellationToken))
        {
            return null;
        }
        TmdbMetadata? metadata = null;
        await RetryAsync(async () =>
        {
            if (movieId >= 0)
            {
                var movie = await client.GetMovieAsync(movieId, Settings.Default.ImdbLanguage, cancellationToken: cancellationToken)
                    ?? throw new InvalidDataException("TMDb returned no movie metadata.");
                metadata = await CreateMetadataAsync(movie.OriginalTitle, movie.Overview, movie.ReleaseDate?.Year ?? 0, movie.Genres, movie.PosterPath, cancellationToken);
            }
            else
            {
                var show = await client.GetTvShowAsync(tvShowId, TvShowMethods.Undefined, Settings.Default.ImdbLanguage, cancellationToken: cancellationToken)
                    ?? throw new InvalidDataException("TMDb returned no series metadata.");
                metadata = await CreateMetadataAsync(show.OriginalName, show.Overview, show.FirstAirDate?.Year ?? 0, show.Genres, show.PosterPath, cancellationToken);
            }
        }, cancellationToken);
        return metadata;
    }

    private async Task<TmdbMetadata> CreateMetadataAsync(string? title, string? description, int year,
        List<TMDbLib.Objects.General.Genre>? genres, string? posterPath, CancellationToken cancellationToken)
    {
        byte[]? poster = null;
        if (posterPath != null)
        {
            var size = client.Config?.Images?.PosterSizes?.LastOrDefault() ?? throw new InvalidDataException("TMDb returned no poster sizes.");
            var bytes = await client.GetImageBytesAsync(size, posterPath, false, cancellationToken);
            poster = Resize(bytes, Settings.Default.PosterWidth, Settings.Default.PosterHeight);
        }
        return new TmdbMetadata(title ?? string.Empty, description ?? string.Empty, year,
            genres?.Select(genre => genre.Name).OfType<string>().ToArray() ?? [], poster);
    }

    public async Task<byte[]?> GetPortraitAsync(string name, CancellationToken cancellationToken)
    {
        byte[]? photo = null;
        await RetryAsync(async () =>
        {
            await EnsureConfigAsync(cancellationToken);
            var result = await client.SearchPersonAsync(name, "ru-RU", cancellationToken: cancellationToken);
            if (result?.Results?.FirstOrDefault()?.ProfilePath is { } path)
            {
                var size = client.Config?.Images?.ProfileSizes?.LastOrDefault() ?? throw new InvalidDataException("TMDb returned no portrait sizes.");
                var bytes = await client.GetImageBytesAsync(size, path, false, cancellationToken);
                photo = Resize(bytes, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
            }
        }, cancellationToken);
        return photo;
    }

    private static byte[] Resize(byte[] bytes, int width, int height)
    {
        using var source = bytes.ToBitmap();
        using var target = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(target);
        graphics.DrawImage(source, new Rectangle(0, 0, width, height));
        return target.ToBytes();
    }

    private async Task<bool> RetryAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await action();
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception exception)
            {
                logger.LogDebug("TMDb lookup attempt '{Attempt}' failed, error type '{ErrorType}'", attempt + 1, exception.GetType().Name);
            }
        }
        return false;
    }

    public void Dispose() => client.Dispose();
}
