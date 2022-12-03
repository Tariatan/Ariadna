using System;
using System.Collections.Generic;
using System.Data;
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

        private System.Windows.Forms.Timer mBlinkTimer = new System.Windows.Forms.Timer();
        private enum EBlinkState { None = 0, TICK, TUCK, }

        private EBlinkState mBlinkState = EBlinkState.None;
        private const int BLINK_COUNT = 3;
        private int mBlinkCount = BLINK_COUNT;

        private const int MIN_NAME_LENGTH = 3;

        public MainPanel()
        {
            InitializeComponent();
            // Set Double buffer
            PropertyInfo aProp = typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp.SetValue(listView, true, null);

            using (var ctx = new AriadnaEntities())
            {
                SetMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title }).ToList());

                listView.VirtualListSize = mMovies.Count();

                m_ToolStripMovieName.Focus();
            }

            // Blink timer
            mBlinkTimer.Tick += new EventHandler(Blink);
        }
        private void MainPanel_Load(object sender, EventArgs e)
        {
            // Bring MainPanel to front
            this.Activate();

            Splasher.Close();
        }
        private void SetMoviesList(List<Utilities.MovieDto> movies)
        {
            mMovies = movies;

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
            var Files = Directory.GetFiles(@"M:\");
            using (var ctx = new AriadnaEntities())
            {
                foreach (var file in Files)
                {
                    if (ctx.Ignores.AsNoTracking().Where(r => r.path == file).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    var path = ctx.Movies.AsNoTracking().Where(r => r.file_path == file).Select(r => r.file_path).FirstOrDefault();
                    if (path == null)
                    {
                        OpenAddMovieFormDialog(file);
                        return true;
                    }
                }
            }

            return false;
        }
        private void ToolStripAddBtn_Click(object sender, EventArgs e)
        {
            AddNewMovie();
        }
        private void MainPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) && e.Modifiers.HasFlag(Keys.Shift))
            {
                DeleteMovie();
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
            if (e.KeyChar == '+')
            {
                e.Handled = true;
                AddNewMovie();
            }
        }
        private void DeleteMovie()
        {
            using (var ctx = new AriadnaEntities())
            {
                var lvi = listView.FocusedItem;
                Movie movie = ctx.Movies.Where(r => r.file_path == lvi.ToolTipText).FirstOrDefault();
                if (movie != null)
                {
                    string msg = movie.title + " / " + movie.title_original + "\n" + movie.file_path;
                    DialogResult dialogResult = MessageBox.Show(msg, "Удалить запись?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == movie.Id)));
                        ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == movie.Id)));
                        ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == movie.Id)));

                        ctx.Movies.Remove(movie);

                        ctx.SaveChanges();
                        SetMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title }).ToList());

                        RebuildCache();
                    }
                }
            }
        }
        private void AddNewMovie()
        {
            if (FindFirstNotInsertedMovie())
            {
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "M:\\";
                openFileDialog.Filter = "Видео файлы|*.avi;*.mkv;*.mpg;*.ts|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenAddMovieFormDialog(openFileDialog.FileName);
                }
            }
        }
        private void OpenAddMovieFormDialog(string fileName)
        {
            MovieData addMovie = new MovieData(fileName);

            addMovie.FormClosed += new FormClosedEventHandler(OnAddMovieFormClosed);
            addMovie.ShowDialog();
        }
        private void OnAddMovieFormClosed(object sender, FormClosedEventArgs e)
        {
            MovieData movieData = (MovieData)sender;
            if (movieData.FormCloseReason == Utilities.EFormCloseReason.SUCCESS)
            {
                using (var ctx = new AriadnaEntities())
                {
                    Movie movie = ctx.Movies.AsNoTracking().Where(r => r.file_path == movieData.FilePath).FirstOrDefault();

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
                        if (mMovies[index].path.Equals(movieData.FilePath))
                        {
                            break;
                        }
                    }

                    // Update poster and title if item already in the list
                    if (index < mMovies.Count)
                    {
                        imageList.Images[imageList.Images.IndexOfKey(movieData.FilePath)] = bmp;
                        listView.Items[index].Text = movie.title;
                        listView.Refresh();
                    }
                    // Simply update list and list count to reflect changes
                    else
                    {
                        SetMoviesList(ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title }).ToList());
                        RebuildCache();
                    }
                }
            }
        }
        private void OnListViewClick(object sender, EventArgs e)
        {
            HideFloatingPanel();
        }
        private ListViewItem BuildListViewItem(int index)
        {
            var movie = mMovies[index];
            using (var ctx = new AriadnaEntities())
            {
                var poster = ctx.Movies.AsNoTracking().Where(r => r.file_path == movie.path).Select(x => new { x.poster }).FirstOrDefault().poster;

                Bitmap image = (poster.Length != 0) ? Utilities.BytesToBitmap(poster) : new Bitmap(Properties.Resources.No_Preview_Image);

                try
                {
                    imageList.Images.Add(movie.path, image);
                }
                catch (InvalidOperationException)
                {
                }

                ListViewItem lvi = new ListViewItem(movie.title, imageList.Images.IndexOfKey(movie.path))
                {
                    ToolTipText = movie.path
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
            mBlinkTimer.Interval = 50;
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
        private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView lv = sender as ListView;
            MovieData addMovie = new MovieData(lv.FocusedItem.ToolTipText);
            // No Taskbar icon for helper dialog
            addMovie.ShowInTaskbar = false;

            addMovie.FormClosed += new FormClosedEventHandler(OnAddMovieFormClosed);
            addMovie.ShowDialog();
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
                    query = query.Where(r => r.title.ToUpper().Contains(m_ToolStripMovieName.Text.ToUpper()));
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

                SetMoviesList(query.OrderBy(r => r.title).Select(x => new Utilities.MovieDto { path = x.file_path, title = x.title }).ToList());
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
            DeleteUnusedGenres();

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
            HideFloatingPanel();
            if (m_ToolStripGenreName.Text.Length > 0)
            {
                m_ToolStripGenreName.Text = "...";
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
            QueryMovies();
        }
        private void OnDirectorNameTextChanged(object sender, EventArgs e)
        {
            if (mSuppressNameChangedEvent)
            {
                return;
            }

            if(m_ToolStripDirectorName.Text.Length == 0)
            {
                HideFloatingPanel();
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var name = m_ToolStripDirectorName.Text.ToUpper();
                var directors = ctx.MovieDirectors.AsNoTracking().Where(r => r.Director.name.ToUpper().Contains(name));

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
                var actors = ctx.MovieCasts.AsNoTracking().Where(r => (r.Actor.name.ToUpper().Contains(name)));

                foreach (var actor in actors)
                {
                    values[actor.Actor.name] = Utilities.BytesToBitmap(actor.Actor.photo);
                }
            }

            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.CAST, false, false, Utilities.PHOTO_W, Utilities.PHOTO_H);

            m_ToolStripActorName.Focus();

            Cursor.Current = Cursors.Default;
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
    }
}
