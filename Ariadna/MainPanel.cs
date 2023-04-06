using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Manina.Windows.Forms;

namespace Ariadna
{
    public partial class MainPanel : Form
    {
        #region Private Fields
        private bool m_SuppressNameChangedEvent = false;

        private ImageFromFileAdaptor m_PosterImageAdaptor = new ImageFromFileAdaptor();
        private ImageListViewAriadnaRenderer m_ListViewRenderer = new ImageListViewAriadnaRenderer();

        private FloatingPanel m_FloatingPanel = new FloatingPanel();

        private System.Windows.Forms.Timer m_TypeTimer = new System.Windows.Forms.Timer();
        private enum ETypeField { None = 0, TITLE, DIRECTOR, ACTOR }
        private ETypeField m_TypeField = ETypeField.None;
        private const int TYPE_TIMOUT_MS = 200;

        private const int MAX_SEARCH_FILTER_COUNT = 200;

        private const string EMPTY_DOTS = ". . .";
        private const string MEDIA_PLAYER_PATH = "C:/Program Files/MEDIA/K-Lite Codec Pack/MPC-HC64/mpc-hc64.exe";
        private const string DEFAULT_MOVIES_PATH = @"M:\";
        private const string DEFAULT_SERIES_PATH = @"S:\";
        #endregion

