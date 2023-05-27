using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TMDbLib.Utilities.Serializer;

namespace Ariadna
{
    public partial class MovieDetailsForm : DetailsForm
    {
        #region Public Fields
        public int TMDBMovieIndex { get; set; }
        public int TMDBTVShowIndex { get; set; }
        #endregion

        #region Private Fields
        private readonly TMDbLib.Client.TMDbClient m_TMDbClient = new TMDbLib.Client.TMDbClient(Utilities.TMDB_API_KEY);
        #endregion

        public MovieDetailsForm(string filePath) : base(filePath) { }
        #region OVERRIDEN FUNCTIONS
        protected override void DoLoad()
        {
            m_CastPhotos.ImageSize = new Size(Utilities.PHOTO_W, Utilities.PHOTO_H);
            m_DirectorsPhotos.ImageSize = new Size(Utilities.PHOTO_W, Utilities.PHOTO_H);

            // Remove extension
            m_TxtTitle.Text = m_TxtTitle.Text.Replace(".avi", "").Replace(".mkv", "").Replace(".m4v", "").Replace(".mp4", "").Replace(".mpg", "").Replace(".ts", "").Replace(".mpeg", "");
            var length = GetVideoDuration(FilePath);
            m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

            FetchClientConfig();

            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Movies.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (entry != null)
                {
                    StoredDBEntryID = entry.Id;
                }
            }

