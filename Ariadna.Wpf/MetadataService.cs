using Ariadna.Storage;
using System.IO;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.TvShows;

namespace Ariadna.Wpf;
internal sealed class MetadataService(CatalogConfiguration configuration) : IDisposable
{
    private readonly TMDbClient client = new(configuration.Get("TmdbApiKey"));
    internal async Task<IReadOnlyCollection<MetadataChoice>> SearchAsync(string name, bool series, CancellationToken cancellationToken)
    {
        var language = configuration.Get("ImdbLanguage");
        if (series)
        {
            var result = await client.SearchTvShowAsync(name, language, cancellationToken: cancellationToken);
            return (result?.Results ?? []).Take(20).Select(show => new MetadataChoice(show.Id, true, show.Name ?? string.Empty, show.OriginalName ?? string.Empty, show.FirstAirDate?.Year ?? 0)).ToArray();
        }

        var movies = await client.SearchMovieAsync(name, language, cancellationToken: cancellationToken);
        return (movies?.Results ?? []).Take(20).Select(movie => new MetadataChoice(movie.Id, false, movie.Title ?? string.Empty, movie.OriginalTitle ?? string.Empty, movie.ReleaseDate?.Year ?? 0)).ToArray();
    }

    internal async Task ApplyAsync(MetadataChoice choice, EntryEditorModel editor, CancellationToken cancellationToken)
    {
        var revision = editor.Revision;
        await client.GetConfigAsync().WaitAsync(cancellationToken);
        string? title;
        string? description;
        string? posterPath;
        int year;
        IEnumerable<string> genres;
        if (choice.Series)
        {
            var show = await client.GetTvShowAsync(choice.Id, TvShowMethods.Undefined, configuration.Get("ImdbLanguage"), cancellationToken: cancellationToken) ?? throw new InvalidDataException("TMDb returned no series metadata.");
            title = show.OriginalName;
            description = show.Overview;
            year = show.FirstAirDate?.Year ?? 0;
            posterPath = show.PosterPath;
            genres = (show.Genres ?? []).Select(genre => genre.Name).OfType<string>();
        }
        else
        {
            var movie = await client.GetMovieAsync(choice.Id, configuration.Get("ImdbLanguage"), cancellationToken: cancellationToken) ?? throw new InvalidDataException("TMDb returned no movie metadata.");
            title = movie.OriginalTitle;
            description = movie.Overview;
            year = movie.ReleaseDate?.Year ?? 0;
            posterPath = movie.PosterPath;
            genres = (movie.Genres ?? []).Select(genre => genre.Name).OfType<string>();
        }

        var posterSize = client.Config?.Images?.PosterSizes?.LastOrDefault() ?? throw new InvalidDataException("TMDb returned no poster sizes.");
        byte[]? poster = posterPath == null ? null : await client.GetImageBytesAsync(posterSize, posterPath, false, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (editor.Revision != revision)
        {
            return;
        }

        editor.OriginalTitle = title ?? string.Empty;
        editor.Description = description ?? string.Empty;
        editor.Year = year.ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach (var genre in genres)
        {
            editor.AddGenre(genre);
        }

        if (poster != null)
        {
            editor.ReplaceImage(string.Empty, poster);
        }
    }

    internal async Task<byte[]?> PortraitAsync(string name, CancellationToken cancellationToken)
    {
        await client.GetConfigAsync().WaitAsync(cancellationToken);
        var people = await client.SearchPersonAsync(name, configuration.Get("ImdbLanguage"), cancellationToken: cancellationToken);
        var path = people?.Results?.FirstOrDefault()?.ProfilePath;
        var size = client.Config?.Images?.ProfileSizes?.LastOrDefault() ?? throw new InvalidDataException("TMDb returned no portrait sizes.");
        return path == null ? null : await client.GetImageBytesAsync(size, path, false, cancellationToken);
    }

    public void Dispose() => client.Dispose();
}
