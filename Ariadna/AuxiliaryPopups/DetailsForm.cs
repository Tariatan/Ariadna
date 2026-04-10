using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Themes;
using DbProvider;
using MediaInfo;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

public partial class DetailsForm : Form
{
    private readonly ILogger m_Logger;

    #region Public Fields
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Utilities.EFormCloseReason FormCloseReason { get; set; }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string FilePath { get; set; }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int StoredDbEntryId { get; set; }
    #endregion

    #region Protected Fields
    protected readonly FloatingPanel FloatingPanel = new();
    #endregion

    #region Private Fields
    private bool m_IsInUpdateMode;
    private bool m_IsShiftPressed;
    private CancellationTokenSource m_VolumeCalculationCancellation;
    private CancellationTokenSource m_MediaInfoLoadingCancellation;
    private readonly Color m_VolumeDefaultForeColor;
    private long m_VolumeUpdateSequence;
    private long m_LastAppliedVolumeUpdateSequence;
    private long m_MediaInfoUpdateSequence;
    private long m_LastAppliedMediaInfoUpdateSequence;

    private const int MAX_GENRE_COUNT_ALLOWED = 5;
    private const int VOLUME_PROGRESS_UPDATE_INTERVAL_MS = 200;
    #endregion

    public DetailsForm(string filePath, ILogger logger)
    {
        m_Logger = logger;
        FilePath = filePath;
        StoredDbEntryId = -1;
        InitializeComponent();
        ApplyTheme();
        m_VolumeDefaultForeColor = m_TxtVolume.ForeColor;

        m_GenresList.Sorting = SortOrder.Ascending;
    }

