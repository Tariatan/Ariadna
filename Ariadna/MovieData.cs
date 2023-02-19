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
    public partial class MovieData : Form
    {
        public Utilities.EFormCloseReason FormCloseReason { get; set; }
        public string FilePath { get; set; }
        public int TMDBMovieIndex { get; set; }
        public int TMDBTVShowIndex { get; set; }

        private bool mIsInUpdateMode = false;
        private bool mIsShiftPressed = false;

        private FloatingPanel mFloatingPanel = new FloatingPanel();

        private Bitmap NO_PREVIEW_IMAGE_SMALL = new Bitmap(Properties.Resources.No_Preview_Image_small);

        const Int32 MAX_GENRE_COUNT_ALLOWED = 5;

        TMDbLib.Client.TMDbClient mTMDbClient = new TMDbLib.Client.TMDbClient(Utilities.TMDB_API_KEY);

        public MovieData(string filePath)
        {
            FilePath = filePath;

            InitializeComponent();
        }
        private void AddMovie_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(FilePath))
            {
                return;
            }

            mFloatingPanel.Deactivate += new System.EventHandler(OnFloatingPanelClosed);

            m_CastPhotos.ImageSize = new Size(Utilities.PHOTO_W, Utilities.PHOTO_H);
            m_DirectorsPhotos.ImageSize = new Size(Utilities.PHOTO_W, Utilities.PHOTO_H);

            // Get path name
            txtTitle.Text = FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
            // Remove extension
            txtTitle.Text = txtTitle.Text.Replace(".avi", "").Replace(".mkv", "").Replace(".m4v", "").Replace(".mpg", "").Replace(".ts", "");
            txtPath.Text = FilePath;

            mIsShiftPressed = false;

            FetchClientConfig();

            using (var ctx = new AriadnaEntities())
            {
                var movie = ctx.Movies.AsNoTracking().Where(r => r.file_path == FilePath).Select(x => x.file_path).FirstOrDefault();
                if (movie != null)
                {
                    FillFieldsFromFile();
                    mIsInUpdateMode = true;
                }
                else
                {
                    if (TMDBMovieIndex != -1)
                    {
                        FillMovieFieldsFromIMDB();
                    }
                    else if(TMDBTVShowIndex != -1)
                    {
                        FillTVShowFieldsFromIMDB();
                    }

                    mIsInUpdateMode = false;
                }
                UpdateInsertButtonText();
            }

            var length = GetVideoDuration(FilePath);
            txtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

        }
        private async void FetchClientConfig()
        {
            await FetchConfig(mTMDbClient);
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
            var entry = await mTMDbClient.GetMovieAsync(TMDBMovieIndex, "ru-RU");
            string year = (entry.ReleaseDate != null) ? entry.ReleaseDate.Value.Year.ToString() : "0";
            FillFields(entry.PosterPath, entry.OriginalTitle, entry.Overview, year, entry.Genres);
        }
        private async void FillTVShowFieldsFromIMDB()
        {
            var entry = await mTMDbClient.GetTvShowAsync(TMDBTVShowIndex, TMDbLib.Objects.TvShows.TvShowMethods.Undefined, "ru-RU");
            string year = (entry.FirstAirDate != null) ? entry.FirstAirDate.Value.Year.ToString() : "0";
            FillFields(entry.PosterPath, entry.OriginalName, entry.Overview, year, entry.Genres);
        }
        private async void FillFields(string posterPath, string origTitle, string overview, string year, List<TMDbLib.Objects.General.Genre> genres)
        {
            if (posterPath != null)
            {
                // Download first available Poster
                string imgSize = mTMDbClient.Config.Images.PosterSizes.Last();
                var UrlOriginal = mTMDbClient.GetImageUrl(imgSize, posterPath).AbsoluteUri;
                byte[] bts = await mTMDbClient.GetImageBytesAsync(imgSize, UrlOriginal);

                // Scale image
                var bmp = new Bitmap(Utilities.POSTER_W, Utilities.POSTER_H);
                Graphics graph = Graphics.FromImage(bmp);
                graph.DrawImage(Utilities.BytesToBitmap(bts), new Rectangle(0, 0, Utilities.POSTER_W, Utilities.POSTER_H));

                // Set Poster image
                picPoster.Image = bmp;
            }

            txtTitleOriginal.Text = origTitle;
            txtYear.Text = year;
            txtDescription.Text = overview;

            foreach (var genre in genres)
            {
                AddGenre(genre.Name);
            }
        }
        private void UpdateInsertButtonText()
        {
            if (mIsInUpdateMode)
            {
                Text = "Обновление записи";
                btnInsert.Text = "Обновить";
            }
            else
            {
                Text = "Добавление записи";
                btnInsert.Text = "Вставить";
            }
        }
        private void MovieData_KeyDown(object sender, KeyEventArgs e)
        {
            mIsShiftPressed = e.Modifiers.HasFlag(Keys.Shift);

            if (e.Modifiers.HasFlag(Keys.Shift))
            {
                btnInsert.Text = "В игнор";
            }

            if (e.KeyCode == Keys.Escape)
            {
                if (mFloatingPanel.Visible)
                {
                    mFloatingPanel.Hide();
                    return;
                }

                this.Close();
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                AddUpdateMovie();
                return;
            }
        }
        private void MovieData_KeyUp(object sender, KeyEventArgs e)
        {
            mIsShiftPressed = e.Modifiers.HasFlag(Keys.Shift);

            if (e.Modifiers.HasFlag(Keys.Shift) == false)
            {
                UpdateInsertButtonText();
            }
        }
        private void PicPoster_DoubleClick(object sender, EventArgs e)
        {
            if (GetBitmapFromDisk(out Bitmap bmp, "Картинка (400x600) (*.*)|*.*", Utilities.POSTER_W, Utilities.POSTER_H))
            {
                picPoster.Image = (bmp != null) ? bmp : new Bitmap(Properties.Resources.No_Preview_Image);
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
                if(prop.ValueAsObject == null)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromTicks((long)(ulong)prop.ValueAsObject);
            }
        }
        private void BtnInsert_Click(object sender, EventArgs e)
        {
            AddUpdateMovie();
        }
        private void AddToIgnoreList()
        {
            using (var ctx = new AriadnaEntities())
            {
                Ignore ignore = ctx.Ignores.Where(r => r.path == FilePath).FirstOrDefault();
                if (ignore == null)
                {
                    ctx.Ignores.Add(new Ignore { path = FilePath });
                    ctx.SaveChanges();
                }
            }

            Close();
        }
        private void AddUpdateMovie()
        {
            if (mIsShiftPressed)
            {
                AddToIgnoreList();
                return;
            }

            btnInsert.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            // Prepare data
            PrepareListView(m_DirectorsList);
            PrepareListView(m_CastList);

            // Store tables with no references
            bool bSuccess = StoreGenres();
            bSuccess = bSuccess && StoreCast();
            bSuccess = bSuccess && StoreDirectors();

            bSuccess = bSuccess && StoreMovie();

            if(bSuccess)
            {
                int movieId = -1;
                using (var ctx = new AriadnaEntities())
                {
                    movieId = ctx.Movies.Where(r => r.file_path == FilePath).Select(x => x.Id).FirstOrDefault();
                }

                if(movieId != -1)
                {
                    // Store tables with references
                    bSuccess = bSuccess && StoreMovieCast(movieId);
                    bSuccess = bSuccess && StoreMovieDirectors(movieId);
                    bSuccess = bSuccess && StoreMovieGenres(movieId);
                }
            }

            Cursor.Current = Cursors.Default;

            if (bSuccess)
            {
                FormCloseReason = Utilities.EFormCloseReason.SUCCESS;
                Close();
            }
            btnInsert.Enabled = true;
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
        private bool StoreMovie()
        {
            string movieTitle = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(movieTitle))
            {
                return false;
            }

            var poster = Utilities.ImageToBytes(picPoster.Image);
            if (poster == null)
            {
                MessageBox.Show("Ошибка сохранения постера", "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                Movie movie = ctx.Movies.Where(r => r.file_path == FilePath).FirstOrDefault();

                Int32 movieYear = 1900;
                Int32.TryParse(txtYear.Text, out movieYear);

                bool bAddMovie = false;
                if (movie == null)
                {
                    bAddMovie = true;
                    movie = new Movie();
                }

                movie.title = movieTitle;
                movie.title_original = txtTitleOriginal.Text.Trim();
                movie.year = movieYear;
                movie.length = TimeSpan.Parse(txtLength.Text);
                movie.file_path = txtPath.Text.Trim();
                movie.description = txtDescription.Text;
                movie.poster = poster;
                movie.creation_time = File.GetCreationTimeUtc(FilePath);
                movie.want_to_see = checkToSee.Checked;

                if (bAddMovie)
                {
                    ctx.Movies.Add(movie);
                }

                try
                {
                    ctx.SaveChanges();
                }
                catch (DbEntityValidationException)
                {
                    MessageBox.Show(movieTitle, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }
            }
            return bSuccess;
        }
        private bool StoreMovieCast(int movieId)
        {
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
        private bool StoreMovieGenres(int movieId)
        {
            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;

                ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == movieId)));
                ctx.SaveChanges();

                foreach (ListViewItem item in m_GenresList.Items)
                {
                    Genre genre = ctx.Genres.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        continue;
                    }

                    MovieGenre movieGenre = ctx.MovieGenres.Where(r => (r.movieId == movieId && r.genreId == genre.Id)).FirstOrDefault();
                    if (movieGenre == null)
                    {
                        ctx.MovieGenres.Add(new MovieGenre { movieId = movieId, genreId = genre.Id });
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
                Movie movie = ctx.Movies.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (movie == null)
                {
                    return;
                }

                txtYear.Text = (movie.year > 0) ? movie.year.ToString() : "";
                txtTitle.Text = movie.title;
                txtTitleOriginal.Text = movie.title_original;
                txtPath.Text = movie.file_path;
                txtDescription.Text = DecorateDescription(movie.description);
                checkToSee.Checked = Convert.ToBoolean(movie.want_to_see);

                picPoster.Image = (movie.poster.Length != 0) ? Utilities.BytesToBitmap(movie.poster) :
                                                               new Bitmap(Properties.Resources.No_Preview_Image);

                var castSet = ctx.MovieCasts.AsNoTracking().ToArray().Where(r => (r.movieId == movie.Id));
                foreach (var cast in castSet)
                {
                    AddNewListItem(m_CastList, m_CastPhotos,
                                   cast.Actor.name, Utilities.BytesToBitmap(cast.Actor.photo));
                }

                var directorsSet = ctx.MovieDirectors.AsNoTracking().ToArray().Where(r => (r.movieId == movie.Id));
                foreach (var directors in directorsSet)
                {
                    AddNewListItem(m_DirectorsList, m_DirectorsPhotos,
                                   directors.Director.name, Utilities.BytesToBitmap(directors.Director.photo));
                }

                var genresSet = ctx.MovieGenres.AsNoTracking().ToArray().Where(r => (r.movieId == movie.Id));
                foreach (var genres in genresSet)
                {
                    string name = genres.Genre.name;
                    m_GenresImages.Images.Add(name, Utilities.GetGenreImage(name));
                    m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));
                }
                m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
            }
        }
        private bool GetBitmapFromDisk(out Bitmap outBmp, string filter, int width, int height)
        {
            outBmp = null;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "T:\\Downloads\\";
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                var fileName = openFileDialog.FileName;
                var extStartPos = fileName.LastIndexOf('.') + 1;
                string ext = "";
                if (extStartPos > 0)
                {
                    ext = fileName.Substring(extStartPos).ToUpper();
                }

                // Check supported extensions
                HashSet<string> exts = new HashSet<string> { "BMP", "GIF", "EXIF", "JPG", "JPEG", "PNG", "TIFF" };
                if ((ext.Length > 0) && !exts.Contains(ext))
                {
                    return false;
                }

                outBmp = new Bitmap(width, height);
                Graphics graph = Graphics.FromImage(outBmp);

                Image img = new Bitmap(fileName);
                graph.DrawImage(img, new Rectangle(0, 0, width, height));

                return true;
            }
        }
        private void OnDirectorsKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.V))
            {
                AddListViewItemFromClipboard(m_DirectorsList, m_DirectorsPhotos);
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                DeleteFocusedListItem(m_DirectorsList);
                return;
            }

            if ((e.KeyData == Keys.F2) && (m_DirectorsList.SelectedItems.Count > 0))
            {
                m_DirectorsList.SelectedItems[0].BeginEdit();
                return;
            }
        }
        private void OnCastKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.V))
            {
                AddListViewItemFromClipboard(m_CastList, m_CastPhotos);
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                DeleteFocusedListItem(m_CastList);
                return;
            }

            if ((e.KeyData == Keys.F2) && (m_DirectorsList.SelectedItems.Count > 0))
            {
                m_CastList.SelectedItems[0].BeginEdit();
                return;
            }
        }
        private async void FetchPreviews(ListView listView, ImageList imageList)
        {
            Cursor.Current = Cursors.WaitCursor;

            foreach (ListViewItem item in listView.Items)
            {
                // Skip entries with valid preview set
                if(Utilities.IsValidPreview(Utilities.ImageToBytes(imageList.Images[imageList.Images.IndexOfKey(item.Text)])))
                {
                    continue;
                }

                TMDbLib.Objects.General.SearchContainer<TMDbLib.Objects.Search.SearchPerson> results = mTMDbClient.SearchPersonAsync(item.Text, "ru-RU").Result;

                if(results.Results.Count > 0)
                {
                    var person = results.Results.First();
                    if (person.ProfilePath == null)
                    {
                        continue;
                    }

                    // Download first available Photo
                    string imgSize = mTMDbClient.Config.Images.ProfileSizes.Last();
                    var UrlOriginal = mTMDbClient.GetImageUrl(imgSize, person.ProfilePath).AbsoluteUri;
                    byte[] bts = await mTMDbClient.GetImageBytesAsync(imgSize, UrlOriginal);

                    // Scale image
                    var bmp = new Bitmap(Utilities.PHOTO_W, Utilities.PHOTO_H);
                    Graphics graph = Graphics.FromImage(bmp);
                    graph.DrawImage(Utilities.BytesToBitmap(bts), new Rectangle(0, 0, Utilities.PHOTO_W, Utilities.PHOTO_H));

                    // Set Photo image
                    imageList.Images[imageList.Images.IndexOfKey(item.Text)] = bmp;
                    listView.Refresh();
                }
            }

            Cursor.Current = Cursors.Default;
        }
        private void OnDescriptionKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.V))
            {
                txtDescription.Text = DecorateDescription(txtDescription.Text);
                return;
            }
        }
        private void OnGenresKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.V))
            {
                AddGenresFromClipboard();
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                DeleteFocusedListItem(m_GenresList);
                m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);

                return;
            }
        }
        private void AddListViewItemFromClipboard(ListView listView, ImageList imageList)
        {
            foreach (var item in Clipboard.GetText().Split(','))
            {
                var name = Utilities.CapitalizeWords(item);

                AddNewListItem(listView, imageList, name);
            }

            FetchPreviews(listView, imageList);
        }
        private void AddNewListItem(ListView listView, ImageList imageList, string name, Bitmap image = null)
        {
            if (name == "↓")
            {
                return;
            }

            if(listView.FindItemWithText(name) != null)
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
        private void DeleteFocusedListItem(ListView listView)
        {
            if (listView.SelectedItems.Count > 0)
            {
                listView.Items.Remove(listView.FocusedItem);
            }
        }
        private void OnDirectorsDoubleClicked(object sender, MouseEventArgs e)
        {
            if (GetBitmapFromDisk(out Bitmap bmp,
                string.Format("Фото ({0}x{1}) (*.*)|*.*", Utilities.PHOTO_W, Utilities.PHOTO_H), Utilities.PHOTO_W, Utilities.PHOTO_H))
            {
                if (bmp == null)
                {
                    bmp = NO_PREVIEW_IMAGE_SMALL;
                }

                m_DirectorsPhotos.Images[m_DirectorsPhotos.Images.IndexOfKey(m_DirectorsList.FocusedItem.Text)] = bmp;
                m_DirectorsList.Refresh();
            }
        }
        private void OnCastDoubleClicked(object sender, MouseEventArgs e)
        {
            if (GetBitmapFromDisk(out Bitmap bmp,
                string.Format("Фото ({0}x{1}) (*.*)|*.*", Utilities.PHOTO_W, Utilities.PHOTO_H), Utilities.PHOTO_W, Utilities.PHOTO_H))
            {
                if (bmp == null)
                {
                    bmp = NO_PREVIEW_IMAGE_SMALL;
                }

                m_CastPhotos.Images[m_CastPhotos.Images.IndexOfKey(m_CastList.FocusedItem.Text)] = bmp;
                m_CastList.Refresh();
            }
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
        private void AddGenresFromClipboard()
        {
            foreach (var item in Clipboard.GetText().Split(','))
            {
                var name = item.Trim();
                AddGenre(name);

                if (m_GenresList.Items.Count == Utilities.MAX_GENRES_COUNT)
                {
                    break;
                }
            }
        }
        private void AddGenre(string name)
        {
            // Only single word allowed
            if (name.Contains(" "))
            {
                return;
            }

            if (m_GenresList.FindItemWithText(name) != null)
            {
                return;
            }

            name = Utilities.CapitalizeWords(name);
            name = Utilities.GetGenreBySynonym(name);
            m_GenresImages.Images.Add(name, Utilities.GetGenreImage(name));
            m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));

            m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
        }
        private void OnAddGenreClicked(object sender, EventArgs e)
        {
            if (mFloatingPanel.Visible)
            {
                mFloatingPanel.Hide();
                return;
            }

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            foreach (var name in Utilities.GENRES_LIST)
            {
                if (m_GenresList.FindItemWithText(name) != null)
                {
                    continue;
                }

                values[name] = Utilities.GetGenreImage(name);
            }

            mFloatingPanel.Location = new Point(this.Location.X + m_GenresList.Location.X + 12,
                                               this.Location.Y + m_GenresList.Location.Y + SystemInformation.CaptionHeight + m_GenresList.Size.Height + 12);
            mFloatingPanel.Size = new Size(m_GenresList.Size.Width, Utilities.GENRE_IMAGE_H * 7 - 10);
            mFloatingPanel.BackColor = this.BackColor;

            mFloatingPanel.UpdateListView(values, FloatingPanel.EPanelContentType.GENRES, false, false, Utilities.GENRE_IMAGE_W, Utilities.GENRE_IMAGE_H);
            if (!mFloatingPanel.Visible)
            {
                mFloatingPanel.Show(this);
            }
        }
        private void OnFloatingPanelClosed(object sender, EventArgs e)
        {
            if (mFloatingPanel.Visible)
            {
                return;
            }

            var name = mFloatingPanel.EntryNames.FirstOrDefault();
            if (name != null)
            {
                name = Utilities.CapitalizeWords(name);
                m_GenresImages.Images.Add(name, Utilities.GetGenreImage(name));
                m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));

                m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
            }
        }
        private void OnDescriptionPasteClick(object sender, EventArgs e)
        {
            txtDescription.Text = DecorateDescription(Clipboard.GetText());
        }
        private void OnGenrePasteClick(object sender, EventArgs e)
        {
            AddGenresFromClipboard();
        }
        private void OnDirectorPasteClick(object sender, EventArgs e)
        {
            AddListViewItemFromClipboard(m_DirectorsList, m_DirectorsPhotos);
        }
        private void OnCastPasteClick(object sender, EventArgs e)
        {
            AddListViewItemFromClipboard(m_CastList, m_CastPhotos);
        }
        private void OnDirectorRenamed(object sender, LabelEditEventArgs e)
        {
            if (e.Label == null)
            {
                return;
            }

            using (var ctx = new AriadnaEntities())
            {
                Director director = ctx.Directors.AsNoTracking().Where(r => r.name == e.Label).FirstOrDefault();
                if ((director != null) && (director.photo != null))
                {
                    m_DirectorsPhotos.Images.SetKeyName(m_DirectorsList.Items[e.Item].ImageIndex, e.Label);
                    m_DirectorsPhotos.Images[m_DirectorsPhotos.Images.IndexOfKey(e.Label)] = Utilities.BytesToBitmap(director.photo);
                    m_DirectorsList.Refresh();
                }
            }
        }
        private void OnActorRenamed(object sender, LabelEditEventArgs e)
        {
            if (e.Label == null)
            {
                return;
            }

            using (var ctx = new AriadnaEntities())
            {
                Actor actor = ctx.Actors.AsNoTracking().Where(r => r.name == e.Label).FirstOrDefault();
                if ((actor != null) && (actor.photo != null))
                {
                    m_CastPhotos.Images.SetKeyName(m_CastList.Items[e.Item].ImageIndex, e.Label);
                    m_CastPhotos.Images[m_CastPhotos.Images.IndexOfKey(e.Label)] = Utilities.BytesToBitmap(actor.photo);
                    m_CastList.Refresh();
                }
            }
        }
        private void HideFloatingPanel(object sender, EventArgs e)
        {
            if (mFloatingPanel.Visible)
            {
                mFloatingPanel.Hide();
            }
        }
    }
}