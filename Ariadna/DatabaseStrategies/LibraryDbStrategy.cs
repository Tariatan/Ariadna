using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.Extension;
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using Ariadna.Storage;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Ariadna.DatabaseStrategies;

public class LibraryDbStrategy : AbstractDbStrategy
{
    private CatalogStore Store => CatalogServices.CreateStore();

    private readonly ILogger m_Logger;
    private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new();

    public LibraryDbStrategy(ILogger logger)
    {
        m_Logger = logger;
        m_PosterImageAdaptor.RootPath = CatalogServices.GetPosterRoot(CatalogKind.Library);
    }

    public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;

    public override List<EntryDto> GetEntries() => QueryEntries(new QueryParams { Subgenre = Utilities.EmptyDots });
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        return Store.Query(CatalogKind.Library, CatalogServices.CreateQuery(values, library: true)).Select(entry => new EntryDto { Id = entry.Id, Title = entry.Title, Path = entry.Path }).ToList();
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var entry = Store.GetEntry(CatalogKind.Library, id);
        return entry == null ? new EntryInfo() : new EntryInfo { Path = entry.Path, Title = entry.Title, TitleOrig = entry.OriginalTitle };
    }
    public override void RemoveEntry(int id)
    {
        var posterPath = CatalogServices.GetPosterRoot(CatalogKind.Library) + id;
        EntryRemovalHelper.RemoveSingleFile(() => RemoveEntryFromDatabase(id), posterPath, FileExists, DeleteFile, ShowMessage);
    }
    public override bool FindNextEntryAutomatically()
    {
        var foundPath = FindNextEntryPathAutomatically();
        if (string.IsNullOrEmpty(foundPath))
        {
            return false;
        }

        ShowDataDialog(foundPath);

        return true;
    }
    public override void FindNextEntryManually()
    {
        const string folderFlag = "File or folder";
        // ReSharper disable once UsingStatementResourceInitialization
        using var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Settings.Default.DefaultLibraryPath,
            Filter = Settings.Default.LibraryFilesFilter,
            FilterIndex = 1,
            RestoreDirectory = true,

            // Allow folders
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = folderFlag,
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }
                
        var path = openFileDialog.FileName;
        if (path.Contains(folderFlag))
        {
            path = Path.GetDirectoryName(openFileDialog.FileName);
        }
        ShowDataDialog(path);
    }

    public override void UpdateSubgenre(MainPanel panel)
    {
        var genreSelected = panel.m_ToolStrip_GenreName.Text != Utilities.EmptyDots;
        panel.m_ToolStrip_SubgenreName.Visible = genreSelected;
        panel.m_ToolStrip_ClearSubgenreBtn.Visible = genreSelected;
        panel.m_ToolStrip_SubgenreNameLbl.Visible = genreSelected;
    }

    public override string[] QuickListFilter() => ["}", "«", "(", "9"];

    public override void ShowEntryDetails(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ShowDataDialog(path);
    }
    protected virtual void ShowDataDialog(string path)
    {
        var detailsForm = new LibraryDetailsForm(path, m_Logger);
        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();
    }
    public override void ExecuteEntry(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        OpenDirectoryPath(path);
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit)
    {
        var values = new SortedDictionary<string, Bitmap>();
        foreach (var person in Store.SuggestPeople(false, true, name, limit))
        {
            values[person.Name] = person.Photo.ToBitmap();
        }
        return values.ToImmutableSortedDictionary();
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name)
    {
        if (name.Contains("English", StringComparison.InvariantCultureIgnoreCase))
        {
            return Utilities.LibraryLanguagesGenres.ToImmutableSortedDictionary();
        }

        if (name.Contains("Literature", StringComparison.InvariantCultureIgnoreCase))
        {
            return Utilities.LibraryLiteratureGenres.ToImmutableSortedDictionary();
        }

        if (name.Contains("Programming", StringComparison.InvariantCultureIgnoreCase))
        {
            return Utilities.LibraryProgrammingGenres.ToImmutableSortedDictionary();
        }

        return Utilities.LibraryMiscGenres.ToImmutableSortedDictionary();
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetGenres()
    {
        return Utilities.LibraryGenres.ToImmutableSortedDictionary();
    }
    public override void FilterControls(MainPanel panel)
    {
        panel.m_ToolStrip_ActorLbl.Visible = false;
        panel.m_ToolStrip_ActorName.Visible = false;
        panel.m_ToolStrip_ClearActorBtn.Visible = false;
        panel.m_ToolStrip_ClearDirectorBtn.Visible = false;
        panel.m_ToolStrip_ClearDirectorBtn.Visible = false;
        panel.m_ToolStrip_ActorSprt.Visible = false;
        panel.m_ToolStrip_SeriesBtn.Visible = false;
        panel.m_ToolStrip_SeriesLbl.Visible = false;
        panel.m_ToolStrip_SeriesSprtr.Visible = false;
        panel.m_ToolStrip_MoviesBtn.Visible = false;
        panel.m_ToolStrip_MoviesLbl.Visible = false;
        panel.m_ToolStrip_MoviesSprtr.Visible = false;

        panel.m_ToolStrip_SubgenreNameLbl.Visible = false;
        panel.m_ToolStrip_SubgenreName.Visible = false;
        panel.m_ToolStrip_ClearSubgenreBtn.Visible = false;

        // ReSharper disable once LocalizableElement
        panel.m_ToolStrip_DirectorLbl.Text = "Authors";

        panel.Icon = Resources.AriadnaLibrary;
    }
    private void OnDetailsFormClosed(object sender, FormClosedEventArgs e)
    {
        var detailsForm = sender as LibraryDetailsForm;
        if (detailsForm!.FormCloseReason != Utilities.EFormCloseReason.SUCCESS)
        {
            return;
        }

        var eventArgs = new EntryInsertedEventArgs(detailsForm.StoredDbEntryId);
        OnEntryInserted(eventArgs);
    }
    protected virtual string FindStoredEntryPathById(int id) => Store.GetEntry(CatalogKind.Library, id)?.Path ?? string.Empty;
    protected virtual void RemoveEntryFromDatabase(int id) { Store.Delete(CatalogKind.Library, id); }
    protected virtual IReadOnlyCollection<string> GetRegisteredPaths() => Store.GetRegisteredPaths(CatalogKind.Library);
    protected virtual string FindNextEntryPathAutomatically()
    {
        var registered = GetRegisteredPaths();
        Func<string, bool> isAlreadyInserted = path => registered.Any(stored => CatalogStore.PathsEqual(stored, path));
        foreach (var baseDir in GetDirectories(Settings.Default.DefaultLibraryPath))
        {
            var foundPath = FindNextEntryPathInBaseDirectory(baseDir, isAlreadyInserted);
            if (!string.IsNullOrEmpty(foundPath))
            {
                return foundPath;
            }
        }

        return string.Empty;
    }
    protected virtual string FindNextEntryPathInBaseDirectory(string baseDir, Func<string, bool> isAlreadyInserted)
    {
        foreach (var subDir in GetDirectoriesRecursive(baseDir))
        {
            var foundPath = FindNextEntryPathInSubDirectory(subDir, isAlreadyInserted);
            if (!string.IsNullOrEmpty(foundPath))
            {
                return foundPath;
            }
        }

        return EntryDiscoveryHelper.FindFirstNotInserted(GetFiles(baseDir), isAlreadyInserted);
    }
    protected virtual string FindNextEntryPathInSubDirectory(string subDir, Func<string, bool> isAlreadyInserted)
    {
        if (Path.GetDirectoryName(subDir)!.Any(char.IsLower))
        {
            return string.Empty;
        }

        if (Path.GetFileName(subDir).Any(char.IsLower))
        {
            return isAlreadyInserted(subDir) ? string.Empty : subDir;
        }

        return EntryDiscoveryHelper.FindFirstNotInserted(GetFiles(subDir), isAlreadyInserted);
    }
    protected virtual string[] GetDirectories(string path) => Directory.GetDirectories(path);
    protected virtual string[] GetDirectoriesRecursive(string path) => Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
    protected virtual string[] GetFiles(string path) => Directory.GetFiles(path);
    protected virtual void OpenDirectoryPath(string path) => EntryExecutionHelper.OpenDirectoryInTotalCommander(path, Settings.Default.TotalCommanderPath, StartProcess);
    protected virtual bool FileExists(string path) => File.Exists(path);
    protected virtual void DeleteFile(string path) => File.Delete(path);
    protected virtual void StartProcess(ProcessStartInfo startInfo) => Process.Start(startInfo);
    protected virtual void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) => MessageBox.Show(text, caption, buttons, icon);

    // ReSharper disable once UnusedMember.Local
}
