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

    internal async Task RefreshAsync(MetadataChoice choice, EntryEditorModel editor, bool poster, CancellationToken cancellationToken)
    {
        var revision = editor.Revision;
        string? description;
        string? posterPath;
        if (choice.Series)
        {
            var show = await client.GetTvShowAsync(choice.Id, TvShowMethods.Undefined, configuration.Get("ImdbLanguage"), cancellationToken: cancellationToken)
                ?? throw new InvalidDataException("TMDb returned no series metadata.");
            description = show.Overview;
            posterPath = show.PosterPath;
        }
        else
        {
            var movie = await client.GetMovieAsync(choice.Id, configuration.Get("ImdbLanguage"), cancellationToken: cancellationToken)
                ?? throw new InvalidDataException("TMDb returned no movie metadata.");
            description = movie.Overview;
            posterPath = movie.PosterPath;
        }

        byte[]? bytes = null;
        if (poster)
        {
            if (string.IsNullOrWhiteSpace(posterPath))
            {
                throw new InvalidDataException("No poster is available for this match.");
            }

            await client.GetConfigAsync().WaitAsync(cancellationToken);
            var size = client.Config?.Images?.PosterSizes?.LastOrDefault()
                ?? throw new InvalidDataException("TMDb returned no poster sizes.");
            bytes = await client.GetImageBytesAsync(size, posterPath, false, cancellationToken)
                ?? throw new InvalidDataException("TMDb returned no poster image.");
        }
        else if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidDataException("No description is available for this match.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (editor.Revision != revision)
        {
            return;
        }

        if (poster)
        {
            editor.ReplaceImage(string.Empty, bytes!);
        }
        else
        {
            editor.Description = description!;
        }
    }

    internal async Task RefreshPeopleAsync(MetadataChoice choice, EntryEditorModel editor, bool cast, CancellationToken cancellationToken)
    {
        var revision = editor.Revision;
        var target = cast ? editor.Actors : editor.People;
        var previous = target.Select(person => (person.Name, person.Photo)).ToArray();
        (int Id, string Name, string? ProfilePath)[] credits;
        if (choice.Series)
        {
            var result = await client.GetTvShowCreditsAsync(choice.Id, configuration.Get("ImdbLanguage"), cancellationToken)
                ?? throw new InvalidDataException("TMDb returned no series credits.");
            credits = cast
                ? (result.Cast ?? []).Select(person => (person.Id, person.Name ?? string.Empty, person.ProfilePath)).ToArray()
                : (result.Crew ?? []).Where(person => person.Job == "Director").Select(person => (person.Id, person.Name ?? string.Empty, person.ProfilePath)).ToArray();
        }
        else
        {
            var result = await client.GetMovieCreditsAsync(choice.Id, cancellationToken)
                ?? throw new InvalidDataException("TMDb returned no movie credits.");
            credits = cast
                ? (result.Cast ?? []).Select(person => (person.Id, person.Name ?? string.Empty, person.ProfilePath)).ToArray()
                : (result.Crew ?? []).Where(person => person.Job == "Director").Select(person => (person.Id, person.Name ?? string.Empty, person.ProfilePath)).ToArray();
        }

        credits = credits.Where(person => !string.IsNullOrWhiteSpace(person.Name)).DistinctBy(person => person.Id).ToArray();
        if (credits.Length == 0)
        {
            throw new InvalidDataException(cast ? "No cast is available for this match." : "No directors are available for this match.");
        }

        await client.GetConfigAsync().WaitAsync(cancellationToken);
        var size = client.Config?.Images?.ProfileSizes?.LastOrDefault()
            ?? throw new InvalidDataException("TMDb returned no portrait sizes.");
        var people = new List<PersonPhoto>();
        foreach (var person in credits)
        {
            var existing = target.FirstOrDefault(value => value.Name.Equals(person.Name, StringComparison.OrdinalIgnoreCase))?.Photo;
            var photo = person.ProfilePath == null ? existing : await client.GetImageBytesAsync(size, person.ProfilePath, false, cancellationToken) ?? existing;
            people.Add(new PersonPhoto(person.Name, photo));
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (editor.Revision != revision || !previous.SequenceEqual(target.Select(person => (person.Name, person.Photo))))
        {
            return;
        }

        target.Clear();
        foreach (var person in people)
        {
            target.Add(new PersonEditorModel(person));
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
