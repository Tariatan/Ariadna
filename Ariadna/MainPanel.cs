using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
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
using Ariadna.Themes;
using Ariadna.Storage;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna;

public partial class MainPanel : UserControl
{
    #region Private Fields
    private bool m_SuppressNameChangedEvent;

    private readonly AbstractDbStrategy m_DbStrategy;

    private readonly ImageListViewAriadnaRenderer m_ListViewRenderer;
    private readonly Theme theme;
    private readonly Font quickListFont = new("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
    private readonly bool hasInitialEntries;
    private bool loaded;
    private bool resourcesDisposed;
    private bool suppressFloatingSelection;
    private Control lastFocusedControl;

    private readonly FloatingPanel m_FloatingPanel = new();

    private readonly Timer m_TypeTimer = new();
    private enum ETypeField { None = 0, Title, Director, Actor }
    private ETypeField m_TypeField;
    private const int TYPE_TIMEOUT_MS = 200;

    private const int MAX_SEARCH_FILTER_COUNT = 200;

    #endregion

    public MainPanel() : this(new MoviesDbStrategy(NullLogger.Instance)) { }

    public MainPanel(AbstractDbStrategy strategy) : this(strategy, Theme.Create(CatalogKind.Movie)) { }

    public MainPanel(AbstractDbStrategy strategy, Theme theme) : this(strategy, theme, null) { }

    internal MainPanel(AbstractDbStrategy strategy, Theme theme, List<EntryDto> initialEntries)
    {
        this.theme = theme;
        hasInitialEntries = initialEntries != null;
        m_DbStrategy = strategy;
        m_ListViewRenderer = new ImageListViewAriadnaRenderer(theme);
        InitializeComponent();
        ApplyTheme();

        m_DbStrategy.FilterControls(this);
        m_DbStrategy.EntryInserted += OnNewEntryInserted;

        m_ImageListView.SetRenderer(m_ListViewRenderer);

        if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
        {
            UpdateImageList(initialEntries ?? m_DbStrategy.GetEntries());
        }

        // Type timer
        m_TypeField = ETypeField.None;
        m_TypeTimer.Tick += OnTypeTimer;
        m_TypeTimer.Interval = TYPE_TIMEOUT_MS;
        m_FloatingPanel.ApplyTheme(theme);
        m_FloatingPanel.Deactivate += OnFloatingPanelClosed;
        m_FloatingPanel.ItemSelected += OnFloatingPanelItemSelected;
    }
    private void ApplyTheme()
    {
        m_ToolStrip.BackColor = theme.MainBackColor;
        m_ToolStrip_AddBtn.ForeColor = theme.MainForeColor;
        m_ToolStrip_NameLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_EntryName.BackColor = theme.ControlsBackColor;
        m_ToolStrip_EntryName.ForeColor = theme.MainForeColor;
        m_ToolStrip_WishlistLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_RecentLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_NewLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_VrLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_nonVRLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_DirectorLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_DirectorName.BackColor = theme.ControlsBackColor;
        m_ToolStrip_DirectorName.ForeColor = theme.MainForeColor;
        m_ToolStrip_ActorLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_ActorName.BackColor = theme.ControlsBackColor;
        m_ToolStrip_ActorName.ForeColor = theme.MainForeColor;
        m_ToolStrip_GenreNameLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_GenreName.BackColor = theme.ControlsBackColor;
        m_ToolStrip_GenreName.ForeColor = theme.MainForeColor;
        m_ToolStrip_SubgenreNameLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_SubgenreName.BackColor = theme.ControlsBackColor;
        m_ToolStrip_SubgenreName.ForeColor = theme.MainForeColor;
        m_ToolStrip_EntriesCountLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_EntriesCount.ForeColor = theme.MainForeColor;
        m_ToolStrip_SeriesLbl.ForeColor = theme.MainForeColor;
        m_ToolStrip_MoviesLbl.ForeColor = theme.MainForeColor;
        m_QuickListFlow.BackColor = theme.MainBackColor;
        BackColor = theme.MainBackColor;
        m_ImageListView.vScrollBar.BackColor = theme.MainBackColor;
    }
    private void MainPanel_Load(object sender, EventArgs e)
        => InitializeCatalog();

    internal void InitializeCatalog()
    {
        if (loaded || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }
        loaded = true;
        if (!hasInitialEntries)
        {
            QueryEntries();
        }
        m_ImageListView.layoutManager.Update(true);
        SelectRandomEntry();
    }

    internal void PreloadVisiblePosters()
    {
        m_ImageListView.layoutManager.Update(true);
        var first = m_ImageListView.layoutManager.FirstPartiallyVisible;
        var last = m_ImageListView.layoutManager.LastPartiallyVisible;
        foreach (var item in m_ImageListView.Items.Cast<ImageListViewItem>().Skip(Math.Max(0, first)).Take(last - first + 1))
        {
            // Request only the initial viewport; the existing worker owns the cached images.
            using var thumbnail = item.ThumbnailImage;
        }
    }
    internal void SuspendCatalog()
    {
        lastFocusedControl = FindFocusedControl(this) ?? lastFocusedControl;
        if (m_TypeField == ETypeField.Title)
        {
            // Apply a pending typed search before its tab is hidden.
            m_TypeTimer.Stop();
            m_TypeField = ETypeField.None;
            QueryEntries();
        }
        m_TypeTimer.Stop();
        m_TypeField = ETypeField.None;
        HideFloatingPanel();
    }

    internal void FocusCatalog()
    {
        if (lastFocusedControl is { IsDisposed: false, CanFocus: true })
        {
            lastFocusedControl.Focus();
        }
        else
        {
            m_ImageListView.Focus();
        }
    }

    internal void HandleKeyDown(KeyEventArgs e) => MainPanel_KeyUp(this, e);
    internal void HandleKeyPress(KeyPressEventArgs e) => MainPanel_KeyPress(this, e);

    private static Control FindFocusedControl(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (child.Focused)
            {
                return child;
            }
            if (child.ContainsFocus)
            {
                return FindFocusedControl(child);
            }
        }
        return null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !resourcesDisposed)
        {
            resourcesDisposed = true;
            m_DbStrategy.EntryInserted -= OnNewEntryInserted;
            m_TypeTimer.Tick -= OnTypeTimer;
            m_TypeTimer.Dispose();
            m_FloatingPanel.Deactivate -= OnFloatingPanelClosed;
            m_FloatingPanel.ItemSelected -= OnFloatingPanelItemSelected;
            m_FloatingPanel.Dispose();
            quickListFont.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void UpdateImageList(List<EntryDto> entries)
    {
        m_ToolStrip_EntriesCount.Text = entries.Count.ToString();

        var firstChars = new HashSet<string>(entries.Count);
        var listViewItems = new List<ImageListViewItem>(entries.Count);
        foreach (var entry in entries)
        {
            firstChars.Add(entry.Title[..1].ToUpper());

            var item = new ImageListViewItem(entry.Id.ToString(), entry.Title);
            listViewItems.Add(item);
        }

        m_ImageListView.SuspendLayout();
        try
        {
            m_ImageListView.Items.Clear();
            m_ImageListView.Items.AddRange(listViewItems.ToArray(), m_DbStrategy.GetPosterImageAdapter());
            FillQuickList(firstChars);
        }
        finally
        {
            m_ImageListView.ResumeLayout(true);
        }
    }
    private void QueryEntries()
    {
        m_TypeTimer.Stop();
        m_TypeField = ETypeField.None;
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
        if (Visible)
        {
            m_ImageListView.Focus();
        }
    }
    private void FillQuickList(HashSet<string> firstChars)
    {
        var excludedCharacters = m_DbStrategy.QuickListFilter();
        var letters = firstChars.Where(firstChar => !excludedCharacters.Any(character => firstChar.Contains(character.ToUpper()))).ToArray();
        var buttons = m_QuickListFlow.Controls.Cast<Button>().ToArray();
        if (buttons.Select(button => button.Text).SequenceEqual(letters))
        {
            return;
        }

        var existingButtons = buttons.ToDictionary(button => button.Text);
        m_QuickListFlow.SuspendLayout();
        try
        {
            m_QuickListFlow.Controls.Clear();
            foreach (var button in buttons)
            {
                if (!letters.Contains(button.Text))
                {
                    button.Click -= OnQuickListClicked;
                    button.Dispose();
                }
            }

            foreach (var letter in letters)
            {
                if (!existingButtons.TryGetValue(letter, out var button))
                {
                    button = new Button
                    {
                        Text = letter,
                        AutoSize = false,
                        Size = new Size(40, 40),
                        BackColor = theme.MainBackColor,
                        ForeColor = theme.MainForeColor,
                        FlatStyle = FlatStyle.Flat,
                        Font = quickListFont,
                    };
                    button.Click += OnQuickListClicked;
                }
                m_QuickListFlow.Controls.Add(button);
            }
        }
        finally
        {
            m_QuickListFlow.ResumeLayout(true);
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
        if (e.Button != MouseButtons.Right)
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
    private void DeleteUnusedGenres() => CatalogServices.CreateStore().DeleteUnusedMovieGenres();

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
        StartTypeTimer(ETypeField.Title);
    }
    private void OnDirectorNameTextChanged(object sender, EventArgs e)
    {
        StartTypeTimer(ETypeField.Director);
    }
    private void OnActorNameTextChanged(object sender, EventArgs e)
    {
        StartTypeTimer(ETypeField.Actor);
    }
    private void OnTypeTimer(object sender, EventArgs e)
    {
        m_TypeTimer.Stop();
        var type = m_TypeField;
        m_TypeField = ETypeField.None;

        switch (type)
        {
            case ETypeField.Title:
                QueryEntries();
                break;
            case ETypeField.Director:
                OnDirectorTypeTimer();
                break;
            case ETypeField.Actor:
                OnActorTypeTimer();
                break;

            case ETypeField.None:
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

        m_FloatingPanel.UpdateListView(values, contentType, checkBox, multiSelect, imageW, imageH);
        if (!m_FloatingPanel.Visible)
        {
            m_FloatingPanel.Show(FindForm());
        }
    }
    private void HideFloatingPanel()
    {
        if (m_FloatingPanel.Visible)
        {
            suppressFloatingSelection = true;
            try
            {
                m_FloatingPanel.Hide();
            }
            finally
            {
                suppressFloatingSelection = false;
            }
        }
    }
    private void OnFloatingPanelClosed(object sender, EventArgs e)
    {
        if (m_FloatingPanel.Visible || suppressFloatingSelection)
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

        if (index is null || m_ImageListView.Items.Count == 0)
        {
            return;
        }

        var selectionIndex = index.Value > 0 ? index.Value - 1 : 0;
        if (selectionIndex >= m_ImageListView.Items.Count)
        {
            selectionIndex = m_ImageListView.Items.Count - 1;
        }

        var selection = m_ImageListView.Items[selectionIndex];
        SelectListItem(selection, selectionIndex);
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
        var origin = m_ToolStrip.PointToScreen(new Point(0, m_ToolStrip.Height));
        return new Rectangle(origin.X + 12, origin.Y + 8, Math.Max(1, Width - 24), imageHeight * 3 + 12);
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
