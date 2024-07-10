using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Themes;

namespace Ariadna
{
    public partial class DetailsForm : Form
    {
        #region Public Fields
        public Utilities.EFormCloseReason FormCloseReason { get; set; }
        public string FilePath { get; set; }
        public int StoredDBEntryID { get; set; }
        #endregion

        #region Protected Fields
        protected readonly FloatingPanel m_FloatingPanel = new FloatingPanel();
        protected readonly Bitmap NO_PREVIEW_IMAGE_SMALL = new Bitmap(Properties.Resources.No_Preview_Image_small);
        #endregion

        #region Private Fields
        private bool m_IsInUpdateMode = false;
        private bool m_IsShiftPressed = false;

        private const int MAX_GENRE_COUNT_ALLOWED = 5;
        #endregion

        public DetailsForm(string filePath)
        {
            FilePath = filePath;
            StoredDBEntryID = -1;
            InitializeComponent();
            ApplyTheme();

            m_GenresList.Sorting = SortOrder.Ascending;
        }
        private void ApplyTheme()
        {
            this.m_TxtTitle.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtTitle.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtPath.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtPath.ForeColor = Theme.DetailsFormForeColorDimmed;
            this.m_TxtTitleOrig.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtTitleOrig.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtYear.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtYear.ForeColor = Theme.DetailsFormForeColor;
            this.m_BtnInsert.BackColor = Theme.DetailsFormConfirmBtnBackColor;
            this.m_BtnInsert.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtLength.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtLength.ForeColor = Theme.DetailsFormForeColorDimmed;
            this.m_LblDuration.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtDescription.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtDescription.ForeColor = Theme.DetailsFormForeColor;
            this.m_WanToSee.ForeColor = Theme.DetailsFormHighlightForeColor;
            this.m_LblGenre.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblTitle.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblTitleOrig.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblYear.ForeColor = Theme.DetailsFormForeColor;
            this.m_DirectorsList.BackColor = Theme.DetailsFormBackColor;
            this.m_DirectorsList.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblDirector.ForeColor = Theme.DetailsFormForeColor;
            this.m_CastList.BackColor = Theme.DetailsFormBackColor;
            this.m_CastList.ForeColor = Theme.DetailsFormForeColor;
            this.m_GenresList.BackColor = Theme.DetailsFormBackColor;
            this.m_GenresList.ForeColor = Theme.DetailsFormForeColor;
            this.m_AddGenreBtn.ForeColor = Theme.DetailsFormForeColor;
            this.m_GenrePaste.ForeColor = Theme.DetailsFormForeColor;
            this.m_DescriptionPaste.ForeColor = Theme.DetailsFormForeColor;
            this.m_DirectorPaste.ForeColor = Theme.DetailsFormForeColor;
            this.m_CastPaste.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblPath.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblCast.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblDescr.ForeColor = Theme.DetailsFormForeColor;
            this.m_VR.ForeColor = Theme.DetailsFormHighlightForeColor;
            this.m_LblVersion.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtVersion.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtVersion.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblVolume.BackColor = Theme.DetailsFormBackColor;
            this.m_LblVolume.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtVolume.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtVolume.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtDimension.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtDimension.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblDimensions.ForeColor = Theme.DetailsFormForeColor;
            this.m_TxtBitrate.BackColor = Theme.DetailsFormBackColor;
            this.m_TxtBitrate.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblBitrate.ForeColor = Theme.DetailsFormForeColor;
            this.m_LblAudioStreams.ForeColor = Theme.DetailsFormForeColor;

            this.BackColor = Theme.DetailsFormBackColor;

        }
        #region VIRTUAL FUNCTIONS
        protected virtual void DoLoad() { }
        protected virtual bool DoStore() { return false; }
        protected virtual void DoAddListViewItemFromClipboard(ListView listView, ImageList imageList) { }
        protected virtual List<string> GetGenres() { return new List<string>(); }
        protected virtual string GetGenreBySynonym(string name) { return ""; }
        protected virtual Bitmap GetGenreImage(string name) { return new Bitmap(Properties.Resources.No_Image); }
        #endregion

        private void OnLoad(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(FilePath))
            {
                return;
            }

            m_FloatingPanel.Deactivate += new System.EventHandler(OnFloatingPanelClosed);

            // Get path name
            m_TxtTitle.Text = FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
            m_TxtPath.Text = FilePath;

