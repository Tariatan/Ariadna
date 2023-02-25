using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Ariadna
{
    public partial class MainPanel : Form
    {
        private ListViewItem[] mCachedList;
        private int mCachedListFirstItemIndex;

        private List<Utilities.MovieDto> mMovies;

        private bool mSuppressNameChangedEvent = false;

        private FloatingPanel mFloatingPanel = new FloatingPanel();

        private System.Windows.Forms.Timer mTypeTimer = new System.Windows.Forms.Timer();
        private enum ETypeField { None = 0, TITLE, DIRECTOR, ACTOR }
        private ETypeField mTypeField = ETypeField.None;
        private const int TYPE_TIMOUT_MS = 200;

        private System.Windows.Forms.Timer mBlinkTimer = new System.Windows.Forms.Timer();
        private enum EBlinkState { None = 0, TICK, TUCK, }

        private EBlinkState mBlinkState = EBlinkState.None;
        private const int BLINK_COUNT = 3;
        private int mBlinkCount = BLINK_COUNT;
        private const int BLINK_INTERVAL_MS = 50;

        private const int MAX_SEARCH_FILTER_COUNT = 200;

        private const string EMPTY_DOTS = ". . .";
        private const string MEDIA_PLAYER_PATH = "C:/Program Files/MEDIA/K-Lite Codec Pack/MPC-HC64/mpc-hc64.exe";
        private const string DEFAULT_MOVIES_PATH = @"M:\";
        private const string DEFAULT_SERIES_PATH = @"S:\";

        public MainPanel()
        {
            InitializeComponent();
            // Set Double buffer
            PropertyInfo aProp = typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp.SetValue(listView, true, null);

            using (var ctx = new AriadnaEntities())
            {
                SetMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());

                listView.VirtualListSize = mMovies.Count();
            }

            // Blink timer
            mBlinkTimer.Tick += new EventHandler(Blink);
            mBlinkTimer.Interval = BLINK_INTERVAL_MS;

            // Type timer
            mTypeField = ETypeField.None;
            mTypeTimer.Tick += new EventHandler(onTypeTimer);
            mTypeTimer.Interval = TYPE_TIMOUT_MS;
        }
        private void MainPanel_Load(object sender, EventArgs e)
        {
            // Bring MainPanel to front
            this.Activate();

            Splasher.Close();

            m_ToolStripMovieName.Focus();
        }
        private void SetMoviesList(List<Utilities.MovieDto> movies)
        {
            mMovies = movies;
            m_ToolStripEntriesCount.Text = mMovies.Count().ToString();

            m_QuickListFlow.Controls.Clear();

            HashSet<string> firstChars = new HashSet<string>();
            foreach (var movie in mMovies)
            {
                firstChars.Add(movie.title.Substring(0, 1));
            }

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

            //Search for a particular virtual item.
            //Notice that we never manually populate the collection!
            //If you leave out the SearchForVirtualItem handler, this will return null.
            ListViewItem lvi = listView.FindItemWithText(charBtn.Text);

            //Select the item found and scroll it into view.
            if (lvi != null)
            {
                listView.SelectedIndices.Add(lvi.Index);
                listView.FocusedItem = lvi;
                listView.EnsureVisible(lvi.Index);
            }
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
        private void ToolStripAddBtn_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
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
            else if(e.Button == MouseButtons.Right)
            {
                AddNewMovie();
            }
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
        private void OnFormClicked(object sender, EventArgs e)
        {
            HideFloatingPanel();
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
                    Movie movie = ctx.Movies.Where(r => r.Id == id).FirstOrDefault();
                    if (movie != null)
                    {
                        return movie.file_path;
                    }
                }
            }

            return "";
        }
        private void DeleteMovie(bool deleteFile = false)
        {
            Int32 id = -1;
            Int32.TryParse(listView.FocusedItem.ToolTipText, out id);
            if (id == -1)
            {
                return;
            }

            using (var ctx = new AriadnaEntities())
            {
                Movie movie = ctx.Movies.Where(r => r.Id == id).FirstOrDefault();
                if (movie != null)
                {
                    string msg = movie.title + " / " + movie.title_original + "\n" + movie.file_path;
                    string caption = deleteFile ? "Удалить запись и файл?" : "Удалить запись?";
                    DialogResult dialogResult = MessageBox.Show(msg, caption, MessageBoxButtons.YesNoCancel, deleteFile ? MessageBoxIcon.Warning : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (dialogResult == DialogResult.Yes)
                    {
                        ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == movie.Id)));
                        ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == movie.Id)));
                        ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == movie.Id)));

                        ctx.Movies.Remove(movie);

                        ctx.SaveChanges();
                        SetMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());

                        RebuildCache();

                        m_ToolStripMovieName.Text = "";

                        if (deleteFile)
                        {
                            // Delete file
                            File.Delete(movie.file_path);
                        }
                    }
                }
            }
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
            if (movieData.FormCloseReason == Utilities.EFormCloseReason.SUCCESS)
            {
                using (var ctx = new AriadnaEntities())
                {
                    Movie movie = ctx.Movies.AsNoTracking().Where(r => r.Id == movieData.StoredDBMovieID).FirstOrDefault();

                    // Retrieve poster
                    Bitmap bmp = new Bitmap(171, 256);
                    Graphics graph = Graphics.FromImage(bmp);
                    Image img = (movie.poster.Length != 0) ? Utilities.BytesToBitmap(movie.poster) :
                                                                new Bitmap(Properties.Resources.No_Preview_Image);

                    graph.DrawImage(img, new Rectangle(0, 0, 171, 256));

                    // Find if item index is already in the list
                    int index = 0;
                    for (; index < mMovies.Count; ++index)
                    {
                        if (mMovies[index].id.Equals(movieData.StoredDBMovieID))
                        {
                            break;
                        }
                    }

                    // Update poster and title if item already in the list
                    if (index < mMovies.Count)
                    {
                        imageList.Images[imageList.Images.IndexOfKey(movieData.StoredDBMovieID.ToString())] = bmp;
                        listView.Items[index].Text = movie.title;
                        listView.Refresh();
                    }
                    // Simply update list and list count to reflect changes
                    else
                    {
                        SetMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());
                        RebuildCache();
                    }
                }
            }
        }
        private ListViewItem BuildListViewItem(int index)
        {
            var movie = mMovies[index];
            using (var ctx = new AriadnaEntities())
            {
                var poster = ctx.Movies.AsNoTracking().Where(r => r.Id == movie.id).Select(x => new { x.poster }).FirstOrDefault().poster;

                Bitmap image = (poster.Length != 0) ? Utilities.BytesToBitmap(poster) : new Bitmap(Properties.Resources.No_Preview_Image);

                try
                {
                    imageList.Images.Add(movie.id.ToString(), image);
                }
                catch (InvalidOperationException)
                {
                }

                ListViewItem lvi = new ListViewItem(movie.title, imageList.Images.IndexOfKey(movie.id.ToString()))
                {
                    ToolTipText = movie.id.ToString()
                };

                return lvi;
            }
        }
        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;

            if (e.Item.Selected)
            {
                Rectangle R = e.Bounds;
                R.Inflate(-2, -2);
                using (Pen pen = new Pen((mBlinkState == EBlinkState.TICK) ? Color.White : Color.Gray, 2f))
                {
                    e.Graphics.DrawRectangle(pen, R);
                }
            }
        }
        private void ListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            mBlinkTimer.Stop();

            mBlinkState = EBlinkState.TICK;
            mBlinkCount = BLINK_COUNT;
            mBlinkTimer.Start();
        }
        private void Blink(object sender, EventArgs e)
        {
            mBlinkState = (mBlinkState == EBlinkState.TICK) ? EBlinkState.TUCK : EBlinkState.TICK;
            listView.RedrawItems(listView.FocusedItem.Index, listView.FocusedItem.Index, false);
            if (--mBlinkCount < 0)
            {
                mBlinkTimer.Stop();
                mBlinkState = EBlinkState.None;
            }
        }
        private void ListView_MouseClicked(object sender, MouseEventArgs e)
        {
            HideFloatingPanel();
            if(e.Button == MouseButtons.Left)
            {
                return;
            }

            // Show Movie details on Mouse Right Click
            ListView lv = sender as ListView;
            var path = FindStoredMoviePathById(lv.FocusedItem.ToolTipText);
            if(string.IsNullOrEmpty(path))
            {
                return;
            }

            MovieData addMovie = new MovieData(path);
            addMovie.FormClosed += new FormClosedEventHandler(OnAddMovieFormClosed);
            addMovie.ShowDialog();
        }
        private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView lv = sender as ListView;
            var path = FindStoredMoviePathById(lv.FocusedItem.ToolTipText);
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
        //The basic VirtualMode function. Dynamically returns a ListViewItem with the required properties.
        private void ListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            //Caching is not required but improves performance on large sets.
            //To leave out caching, don't connect the CacheVirtualItems event
            //and make sure myCache is null.
            //check to see if the requested item is currently in the cache
            if (mCachedList != null && e.ItemIndex >= mCachedListFirstItemIndex && e.ItemIndex < mCachedListFirstItemIndex + mCachedList.Length)
            {
                //A cache hit, so get the ListViewItem from the cache instead of making a new one.
                e.Item = mCachedList[e.ItemIndex - mCachedListFirstItemIndex];
            }
            else
            {
                //A cache miss, so create a new ListViewItem and pass it back.
                e.Item = BuildListViewItem(e.ItemIndex);
            }
        }
        //Manages the cache.  ListView calls this when it might need a cache refresh.
        private void ListView_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if (mCachedList != null && e.StartIndex >= mCachedListFirstItemIndex && e.EndIndex <= mCachedListFirstItemIndex + mCachedList.Length)
            {
                //If the newly requested cache is a subset of the old cache,
                //no need to rebuild everything, so do nothing.
                return;
            }

            //Now we need to rebuild the cache.
            mCachedListFirstItemIndex = e.StartIndex;
            int length = (e.EndIndex - e.StartIndex + 1); //indexes are inclusive
            mCachedList = new ListViewItem[Math.Min(length, mMovies.Count)];

            //Fill the cache with the appropriate ListViewItems.
            for (int i = 0; i < mCachedList.Length; i++)
            {
                mCachedList[i] = BuildListViewItem(i + mCachedListFirstItemIndex);
            }
        }
        private void ListView_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {
            e.Index = mMovies.FindIndex(r => r.title.StartsWith(e.Text, StringComparison.CurrentCultureIgnoreCase));
        }
        private void RebuildCache()
        {
            // Rebuild the Cache
            mCachedList = null;
            // Clear all images to avoid invalid images in Cache
            imageList.Images.Clear();
            listView.Items.Clear();
            listView.VirtualListSize = mMovies.Count();
            listView.Invalidate();
        }
        private void QueryMovies()
        {
            HideFloatingPanel();

            Cursor.Current = Cursors.WaitCursor;

            using (var ctx = new AriadnaEntities())
            {
                IQueryable<Movie> query = ctx.Movies.AsNoTracking();

                // -- Search Movie Name --
                if (m_ToolStripMovieName.Text.Length > 0)
                {
                    var toSearch = m_ToolStripMovieName.Text.ToUpper();
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
                if (m_ToolStripDirectorName.Text.Length > 0)
                {
                    var entry = ctx.Directors.AsNoTracking().Where(r => r.name == m_ToolStripDirectorName.Text).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieDirectors.Any(l => (l.directorId == entry.Id)));
                    }
                }
                // -- ACTOR NAME --
                if (m_ToolStripActorName.Text.Length > 0)
                {
                    var entry = ctx.Actors.AsNoTracking().Where(r => r.name == m_ToolStripActorName.Text).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieCasts.Any(l => (l.actorId == entry.Id)));
                    }
                }
                // -- GENRE --
                if (m_ToolStripGenreName.Text.Length > 0)
                {
                    var entry = ctx.Genres.AsNoTracking().Where(r => r.name == m_ToolStripGenreName.Text).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieGenres.Any(l => (l.genreId == entry.Id)));
                    }
                }

                SetMoviesList(query.OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title, id = x.Id }).ToList());
            }

            RebuildCache();

            Cursor.Current = Cursors.Default;
        }
        private void ToolStripRecentBtn_Click(object sender, EventArgs e)
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
        private void ToolStrip_NewBtn_Click(object sender, EventArgs e)
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
        private void ToolStrip_WishlistBtn_Click(object sender, EventArgs e)
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
        private void OnToolStripGenreClicked(object sender, EventArgs e)
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
        private void OnToolStripClearDirectorBtnClick(object sender, EventArgs e)
        {
            HideFloatingPanel();
            if (m_ToolStripDirectorName.Text.Length > 0)
            {
                m_ToolStripDirectorName.Text = "";
                QueryMovies();
            }
        }
        private void OnToolStripClearActorBtnClick(object sender, EventArgs e)
        {
            //DeleteUnusedActors();

            HideFloatingPanel();
            if (m_ToolStripActorName.Text.Length > 0)
            {
                m_ToolStripActorName.Text = "";
                QueryMovies();
            }
        }
        private void OnToolStripClearTitleBtnClick(object sender, EventArgs e)
        {
            if (m_ToolStripMovieName.Text.Length > 0)
            {
                m_ToolStripMovieName.Text = "";
                QueryMovies();
            }
        }
        private void OnToolStripClearGenreBtnClick(object sender, EventArgs e)
        {
            //DeleteUnusedGenres();

            HideFloatingPanel();
            if ((m_ToolStripGenreName.Text.Length > 0) && (m_ToolStripGenreName.Text != EMPTY_DOTS))
            {

                m_ToolStripGenreName.Text = EMPTY_DOTS;
                QueryMovies();
            }
        }
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
            mTypeTimer.Stop();
            mTypeField = ETypeField.TITLE;
            mTypeTimer.Start();
        }
        private void onTypeTimer(object sender, EventArgs e)
        {
            mTypeTimer.Stop();
            var typeField = mTypeField;
            mTypeField = ETypeField.None;

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
        private void OnDirectorNameTextChanged(object sender, EventArgs e)
        {
            mTypeTimer.Stop();
            mTypeField = ETypeField.DIRECTOR;
            mTypeTimer.Start();
        }
        private void onDirectorTypeTimer()
        {
            if (mSuppressNameChangedEvent)
            {
                return;
            }

            if (m_ToolStripDirectorName.Text.Length == 0)
            {
                HideFloatingPanel();
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var name = m_ToolStripDirectorName.Text.ToUpper();
                var directors = ctx.MovieDirectors.AsNoTracking().Where(r => r.Director.name.ToUpper().Contains(name)).Take(MAX_SEARCH_FILTER_COUNT);

                foreach (var director in directors)
                {
                    values[director.Director.name] = Utilities.BytesToBitmap(director.Director.photo);
                }
            }

            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.DIRECTORS, false, false, Utilities.PHOTO_W, Utilities.PHOTO_H);

            m_ToolStripDirectorName.Focus();

            Cursor.Current = Cursors.Default;
        }
        private void OnActorNameTextChanged(object sender, EventArgs e)
        {
            mTypeTimer.Stop();
            mTypeField = ETypeField.ACTOR;
            mTypeTimer.Start();
        }
        private void onActorTypeTimer()
        {
            if (mSuppressNameChangedEvent)
            {
                return;
            }

            if (m_ToolStripActorName.Text.Length == 0)
            {
                HideFloatingPanel();
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var name = m_ToolStripActorName.Text.ToUpper();
                var actors = ctx.MovieCasts.AsNoTracking().Where(r => (r.Actor.name.ToUpper().Contains(name))).Take(MAX_SEARCH_FILTER_COUNT);

                foreach (var actor in actors)
                {
                    values[actor.Actor.name] = Utilities.BytesToBitmap(actor.Actor.photo);
                }
            }

            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.CAST, false, false, Utilities.PHOTO_W, Utilities.PHOTO_H);

            m_ToolStripActorName.Focus();

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
        private void ShowFloatingPanel(SortedDictionary<string, Bitmap> values, FloatingPanel.EPanelContentType contentType, bool checkBox, bool multiSelect, int imageW, int imageH)
        {
            var width = this.Size.Width - 12 * 2;
            var height = imageH * 3 + 12;
            var x = this.Location.X + 12;
            var y = this.Location.Y + SystemInformation.CaptionHeight + toolStrip.Size.Height + 8;

            mFloatingPanel.Location = new Point(x, y);
            mFloatingPanel.Size = new Size(width, height);

            mFloatingPanel.Deactivate += new System.EventHandler(OnFloatingPanelClosed);
            mFloatingPanel.ItemSelected += new System.EventHandler(OnFloatingPanelItemSelected);

            mFloatingPanel.UpdateListView(values, contentType, checkBox, multiSelect, imageW, imageH);
            if (!mFloatingPanel.Visible)
            {
                mFloatingPanel.Show(this);
            }
        }
        private void HideFloatingPanel()
        {
            if (mFloatingPanel.Visible)
            {
                mFloatingPanel.Hide();
            }
        }
        private void OnFloatingPanelClosed(object sender, EventArgs e)
        {
            if (!mFloatingPanel.Visible)
            {
                mSuppressNameChangedEvent = true;
                if (mFloatingPanel.PanelContentType == FloatingPanel.EPanelContentType.DIRECTORS)
                {
                    m_ToolStripDirectorName.Text = mFloatingPanel.EntryNames.FirstOrDefault();
                }
                else if (mFloatingPanel.PanelContentType == FloatingPanel.EPanelContentType.CAST)
                {
                    m_ToolStripActorName.Text = mFloatingPanel.EntryNames.FirstOrDefault();
                }
                else if (mFloatingPanel.PanelContentType == FloatingPanel.EPanelContentType.GENRES)
                {
                    m_ToolStripGenreName.Text = mFloatingPanel.EntryNames.FirstOrDefault();
                }

                QueryMovies();
                mSuppressNameChangedEvent = false;
            }
        }
        private void OnFloatingPanelItemSelected(object sender, EventArgs e)
        {
            if (mFloatingPanel.Visible)
            {
                var genres = string.Join(" ", mFloatingPanel.EntryNames.ToArray());
                m_ToolStripGenreName.Text = genres;

                QueryMovies();
            }
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
    }
}