    private void ApplyTheme()
    {
        m_TxtTitle.BackColor = Theme.DetailsFormBackColor;
        m_TxtTitle.ForeColor = Theme.DetailsFormForeColor;
        m_TxtPath.BackColor = Theme.DetailsFormBackColor;
        m_TxtPath.ForeColor = Theme.DetailsFormForeColorDimmed;
        m_TxtTitleOrig.BackColor = Theme.DetailsFormBackColor;
        m_TxtTitleOrig.ForeColor = Theme.DetailsFormForeColor;
        m_TxtYear.BackColor = Theme.DetailsFormBackColor;
        m_TxtYear.ForeColor = Theme.DetailsFormForeColor;
        m_BtnInsert.BackColor = Theme.DetailsFormConfirmBtnBackColor;
        m_BtnInsert.ForeColor = Theme.DetailsFormForeColor;
        m_TxtLength.BackColor = Theme.DetailsFormBackColor;
        m_TxtLength.ForeColor = Theme.DetailsFormForeColorDimmed;
        m_LblDuration.ForeColor = Theme.DetailsFormForeColor;
        m_TxtDescription.BackColor = Theme.DetailsFormBackColor;
        m_TxtDescription.ForeColor = Theme.DetailsFormForeColor;
        m_WantToSee.ForeColor = Theme.DetailsFormHighlightForeColor;
        m_LblGenre.ForeColor = Theme.DetailsFormForeColor;
        m_LblTitle.ForeColor = Theme.DetailsFormForeColor;
        m_LblTitleOrig.ForeColor = Theme.DetailsFormForeColor;
        m_LblYear.ForeColor = Theme.DetailsFormForeColor;
        m_DirectorsList.BackColor = Theme.DetailsFormBackColor;
        m_DirectorsList.ForeColor = Theme.DetailsFormForeColor;
        m_LblDirector.ForeColor = Theme.DetailsFormForeColor;
        m_CastList.BackColor = Theme.DetailsFormBackColor;
        m_CastList.ForeColor = Theme.DetailsFormForeColor;
        m_GenresList.BackColor = Theme.DetailsFormBackColor;
        m_GenresList.ForeColor = Theme.DetailsFormForeColor;
        m_AddGenreBtn.ForeColor = Theme.DetailsFormForeColor;
        m_GenrePaste.ForeColor = Theme.DetailsFormForeColor;
        m_DescriptionPaste.ForeColor = Theme.DetailsFormForeColor;
        m_DirectorPaste.ForeColor = Theme.DetailsFormForeColor;
        m_CastPaste.ForeColor = Theme.DetailsFormForeColor;
        m_LblPath.ForeColor = Theme.DetailsFormForeColor;
        m_LblCast.ForeColor = Theme.DetailsFormForeColor;
        m_LblDescr.ForeColor = Theme.DetailsFormForeColor;
        m_VR.ForeColor = Theme.DetailsFormHighlightForeColor;
        m_LblVersion.ForeColor = Theme.DetailsFormForeColor;
        m_TxtVersion.BackColor = Theme.DetailsFormBackColor;
        m_TxtVersion.ForeColor = Theme.DetailsFormForeColor;
        m_LblVolume.BackColor = Theme.DetailsFormBackColor;
        m_LblVolume.ForeColor = Theme.DetailsFormForeColor;
        m_TxtVolume.BackColor = Theme.DetailsFormBackColor;
        m_TxtVolume.ForeColor = Theme.DetailsFormForeColor;
        m_TxtDimension.BackColor = Theme.DetailsFormBackColor;
        m_TxtDimension.ForeColor = Theme.DetailsFormForeColor;
        m_LblDimensions.ForeColor = Theme.DetailsFormForeColor;
        m_TxtBitrate.BackColor = Theme.DetailsFormBackColor;
        m_TxtBitrate.ForeColor = Theme.DetailsFormForeColor;
        m_LblBitrate.ForeColor = Theme.DetailsFormForeColor;
        m_LblAudioStreams.ForeColor = Theme.DetailsFormForeColor;

        BackColor = Theme.DetailsFormBackColor;
    }
    #region VIRTUAL FUNCTIONS
    protected virtual void DoLoad() { }
    protected virtual void PrepareForStore() { }
    protected virtual bool StorePreEntryData() { return true; }
    protected virtual bool StoreMainEntry() { return false; }
    protected virtual bool StorePostEntryData() { return true; }
    protected virtual bool ShouldStoreRelatedData() { return StoredDbEntryId != -1; }
    protected virtual void StoreRelatedData() { }
    protected virtual bool DoStore()
    {
        PrepareForStore();

        var bSuccess = StorePreEntryData();
        bSuccess = bSuccess && StoreMainEntry();
        bSuccess = bSuccess && StorePostEntryData();

        if (!bSuccess)
        {
            return false;
        }

        if (ShouldStoreRelatedData())
        {
            StoreRelatedData();
        }

        return true;
    }
    protected virtual void DoAddListViewItemFromClipboard(ListView listView, ImageList imageList) { }
    protected virtual List<string> GetGenres() { return []; }
    protected virtual string GetGenreBySynonym(string name) { return string.Empty; }
    protected virtual Bitmap GetGenreImage(string name) { return new Bitmap(Resources.No_Image); }
    #endregion

