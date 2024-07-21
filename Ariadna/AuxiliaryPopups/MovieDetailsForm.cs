using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;
using TMDbLib.Objects.General;
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
        m_TxtTitle.Text = m_TxtTitle.Text.RemoveExtensions();
        var length = Utilities.GetVideoDuration(FilePath);
        m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

        FetchClientConfig();

        using var ctx = new AriadnaEntities();
        var entry = ctx.Movies.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry != null)
        {
            StoredDbEntryId = entry.Id;
        }

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
    protected override bool DoStore()
    {
        // Prepare data
        m_DirectorsList.Capitalize();
        m_CastList.Capitalize();

        // Store tables with no references
        var bSuccess = StoreGenres();
        bSuccess = bSuccess && StoreCast();
        bSuccess = bSuccess && StoreDirectors();

        bSuccess = bSuccess && StoreEntry();

        if (!bSuccess)
        {
            return false;
        }

        if (StoredDbEntryId == -1)
        {
            return true;
        }

        // Store tables with references
        StoreMovieCast(StoredDbEntryId);
        StoreMovieDirectors(StoredDbEntryId);
        StoreEntryGenres(StoredDbEntryId);

        return true;
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
            client.SetConfig(TMDbJsonSerializer.Instance.DeserializeFromString<TMDbConfig>(json));
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
            var bts = await m_TmDbClient.GetImageBytesAsync(imgSize, urlOriginal);

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
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;
        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.Genres.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                ctx.Genres.Add(new Genre { name = item.Text });
                bNeedToSaveChanges = true;
            }
        }

        if (!bNeedToSaveChanges)
        {
            return true;
        }
            
        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(Resources.Oops, Resources.FailedToSaveGenres, MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        return bSuccess;
    }
    private bool StoreCast()
    {
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        foreach (ListViewItem item in m_CastList.Items)
        {
            var bAddEntry = false;
            var actor = ctx.Actors.FirstOrDefault(r => r.name == item.Text);
            if (actor == null)
            {
                bAddEntry = true;
                actor = new Actor();
            }
            actor.name = item.Text;
            actor.photo =m_CastPhotos.Images[item.Text].ToBytes();

            if (bAddEntry)
            {
                ctx.Actors.Add(actor);
            }
        }

        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(Resources.Oops, Resources.FailedToSaveCast, MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        return bSuccess;
    }
    private bool StoreDirectors()
    {
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        foreach (ListViewItem item in m_DirectorsList.Items)
        {
            var bAddEntry = false;
            var director = ctx.Directors.FirstOrDefault(r => r.name == item.Text);
            if (director == null)
            {
                bAddEntry = true;
                director = new Director();
            }

            director.name = item.Text;
            director.photo = m_DirectorsPhotos.Images[item.Text].ToBytes();

            if (bAddEntry)
            {
                ctx.Directors.Add(director);
            }
        }

        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(Resources.Oops, Resources.FailedToSaveDirectors, MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        return bSuccess;
    }
    private bool StoreEntry()
    {
        var title = m_TxtTitle.Text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        var bSuccess = true;
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

        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(title, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ctx.Movies.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault()!.Id;
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        try
        {
            m_PicPoster.Image.Save(Settings.Default.MoviePostersRootPath + StoredDbEntryId, ImageFormat.Png);
        }
        catch (Exception)
        {
            MessageBox.Show(Resources.FailedToSavePoster, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return true;
    }
    private void StoreMovieCast(int movieId)
    {
        if (m_CastList.Items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == movieId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_CastList.Items)
        {
            var actor = ctx.Actors.FirstOrDefault(r => r.name == item.Text);
            if (actor == null)
            {
                continue;
            }

            var movieCast = ctx.MovieCasts.FirstOrDefault(r => (r.movieId == movieId && r.actorId == actor.Id));
            if (movieCast == null)
            {
                ctx.MovieCasts.Add(new MovieCast { movieId = movieId, actorId = actor.Id });
                bNeedToSaveChanges = true;
            }
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    private void StoreMovieDirectors(int movieId)
    {
        if (m_DirectorsList.Items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == movieId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_DirectorsList.Items)
        {
            var director = ctx.Directors.FirstOrDefault(r => r.name == item.Text);
            if (director == null)
            {
                continue;
            }

            var movieDirector = ctx.MovieDirectors.FirstOrDefault(r => (r.movieId == movieId && r.directorId == director.Id));
            if (movieDirector == null)
            {
                ctx.MovieDirectors.Add(new MovieDirector { movieId = movieId, directorId = director.Id });
                bNeedToSaveChanges = true;
            }
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    private void StoreEntryGenres(int entryId)
    {
        if (m_GenresList.Items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == entryId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.Genres.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                continue;
            }

            var entryGenre = ctx.MovieGenres.FirstOrDefault(r => (r.movieId == entryId && r.genreId == genre.Id));
            if (entryGenre == null)
            {
                ctx.MovieGenres.Add(new MovieGenre { movieId = entryId, genreId = genre.Id });
                bNeedToSaveChanges = true;
            }
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    private void FillFieldsFromFile()
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Movies.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        m_TxtYear.Text = (entry.year > 0) ? entry.year.ToString() : string.Empty;
        m_TxtTitle.Text = entry.title;
        m_TxtTitleOrig.Text = entry.title_original;
        m_TxtPath.Text = entry.file_path;
        m_TxtDescription.Text = Utilities.DecorateDescription(entry.description);
        m_WantToSee.Checked = Convert.ToBoolean(entry.want_to_see);

        var filename = Settings.Default.MoviePostersRootPath + entry.Id;
        if (File.Exists(filename))
        {
            using var bmpTemp = new Bitmap(filename);
            m_PicPoster.Image = new Bitmap(bmpTemp);
        }

        var castSet = ctx.MovieCasts.AsNoTracking().ToArray().Where(r => (r.movieId == entry.Id));
        foreach (var cast in castSet)
        {
            AddNewListItem(m_CastList, m_CastPhotos,
                cast.Actor.name, cast.Actor.photo.ToBitmap());
        }

        var directorsSet = ctx.MovieDirectors.AsNoTracking().ToArray().Where(r => (r.movieId == entry.Id));
        foreach (var directors in directorsSet)
        {
            AddNewListItem(m_DirectorsList, m_DirectorsPhotos, directors.Director.name, directors.Director.photo.ToBitmap());
        }

        var genresSet = ctx.MovieGenres.AsNoTracking().ToArray().Where(r => (r.movieId == entry.Id));
        foreach (var genres in genresSet)
        {
            AddGenre(genres.Genre.name);
        }
    }
    private async void FetchPreviews(ListView listView, ImageList imageList)
    {
        foreach (ListViewItem item in listView.Items)
        {
            // Skip entries with valid preview set
            if (Utilities.IsValidPreview(imageList.Images[imageList.Images.IndexOfKey(item.Text)].ToBytes()))
            {
                continue;
            }

            var results = m_TmDbClient.SearchPersonAsync(item.Text, "ru-RU").Result;

            if (results.Results.Count <= 0)
            {
                continue;
            }

            var person = results.Results.First();
            if (person.ProfilePath == null)
            {
                continue;
            }

            // Download first available Photo
            var imgSize = m_TmDbClient.Config.Images.ProfileSizes.Last();
            var urlOriginal = m_TmDbClient.GetImageUrl(imgSize, person.ProfilePath).AbsoluteUri;
            var bts = await m_TmDbClient.GetImageBytesAsync(imgSize, urlOriginal);

            // Scale image
            var bmp = new Bitmap(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
            var graph = Graphics.FromImage(bmp);
            graph.DrawImage(bts.ToBitmap(), new Rectangle(0, 0, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight));

            // Set Photo image
            imageList.Images[imageList.Images.IndexOfKey(item.Text)] = bmp;
            listView.Refresh();
        }
    }
    private void AddNewListItem(ListView listView, ImageList imageList, string name, Bitmap image = null)
    {
        if (name == "↓")
        {
            return;
        }

        if (listView.FindItemWithText(name) != null)
        {
            return;
        }

        if (image == null)
        {
            using var ctx = new AriadnaEntities();
            var director = ctx.Directors.AsNoTracking().FirstOrDefault(r => r.name == name);
            if (director?.photo != null)
            {
                image = director.photo.ToBitmap();
            }

            if (image == null)
            {
                var actor = ctx.Actors.AsNoTracking().FirstOrDefault(r => r.name == name);
                if (actor?.photo != null && Utilities.IsValidPreview(actor.photo))
                {
                    image = actor.photo.ToBitmap();
                }
            }

            image ??= new Bitmap(Resources.No_Preview_Image_small);
        }

        imageList.Images.Add(name, image);
        listView.Items.Add(new ListViewItem(name, imageList.Images.IndexOfKey(name)));
    }
}