            if (StoredDBEntryID != -1)
            {
                FillFieldsFromFile();
            }
            else
            {
                if (TMDBMovieIndex != -1)
                {
                    FillMovieFieldsFromIMDB();
                }
                else if (TMDBTVShowIndex != -1)
                {
                    FillTVShowFieldsFromIMDB();
                }
            }
        }
        protected override bool DoStore()
        {
            // Prepare data
            PrepareListView(m_DirectorsList);
            PrepareListView(m_CastList);

            // Store tables with no references
            bool bSuccess = StoreGenres();
            bSuccess = bSuccess && StoreCast();
            bSuccess = bSuccess && StoreDirectors();

            bSuccess = bSuccess && StoreEntry();

            if (bSuccess)
            {
                if (StoredDBEntryID != -1)
                {
                    // Store tables with references
                    bSuccess = bSuccess && StoreMovieCast(StoredDBEntryID);
                    bSuccess = bSuccess && StoreMovieDirectors(StoredDBEntryID);
                    bSuccess = bSuccess && StoreEntryGenres(StoredDBEntryID);
                }
            }

            return bSuccess;
        }
        protected override void DoAddListViewItemFromClipboard(ListView listView, ImageList imageList)
        {
            foreach (var item in Clipboard.GetText().Split(','))
            {
                var name = Utilities.CapitalizeWords(item);

                AddNewListItem(listView, imageList, name);
            }

            FetchPreviews(listView, imageList);
        }
        protected override List<string> GetGenres()
        {
            return Utilities.MOVIE_GENRES.Keys.ToList();
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
            await FetchConfig(m_TMDbClient);
        }
        private static async Task FetchConfig(TMDbLib.Client.TMDbClient client)
        {
            FileInfo configJson = new FileInfo("config.json");

            if (configJson.Exists && configJson.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-10))
            {
                string json = File.ReadAllText(configJson.FullName, System.Text.Encoding.UTF8);
                client.SetConfig(TMDbLib.Utilities.Serializer.TMDbJsonSerializer.Instance.DeserializeFromString<TMDbLib.Objects.General.TMDbConfig>(json));
            }
            else
            {
                TMDbLib.Objects.General.TMDbConfig config = await client.GetConfigAsync();
                string json = TMDbJsonSerializer.Instance.SerializeToString(config);
                File.WriteAllText(configJson.FullName, json, System.Text.Encoding.UTF8);
            }
        }
        private async void FillMovieFieldsFromIMDB()
        {
            var entry = await m_TMDbClient.GetMovieAsync(TMDBMovieIndex, "ru-RU");
            string year = (entry.ReleaseDate != null) ? entry.ReleaseDate.Value.Year.ToString() : "0";
            FillFields(entry.PosterPath, entry.OriginalTitle, entry.Overview, year, entry.Genres);
        }
        private async void FillTVShowFieldsFromIMDB()
        {
            var entry = await m_TMDbClient.GetTvShowAsync(TMDBTVShowIndex, TMDbLib.Objects.TvShows.TvShowMethods.Undefined, "ru-RU");
            string year = (entry.FirstAirDate != null) ? entry.FirstAirDate.Value.Year.ToString() : "0";
            FillFields(entry.PosterPath, entry.OriginalName, entry.Overview, year, entry.Genres);
        }
        private async void FillFields(string posterPath, string origTitle, string overview, string year, List<TMDbLib.Objects.General.Genre> genres)
        {
            if (posterPath != null)
            {
                // Download first available Poster
                string imgSize = m_TMDbClient.Config.Images.PosterSizes.Last();
                var UrlOriginal = m_TMDbClient.GetImageUrl(imgSize, posterPath).AbsoluteUri;
                byte[] bts = await m_TMDbClient.GetImageBytesAsync(imgSize, UrlOriginal);

                // Scale image
                var bmp = new Bitmap(Utilities.POSTER_W, Utilities.POSTER_H);
                Graphics graph = Graphics.FromImage(bmp);
                graph.DrawImage(Utilities.BytesToBitmap(bts), new Rectangle(0, 0, Utilities.POSTER_W, Utilities.POSTER_H));

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
        private static TimeSpan GetVideoDuration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return TimeSpan.Zero;
            }

            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                if (prop.ValueAsObject == null)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromTicks((long)(ulong)prop.ValueAsObject);
            }
        }
        private void PrepareListView(ListView listView)
        {
            foreach (ListViewItem item in listView.Items)
            {
                item.Text = Utilities.CapitalizeWords(item.Text.Trim());
            }
        }
        private bool StoreGenres()
        {
            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;
                foreach (ListViewItem item in m_GenresList.Items)
                {
                    Genre genre = ctx.Genres.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        ctx.Genres.Add(new Genre { name = item.Text });
                        bNeedToSaveChanges = true;
                    }
                }

                if (bNeedToSaveChanges)
                {
                    try
                    {
                        ctx.SaveChanges();
                    }
                    catch (DbEntityValidationException)
                    {
                        MessageBox.Show("Ой", "Ошибка сохранения списка жанров", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        bSuccess = false;
                    }
                }
            }
            return bSuccess;
        }
        private bool StoreCast()
        {
            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                foreach (ListViewItem item in m_CastList.Items)
                {
                    bool bAddEntry = false;
                    Actor actor = ctx.Actors.Where(r => r.name == item.Text).FirstOrDefault();
                    if (actor == null)
                    {
                        bAddEntry = true;
                        actor = new Actor();
                    }
                    actor.name = item.Text;
                    actor.photo = Utilities.ImageToBytes(m_CastPhotos.Images[item.Text]);

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
                    MessageBox.Show("Ой!", "Ошибка сохранения списка актеров", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }
            }

            return bSuccess;
        }
        private bool StoreDirectors()
        {
            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                foreach (ListViewItem item in m_DirectorsList.Items)
                {
                    bool bAddEntry = false;
                    Director director = ctx.Directors.Where(r => r.name == item.Text).FirstOrDefault();
                    if (director == null)
                    {
                        bAddEntry = true;
                        director = new Director();
                    }

                    director.name = item.Text;
                    director.photo = Utilities.ImageToBytes(m_DirectorsPhotos.Images[item.Text]);

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
                    MessageBox.Show("Ой!", "Ошибка сохранения списка режисеров", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }
            }
            return bSuccess;
        }
        private bool StoreEntry()
        {
            string title = m_TxtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                return false;
            }

            bool bSuccess = true;
            string path = "";
            using (var ctx = new AriadnaEntities())
            {
                Movie entry = null;
                if (StoredDBEntryID != -1)
                {
                    entry = ctx.Movies.Where(r => r.Id == StoredDBEntryID).FirstOrDefault();
                }

                bool bAddMovie = false;
                if (entry == null)
                {
                    bAddMovie = true;
                    entry = new Movie();
                }

                Int32.TryParse(m_TxtYear.Text, out int movieYear);

                entry.title = title;
                entry.title_original = m_TxtTitleOrig.Text.Trim();
                entry.year = movieYear;
                entry.length = TimeSpan.Parse(m_TxtLength.Text);
                entry.file_path = m_TxtPath.Text.Trim();
                entry.description = m_TxtDescription.Text;
                entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
                entry.want_to_see = m_WanToSee.Checked;

                if (bAddMovie)
                {
                    ctx.Movies.Add(entry);
                }

                try
                {
                    ctx.SaveChanges();
                }
                catch (DbEntityValidationException)
                {
                    MessageBox.Show(title, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }

                path = entry.file_path;
            }

            if (bSuccess)
            {
                using (var ctx = new AriadnaEntities())
                {
                    StoredDBEntryID = ctx.Movies.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault().Id;
                    bSuccess = (StoredDBEntryID != -1);
                }
            }

            if (bSuccess)
            {
                try
                {
                    m_PicPoster.Image.Save(Utilities.MOVIE_POSTERS_ROOT_PATH + StoredDBEntryID.ToString(), System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка сохранения постера", "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return bSuccess;
        }
        private bool StoreMovieCast(int movieId)
        {
            if (m_CastList.Items.Count == 0)
            {
                return true;
            }

            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;

                ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == movieId)));
                ctx.SaveChanges();

                foreach (ListViewItem item in m_CastList.Items)
                {
                    Actor actor = ctx.Actors.Where(r => r.name == item.Text).FirstOrDefault();
                    if (actor == null)
                    {
                        continue;
                    }

                    MovieCast movieCast = ctx.MovieCasts.Where(r => (r.movieId == movieId && r.actorId == actor.Id)).FirstOrDefault();
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
            return bSuccess;
        }
        private bool StoreMovieDirectors(int movieId)
        {
            if (m_DirectorsList.Items.Count == 0)
            {
                return true;
            }

            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;

                ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == movieId)));
                ctx.SaveChanges();

                foreach (ListViewItem item in m_DirectorsList.Items)
                {
                    Director director = ctx.Directors.Where(r => r.name == item.Text).FirstOrDefault();
                    if (director == null)
                    {
                        continue;
                    }

                    MovieDirector movieDirector = ctx.MovieDirectors.Where(r => (r.movieId == movieId && r.directorId == director.Id)).FirstOrDefault();
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
            return bSuccess;
        }
        private bool StoreEntryGenres(int entryId)
        {
            if (m_GenresList.Items.Count == 0)
            {
                return true;
            }

            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;

                ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == entryId)));
                ctx.SaveChanges();

                foreach (ListViewItem item in m_GenresList.Items)
                {
                    Genre genre = ctx.Genres.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        continue;
                    }

                    var entryGenre = ctx.MovieGenres.Where(r => (r.movieId == entryId && r.genreId == genre.Id)).FirstOrDefault();
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
            return bSuccess;
        }
        private void FillFieldsFromFile()
        {
            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Movies.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (entry == null)
                {
                    return;
                }

                m_TxtYear.Text = (entry.year > 0) ? entry.year.ToString() : "";
                m_TxtTitle.Text = entry.title;
                m_TxtTitleOrig.Text = entry.title_original;
                m_TxtPath.Text = entry.file_path;
                m_TxtDescription.Text = DecorateDescription(entry.description);
                m_WanToSee.Checked = Convert.ToBoolean(entry.want_to_see);

                string filename = Utilities.MOVIE_POSTERS_ROOT_PATH + entry.Id.ToString();
                if (File.Exists(filename))
                {
                    using (var bmpTemp = new Bitmap(filename))
                    {
                        m_PicPoster.Image = new Bitmap(bmpTemp);
                    }
                }

                var castSet = ctx.MovieCasts.AsNoTracking().ToArray().Where(r => (r.movieId == entry.Id));
                foreach (var cast in castSet)
                {
                    AddNewListItem(m_CastList, m_CastPhotos,
                                   cast.Actor.name, Utilities.BytesToBitmap(cast.Actor.photo));
                }

                var directorsSet = ctx.MovieDirectors.AsNoTracking().ToArray().Where(r => (r.movieId == entry.Id));
                foreach (var directors in directorsSet)
                {
                    AddNewListItem(m_DirectorsList, m_DirectorsPhotos,
                                   directors.Director.name, Utilities.BytesToBitmap(directors.Director.photo));
                }

                var genresSet = ctx.MovieGenres.AsNoTracking().ToArray().Where(r => (r.movieId == entry.Id));
                foreach (var genres in genresSet)
                {
                    AddGenre(genres.Genre.name);
                }
            }
        }
        private async void FetchPreviews(ListView listView, ImageList imageList)
        {
            foreach (ListViewItem item in listView.Items)
            {
                // Skip entries with valid preview set
                if (Utilities.IsValidPreview(Utilities.ImageToBytes(imageList.Images[imageList.Images.IndexOfKey(item.Text)])))
                {
                    continue;
                }

                TMDbLib.Objects.General.SearchContainer<TMDbLib.Objects.Search.SearchPerson> results = m_TMDbClient.SearchPersonAsync(item.Text, "ru-RU").Result;

                if (results.Results.Count > 0)
                {
                    var person = results.Results.First();
                    if (person.ProfilePath == null)
                    {
                        continue;
                    }

                    // Download first available Photo
                    string imgSize = m_TMDbClient.Config.Images.ProfileSizes.Last();
                    var UrlOriginal = m_TMDbClient.GetImageUrl(imgSize, person.ProfilePath).AbsoluteUri;
                    byte[] bts = await m_TMDbClient.GetImageBytesAsync(imgSize, UrlOriginal);

                    // Scale image
                    var bmp = new Bitmap(Utilities.PHOTO_W, Utilities.PHOTO_H);
                    Graphics graph = Graphics.FromImage(bmp);
                    graph.DrawImage(Utilities.BytesToBitmap(bts), new Rectangle(0, 0, Utilities.PHOTO_W, Utilities.PHOTO_H));

                    // Set Photo image
                    imageList.Images[imageList.Images.IndexOfKey(item.Text)] = bmp;
                    listView.Refresh();
                }
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
                using (var ctx = new AriadnaEntities())
                {
                    Director director = ctx.Directors.AsNoTracking().Where(r => r.name == name).FirstOrDefault();
                    if (director != null)
                    {
                        if (director.photo != null)
                        {
                            image = Utilities.BytesToBitmap(director.photo);
                        }
                    }
                    if (image == null)
                    {
                        Actor actor = ctx.Actors.AsNoTracking().Where(r => r.name == name).FirstOrDefault();
                        if (actor != null)
                        {
                            if ((actor.photo != null) && Utilities.IsValidPreview(actor.photo))
                            {
                                image = Utilities.BytesToBitmap(actor.photo);
                            }
                        }
                    }
                }
                if (image == null)
                {
                    image = NO_PREVIEW_IMAGE_SMALL;
                }
            }

            imageList.Images.Add(name, image);
            listView.Items.Add(new ListViewItem(name, imageList.Images.IndexOfKey(name)));
        }
        private string DecorateDescription(string description)
        {
            var paragraphs = description.Split('\n');
            var decoratedText = "";
            foreach (var paragraph in paragraphs)
            {
                if (decoratedText.Length > 0)
                {
                    decoratedText += "\r\n";
                }
                decoratedText += "\t" + paragraph.Trim();
            }

            return decoratedText;
        }
    }
}