    private void OnLoad(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        FloatingPanel.Deactivate += OnFloatingPanelClosed;

        // Get path name
        m_TxtTitle.Text = FilePath[(FilePath.LastIndexOf('\\') + 1)..];
        m_TxtPath.Text = FilePath;

        StartVolumeCalculation(FilePath);

        m_IsShiftPressed = false;

        // Delegate to derived class
        DoLoad();

        m_IsInUpdateMode = (StoredDbEntryId != -1);
        UpdateInsertButtonText();
        m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
    }
    private async void StartVolumeCalculation(string path)
    {
        CancelVolumeCalculation();

        // Check if it is a single file first
        if (FileExists(path))
        {
            SetVolume(GetFileSize(path), false);
            return;
        }

        if (!DirectoryExists(path))
        {
            SetVolume(0, false);
            return;
        }

        var cancellation = new CancellationTokenSource();
        m_VolumeCalculationCancellation = cancellation;
        SetVolume(0, true);

        // Try calculating directory volume otherwise
        try
        {
            var volume = await CalculateDirectoryVolumeAsync(
                path,
                cancellation.Token,
                bytes => UpdateVolumeOnUiThread(bytes, true, cancellation.Token, Interlocked.Increment(ref m_VolumeUpdateSequence)));

            UpdateVolumeOnUiThread(volume, false, cancellation.Token, Interlocked.Increment(ref m_VolumeUpdateSequence));
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            if (ReferenceEquals(m_VolumeCalculationCancellation, cancellation))
            {
                m_VolumeCalculationCancellation.Dispose();
                m_VolumeCalculationCancellation = null;
            }
        }
    }
    private void CancelVolumeCalculation()
    {
        if (m_VolumeCalculationCancellation == null)
        {
            return;
        }

        m_VolumeCalculationCancellation.Cancel();
        m_VolumeCalculationCancellation.Dispose();
        m_VolumeCalculationCancellation = null;
    }
    private void UpdateVolumeOnUiThread(long volume, bool isInProgress, CancellationToken token, long updateSequence)
    {
        if (token.IsCancellationRequested || IsDisposed)
        {
            return;
        }

        try
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => UpdateVolumeOnUiThread(volume, isInProgress, token, updateSequence));
                return;
            }
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (token.IsCancellationRequested || IsDisposed)
        {
            return;
        }

        if (updateSequence < Interlocked.Read(ref m_LastAppliedVolumeUpdateSequence))
        {
            return;
        }

        Interlocked.Exchange(ref m_LastAppliedVolumeUpdateSequence, updateSequence);
        SetVolume(volume, isInProgress);
    }
    private void SetVolume(long volume, bool isInProgress)
    {
        m_TxtVolume.Text = FormatVolume(volume);
        m_TxtVolume.ForeColor = isInProgress ? Color.Yellow : m_VolumeDefaultForeColor;
    }
    private static string FormatVolume(long volume)
    {
        volume /= 1024 * 1024;
        var v = volume.ToString();
        if (v.Length > 3)
        {
            v = v.Insert(v.Length - 3, " ");
        }

        return v + " Mb";
    }
    protected virtual bool FileExists(string path)
    {
        return File.Exists(path);
    }
    protected virtual bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }
    protected virtual long GetFileSize(string path)
    {
        return new FileInfo(path).Length;
    }
    protected virtual Task<long> CalculateDirectoryVolumeAsync(string path, CancellationToken cancellationToken, Action<long> reportProgress)
    {
        return Task.Run(() => CalculateDirectoryVolume(path, cancellationToken, reportProgress), cancellationToken);
    }
    private long CalculateDirectoryVolume(string path, CancellationToken cancellationToken, Action<long> reportProgress)
    {
        var directories = new Stack<string>();
        directories.Push(path);

        long totalSize = 0;
        var lastProgressUpdate = Environment.TickCount64;

        while (directories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentPath = directories.Pop();
            foreach (var filePath in EnumerateFilesSafe(currentPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    totalSize += GetFileSize(filePath);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                if ((Environment.TickCount64 - lastProgressUpdate) < VOLUME_PROGRESS_UPDATE_INTERVAL_MS)
                {
                    continue;
                }

                reportProgress(totalSize);
                lastProgressUpdate = Environment.TickCount64;
            }

            foreach (var directoryPath in EnumerateDirectoriesSafe(currentPath))
            {
                directories.Push(directoryPath);
            }
        }

        reportProgress(totalSize);
        return totalSize;
    }
    protected virtual IEnumerable<string> EnumerateFilesSafe(string path)
    {
        try
        {
            return Directory.EnumerateFiles(path).ToArray();
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }
    protected virtual IEnumerable<string> EnumerateDirectoriesSafe(string path)
    {
        try
        {
            return Directory.EnumerateDirectories(path).ToArray();
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }
    private void OnFormClosed(object sender, FormClosedEventArgs e)
    {
        CancelVolumeCalculation();
        CancelMediaInfoLoading();
    }
    private void UpdateInsertButtonText()
    {
        if (m_IsInUpdateMode)
        {
            Text = Resources.UpdateEntry;
            m_BtnInsert.Text = Resources.Update;
        }
        else
        {
            Text = Resources.InsertEntry;
            m_BtnInsert.Text = Resources.Insert;
        }
    }
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        m_IsShiftPressed = e.Modifiers.HasFlag(Keys.Shift);

        if (e.Modifiers.HasFlag(Keys.Shift))
        {
            m_BtnInsert.Text = Resources.Ignore;
        }

        switch (e.KeyCode)
        {
            case Keys.Escape when FloatingPanel.Visible:
                FloatingPanel.Hide();
                return;
            case Keys.Escape:
                Close();
                return;
            case Keys.Enter:
                AddUpdateEntry();
                break;
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
        if (Utilities.GetBitmapFromDisk(out var bmp,
                $"Image ({Settings.Default.PosterWidth}x{Settings.Default.PosterHeight}) (*.*)|*.*", Settings.Default.PosterWidth, Settings.Default.PosterHeight))
        {
            m_PicPoster.Image = (bmp != null) ? new Bitmap(bmp) : new Bitmap(Resources.No_Preview_Image);
        }
    }
    private void OnBtnInsert_Click(object sender, EventArgs e)
    {
        AddUpdateEntry();
    }
    private void AddToIgnoreList()
    {
        using var ctx = new AriadnaEntities();
        if (ctx.Ignores.FirstOrDefault(r => r.path == FilePath) == null)
        {
            ctx.Ignores.Add(new Ignore { path = FilePath });
            ctx.SaveChanges();
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
        var bSuccess = DoStore();

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
        if (e.Control && e.KeyCode == Keys.V)
        {
            DoAddListViewItemFromClipboard(m_DirectorsList, m_DirectorsPhotos);
            return;
        }

        if (e.KeyCode == Keys.Delete)
        {
            m_DirectorsList.DeleteFocusedListItem();
            return;
        }

        if (e.KeyData == Keys.F2 && m_DirectorsList.SelectedItems.Count > 0)
        {
            m_DirectorsList.SelectedItems[0].BeginEdit();
        }
    }
    private void OnCastKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            DoAddListViewItemFromClipboard(m_CastList, m_CastPhotos);
            return;
        }

        if (e.KeyCode == Keys.Delete)
        {
            m_CastList.DeleteFocusedListItem();
            return;
        }

        if (e.KeyData == Keys.F2 && m_DirectorsList.SelectedItems.Count > 0)
        {
            m_CastList.SelectedItems[0].BeginEdit();
        }
    }
    private void OnDescriptionKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            m_TxtDescription.Text = Utilities.DecorateDescription(m_TxtDescription.Text);
        }
    }
    private void OnGenresKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            AddGenresFromClipboard();
            return;
        }

        if (e.KeyCode == Keys.Delete)
        {
            m_GenresList.DeleteFocusedListItem();
            m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
        }
    }
    private void OnDirectorsDoubleClicked(object sender, MouseEventArgs e)
    {
        if (Utilities.GetBitmapFromDisk(out Bitmap bmp,
                $"Photo ({Settings.Default.PortraitWidth}x{Settings.Default.PortraitHeight}) (*.*)|*.*", Settings.Default.PortraitWidth, Settings.Default.PortraitHeight))
        {
            bmp ??= new Bitmap(Resources.No_Preview_Image_small);

            m_DirectorsPhotos.Images[m_DirectorsPhotos.Images.IndexOfKey(m_DirectorsList.FocusedItem!.Text)] = bmp;
            m_DirectorsList.Refresh();
        }
    }
    private void OnCastDoubleClicked(object sender, MouseEventArgs e)
    {
        if (Utilities.GetBitmapFromDisk(out Bitmap bmp,
                $"Photo ({Settings.Default.PortraitWidth}x{Settings.Default.PortraitHeight}) (*.*)|*.*", Settings.Default.PortraitWidth, Settings.Default.PortraitHeight))
        {
            bmp ??= new Bitmap(Resources.No_Preview_Image_small);

            m_CastPhotos.Images[m_CastPhotos.Images.IndexOfKey(m_CastList.FocusedItem!.Text)] = bmp;
            m_CastList.Refresh();
        }
    }
    private void AddGenresFromClipboard()
    {
        foreach (var item in Clipboard.GetText().Split(','))
        {
            var name = item.Trim();
            AddGenre(name);

            if (m_GenresList.Items.Count == Settings.Default.MaxGenresCount)
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

        name = GetGenreBySynonym(name.Capitalize());
        m_GenresImages.Images.Add(name, GetGenreImage(name));
        m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));
        m_GenresList.Sort();
        m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
    }
    private void OnAddGenreClicked(object sender, EventArgs e)
    {
        if (FloatingPanel.Visible)
        {
            FloatingPanel.Hide();
            return;
        }

        var values = new SortedDictionary<string, Bitmap>();
        foreach (var name in GetGenres().Where(name => m_GenresList.FindItemWithText(name) == null))
        {
            values[name] = GetGenreImage(name);
        }

        FloatingPanel.Location = new Point(Location.X + m_GenresList.Location.X + 12,
            Location.Y + m_GenresList.Location.Y + SystemInformation.CaptionHeight + m_GenresList.Size.Height + 12);
        FloatingPanel.Size = m_GenresList.Size with { Height = Settings.Default.GenreImageHeight * 7 - 10 };
        FloatingPanel.BackColor = BackColor;

        FloatingPanel.UpdateListView(values.ToImmutableSortedDictionary(), FloatingPanel.EPanelContentType.GENRES, false, false, Settings.Default.GenreImageWidth, Settings.Default.GenreImageHeight);
        if (!FloatingPanel.Visible)
        {
            FloatingPanel.Show(this);
        }
    }
    private void OnFloatingPanelClosed(object sender, EventArgs e)
    {
        if (FloatingPanel.Visible)
        {
            return;
        }

        var name = FloatingPanel.EntryNames.FirstOrDefault();
        if (name == null)
        {
            return;
        }
        
        name = name.Capitalize();
        m_GenresImages.Images.Add(name, GetGenreImage(name));
        m_GenresList.Items.Add(new ListViewItem(name, m_GenresImages.Images.IndexOfKey(name)));

        m_AddGenreBtn.Visible = (m_GenresList.Items.Count < MAX_GENRE_COUNT_ALLOWED);
    }
    private void OnDescriptionPasteClick(object sender, EventArgs e)
    {
        m_TxtDescription.Text = Utilities.DecorateDescription(Clipboard.GetText());
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

        using var ctx = new AriadnaEntities();
        var director = ctx.Directors.AsNoTracking().FirstOrDefault(r => r.name == e.Label);
        if (director is { photo: not null })
        {
            m_DirectorsPhotos.Images.SetKeyName(m_DirectorsList.Items[e.Item].ImageIndex, e.Label);
            m_DirectorsPhotos.Images[m_DirectorsPhotos.Images.IndexOfKey(e.Label)] = director.photo.ToBitmap();
            m_DirectorsList.Refresh();
        }
    }
    private void OnActorRenamed(object sender, LabelEditEventArgs e)
    {
        if (e.Label == null)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var actor = ctx.Actors.AsNoTracking().FirstOrDefault(r => r.name == e.Label);
        if (actor is { photo: not null })
        {
            m_CastPhotos.Images.SetKeyName(m_CastList.Items[e.Item].ImageIndex, e.Label);
            m_CastPhotos.Images[m_CastPhotos.Images.IndexOfKey(e.Label)] = actor.photo.ToBitmap();
            m_CastList.Refresh();
        }
    }
    private void HideFloatingPanel(object sender, EventArgs e)
    {
        if (FloatingPanel.Visible)
        {
            FloatingPanel.Hide();
        }
    }
    private void OnFilePathChanged(object sender, EventArgs e)
    {
        FilePath = m_TxtPath.Text;
    }
    private void OnPreview_DoubleClick(object sender, EventArgs e)
    {
        var pic = sender as PictureBox;

        if (!Utilities.GetBitmapFromDisk(out var bmp,
                $"Image ({Settings.Default.PreviewWidth}x{Settings.Default.PreviewHeight}) (*.*)|*.*",
                Settings.Default.PreviewWidth, Settings.Default.PreviewHeight))
        {
            return;
        }
        
        var preview = (bmp != null) ? new Bitmap(bmp) : new Bitmap(Resources.No_Preview_Image);
        if (pic!.Name.Equals(m_Preview1.Name))
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

    private void OnClick(object sender, EventArgs e)
    {
        var pic = sender as PictureBox;
        if(pic!.Image.Width != pic.Width)
        {
            m_PreviewFull.Image = new Bitmap(pic.Image);
        }
    }
    public sealed record MediaInfoSnapshot(int Width, int Height, int VideoRate, IReadOnlyList<string> Languages);
    protected void FillMediaInfo(string path)
    {
        StartMediaInfoLoading(path);
    }
    private async void StartMediaInfoLoading(string path)
    {
        CancelMediaInfoLoading();
        ClearMediaInfo();

        var cancellation = new CancellationTokenSource();
        m_MediaInfoLoadingCancellation = cancellation;

        try
        {
            var snapshot = await LoadMediaInfoAsync(path, cancellation.Token);
            UpdateMediaInfoOnUiThread(snapshot, cancellation.Token, Interlocked.Increment(ref m_MediaInfoUpdateSequence));
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            if (ReferenceEquals(m_MediaInfoLoadingCancellation, cancellation))
            {
                m_MediaInfoLoadingCancellation.Dispose();
                m_MediaInfoLoadingCancellation = null;
            }
        }
    }
    private void CancelMediaInfoLoading()
    {
        if (m_MediaInfoLoadingCancellation == null)
        {
            return;
        }

        m_MediaInfoLoadingCancellation.Cancel();
        m_MediaInfoLoadingCancellation.Dispose();
        m_MediaInfoLoadingCancellation = null;
    }
    protected virtual Task<MediaInfoSnapshot> LoadMediaInfoAsync(string path, CancellationToken cancellationToken)
    {
        return Task.Run(() => LoadMediaInfo(path, cancellationToken), cancellationToken);
    }
    private MediaInfoSnapshot LoadMediaInfo(string path, CancellationToken cancellationToken)
    {
        path = ResolveMediaInfoPath(path);
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var info = new MediaInfoWrapper(path, m_Logger);
        cancellationToken.ThrowIfCancellationRequested();
        return new MediaInfoSnapshot(
            info.Width,
            info.Height,
            info.VideoRate,
            info.AudioStreams.Select(stream => stream.Language).ToArray());
    }
    private string ResolveMediaInfoPath(string path)
    {
        if (!DirectoryExists(path))
        {
            return path;
        }

        var firstFile = EnumerateFilesSafe(path).FirstOrDefault();
        if (string.IsNullOrEmpty(firstFile))
        {
            var firstSubDir = EnumerateDirectoriesSafe(path).FirstOrDefault();
            if (string.IsNullOrEmpty(firstSubDir) is false)
            {
                firstFile = EnumerateFilesSafe(firstSubDir!).FirstOrDefault();
            }
        }

        return firstFile;
    }
    private void UpdateMediaInfoOnUiThread(MediaInfoSnapshot snapshot, CancellationToken token, long updateSequence)
    {
        if (token.IsCancellationRequested || IsDisposed)
        {
            return;
        }

        try
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => UpdateMediaInfoOnUiThread(snapshot, token, updateSequence));
                return;
            }
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (token.IsCancellationRequested || IsDisposed)
        {
            return;
        }

        if (updateSequence < Interlocked.Read(ref m_LastAppliedMediaInfoUpdateSequence))
        {
            return;
        }

        Interlocked.Exchange(ref m_LastAppliedMediaInfoUpdateSequence, updateSequence);
        ApplyMediaInfo(snapshot);
    }
    private void ClearMediaInfo()
    {
        m_TxtDimension.Text = string.Empty;
        m_TxtBitrate.Text = string.Empty;

        foreach (var flag in GetAudioFlags())
        {
            flag.Image = null;
        }
    }
    private void ApplyMediaInfo(MediaInfoSnapshot snapshot)
    {
        ClearMediaInfo();
        if (snapshot == null)
        {
            return;
        }

        m_TxtDimension.Text = snapshot.Width.ToString() + 'x' + snapshot.Height;
        m_TxtBitrate.Text = (snapshot.VideoRate / 1000000).ToString() + ' ' + Resources.Mbps;

        var flags = GetAudioFlags();
        for (var i = 0; i < Math.Min(flags.Count, snapshot.Languages.Count); i++)
        {
            flags[i].Image = GetFlagImage(snapshot.Languages[i]);
        }
    }
    private List<PictureBox> GetAudioFlags()
    {
        return [m_PicFlag1, m_PicFlag2, m_PicFlag3, m_PicFlag4];
    }
    private static Bitmap GetFlagImage(string language)
    {
        return language switch
        {
            "Ukrainian" => Resources.ua_flag,
            "Russian" => Resources.ru_flag,
            "English" => Resources.en_flag,
            "French" => Resources.fr_flag,
            _ => Resources.unknown
        };
    }
    protected void AddNewListItem(ListView listView, ImageList imageList, string name, Bitmap image = null)
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
            using var ctx = new AriadnaEntities();
            var director = ctx.Directors.AsNoTracking().FirstOrDefault(r => r.name == name);
            if (director?.photo != null)
            {
                image = director.photo.ToBitmap();
            }

            if (image == null)
            {
                var actor = ctx.Actors.AsNoTracking().FirstOrDefault(r => r.name == name);
                if (actor?.photo != null && Utilities.IsValidPreview(actor.photo))
                {
                    image = actor.photo.ToBitmap();
                }
            }

            image ??= new Bitmap(Resources.No_Preview_Image_small);
        }

        imageList.Images.Add(name, image);
        listView.Items.Add(new ListViewItem(name, imageList.Images.IndexOfKey(name)));
    }
    protected bool TrySaveChanges(AriadnaEntities ctx, string text, string caption)
    {
        try
        {
            ctx.SaveChanges();
            return true;
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
    protected int ResolveStoredEntryId(IQueryable<int?> idQuery)
    {
        return idQuery.FirstOrDefault() ?? -1;
    }
    protected bool SaveMissingListEntries(ListView.ListViewItemCollection items, Func<AriadnaEntities, string, bool> addMissingEntry, string text, string caption)
    {
        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        foreach (ListViewItem item in items)
        {
            bNeedToSaveChanges = addMissingEntry(ctx, item.Text) || bNeedToSaveChanges;
        }

        if (!bNeedToSaveChanges)
        {
            return true;
        }

        return TrySaveChanges(ctx, text, caption);
    }
    protected bool SaveNamedPhotoEntries(ListView.ListViewItemCollection items, ImageList imageList, Func<AriadnaEntities, string, byte[], bool> upsertEntry, string text, string caption)
    {
        using var ctx = new AriadnaEntities();

        foreach (ListViewItem item in items)
        {
            var photo = imageList.Images[item.Text]?.ToBytes();
            upsertEntry(ctx, item.Text, photo);
        }

        return TrySaveChanges(ctx, text, caption);
    }
    protected void ReplaceListRelations(ListView.ListViewItemCollection items, Action<AriadnaEntities> removeExistingRelations, Func<AriadnaEntities, string, bool> addRelationIfMissing)
    {
        if (items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        removeExistingRelations(ctx);
        ctx.SaveChanges();

        foreach (ListViewItem item in items)
        {
            bNeedToSaveChanges = addRelationIfMissing(ctx, item.Text) || bNeedToSaveChanges;
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    protected void LoadPosterImage(string path, PictureBox pictureBox)
    {
        if (!File.Exists(path))
        {
            return;
        }

        using var bmpTemp = new Bitmap(path);
        pictureBox.Image = new Bitmap(bmpTemp);
    }
    protected void LoadBaseEntryFields(string title, string originalTitle, int year, string path)
    {
        m_TxtYear.Text = (year > 0) ? year.ToString() : string.Empty;
        m_TxtTitle.Text = title;
        m_TxtTitleOrig.Text = originalTitle;
        m_TxtPath.Text = path;
    }
    protected void LoadDescribedEntryFields(string description, bool wantToSee)
    {
        m_TxtDescription.Text = Utilities.DecorateDescription(description);
        m_WantToSee.Checked = wantToSee;
    }
    protected void LoadGenres<T>(IEnumerable<T> relations, Func<T, string> getGenreName)
    {
        foreach (var relation in relations)
        {
            AddGenre(getGenreName(relation));
        }
    }
    protected void LoadNamedItems<T>(ListView listView, ImageList imageList, IEnumerable<T> relations, Func<T, string> getName, Func<T, byte[]> getPhoto)
    {
        foreach (var relation in relations)
        {
            AddNewListItem(listView, imageList, getName(relation), getPhoto(relation).ToBitmap());
        }
    }
    protected bool TrySavePng(Image image, string path, string caption)
    {
        try
        {
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            return true;
        }
        catch (Exception)
        {
            MessageBox.Show(Resources.FailedToSavePoster, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
}