using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.DBStrategies;
using Ariadna.Extension;
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using Ariadna.SplashScreen;
using Ariadna.Themes;
using Manina.Windows.Forms;

namespace Ariadna;

public partial class MainPanel : Form
{
    #region Private Fields
    private bool m_SuppressNameChangedEvent;

    private readonly AbstractDbStrategy m_DbStrategy;

    private readonly ImageListViewAriadnaRenderer m_ListViewRenderer = new();

    private readonly FloatingPanel m_FloatingPanel = new();

    private readonly Timer m_TypeTimer = new();
    private enum ETypeField { NONE = 0, TITLE, DIRECTOR, ACTOR }
    private ETypeField m_TypeField;
    private const int TYPE_TIMEOUT_MS = 200;

    private const int MAX_SEARCH_FILTER_COUNT = 200;

    private const string EMPTY_DOTS = ". . .";
    #endregion

    public MainPanel(AbstractDbStrategy strategy)
    {
        InitializeComponent();
        ApplyTheme();

        m_DbStrategy = strategy;
        m_DbStrategy.FilterControls(this);
        m_DbStrategy.EntryInserted += OnNewEntryInserted;

        m_ImageListView.SetRenderer(m_ListViewRenderer);

        UpdateImageList(m_DbStrategy.GetEntries());

        // Type timer
        m_TypeField = ETypeField.NONE;
        m_TypeTimer.Tick += OnTypeTimer;
        m_TypeTimer.Interval = TYPE_TIMEOUT_MS;
    }
    private void ApplyTheme()
    {
        m_ToolStrip.BackColor = Theme.MainBackColor;
        m_ToolStrip_AddBtn.ForeColor = Theme.MainForeColor;
        m_ToolStrip_NameLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_EntryName.BackColor = Theme.ControlsBackColor;
        m_ToolStrip_EntryName.ForeColor = Theme.MainForeColor;
        m_ToolStrip_WishlistLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_RecentLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_NewLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_VRLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_nonVRLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_DirectorLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_DirectorName.BackColor = Theme.ControlsBackColor;
        m_ToolStrip_DirectorName.ForeColor = Theme.MainForeColor;
        m_ToolStrip_ActorLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_ActorName.BackColor = Theme.ControlsBackColor;
        m_ToolStrip_ActorName.ForeColor = Theme.MainForeColor;
        m_ToolStrip_GenreNameLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_GenreName.BackColor = Theme.ControlsBackColor;
        m_ToolStrip_GenreName.ForeColor = Theme.MainForeColor;
        m_ToolStrip_EntriesCountLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_EntriesCount.ForeColor = Theme.MainForeColor;
        m_ToolStrip_SeriesLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_MoviesLbl.ForeColor = Theme.MainForeColor;
        m_QuickListFlow.BackColor = Theme.MainBackColor;
        BackColor = Theme.MainBackColor;

    }
    private void MainPanel_Load(object sender, EventArgs e)
    {
        // Bring MainPanel to front
        Activate();

        Splasher.Close();

        m_ToolStrip_EntryName.Focus();
    }
    private void UpdateImageList(List<EntryDto> entries)
    {
        m_ToolStrip_EntriesCount.Text = entries.Count.ToString();

        m_ImageListView.Items.Clear();

        var firstChars = new HashSet<string>(entries.Count);
        var listViewItems = new List<ImageListViewItem>(entries.Count);
        foreach (var entry in entries)
        {
            firstChars.Add(entry.Title[..1]);

            var item = new ImageListViewItem(entry.Id.ToString(), entry.Title);
            listViewItems.Add(item);
        }

        m_ImageListView.Items.AddRange(listViewItems.ToArray(), m_DbStrategy.GetPosterImageAdapter());

        FillQuickList(firstChars);
    }
    private void QueryEntries()
    {
        HideFloatingPanel();

        Cursor.Current = Cursors.WaitCursor;

        var values = new AbstractDbStrategy.QueryParams
        {
            Name = m_ToolStrip_EntryName.Text,
            Director = m_ToolStrip_DirectorName.Text,
            Actor = m_ToolStrip_ActorName.Text,
            Genre = m_ToolStrip_GenreName.Text,
            IsWish = m_ToolStrip_WishlistBtn.Checked,
            IsRecent = m_ToolStrip_RecentBtn.Checked,
            IsNew = m_ToolStrip_NewBtn.Checked,
            IsVr = m_ToolStrip_VRBtn.Checked,
            IsNonVr = m_ToolStrip_nonVRBtn.Checked,
            IsSeries = m_ToolStrip_SeriesBtn.Checked,
            IsMovies = m_ToolStrip_MoviesBtn.Checked
        };

        UpdateImageList(m_DbStrategy.QueryEntries(values));

        Cursor.Current = Cursors.Default;
    }
    private void FillQuickList(HashSet<string> firstChars)
    {
        m_QuickListFlow.Controls.Clear();

        foreach (var firstChar in firstChars)
        {
            var any = false;
            // Filter out following symbols
            foreach (var c in new[] { "}", "«" })
            {
                if (firstChar.Contains(c))
                {
                    any = true;
                    break;
                }
            }

            if (any)
            {
                continue;
            }

            var btn = new Button
            {
                Text = firstChar,
                AutoSize = false,
                Size = new Size(40, 40),
                BackColor = Theme.MainBackColor,
                ForeColor = Theme.MainForeColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold),
            };
            btn.Click += OnQuickListClicked;
            m_QuickListFlow.Controls.Add(btn);
        }
    }
    private void OnQuickListClicked(object sender, EventArgs e)
    {
        Button charBtn = sender as Button;

        var selection = m_ImageListView.Items.FirstOrDefault(x => x.Text.StartsWith(charBtn!.Text));
        m_ImageListView.EnsureVisible(selection!.Index);
        selection.Selected = true;
    }
    private void MainPanel_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Delete:
                RemoveEntry(e.Modifiers.HasFlag(Keys.Shift));
                return;
            case Keys.Escape:
                HideFloatingPanel();
                return;
        }
    }
    private void MainPanel_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar != '+')
        {
            return;
        }
        
        e.Handled = true;

        if(FindNextEntryAutomatically() is false)
        {
            m_DbStrategy.FindNextEntryManually();
        }
    }
    private bool FindNextEntryAutomatically()
    {
        Cursor.Current = Cursors.WaitCursor;
        var bNotInsertedEntryFound = m_DbStrategy.FindNextEntryAutomatically();
        Cursor.Current = Cursors.Default;

        return bNotInsertedEntryFound;
    }
    private void RemoveEntry(bool deleteFile = false)
    {
        if (m_ImageListView.Items.FocusedItem == null)
        {
            return;
        }

        var id = ((string)m_ImageListView.Items.FocusedItem.VirtualItemKey).ToInt();
        if (id == -1)
        {
            return;
        }

        var info = m_DbStrategy.GetEntryInfo(id);

        var msg = info.Title + " / " + info.TitleOrig + "\n" + info.Path;
        var caption = deleteFile ? Resources.DeleteEntryAndFile : Resources.DeleteEntry;
        var dialogResult = MessageBox.Show(msg, caption, MessageBoxButtons.YesNoCancel, deleteFile ? MessageBoxIcon.Warning : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (dialogResult != DialogResult.Yes)
        {
            return;
        }

        m_DbStrategy.RemoveEntry(id);

        QueryEntries();
        //UpdateImageList(dbStrategy.GetEntries());

        m_ToolStrip_EntryName.Text = string.Empty;

        if (!deleteFile)
        {
            return;
        }

        if (File.Exists(info.Path))
        {
            // Delete file
            File.SetAttributes(info.Path, FileAttributes.Normal);
            File.Delete(info.Path);
        }
        // Checked if it is a directory
        else if (Directory.Exists(info.Path))
        {
            try
            {
                var dir = new DirectoryInfo(info.Path);
                dir.Attributes &= ~FileAttributes.ReadOnly;
                dir.Delete(true);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        else
        {
            MessageBox.Show(info.Path, Resources.PathNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    private void OnNewEntryInserted(object sender, AbstractDbStrategy.EntryInsertedEventArgs e)
    {
        QueryEntries();
        //UpdateImageList(dbStrategy.GetEntries());

        var selection = m_ImageListView.Items.FirstOrDefault(x => (string)x.VirtualItemKey == e.Id.ToString());
        if (selection == null)
        {
            return;
        }
        
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

        // Show Entry details on Mouse Right Click
        if (e.Button == MouseButtons.Left)
        {
            return;
        }

        var lv = sender as ImageListView;
        if (lv!.Items.FocusedItem == null)
        {
            return;
        }

        m_DbStrategy.ShowEntryDetails(((string)lv.Items.FocusedItem.VirtualItemKey).ToInt());
    }
    private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        var lv = sender as ImageListView;
        m_DbStrategy.ExecuteEntry(((string)lv!.Items.FocusedItem.VirtualItemKey).ToInt());
    }
    #endregion
    #region ToolStrip Handlers
    private void ToolStrip_AddBtn_MouseUp(object sender, MouseEventArgs e)
    {
        // Auto search on left click
        if (e.Button == MouseButtons.Left)
        {
            if (FindNextEntryAutomatically())
            {
                return;
            }
        }

        m_DbStrategy.FindNextEntryManually();
    }
    private void ToolStrip_CheckboxedFilter_Clicked(object sender, EventArgs e)
    {
        var checkboxedFilter = sender as ToolStripButton;
        if (checkboxedFilter!.Checked)
        {
            checkboxedFilter.Image = Resources.icon_unchecked;
            checkboxedFilter.Checked = false;
        }
        else
        {
            checkboxedFilter.Image = Resources.icon_checked;
            checkboxedFilter.Checked = true;
        }

        QueryEntries();
    }
    private void ToolStrip_ToolStrip_VRBtn_Clicked(object sender, EventArgs e)
    {
        m_ToolStrip_nonVRBtn.Checked = false;
        m_ToolStrip_nonVRBtn.Image = Resources.icon_unchecked;

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_ToolStrip_NonVRBtn_Clicked(object sender, EventArgs e)
    {
        m_ToolStrip_VRBtn.Checked = false;
        m_ToolStrip_VRBtn.Image = Resources.icon_unchecked;

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_ToolStrip_SeriesBtn_Clicked(object sender, EventArgs e)
    {
        m_ToolStrip_MoviesBtn.Checked = false;
        m_ToolStrip_MoviesBtn.Image = Resources.icon_unchecked;

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_ToolStrip_MoviesBtn_Clicked(object sender, EventArgs e)
    {
        m_ToolStrip_SeriesBtn.Checked = false;
        m_ToolStrip_SeriesBtn.Image = Resources.icon_unchecked;

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_Genre_Clicked(object sender, EventArgs e)
    {
        var values = m_DbStrategy.GetGenres(m_ToolStrip_GenreName.Text!.ToUpper());
        ShowFloatingPanel(values, FloatingPanel.EPanelContentType.GENRES, false, false, Settings.Default.GenreImageWidth, Settings.Default.GenreImageHeight);
    }
    private void ToolStrip_ClearDirectorBtn_Clicked(object sender, EventArgs e)
    {
        HideFloatingPanel();
        if (m_ToolStrip_DirectorName.Text.Length > 0)
        {
            m_ToolStrip_DirectorName.Text = string.Empty;
            QueryEntries();
        }
    }
    private void ToolStrip_ClearActorBtn_Clicked(object sender, EventArgs e)
    {
        //DeleteUnusedActors();

        HideFloatingPanel();
        if (m_ToolStrip_ActorName.Text.Length > 0)
        {
            m_ToolStrip_ActorName.Text = string.Empty;
            QueryEntries();
        }
    }
    private void ToolStrip_ClearTitleBtn_Clicked(object sender, EventArgs e)
    {
        if (m_ToolStrip_EntryName.Text.Length > 0)
        {
            m_ToolStrip_EntryName.Text = string.Empty;
            QueryEntries();
        }
    }
    private void ToolStrip_ClearGenreBtn_Clicked(object sender, EventArgs e)
    {
        DeleteUnusedGenres();

        HideFloatingPanel();
            
        if (!string.IsNullOrEmpty(m_ToolStrip_GenreName.Text) && (m_ToolStrip_GenreName.Text != EMPTY_DOTS))
        {

            m_ToolStrip_GenreName.Text = EMPTY_DOTS;
            QueryEntries();
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void DeleteUnusedActors()
    {
        using var ctx = new AriadnaEntities();
        var actors = ctx.Actors.ToList();

        var bNeedToSaveChanges = false;
        foreach (var actor in actors)
        {
            var usedActor = ctx.MovieCasts.FirstOrDefault(r => (r.actorId == actor.Id));
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

    private void DeleteUnusedGenres()
    {
        using var ctx = new AriadnaEntities();
        var genres = ctx.Genres.ToList();

        var bNeedToSaveChanges = false;
        foreach (var genre in genres)
        {
            var usedGenres = ctx.MovieGenres.FirstOrDefault(r => (r.genreId == genre.Id));
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

    #endregion
    #region Edit Fields operations
    private void OnEntryNameConfirmed(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            QueryEntries();
        }
    }
    private void OnEntryNameTextChanged(object sender, EventArgs e)
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
    private void OnTypeTimer(object sender, EventArgs e)
    {
        m_TypeTimer.Stop();
        var type = m_TypeField;
        m_TypeField = ETypeField.NONE;

        switch (type)
        {
            case ETypeField.TITLE:
                QueryEntries();
                break;
            case ETypeField.DIRECTOR:
                OnDirectorTypeTimer();
                break;
            case ETypeField.ACTOR:
                OnActorTypeTimer();
                break;

            case ETypeField.NONE:
            default:
                break;
        }
    }
    private void OnDirectorTypeTimer()
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
        {
            var values = m_DbStrategy.GetDirectors(m_ToolStrip_DirectorName.Text.ToUpper(), MAX_SEARCH_FILTER_COUNT);
            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.DIRECTORS, false, false, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
        }
        Cursor.Current = Cursors.Default;

        m_ToolStrip_DirectorName.Focus();
    }
    private void OnActorTypeTimer()
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
        {
            var values = m_DbStrategy.GetActors(m_ToolStrip_ActorName.Text.ToUpper(), MAX_SEARCH_FILTER_COUNT);
            ShowFloatingPanel(values, FloatingPanel.EPanelContentType.CAST, false, false, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
        }
        Cursor.Current = Cursors.Default;

        m_ToolStrip_ActorName.Focus();
    }
    #endregion
    #region Floating Panel operations
    private void ShowFloatingPanel(SortedDictionary<string, Bitmap> values, FloatingPanel.EPanelContentType contentType, bool checkBox, bool multiSelect, int imageW, int imageH)
    {
        var width = Size.Width - 12 * 2;
        var height = imageH * 3 + 12;
        var x = Location.X + 12;
        var y = Location.Y + SystemInformation.CaptionHeight + m_ToolStrip.Size.Height + 8;

        m_FloatingPanel.Location = new Point(x, y);
        m_FloatingPanel.Size = new Size(width, height);

        m_FloatingPanel.Deactivate += OnFloatingPanelClosed;
        m_FloatingPanel.ItemSelected += OnFloatingPanelItemSelected;

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
        if (m_FloatingPanel.Visible)
        {
            return;
        }

        var result = m_FloatingPanel.EntryNames.FirstOrDefault();
        if(string.IsNullOrEmpty(result))
        {
            return;
        }

        switch (m_FloatingPanel.PanelContentType)
        {
            case FloatingPanel.EPanelContentType.DIRECTORS:
                m_ToolStrip_DirectorName.Text = result;
                break;
            case FloatingPanel.EPanelContentType.CAST:
                m_ToolStrip_ActorName.Text = result;
                break;
            case FloatingPanel.EPanelContentType.GENRES:
                m_ToolStrip_GenreName.Text = result;
                break;
        }

        m_SuppressNameChangedEvent = true;
        QueryEntries();
        m_SuppressNameChangedEvent = false;
    }
    private void OnFloatingPanelItemSelected(object sender, EventArgs e)
    {
        if (m_FloatingPanel.Visible)
        {
            m_ToolStrip_GenreName.Text = string.Join(" ", m_FloatingPanel.EntryNames);

            QueryEntries();
        }
    }
    #endregion
    #region Hide Floating Panel handlers
    private void OnFormClicked(object sender, EventArgs e) => HideFloatingPanel();
    private void OnPanelMoved(object sender, EventArgs e) => HideFloatingPanel();
    private void OnPanelResized(object sender, EventArgs e) => HideFloatingPanel();
    private void OnListViewClick(object sender, EventArgs e) => HideFloatingPanel();
    private void OnListViewKeyDown(object sender, KeyEventArgs e) => HideFloatingPanel();
    private void OnMouseCaptureChanged(object sender, EventArgs e) => HideFloatingPanel();
    private void OnEntryNameTextEntered(object sender, EventArgs e) => HideFloatingPanel();
    #endregion
}