        public MainPanel()
        {
            InitializeComponent();

            m_PosterImageAdaptor.RootPath = Utilities.POSTERS_ROOT_PATH;

            m_ImageListView.SetRenderer(m_ListViewRenderer);

            using (var ctx = new AriadnaEntities())
            {
                var movies = ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList();
                UpdateMoviesList(movies);
            }

            // Type timer
            m_TypeField = ETypeField.None;
            m_TypeTimer.Tick += new EventHandler(onTypeTimer);
            m_TypeTimer.Interval = TYPE_TIMOUT_MS;
        }
        private void MainPanel_Load(object sender, EventArgs e)
        {
            // Bring MainPanel to front
            Activate();

            Splasher.Close();

            m_ToolStrip_MovieName.Focus();
        }
        private void UpdateMoviesList(List<Utilities.MovieDto> movies)
        {
            m_ToolStripEntriesCount.Text = movies.Count().ToString();

            m_ImageListView.Items.Clear();

            HashSet<string> firstChars = new HashSet<string>(movies.Count);
            List<ImageListViewItem> listViewItems = new List<ImageListViewItem>(movies.Count);
            foreach (var movie in movies)
            {
                firstChars.Add(movie.title.Substring(0, 1));

                ImageListViewItem item = new ImageListViewItem(movie.id.ToString(), movie.title);
                listViewItems.Add(item);
            }

            m_ImageListView.Items.AddRange(listViewItems.ToArray(), m_PosterImageAdaptor);

            FillQuickList(firstChars);
        }
        private void FillQuickList(HashSet<string> firstChars)
        {
            m_QuickListFlow.Controls.Clear();

            foreach (var firstChar in firstChars)
            {
                Button btn = new Button
                {
                    Text = firstChar,
                    AutoSize = false,
                    Size = new Size(40, 40),
                    BackColor = Color.DarkMagenta,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold),
                };
                btn.Click += new System.EventHandler(OnQuickListClicked);
                m_QuickListFlow.Controls.Add(btn);
            }
        }
        private void OnQuickListClicked(object sender, EventArgs e)
        {
            Button charBtn = (Button)sender;

            var selection = m_ImageListView.Items.Where(x => x.Text.StartsWith(charBtn.Text)).FirstOrDefault();
            m_ImageListView.EnsureVisible(selection.Index);
            selection.Selected = true;
        }
        private void MainPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteMovie(e.Modifiers.HasFlag(Keys.Shift));
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                HideFloatingPanel();
                return;
            }
        }
        private void MainPanel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == '+')
            {
                e.Handled = true;

                if (FindFirstNotInsertedMovie())
                {
                    return;
                }
                else if (FindFirstNotInsertedSeries())
                {
                    return;
                }

                AddNewMovie();
            }
        }
        private string FindStoredMoviePathById(string sId)
        {
            Int32 id = -1;
            Int32.TryParse(sId, out id);
            if (id != -1)
            {
                using (var ctx = new AriadnaEntities())
                {
                    var path = ctx.Movies.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault().file_path;
                    if (!string.IsNullOrEmpty(path))
                    {
                        return path;
                    }
                }
            }

            return "";
        }
        private bool FindFirstNotInsertedMovie()
        {
            return FindFirstNotInserted(Directory.GetFiles(DEFAULT_MOVIES_PATH));
        }
        private bool FindFirstNotInsertedSeries()
        {
            return FindFirstNotInserted(Directory.GetDirectories(DEFAULT_SERIES_PATH));
        }
        private bool FindFirstNotInserted(String[] paths)
        {
            Cursor.Current = Cursors.WaitCursor;

            bool bFound = false;
            using (var ctx = new AriadnaEntities())
            {
                foreach (var path in paths)
                {
                    if (ctx.Ignores.AsNoTracking().Where(r => r.path == path).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    if (ctx.Movies.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() == null)
                    {
                        OpenAddMovieFormDialog(path);
                        bFound = true;

                        break;
                    }
                }
            }

            Cursor.Current = Cursors.Default;

            return bFound;
        }
        private void AddNewMovie()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = DEFAULT_MOVIES_PATH;
                openFileDialog.Filter = "Видео файлы|*.avi;*.mkv;*.mpg;*.mp4;*.m4v;*.ts|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                // Allow folders too
                openFileDialog.ValidateNames = false;
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                const string folderFlag = "File or folder";
                openFileDialog.FileName = folderFlag;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = openFileDialog.FileName;
                    if (path.Contains(folderFlag))
                    {
                        path = Path.GetDirectoryName(openFileDialog.FileName);
                    }
                    OpenAddMovieFormDialog(path);
                }
            }
        }
        private void DeleteMovie(bool deleteFile = false)
        {
            if (m_ImageListView.Items.FocusedItem == null)
            {
                return;
            }

            Int32 id = -1;
            Int32.TryParse((string)m_ImageListView.Items.FocusedItem.VirtualItemKey, out id);
            if (id == -1)
            {
                return;
            }

            using (var ctx = new AriadnaEntities())
            {
                Movie movie = ctx.Movies.Where(r => r.Id == id).FirstOrDefault();
                if (movie == null)
                {
                    return;
                }

                string msg = movie.title + " / " + movie.title_original + "\n" + movie.file_path;
                string caption = deleteFile ? "Удалить запись и файл?" : "Удалить запись?";
                DialogResult dialogResult = MessageBox.Show(msg, caption, MessageBoxButtons.YesNoCancel, deleteFile ? MessageBoxIcon.Warning : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }

                ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == movie.Id)));
                ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == movie.Id)));
                ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == movie.Id)));

                ctx.Movies.Remove(movie);

                ctx.SaveChanges();

                string posterPath = Utilities.POSTERS_ROOT_PATH + movie.Id;
                if (File.Exists(posterPath))
                {
                    File.Delete(posterPath);
                }

                UpdateMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());

                m_ToolStrip_MovieName.Text = "";

                if (!deleteFile)
                {
                    return;
                }

                if (File.Exists(movie.file_path))
                {
                    // Delete file
                    File.Delete(movie.file_path);
                }
                // Checked if it is a directory
                else if (Directory.Exists(movie.file_path))
                {
                    try
                    {
                        var dir = new DirectoryInfo(movie.file_path);
                        dir.Attributes = dir.Attributes & ~FileAttributes.ReadOnly;
                        dir.Delete(true);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show(movie.file_path, "Путь не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void OpenAddMovieFormDialog(string path)
        {
            FetchMovieFromIMDB(path);
        }
        private async void FetchMovieFromIMDB(string path)
        {
            TMDbLib.Client.TMDbClient client = new TMDbLib.Client.TMDbClient(Utilities.TMDB_API_KEY);
            MovieData addMovie = new MovieData(path);
            addMovie.TMDBMovieIndex = -1;
            addMovie.TMDBTVShowIndex = -1;

            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                var tvShowsResults = await client.SearchTvShowAsync(dir.Name, "ru-RU");
                var tvShows = tvShowsResults.Results;
                if (tvShows.Count > 0)
                {
                    int choice = (tvShows.Count > 1) ? GetBestTVShowChoice(tvShows, path) : 0;
                    if(choice >= 0)
                    {
                        addMovie.TMDBTVShowIndex = tvShows[choice].Id;
                    }
                }
            }
            else
            {
                string query = Path.GetFileNameWithoutExtension(path);
                var moviesResults = await client.SearchMovieAsync(query, "ru-RU");
                var movies = moviesResults.Results;

                if (movies.Count > 0)
                {
                    int choice = (movies.Count > 1) ? GetBestMovieChoice(movies, path) : 0;
                    if (choice >= 0)
                    {
                        addMovie.TMDBMovieIndex = movies[choice].Id;
                    }
                }
            }

            addMovie.FormClosed += new FormClosedEventHandler(OnAddMovieFormClosed);
            addMovie.ShowDialog();
        }
        private int GetBestMovieChoice(List<TMDbLib.Objects.Search.SearchMovie> movies, string path)
        {
            List<Utilities.MovieChoiceDto> titles = new List<Utilities.MovieChoiceDto>();
            foreach (var result in movies.Take(20))
            {
                int y = (result.ReleaseDate != null) ? result.ReleaseDate.Value.Year : 0;
                titles.Add(new Utilities.MovieChoiceDto { titleRu = result.Title, titleOrig = result.OriginalTitle, year = y });
            }
            ChoicePopup choice = new ChoicePopup(path, titles);

            choice.ShowDialog(this);
            return choice.index;
        }
        private int GetBestTVShowChoice(List<TMDbLib.Objects.Search.SearchTv> movies, string path)
        {
            List<Utilities.MovieChoiceDto> titles = new List<Utilities.MovieChoiceDto>();
            foreach (var result in movies.Take(20))
            {
                int y = (result.FirstAirDate != null) ? result.FirstAirDate.Value.Year : 0;
                titles.Add(new Utilities.MovieChoiceDto { titleRu = result.Name, titleOrig = result.OriginalName, year = y });
            }
            ChoicePopup choice = new ChoicePopup(path, titles);

            choice.ShowDialog(this);
            return choice.index;
        }
        private void OnAddMovieFormClosed(object sender, FormClosedEventArgs e)
        {
            MovieData movieData = (MovieData)sender;
            if (movieData.FormCloseReason != Utilities.EFormCloseReason.SUCCESS)
            {
                return;
            }

            using (var ctx = new AriadnaEntities())
            {
                UpdateMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());
            }

            var selection = m_ImageListView.Items.Where(x => (string)x.VirtualItemKey == movieData.StoredDBMovieID.ToString()).FirstOrDefault();
            m_ImageListView.EnsureVisible(selection.Index);
            selection.Selected = true;
        }
        #region Image List View handlers
        private void ListView_ItemSelectionChanged(object sender, EventArgs e)
        {
            m_ListViewRenderer.Blink();
        }
        private void ListView_MouseClicked(object sender, MouseEventArgs e)
        {
            HideFloatingPanel();
            if(e.Button == MouseButtons.Left)
            {
                return;
            }

            // Show Movie details on Mouse Right Click
            ImageListView lv = sender as ImageListView;
            if (lv.Items.FocusedItem == null)
            {
                return;
            }
            
            var path = FindStoredMoviePathById((string)lv.Items.FocusedItem.VirtualItemKey);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            MovieData addMovie = new MovieData(path);
            addMovie.FormClosed += new FormClosedEventHandler(OnAddMovieFormClosed);
            addMovie.ShowDialog();
        }
        private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ImageListView lv = sender as ImageListView;
            var path = FindStoredMoviePathById((string)lv.Items.FocusedItem.VirtualItemKey);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Check if it is a file first
            if (File.Exists(path))
            {
                if (!File.Exists(MEDIA_PLAYER_PATH))
                {
                    return;
                }

                // Enclose the path in quotes as required by MPC
                Process.Start(MEDIA_PLAYER_PATH, "\"" + path + "\"");
            }
            // Checked if it is a directory
            else if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                MessageBox.Show(path, "Путь не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion
        private void QueryMovies()
        {
            HideFloatingPanel();

            Cursor.Current = Cursors.WaitCursor;

            using (var ctx = new AriadnaEntities())
            {
                IQueryable<Movie> query = ctx.Movies.AsNoTracking();

                // -- Search Movie Name --
                if (m_ToolStrip_MovieName.Text.Length > 0)
                {
                    var toSearch = m_ToolStrip_MovieName.Text.ToUpper();
                    query = query.Where(r => r.title.ToUpper().Contains(toSearch) || 
                                             r.title_original.ToUpper().Contains(toSearch) ||
                                             r.file_path.ToUpper().Contains(toSearch));
                }
                // -- WISH LIST --
                if (m_ToolStrip_WishlistBtn.Checked)
                {
                    query = query.Where(r => (r.want_to_see == true));
                }
                // -- RECENTLY Added --
                if (m_ToolStrip_RecentBtn.Checked)
                {
                    var now = DateTime.Now.AddMonths(-6);
                    var ct = DateTime.Now;
                    query = query.Where(r => ((r.creation_time.Value.Year >= now.Year) && (r.creation_time.Value.Month >= now.Month)));
                }
                // -- NEW Movies --
                if (m_ToolStrip_NewBtn.Checked)
                {
                    query = query.Where(r => ((r.year == (DateTime.Now.Year)) || r.year == (DateTime.Now.Year - 1)));
                }
                // -- DIRECTOR NAME --
                if (m_ToolStrip_DirectorName.Text.Length > 0)
                {
                    var entry = ctx.Directors.AsNoTracking().Where(r => r.name == m_ToolStrip_DirectorName.Text).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieDirectors.Any(l => (l.directorId == entry.Id)));
                    }
                }
                // -- ACTOR NAME --
                if (m_ToolStrip_ActorName.Text.Length > 0)
                {
                    var entry = ctx.Actors.AsNoTracking().Where(r => r.name == m_ToolStrip_ActorName.Text).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieCasts.Any(l => (l.actorId == entry.Id)));
                    }
                }
                // -- GENRE --
                if (m_ToolStrip_GenreName.Text.Length > 0)
                {
                    var entry = ctx.Genres.AsNoTracking().Where(r => r.name == m_ToolStrip_GenreName.Text).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieGenres.Any(l => (l.genreId == entry.Id)));
                    }
                }

                UpdateMoviesList(query.OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());
            }

            Cursor.Current = Cursors.Default;
        }
        #region ToolStrip Handlers
        private void ToolStrip_AddBtn_MouseUp(object sender, MouseEventArgs e)
        {
            // Auto search on left click
            if (e.Button == MouseButtons.Left)
            {
                if (FindFirstNotInsertedMovie() || FindFirstNotInsertedSeries())
                {
                    return;
                }
            }

            AddNewMovie();
        }
        private void ToolStrip_RecentBtn_Clicked(object sender, EventArgs e)
        {
            if (m_ToolStrip_RecentBtn.Checked)
            {
                m_ToolStrip_RecentBtn.Image = Properties.Resources.icon_unchecked;
                m_ToolStrip_RecentBtn.Checked = false;
            }
            else
            {
                m_ToolStrip_RecentBtn.Image = Properties.Resources.icon_checked;
                m_ToolStrip_RecentBtn.Checked = true;
            }

            QueryMovies();
        }
        private void ToolStrip_NewBtn_Clicked(object sender, EventArgs e)
        {
            if (m_ToolStrip_NewBtn.Checked)
            {
                m_ToolStrip_NewBtn.Image = Properties.Resources.icon_unchecked;
                m_ToolStrip_NewBtn.Checked = false;
            }
            else
            {
                m_ToolStrip_NewBtn.Image = Properties.Resources.icon_checked;
                m_ToolStrip_NewBtn.Checked = true;
            }

            QueryMovies();
        }
        private void ToolStrip_WishlistBtn_Clicked(object sender, EventArgs e)
        {
            if (m_ToolStrip_WishlistBtn.Checked)
            {
                m_ToolStrip_WishlistBtn.Image = Properties.Resources.icon_unchecked;
                m_ToolStrip_WishlistBtn.Checked = false;
            }
            else
            {
                m_ToolStrip_WishlistBtn.Image = Properties.Resources.icon_checked;
                m_ToolStrip_WishlistBtn.Checked = true;
            }

            QueryMovies();
        }
        private void ToolStrip_Genre_Clicked(object sender, EventArgs e)
        {
            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var genres = ctx.Genres.AsNoTracking().ToList();

                foreach (var genre in genres)
                {
                    values[genre.name] = Utilities.GetGenreImage(genre.name);
                }
            }

            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.GENRES, false, false, Utilities.GENRE_IMAGE_W, Utilities.GENRE_IMAGE_H);
        }
        private void ToolStrip_ClearDirectorBtn_Clicked(object sender, EventArgs e)
        {
            HideFloatingPanel();
            if (m_ToolStrip_DirectorName.Text.Length > 0)
            {
                m_ToolStrip_DirectorName.Text = "";
                QueryMovies();
            }
        }
        private void ToolStrip_ClearActorBtn_Clicked(object sender, EventArgs e)
        {
            //DeleteUnusedActors();

            HideFloatingPanel();
            if (m_ToolStrip_ActorName.Text.Length > 0)
            {
                m_ToolStrip_ActorName.Text = "";
                QueryMovies();
            }
        }
        private void ToolStrip_ClearTitleBtn_Clicked(object sender, EventArgs e)
        {
            if (m_ToolStrip_MovieName.Text.Length > 0)
            {
                m_ToolStrip_MovieName.Text = "";
                QueryMovies();
            }
        }
        private void ToolStrip_ClearGenreBtn_Clicked(object sender, EventArgs e)
        {
            //DeleteUnusedGenres();

            HideFloatingPanel();
            if ((m_ToolStrip_GenreName.Text.Length > 0) && (m_ToolStrip_GenreName.Text != EMPTY_DOTS))
            {

                m_ToolStrip_GenreName.Text = EMPTY_DOTS;
                QueryMovies();
            }
        }
        #endregion
        #region Edit Fields operations
        private void OnMovieNameConfirmed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                QueryMovies();
            }
        }
        private void OnMovieNameTextChanged(object sender, EventArgs e)
        {
            m_TypeTimer.Stop();
            m_TypeField = ETypeField.TITLE;
            m_TypeTimer.Start();
        }
        private void OnDirectorNameTextChanged(object sender, EventArgs e)
        {
            m_TypeTimer.Stop();
            m_TypeField = ETypeField.DIRECTOR;
            m_TypeTimer.Start();
        }
        private void OnActorNameTextChanged(object sender, EventArgs e)
        {
            m_TypeTimer.Stop();
            m_TypeField = ETypeField.ACTOR;
            m_TypeTimer.Start();
        }
        private void onTypeTimer(object sender, EventArgs e)
        {
            m_TypeTimer.Stop();
            var typeField = m_TypeField;
            m_TypeField = ETypeField.None;

            switch (typeField)
            {
                case ETypeField.TITLE:
                    QueryMovies();
                    break;
                case ETypeField.DIRECTOR:
                    onDirectorTypeTimer();
                    break;
                case ETypeField.ACTOR:
                    onActorTypeTimer();
                    break;

                default:
                    break;
            }
        }
        private void onDirectorTypeTimer()
        {
            if (m_SuppressNameChangedEvent)
            {
                return;
            }

            if (m_ToolStrip_DirectorName.Text.Length == 0)
            {
                HideFloatingPanel();
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var name = m_ToolStrip_DirectorName.Text.ToUpper();
                var directors = ctx.MovieDirectors.AsNoTracking().Where(r => r.Director.name.ToUpper().Contains(name)).Take(MAX_SEARCH_FILTER_COUNT);

                foreach (var director in directors)
                {
                    values[director.Director.name] = Utilities.BytesToBitmap(director.Director.photo);
                }
            }

            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.DIRECTORS, false, false, Utilities.PHOTO_W, Utilities.PHOTO_H);

            m_ToolStrip_DirectorName.Focus();

            Cursor.Current = Cursors.Default;
        }
        private void onActorTypeTimer()
        {
            if (m_SuppressNameChangedEvent)
            {
                return;
            }

            if (m_ToolStrip_ActorName.Text.Length == 0)
            {
                HideFloatingPanel();
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var name = m_ToolStrip_ActorName.Text.ToUpper();
                var actors = ctx.MovieCasts.AsNoTracking().Where(r => (r.Actor.name.ToUpper().Contains(name))).Take(MAX_SEARCH_FILTER_COUNT);

                foreach (var actor in actors)
                {
                    values[actor.Actor.name] = Utilities.BytesToBitmap(actor.Actor.photo);
                }
            }

            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.CAST, false, false, Utilities.PHOTO_W, Utilities.PHOTO_H);

            m_ToolStrip_ActorName.Focus();

            Cursor.Current = Cursors.Default;
        }
        private void DeleteUnusedActors()
        {
            using (var ctx = new AriadnaEntities())
            {
                var actors = ctx.Actors.ToList();

                bool bNeedToSaveChanges = false;
                foreach (var actor in actors)
                {
                    var usedActor = ctx.MovieCasts.Where(r => (r.actorId == actor.Id)).FirstOrDefault();
                    if (usedActor == null)
                    {
                        ctx.Actors.Remove(actor);
                        bNeedToSaveChanges = true;
                    }
                }
                if (bNeedToSaveChanges)
                {
                    ctx.SaveChanges();
                }
            }
        }
        private void DeleteUnusedGenres()
        {
            using (var ctx = new AriadnaEntities())
            {
                var genres = ctx.Genres.ToList();

                bool bNeedToSaveChanges = false;
                foreach (var genre in genres)
                {
                    var usedGenres = ctx.MovieGenres.Where(r => (r.genreId == genre.Id)).FirstOrDefault();
                    if (usedGenres == null)
                    {
                        ctx.Genres.Remove(genre);
                        bNeedToSaveChanges = true;
                    }
                }
                if (bNeedToSaveChanges)
                {
                    ctx.SaveChanges();
                }
            }
        }
        #endregion
        #region Floating Panel operations
        private void ShowFloatingPanel(SortedDictionary<string, Bitmap> values, FloatingPanel.EPanelContentType contentType, bool checkBox, bool multiSelect, int imageW, int imageH)
        {
            var width = this.Size.Width - 12 * 2;
            var height = imageH * 3 + 12;
            var x = this.Location.X + 12;
            var y = this.Location.Y + SystemInformation.CaptionHeight + m_ToolStrip.Size.Height + 8;

            m_FloatingPanel.Location = new Point(x, y);
            m_FloatingPanel.Size = new Size(width, height);

            m_FloatingPanel.Deactivate += new System.EventHandler(OnFloatingPanelClosed);
            m_FloatingPanel.ItemSelected += new System.EventHandler(OnFloatingPanelItemSelected);

            m_FloatingPanel.UpdateListView(values, contentType, checkBox, multiSelect, imageW, imageH);
            if (!m_FloatingPanel.Visible)
            {
                m_FloatingPanel.Show(this);
            }
        }
        private void HideFloatingPanel()
        {
            if (m_FloatingPanel.Visible)
            {
                m_FloatingPanel.Hide();
            }
        }
        private void OnFloatingPanelClosed(object sender, EventArgs e)
        {
            if (!m_FloatingPanel.Visible)
            {
                m_SuppressNameChangedEvent = true;
                if (m_FloatingPanel.PanelContentType == FloatingPanel.EPanelContentType.DIRECTORS)
                {
                    m_ToolStrip_DirectorName.Text = m_FloatingPanel.EntryNames.FirstOrDefault();
                }
                else if (m_FloatingPanel.PanelContentType == FloatingPanel.EPanelContentType.CAST)
                {
                    m_ToolStrip_ActorName.Text = m_FloatingPanel.EntryNames.FirstOrDefault();
                }
                else if (m_FloatingPanel.PanelContentType == FloatingPanel.EPanelContentType.GENRES)
                {
                    m_ToolStrip_GenreName.Text = m_FloatingPanel.EntryNames.FirstOrDefault();
                }

                QueryMovies();
                m_SuppressNameChangedEvent = false;
            }
        }
        private void OnFloatingPanelItemSelected(object sender, EventArgs e)
        {
            if (m_FloatingPanel.Visible)
            {
                var genres = string.Join(" ", m_FloatingPanel.EntryNames.ToArray());
                m_ToolStrip_GenreName.Text = genres;

                QueryMovies();
            }
        }
        #endregion
        #region Hide Floating Panel handlers
        private void OnFormClicked(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        private void OnPanelMoved(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        private void OnPanelResized(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        private void OnListViewClick(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        private void OnListViewKeyDown(object sender, KeyEventArgs e)
        {
            HideFloatingPanel();
        }
        private void OnMouseCaptureChanged(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        private void OnMovieNameTextEntered(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        #endregion
    }
}
