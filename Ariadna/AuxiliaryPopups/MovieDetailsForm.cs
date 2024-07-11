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
using Ariadna.Properties;
using MediaInfo;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;
using TMDbLib.Utilities.Serializer;

namespace Ariadna.AuxiliaryPopups
{
    public class MovieDetailsForm : DetailsForm
    {
        #region Public Fields
        public int TmdbMovieIndex { get; set; }
        public int TmdbTvShowIndex { get; set; }
        #endregion

        #region Private Fields
        private readonly TMDbClient m_TMDbClient = new TMDbClient(Settings.Default.TmdbApiKey);
        #endregion

        public MovieDetailsForm(string filePath) : base(filePath) { }
        #region OVERRIDEN FUNCTIONS
        protected override void DoLoad()
        {
            m_CastPhotos.ImageSize = new Size(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
            m_DirectorsPhotos.ImageSize = new Size(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);

            // Remove extension
            m_TxtTitle.Text = m_TxtTitle.Text.Replace(".avi", "").Replace(".mkv", "").Replace(".m4v", "").Replace(".mp4", "").Replace(".mpg", "").Replace(".ts", "").Replace(".mpeg", "");
            var length = Utilities.GetVideoDuration(FilePath);
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
                if (TmdbMovieIndex != -1)
                {
                    FillMovieFieldsFromIMDB();
                }
                else if (TmdbTvShowIndex != -1)
                {
                    FillTVShowFieldsFromIMDB();
                }
            }

            FillMediaInfo(FilePath);
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
        private static async Task FetchConfig(TMDbClient client)
        {
            FileInfo configJson = new FileInfo("config.json");

            if (configJson.Exists && configJson.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-10))
            {
                string json = File.ReadAllText(configJson.FullName, Encoding.UTF8);
                client.SetConfig(TMDbJsonSerializer.Instance.DeserializeFromString<TMDbConfig>(json));
            }
            else
            {
                TMDbConfig config = await client.GetConfigAsync();
                string json = TMDbJsonSerializer.Instance.SerializeToString(config);
                File.WriteAllText(configJson.FullName, json, Encoding.UTF8);
            }
        }
        private async void FillMovieFieldsFromIMDB()
        {
            var entry = await m_TMDbClient.GetMovieAsync(TmdbMovieIndex, "ru-RU");
            string year = (entry.ReleaseDate != null) ? entry.ReleaseDate.Value.Year.ToString() : "0";
            FillFields(entry.PosterPath, entry.OriginalTitle, entry.Overview, year, entry.Genres);
        }
        private async void FillTVShowFieldsFromIMDB()
        {
            var entry = await m_TMDbClient.GetTvShowAsync(TmdbTvShowIndex, TvShowMethods.Undefined, "ru-RU");
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
                var bmp = new Bitmap(Settings.Default.PosterWidth, Settings.Default.PosterHeight);
                Graphics graph = Graphics.FromImage(bmp);
                graph.DrawImage(Utilities.BytesToBitmap(bts), new Rectangle(0, 0, Settings.Default.PosterWidth, Settings.Default.PosterHeight));

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

                bool bAddEntry = false;
                if (entry == null)
                {
                    bAddEntry = true;
                    entry = new Movie();
                }

                Int32.TryParse(m_TxtYear.Text, out int year);

                entry.title = title;
                entry.title_original = m_TxtTitleOrig.Text.Trim();
                entry.year = year;
                entry.file_path = m_TxtPath.Text.Trim();
                entry.description = m_TxtDescription.Text;
                entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
                entry.want_to_see = m_WanToSee.Checked;

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
                    m_PicPoster.Image.Save(Settings.Default.MoviePostersRootPath + StoredDBEntryID, ImageFormat.Png);
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
                m_TxtDescription.Text = Utilities.DecorateDescription(entry.description);
                m_WanToSee.Checked = Convert.ToBoolean(entry.want_to_see);

                string filename = Settings.Default.MoviePostersRootPath + entry.Id;
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
        private void FillMediaInfo(string path)
        {
            if (Directory.Exists(path))
            {
                var firstFile = Directory.EnumerateFiles(path).FirstOrDefault();
                if(String.IsNullOrEmpty(firstFile))
                {
                    var firstSubDir = Directory.GetDirectories(path).FirstOrDefault();
                    firstFile = Directory.EnumerateFiles(firstSubDir).FirstOrDefault();
                }

                path = firstFile;
            }

            if(String.IsNullOrEmpty(path))
            {
                return;
            }

            var info = new MediaInfoWrapper(path);
            m_TxtDimension.Text = info.Width + "x" + info.Height;
            m_TxtBitrate.Text = (info.VideoRate / 1000000) + " Mbps";

            var audios = info.AudioStreams;
            List<PictureBox> flags = new List<PictureBox> { m_PicFlag1, m_PicFlag2, m_PicFlag3, m_PicFlag4 };
            foreach (var flag in flags)
            {
                flag.Image = null;
            }

            int index = 0;
            foreach (var stream in audios)
            {
                // Limit number of audio tracks
                if(index >= flags.Count)
                {
                    break;
                }

                if (stream.Language.Equals("Russian"))
                {
                    flags[index++].Image = Resources.ru;
                }
                else if (stream.Language.Equals("English"))
                {
                    flags[index++].Image = Resources.en;
                }
                else if (stream.Language.Equals("French"))
                {
                    flags[index++].Image = Resources.fr;
                }
                else if (stream.Language.Equals("Ukrainian"))
                {
                    flags[index++].Image = Resources.ua;
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

                SearchContainer<SearchPerson> results = m_TMDbClient.SearchPersonAsync(item.Text, "ru-RU").Result;

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
                    var bmp = new Bitmap(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
                    Graphics graph = Graphics.FromImage(bmp);
                    graph.DrawImage(Utilities.BytesToBitmap(bts), new Rectangle(0, 0, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight));

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