            m_TxtVolume.Text = CalculateVolume(FilePath);

            m_IsShiftPressed = false;

            // Delegate to derived class
            DoLoad();

            m_IsInUpdateMode = (StoredDBEntryID != -1);
            UpdateInsertButtonText();
            m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
        }
        private string CalculateVolume(string path)
        {
            long volume = 0;
            // Check if it is a file first
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                volume = fi.Length;
            }
            // Checked if it is a directory
            else if (Directory.Exists(path))
            {
                Cursor.Current = Cursors.WaitCursor;
                volume = GetDirSize(path);
                Cursor.Current = Cursors.Default;
            }

            volume /= 1024 * 1024;
            string v = volume.ToString();
            if(v.Length > 3)
            {
                v = v.Insert(v.Length - 3, " ");
            }
            v += " Mb";

            return v;
        }
        private long GetDirSize(string path)
        {
            return Directory.EnumerateFiles(path).Sum(x => new FileInfo(x).Length)
                 + Directory.EnumerateDirectories(path).Sum(x => GetDirSize(x));
        }
        private void UpdateInsertButtonText()
        {
            if (m_IsInUpdateMode)
            {
                Text = "Обновление записи";
                m_BtnInsert.Text = "Обновить";
            }
            else
            {
                Text = "Добавление записи";
                m_BtnInsert.Text = "Вставить";
            }
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            m_IsShiftPressed = e.Modifiers.HasFlag(Keys.Shift);

            if (e.Modifiers.HasFlag(Keys.Shift))
            {
                m_BtnInsert.Text = "В игнор";
            }

            if (e.KeyCode == Keys.Escape)
            {
                if (m_FloatingPanel.Visible)
                {
                    m_FloatingPanel.Hide();
                    return;
                }

                Close();
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                AddUpdateEntry();
                return;
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            m_IsShiftPressed = e.Modifiers.HasFlag(Keys.Shift);

            if (e.Modifiers.HasFlag(Keys.Shift) == false)
            {
                UpdateInsertButtonText();
            }
        }
        private void OnPicPoster_DoubleClick(object sender, EventArgs e)
        {
            if (Utilities.GetBitmapFromDisk(out Bitmap bmp,
                string.Format("Image ({0}x{1}) (*.*)|*.*", Properties.Settings.Default.PosterWidth, Properties.Settings.Default.PosterHeight), Properties.Settings.Default.PosterWidth, Properties.Settings.Default.PosterHeight))
            {
                m_PicPoster.Image = (bmp != null) ? new Bitmap(bmp) : new Bitmap(Properties.Resources.No_Preview_Image);
            }
        }
        private void OnBtnInsert_Click(object sender, EventArgs e)
        {
            AddUpdateEntry();
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
        private void AddUpdateEntry()
        {
            if (m_IsShiftPressed)
            {
                AddToIgnoreList();
                return;
            }

            m_BtnInsert.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            // Delegate to derived class
            bool bSuccess = DoStore();

            Cursor.Current = Cursors.Default;

            if (bSuccess)
            {
                FormCloseReason = Utilities.EFormCloseReason.SUCCESS;
                Close();
            }
            m_BtnInsert.Enabled = true;
        }
        private void OnDirectorsKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.V))
            {
                DoAddListViewItemFromClipboard(m_DirectorsList, m_DirectorsPhotos);
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
                DoAddListViewItemFromClipboard(m_CastList, m_CastPhotos);
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
        private void OnDescriptionKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.V))
            {
                m_TxtDescription.Text = DecorateDescription(m_TxtDescription.Text);
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
        private void DeleteFocusedListItem(ListView listView)
        {
            if (listView.SelectedItems.Count > 0)
            {
                listView.Items.Remove(listView.FocusedItem);
            }
        }
        private void OnDirectorsDoubleClicked(object sender, MouseEventArgs e)
        {
            if (Utilities.GetBitmapFromDisk(out Bitmap bmp,
                string.Format("Фото ({0}x{1}) (*.*)|*.*", Properties.Settings.Default.PortraitWidth, Properties.Settings.Default.PortraitHeight), Properties.Settings.Default.PortraitWidth, Properties.Settings.Default.PortraitHeight))
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
            if (Utilities.GetBitmapFromDisk(out Bitmap bmp,
                string.Format("Фото ({0}x{1}) (*.*)|*.*", Properties.Settings.Default.PortraitWidth, Properties.Settings.Default.PortraitHeight), Properties.Settings.Default.PortraitWidth, Properties.Settings.Default.PortraitHeight))
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

                if (m_GenresList.Items.Count == Properties.Settings.Default.MaxGenresCount)
                {
                    break;
                }
            }
        }
        protected void AddGenre(string name)
        {
            if (m_GenresList.FindItemWithText(name) != null)
            {
                return;
            }

            name = Utilities.CapitalizeWords(name);
            name = GetGenreBySynonym(name);
            m_GenresImages.Images.Add(name, GetGenreImage(name));
            m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));
            m_GenresList.Sort();
            m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
        }
        private void OnAddGenreClicked(object sender, EventArgs e)
        {
            if (m_FloatingPanel.Visible)
            {
                m_FloatingPanel.Hide();
                return;
            }

            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            foreach (var name in GetGenres())
            {
                if (m_GenresList.FindItemWithText(name) != null)
                {
                    continue;
                }

                values[name] = GetGenreImage(name);
            }

            m_FloatingPanel.Location = new Point(Location.X + m_GenresList.Location.X + 12,
                                                 Location.Y + m_GenresList.Location.Y + SystemInformation.CaptionHeight + m_GenresList.Size.Height + 12);
            m_FloatingPanel.Size = new Size(m_GenresList.Size.Width, Properties.Settings.Default.GenreImageHeight * 7 - 10);
            m_FloatingPanel.BackColor = BackColor;

            m_FloatingPanel.UpdateListView(values, FloatingPanel.EPanelContentType.GENRES, false, false, Properties.Settings.Default.GenreImageWidth, Properties.Settings.Default.GenreImageHeight);
            if (!m_FloatingPanel.Visible)
            {
                m_FloatingPanel.Show(this);
            }
        }
        private void OnFloatingPanelClosed(object sender, EventArgs e)
        {
            if (m_FloatingPanel.Visible)
            {
                return;
            }

            var name = m_FloatingPanel.EntryNames.FirstOrDefault();
            if (name != null)
            {
                name = Utilities.CapitalizeWords(name);
                m_GenresImages.Images.Add(name, GetGenreImage(name));
                m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));

                m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
            }
        }
        private void OnDescriptionPasteClick(object sender, EventArgs e)
        {
            m_TxtDescription.Text = DecorateDescription(Clipboard.GetText());
        }
        private void OnGenrePasteClick(object sender, EventArgs e)
        {
            AddGenresFromClipboard();
        }
        private void OnDirectorPasteClick(object sender, EventArgs e)
        {
            DoAddListViewItemFromClipboard(m_DirectorsList, m_DirectorsPhotos);
        }
        private void OnCastPasteClick(object sender, EventArgs e)
        {
            DoAddListViewItemFromClipboard(m_CastList, m_CastPhotos);
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
            if (m_FloatingPanel.Visible)
            {
                m_FloatingPanel.Hide();
            }
        }
        private void OnFilePathChanged(object sender, EventArgs e)
        {
            FilePath = m_TxtPath.Text;
        }
        private void OnPreview_DoubleClick(object sender, EventArgs e)
        {
            var pic = sender as PictureBox;

            if (Utilities.GetBitmapFromDisk(out Bitmap bmp,
                string.Format("Image ({0}x{1}) (*.*)|*.*", Properties.Settings.Default.PreviewWidth, Properties.Settings.Default.PreviewHeight), Properties.Settings.Default.PreviewWidth, Properties.Settings.Default.PreviewHeight))
            {
                var preview = (bmp != null) ? new Bitmap(bmp) : new Bitmap(Properties.Resources.No_Preview_Image);
                if (pic.Name.Equals(m_Preview1.Name))
                {
                    m_Preview1.Image = preview;
                }
                else if (pic.Name.Equals(m_Preview2.Name))
                {
                    m_Preview2.Image = preview;
                }
                if (pic.Name.Equals(m_Preview3.Name))
                {
                    m_Preview3.Image = preview;
                }
                if (pic.Name.Equals(m_Preview4.Name))
                {
                    m_Preview4.Image = preview;
                }

                m_PreviewFull.Image = preview;
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            var pic = sender as PictureBox;
            if(pic.Image.Width != pic.Width)
            {
                m_PreviewFull.Image = new Bitmap(pic.Image);
            }
        }
    }
}