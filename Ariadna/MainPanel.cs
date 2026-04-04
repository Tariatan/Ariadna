using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.DatabaseStrategies;
using Ariadna.Extension;
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using Ariadna.SplashScreen;
using Ariadna.Themes;
using DbProvider;
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
        m_ToolStrip_VrLbl.ForeColor = Theme.MainForeColor;
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
        m_ToolStrip_SubgenreNameLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_SubgenreName.BackColor = Theme.ControlsBackColor;
        m_ToolStrip_SubgenreName.ForeColor = Theme.MainForeColor;
        m_ToolStrip_EntriesCountLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_EntriesCount.ForeColor = Theme.MainForeColor;
        m_ToolStrip_SeriesLbl.ForeColor = Theme.MainForeColor;
        m_ToolStrip_MoviesLbl.ForeColor = Theme.MainForeColor;
        m_QuickListFlow.BackColor = Theme.MainBackColor;
        BackColor = Theme.MainBackColor;
    }
    private void MainPanel_Load(object sender, EventArgs e)
    {
        // Restore position and size
        if (Settings.Default.FormSize != Size.Empty)
        {
            Size = Settings.Default.FormSize;
        }

        if (Settings.Default.FormLocation != Point.Empty)
        {
            Location = Settings.Default.FormLocation;
        }

        // Bring MainPanel to front
        Activate();

        Splasher.Close();

        m_ToolStrip_EntryName.Focus();

        // Show Recent entries at startup
//        m_ToolStrip_RecentBtn.Checked = true;
//        m_ToolStrip_RecentBtn.Image = Resources.icon_checked;
        QueryEntries();
        SelectRandomEntry();
    }
    private void OnFormClosing(object sender, FormClosingEventArgs e)
    {
        // Save position and size
        Settings.Default.FormLocation = Location;
        Settings.Default.FormSize = Size;
        Settings.Default.Save();
    }
    private void UpdateImageList(List<EntryDto> entries)
    {
        m_ToolStrip_EntriesCount.Text = entries.Count.ToString();

        m_ImageListView.Items.Clear();

        var firstChars = new HashSet<string>(entries.Count);
        var listViewItems = new List<ImageListViewItem>(entries.Count);
        foreach (var entry in entries)
        {
            firstChars.Add(entry.Title[..1].ToUpper());

            var item = new ImageListViewItem(entry.Id.ToString(), entry.Title);
            listViewItems.Add(item);
        }

        m_ImageListView.Items.AddRange(listViewItems.ToArray(), m_DbStrategy.GetPosterImageAdapter());

        FillQuickList(firstChars);
    }
    private void QueryEntries()
    {
        HideFloatingPanel();
        RunWithWaitCursor(() => UpdateImageList(m_DbStrategy.QueryEntries(CreateQueryParams())));
    }
    private void SelectRandomEntry()
    {
        if (m_ImageListView.Items.Count == 0)
        {
            return;
        }

        var randomIndex = Random.Shared.Next(m_ImageListView.Items.Count);
        var selection = m_ImageListView.Items[randomIndex];
        if (selection == null)
        {
            return;
        }

        SelectListItem(selection, selection.Index);
        m_ImageListView.Focus();
    }
    private void FillQuickList(HashSet<string> firstChars)
    {
        m_QuickListFlow.Controls.Clear();

        foreach (var firstChar in firstChars)
        {
            var any = false;
            // Filter out following symbols
            foreach (var c in m_DbStrategy.QuickListFilter())
            {
                if (firstChar.Contains(c.ToUpper()))
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
        var charBtn = sender as Button;

        var selection = m_ImageListView.Items.FirstOrDefault(x => x.Text.StartsWith(charBtn!.Text, true, null));
        if (selection == null)
        {
            return;
        }

        SelectListItem(selection, selection.Index);
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

        FindNextEntry();
    }
    private bool FindNextEntryAutomatically()
    {
        return RunWithWaitCursor(m_DbStrategy.FindNextEntryAutomatically);
    }
    private void RemoveEntry(bool deleteFile = false)
    {
        if (!TryGetFocusedEntryId(m_ImageListView, out var id))
        {
            return;
        }

        var index = m_ImageListView.Items.FirstOrDefault(x => (string)x.VirtualItemKey == id.ToString())?.Index;
        var info = GetEntryInfo(id);

        var msg = info.Title + " / " + info.TitleOrig + '\n' + info.Path;
        var caption = deleteFile ? Resources.DeleteEntryAndFile : Resources.DeleteEntry;
        var dialogResult = ShowRemoveEntryConfirmation(msg, caption, deleteFile);
        if (dialogResult != DialogResult.Yes)
        {
            return;
        }

        m_DbStrategy.RemoveEntry(id);

        RefreshAfterEntryRemoval(index);

        if (!deleteFile)
        {
            return;
        }

        if (FileExists(info.Path))
        {
            DeleteFile(info.Path);
        }
        else if (DirectoryExists(info.Path))
        {
            try
            {
                DeleteDirectory(info.Path);
            }
            catch (IOException ex)
            {
                ShowMessage(ex.Message);
            }
        }
        else
        {
            ShowPathNotFoundMessage(info.Path);
        }
    }
    protected virtual DialogResult ShowRemoveEntryConfirmation(string message, string caption, bool deleteFile)
    {
        return MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel, deleteFile ? MessageBoxIcon.Warning : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
    }
    protected virtual void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }
    protected virtual void ShowPathNotFoundMessage(string path)
    {
        MessageBox.Show(path, Resources.PathNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    protected virtual bool FileExists(string path)
    {
        return File.Exists(path);
    }
    protected virtual bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }
    protected virtual void DeleteFile(string path)
    {
        var file = new FileInfo(path);
        file.Attributes &= ~FileAttributes.ReadOnly;
        file.Delete();
    }
    protected virtual void DeleteDirectory(string path)
    {
        var dir = new DirectoryInfo(path);
        ClearReadOnlyAttributes(dir);
        dir.Delete(true);
    }
    private static void ClearReadOnlyAttributes(DirectoryInfo directory)
    {
        foreach (var subDirectory in directory.GetDirectories("*", SearchOption.AllDirectories))
        {
            subDirectory.Attributes &= ~FileAttributes.ReadOnly;
        }

        foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes &= ~FileAttributes.ReadOnly;
        }

        directory.Attributes &= ~FileAttributes.ReadOnly;
    }
    private void OnNewEntryInserted(object sender, AbstractDbStrategy.EntryInsertedEventArgs e)
    {
        QueryEntries();

        var selection = m_ImageListView.Items.FirstOrDefault(x => (string)x.VirtualItemKey == e.Id.ToString());

        if (selection == null)
        {
            return;
        }

        SelectListItem(selection, selection.Index == 0 ? 0 : selection.Index - 1);
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

        if (TryGetFocusedEntryId(lv, out var id))
        {
            m_DbStrategy.ShowEntryDetails(id);
        }
    }
    private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        var lv = sender as ImageListView;
        if (TryGetFocusedEntryId(lv!, out var id))
        {
            m_DbStrategy.ExecuteEntry(id);
        }
    }
    #endregion
    #region ToolStrip Handlers
    private void ToolStrip_AddBtn_MouseUp(object sender, MouseEventArgs e)
    {
        FindNextEntry(e.Button == MouseButtons.Left);
    }
    private void ToolStrip_CheckboxedFilter_Clicked(object sender, EventArgs e)
    {
        var checkboxedFilter = sender as ToolStripButton;
        SetToolStripButtonCheckedState(checkboxedFilter!, !checkboxedFilter!.Checked);

        QueryEntries();
    }
    private void ToolStrip_ToolStrip_VRBtn_Clicked(object sender, EventArgs e)
    {
        SetToolStripButtonCheckedState(m_ToolStrip_nonVRBtn, false);

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_ToolStrip_NonVRBtn_Clicked(object sender, EventArgs e)
    {
        SetToolStripButtonCheckedState(m_ToolStrip_VRBtn, false);

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_ToolStrip_SeriesBtn_Clicked(object sender, EventArgs e)
    {
        SetToolStripButtonCheckedState(m_ToolStrip_MoviesBtn, false);

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_ToolStrip_MoviesBtn_Clicked(object sender, EventArgs e)
    {
        SetToolStripButtonCheckedState(m_ToolStrip_SeriesBtn, false);

        ToolStrip_CheckboxedFilter_Clicked(sender, e);
    }
    private void ToolStrip_Genre_Clicked(object sender, EventArgs e)
    {
        ShowGenrePanel(m_DbStrategy.GetGenres(), FloatingPanel.EPanelContentType.GENRES);
    }
    private void ToolStrip_Subgenre_Clicked(object sender, EventArgs e)
    {
        ShowGenrePanel(m_DbStrategy.GetSubgenres(m_ToolStrip_GenreName.Text), FloatingPanel.EPanelContentType.SUBGENRES);
    }
    private void ToolStrip_ClearDirectorBtn_Clicked(object sender, EventArgs e)
    {
        ClearTextAndQuery(m_ToolStrip_DirectorName);
    }
    private void ToolStrip_ClearActorBtn_Clicked(object sender, EventArgs e)
    {
        //DeleteUnusedActors();
        ClearTextAndQuery(m_ToolStrip_ActorName);
    }
    private void ToolStrip_ClearTitleBtn_Clicked(object sender, EventArgs e)
    {
        ClearTextAndQuery(m_ToolStrip_EntryName, hideFloatingPanel: false);
    }
    private void ToolStrip_ClearGenreBtn_Clicked(object sender, EventArgs e)
    {
        ClearTextAndQuery(m_ToolStrip_GenreName, Utilities.EmptyDots, DeleteUnusedGenres);
    }
    private void ToolStrip_ClearSubgenreBtn_Clicked(object sender, EventArgs e)
    {
        ClearTextAndQuery(m_ToolStrip_SubgenreName, Utilities.EmptyDots);
    }
    private void OnGenreChanged(object sender, EventArgs e)
    {
        m_DbStrategy.UpdateSubgenre(this);
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
        StartTypeTimer(ETypeField.TITLE);
    }
    private void OnDirectorNameTextChanged(object sender, EventArgs e)
    {
        StartTypeTimer(ETypeField.DIRECTOR);
    }
    private void OnActorNameTextChanged(object sender, EventArgs e)
    {
        StartTypeTimer(ETypeField.ACTOR);
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
        ShowSearchResults(m_ToolStrip_DirectorName, m_DbStrategy.GetDirectors, FloatingPanel.EPanelContentType.DIRECTORS);
    }
    private void OnActorTypeTimer()
    {
        ShowSearchResults(m_ToolStrip_ActorName, m_DbStrategy.GetActors, FloatingPanel.EPanelContentType.CAST);
    }
    #endregion
    #region Floating Panel operations
    private void ShowFloatingPanel(ImmutableSortedDictionary<string, Bitmap> values, FloatingPanel.EPanelContentType contentType, bool checkBox, bool multiSelect, int imageW, int imageH)
    {
        m_FloatingPanel.Bounds = GetFloatingPanelBounds(imageH);

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
        if (string.IsNullOrEmpty(result))
        {
            return;
        }

        ApplyFloatingPanelSelectionResult(result);

        RunWithSuppressedNameChangedEvents(QueryEntries);
    }
    private void OnFloatingPanelItemSelected(object sender, EventArgs e)
    {
        if (m_FloatingPanel.Visible)
        {
            m_ToolStrip_GenreName.Text = string.Join(" ", m_FloatingPanel.EntryNames);

            QueryEntries();
        }
    }

    private AbstractDbStrategy.QueryParams CreateQueryParams()
    {
        return new AbstractDbStrategy.QueryParams
        {
            Name = m_ToolStrip_EntryName.Text,
            Director = m_ToolStrip_DirectorName.Text,
            Actor = m_ToolStrip_ActorName.Text,
            Genre = m_ToolStrip_GenreName.Text,
            Subgenre = m_ToolStrip_SubgenreName.Text,
            IsWish = m_ToolStrip_WishlistBtn.Checked,
            IsRecent = m_ToolStrip_RecentBtn.Checked,
            IsNew = m_ToolStrip_NewBtn.Checked,
            IsVr = m_ToolStrip_VRBtn.Checked,
            IsNonVr = m_ToolStrip_nonVRBtn.Checked,
            IsSeries = m_ToolStrip_SeriesBtn.Checked,
            IsMovies = m_ToolStrip_MoviesBtn.Checked
        };
    }

    private static void SetToolStripButtonCheckedState(ToolStripButton button, bool isChecked)
    {
        button.Checked = isChecked;
        button.Image = isChecked ? Resources.icon_checked : Resources.icon_unchecked;
    }

    private void ApplyFloatingPanelSelectionResult(string result)
    {
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
            case FloatingPanel.EPanelContentType.SUBGENRES:
                m_ToolStrip_SubgenreName.Text = result;
                break;
        }
    }

    private void FindNextEntry(bool tryAutomaticFirst = true)
    {
        if (tryAutomaticFirst && FindNextEntryAutomatically())
        {
            return;
        }

        m_DbStrategy.FindNextEntryManually();
    }

    private EntryInfo GetEntryInfo(int id)
    {
        return m_DbStrategy.GetEntryInfo(id);
    }

    private void RefreshAfterEntryRemoval(int? index)
    {
        QueryEntries();
        m_ToolStrip_EntryName.Text = string.Empty;

        if (index is not null)
        {
            m_ImageListView.EnsureVisible(index.Value);
        }
    }

    private void StartTypeTimer(ETypeField typeField)
    {
        m_ImageListView.Items.FocusedItem = null;
        m_TypeTimer.Stop();
        m_TypeField = typeField;
        m_TypeTimer.Start();
    }

    private void ShowSearchResults(ToolStripTextBox textBox, Func<string, int, ImmutableSortedDictionary<string, Bitmap>> valueProvider, FloatingPanel.EPanelContentType contentType)
    {
        if (m_SuppressNameChangedEvent)
        {
            return;
        }

        if (textBox.Text.Length == 0)
        {
            HideFloatingPanel();
            return;
        }

        var values = RunWithWaitCursor(() => valueProvider(textBox.Text.ToUpper(), MAX_SEARCH_FILTER_COUNT));
        ShowPortraitPanel(values, contentType);
        textBox.Focus();
    }

    private void ShowGenrePanel(ImmutableSortedDictionary<string, Bitmap> values, FloatingPanel.EPanelContentType contentType)
    {
        ShowFloatingPanel(values, contentType, false, false, Settings.Default.GenreImageWidth, Settings.Default.GenreImageHeight);
    }

    private void ShowPortraitPanel(ImmutableSortedDictionary<string, Bitmap> values, FloatingPanel.EPanelContentType contentType)
    {
        ShowFloatingPanel(values, contentType, false, false, Settings.Default.PortraitWidth, Settings.Default.PortraitHeight);
    }

    private Rectangle GetFloatingPanelBounds(int imageHeight)
    {
        var width = Size.Width - 12 * 2;
        var height = imageHeight * 3 + 12;
        var x = Location.X + 12;
        var y = Location.Y + SystemInformation.CaptionHeight + m_ToolStrip.Size.Height + 8;
        return new Rectangle(x, y, width, height);
    }

    private void ClearTextAndQuery(ToolStripItem item, string emptyValue = "", Action beforeClear = null, bool hideFloatingPanel = true)
    {
        beforeClear?.Invoke();

        if (hideFloatingPanel)
        {
            HideFloatingPanel();
        }

        if (string.IsNullOrEmpty(item.Text) || item.Text == emptyValue)
        {
            return;
        }

        item.Text = emptyValue;
        QueryEntries();
    }

    private void RunWithSuppressedNameChangedEvents(Action action)
    {
        m_SuppressNameChangedEvent = true;
        action();
        m_SuppressNameChangedEvent = false;
    }

    private static bool TryGetFocusedEntryId(ImageListView listView, out int id)
    {
        id = -1;
        if (listView.Items.FocusedItem == null)
        {
            return false;
        }

        id = ((string)listView.Items.FocusedItem.VirtualItemKey).ToInt();
        return id != -1;
    }

    private void SelectListItem(ImageListViewItem item, int ensureVisibleIndex)
    {
        foreach (var currentItem in m_ImageListView.Items.Cast<ImageListViewItem>())
        {
            if (currentItem != item && currentItem.Selected)
            {
                currentItem.Selected = false;
            }
        }

        m_ImageListView.Items.FocusedItem = item;
        item.Selected = true;
        m_ImageListView.EnsureVisible(ensureVisibleIndex);
    }

    private static void RunWithWaitCursor(Action action)
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            action();
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    private static T RunWithWaitCursor<T>(Func<T> func)
    {
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            return func();
        }
        finally
        {
            Cursor.Current = Cursors.Default;
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

    private void OnEntryNameTextEntered(object sender, EventArgs e)
    {
        m_ImageListView.Items.FocusedItem = null;
        HideFloatingPanel();
    }
    #endregion
}
