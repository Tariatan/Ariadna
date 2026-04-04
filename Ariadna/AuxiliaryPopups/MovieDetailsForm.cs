using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using DbProvider;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;
using TMDbLib.Objects.TvShows;
using TMDbLib.Utilities.Serializer;
namespace Ariadna.AuxiliaryPopups;

public class MovieDetailsForm(string filePath, ILogger logger) : DetailsForm(filePath, logger)
{
    #region Public Fields
    public int TmdbMovieIndex { get; set; }
    public int TmdbTvShowIndex { get; set; }
    #endregion

    #region Private Fields
    private readonly TMDbClient m_TmDbClient = new(Settings.Default.TmdbApiKey);
    #endregion

    #region OVERRIDEN FUNCTIONS
    protected override void DoLoad()
    {
        m_CastPhotos.ImageSize = new Size(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
        m_DirectorsPhotos.ImageSize = new Size(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);

        // Remove extension
        m_TxtTitle.Text = m_TxtTitle.Text.RemoveExtension();
        var length = Utilities.GetVideoDuration(FilePath);
        m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

        FetchClientConfig();

        StoredDbEntryId = GetStoredEntryId();

        if (StoredDbEntryId != -1)
        {
            FillFieldsFromFile();
        }
        else
        {
            if (TmdbMovieIndex != -1)
            {
                FillMovieFieldsFromImdb();
            }
            else if (TmdbTvShowIndex != -1)
            {
                FillTvShowFieldsFromImdb();
            }
        }

        FillMediaInfo(FilePath);
    }
    protected override void PrepareForStore()
    {
        m_DirectorsList.Capitalize();
        m_CastList.Capitalize();
    }
    protected override bool StorePreEntryData()
    {
        var bSuccess = StoreGenres();
        bSuccess = bSuccess && StoreCast();
        bSuccess = bSuccess && StoreDirectors();
        return bSuccess;
    }
    protected override bool StoreMainEntry()
    {
        return StoreEntry();
    }
    protected override void StoreRelatedData()
    {
        StoreMovieCast(StoredDbEntryId);
        StoreMovieDirectors(StoredDbEntryId);
        StoreEntryGenres(StoredDbEntryId);
    }

    protected override void DoAddListViewItemFromClipboard(ListView listView, ImageList imageList)
    {
        foreach (var item in Clipboard.GetText().Split(','))
        {
            AddNewListItem(listView, imageList, item.Capitalize());
        }

        FetchPreviews(listView, imageList);
    }
    protected override List<string> GetGenres()
    {
        return Utilities.MovieGenres.Keys.ToList();
    }
    protected override string GetGenreBySynonym(string name)
    {
        return Utilities.GetMovieGenreBySynonym(name);
    }
    protected override Bitmap GetGenreImage(string name)
    {
        return Utilities.GetMovieGenreImage(name);
    }
    protected virtual int GetStoredEntryId()
    {
        using var ctx = new AriadnaEntities();
        return ResolveStoredEntryId(ctx.Movies.AsNoTracking().Where(r => r.file_path == FilePath).Select(r => (int?)r.Id));
    }
    #endregion

    private async void FetchClientConfig()
    {
        await FetchConfig(m_TmDbClient);
    }
    private static async Task FetchConfig(TMDbClient client)
    {
        var configJson = new FileInfo("config.json");

        if (configJson.Exists && configJson.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-10))
        {
            var json = await File.ReadAllTextAsync(configJson.FullName, Encoding.UTF8);
            client.SetConfig(TMDbJsonSerializer.Instance.DeserializeFromString<TMDbLib.Objects.General.TMDbConfig>(json));
        }
        else
        {
            var config = await client.GetConfigAsync();
            var json = TMDbJsonSerializer.Instance.SerializeToString(config);
            await File.WriteAllTextAsync(configJson.FullName, json, Encoding.UTF8);
        }
    }
    private async void FillMovieFieldsFromImdb()
    {
        var entry = await m_TmDbClient.GetMovieAsync(TmdbMovieIndex, Settings.Default.ImdbLanguage);
        var year = entry.ReleaseDate != null ? entry.ReleaseDate.Value.Year.ToString() : "0";
        FillFields(entry.PosterPath, entry.OriginalTitle, entry.Overview, year, entry.Genres);
    }
    private async void FillTvShowFieldsFromImdb()
    {
        var entry = await m_TmDbClient.GetTvShowAsync(TmdbTvShowIndex, TvShowMethods.Undefined, Settings.Default.ImdbLanguage);
        var year = (entry.FirstAirDate != null) ? entry.FirstAirDate.Value.Year.ToString() : "0";
        FillFields(entry.PosterPath, entry.OriginalName, entry.Overview, year, entry.Genres);
    }
    private async void FillFields(string posterPath, string origTitle, string overview, string year, List<TMDbLib.Objects.General.Genre> genres)
    {
        if (posterPath != null)
        {
            // Download first available Poster
            var imgSize = m_TmDbClient.Config.Images.PosterSizes.Last();
            var urlOriginal = m_TmDbClient.GetImageUrl(imgSize, posterPath).AbsoluteUri;
            var bts = await m_TmDbClient.GetImageBytesAsync(imgSize, urlOriginal, false, CancellationToken.None);

            // Scale image
            var bmp = new Bitmap(Settings.Default.PosterWidth, Settings.Default.PosterHeight);
            var graph = Graphics.FromImage(bmp);
            graph.DrawImage(bts.ToBitmap(), new Rectangle(0, 0, Settings.Default.PosterWidth, Settings.Default.PosterHeight));

            // Set Poster image
            m_PicPoster.Image = new Bitmap(bmp);
        }

        m_TxtTitleOrig.Text = origTitle;
        m_TxtYear.Text = year;
        m_TxtDescription.Text = overview;

        foreach (var genre in genres)
        {
            AddGenre(genre.Name);
        }
    }
    private bool StoreGenres()
    {
        return SaveMissingListEntries(
            m_GenresList.Items,
            (ctx, name) =>
            {
                if (ctx.Genres.FirstOrDefault(r => r.name == name) != null)
                {
                    return false;
                }

                ctx.Genres.Add(new Genre { name = name });
                return true;
            },
            Resources.Oops,
            Resources.FailedToSaveGenres);
    }
    private bool StoreCast()
    {
        return SaveNamedPhotoEntries(
            m_CastList.Items,
            m_CastPhotos,
            (ctx, name, photo) =>
            {
                var bAddEntry = false;
                var actor = ctx.Actors.FirstOrDefault(r => r.name == name);
                if (actor == null)
                {
                    bAddEntry = true;
                    actor = new Actor();
                }

                actor.name = name;
                actor.photo = photo;

                if (bAddEntry)
                {
                    ctx.Actors.Add(actor);
                }

                return true;
            },
            Resources.Oops,
            Resources.FailedToSaveCast);
    }
    private bool StoreDirectors()
    {
        return SaveNamedPhotoEntries(
            m_DirectorsList.Items,
            m_DirectorsPhotos,
            (ctx, name, photo) =>
            {
                var bAddEntry = false;
                var director = ctx.Directors.FirstOrDefault(r => r.name == name);
                if (director == null)
                {
                    bAddEntry = true;
                    director = new Director();
                }

                director.name = name;
                director.photo = photo;

                if (bAddEntry)
                {
                    ctx.Directors.Add(director);
                }

                return true;
            },
            Resources.Oops,
            Resources.FailedToSaveDirectors);
    }
    private bool StoreEntry()
    {
        var title = m_TxtTitle.Text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        using var ctx = new AriadnaEntities();
        Movie entry = null;
        if (StoredDbEntryId != -1)
        {
            entry = ctx.Movies.FirstOrDefault(r => r.Id == StoredDbEntryId);
        }

        var bAddEntry = false;
        if (entry == null)
        {
            bAddEntry = true;
            entry = new Movie();
        }

        entry.title = title;
        entry.title_original = m_TxtTitleOrig.Text.Trim();
        entry.year = m_TxtYear.Text.ToInt();
        entry.file_path = m_TxtPath.Text.Trim();
        entry.description = m_TxtDescription.Text;
        entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
        entry.want_to_see = m_WantToSee.Checked;

        if (bAddEntry)
        {
            ctx.Movies.Add(entry);
        }

        var bSuccess = TrySaveChanges(ctx, title, Resources.FailedToSaveEntry);

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ResolveStoredEntryId(ctx.Movies.AsNoTracking().Where(r => r.file_path == path).Select(r => (int?)r.Id));
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        TrySavePng(m_PicPoster.Image, Settings.Default.MoviePostersRootPath + StoredDbEntryId, Resources.FailedToSaveEntry);

        return true;
    }
    private void StoreMovieCast(int movieId)
    {
        ReplaceListRelations(
            m_CastList.Items,
            ctx => ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => r.movieId == movieId)),
            (ctx, name) =>
            {
                var actor = ctx.Actors.FirstOrDefault(r => r.name == name);
                if (actor == null)
                {
                    return false;
                }

                if (ctx.MovieCasts.FirstOrDefault(r => r.movieId == movieId && r.actorId == actor.Id) != null)
                {
                    return false;
                }

                ctx.MovieCasts.Add(new MovieCast { movieId = movieId, actorId = actor.Id });
                return true;
            });
    }
    private void StoreMovieDirectors(int movieId)
    {
        ReplaceListRelations(
            m_DirectorsList.Items,
            ctx => ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => r.movieId == movieId)),
            (ctx, name) =>
            {
                var director = ctx.Directors.FirstOrDefault(r => r.name == name);
                if (director == null)
                {
                    return false;
                }

                if (ctx.MovieDirectors.FirstOrDefault(r => r.movieId == movieId && r.directorId == director.Id) != null)
                {
                    return false;
                }

                ctx.MovieDirectors.Add(new MovieDirector { movieId = movieId, directorId = director.Id });
                return true;
            });
    }
    private void StoreEntryGenres(int entryId)
    {
        ReplaceListRelations(
            m_GenresList.Items,
            ctx => ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => r.movieId == entryId)),
            (ctx, name) =>
            {
                var genre = ctx.Genres.FirstOrDefault(r => r.name == name);
                if (genre == null)
                {
                    return false;
                }

                if (ctx.MovieGenres.FirstOrDefault(r => r.movieId == entryId && r.genreId == genre.Id) != null)
                {
                    return false;
                }

                ctx.MovieGenres.Add(new MovieGenre { movieId = entryId, genreId = genre.Id });
                return true;
            });
    }
    private void FillFieldsFromFile()
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Movies.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        LoadBaseEntryFields(entry.title, entry.title_original, entry.year, entry.file_path);
        LoadDescribedEntryFields(entry.description, Convert.ToBoolean(entry.want_to_see));

        LoadPosterImage(Settings.Default.MoviePostersRootPath + entry.Id, m_PicPoster);

        LoadNamedItems(
            m_CastList,
            m_CastPhotos,
            ctx.MovieCasts.AsNoTracking().ToArray().Where(r => r.movieId == entry.Id),
            cast => cast.Actor.name,
            cast => cast.Actor.photo);

        LoadNamedItems(
            m_DirectorsList,
            m_DirectorsPhotos,
            ctx.MovieDirectors.AsNoTracking().ToArray().Where(r => r.movieId == entry.Id),
            directors => directors.Director.name,
            directors => directors.Director.photo);

        LoadGenres(
            ctx.MovieGenres.AsNoTracking().ToArray().Where(r => r.movieId == entry.Id),
            genres => genres.Genre.name);
    }
    private async void FetchPreviews(ListView listView, ImageList imageList)
    {
        var actorNames = (from ListViewItem item in listView.Items 
            where !Utilities.IsValidPreview(imageList.Images[imageList.Images.IndexOfKey(item.Text)].ToBytes())
            select item.Text).ToList();

        var downloadTasks = actorNames.Select(name => FetchAndDownloadActorPhotoAsync(name, listView, imageList));
        await Task.WhenAll(downloadTasks);
    }

    private async Task FetchAndDownloadActorPhotoAsync(string actorName, ListView listView, ImageList imageList)
    {
        try
        {
            // Search for the actor
            var searchResult = await m_TmDbClient.SearchPersonAsync(actorName, "ru-RU");
            var actor = searchResult.Results?.FirstOrDefault();

            if (actor == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(actor.ProfilePath))
            {
                return;
            }

            // Download first available Photo
            var imgSize = m_TmDbClient.Config.Images.ProfileSizes.Last();
            var urlOriginal = m_TmDbClient.GetImageUrl(imgSize, actor.ProfilePath).AbsoluteUri;
            var bts = await m_TmDbClient.GetImageBytesAsync(imgSize, urlOriginal);

            // Scale image
            var bmp = new Bitmap(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
            var graph = Graphics.FromImage(bmp);
            graph.DrawImage(bts.ToBitmap(), new Rectangle(0, 0, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight));

            // Set Photo image
            imageList.Images[imageList.Images.IndexOfKey(actorName)] = bmp;
            listView.Refresh();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}