using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity.Validation;
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
using DbProvider;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Ariadna.DatabaseStrategies;

public class LibraryDbStrategy : AbstractDbStrategy
{
    private readonly ILogger m_Logger;
    private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new();

    public LibraryDbStrategy(ILogger logger)
    {
        m_Logger = logger;
        m_PosterImageAdaptor.RootPath = Settings.Default.LibraryPostersRootPath;
    }

    public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;

    public override List<EntryDto> GetEntries()
    {
        using var ctx = new AriadnaEntities();
        return ctx.Libraries.AsNoTracking().OrderBy(r => r.title).
            Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        using var ctx = new AriadnaEntities();
        return QueryEntries(values, CreateQuerySource(ctx));
    }
    protected List<EntryDto> QueryEntries(QueryParams values, LibraryQuerySource source)
    {
        IQueryable<Library> query = source.Entries;

        // -- Search Name --
        if (!string.IsNullOrEmpty(values.Name))
        {
            var toSearch = values.Name.ToUpper();
            query = query.Where(r => r.title.ToUpper().Contains(toSearch) ||
                                     r.title_original.ToUpper().Contains(toSearch) ||
                                     r.file_path.ToUpper().Contains(toSearch));
        }
        // -- AUTHOR NAME --
        if (!string.IsNullOrEmpty(values.Director))
        {
            var authorId = source.FindAuthorId(values.Director);
            if (authorId.HasValue)
            {
                query = query.Where(r => r.LibraryAuthors.Any(l => l.authorId == authorId.Value));
            }
        }
        // -- GENRE --
        var genre = values.Subgenre != Utilities.EmptyDots ? values.Subgenre : values.Genre;
        if (!string.IsNullOrEmpty(genre))
        {
            var genreId = source.FindGenreId(genre);
            if (genreId.HasValue)
            {
                query = query.Where(r => r.LibraryGenres.Any(l => l.genreId == genreId.Value));
            }
        }
        // -- WISH LIST --
        if (values.IsWish)
        {
            query = query.Where(r => (r.want_to_see == true));
        }
        // -- RECENTLY Added --
        if (values.IsRecent)
        {
            var recentDateStart = DateTime.Now.AddMonths(-Settings.Default.RecentInMonth);
            query = query.Where(r => ((r.creation_time > recentDateStart)));
        }
        // -- NEW --
        if (values.IsNew)
        {
            query = query.Where(r => ((r.year == (DateTime.Now.Year)) || r.year == (DateTime.Now.Year - 1)));
        }

        return query.OrderBy(r => r.title).Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    protected virtual LibraryQuerySource CreateQuerySource(AriadnaEntities ctx)
    {
        return new LibraryQuerySource
        {
            Entries = ctx.Libraries.AsNoTracking(),
            FindAuthorId = name => ctx.Authors.AsNoTracking().Where(r => r.name == name).Select(r => (int?)r.Id).FirstOrDefault(),
            FindGenreId = name => ctx.GenreOfLibraries.AsNoTracking().Where(r => r.name == name).Select(r => (int?)r.Id).FirstOrDefault(),
        };
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var details = new EntryInfo();
        using var ctx = new AriadnaEntities();

        var entry = ctx.Libraries.FirstOrDefault(r => r.Id == id);
        if (entry == null)
        {
            return details;
        }

        details.Path = entry.file_path;
        details.Title = entry.title;
        details.TitleOrig = entry.title_original;

        return details;
    }
    public override void RemoveEntry(int id)
    {
        var posterPath = Settings.Default.LibraryPostersRootPath + id;
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
    protected virtual bool IsAlreadyInserted(string subDir, AriadnaEntities ctx)
    {
        if (ctx.Ignores.AsNoTracking().FirstOrDefault(r => r.path == subDir) is not null)
        {
            return true;
        }

        return ctx.Libraries.AsNoTracking().Where(r => r.file_path == subDir).Select(r => r.file_path).FirstOrDefault() is not null;
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
        using var ctx = new AriadnaEntities();
        var directors = ctx.LibraryAuthors.AsNoTracking().Where(r => r.Author.name.ToUpper().Contains(name)).Take(limit);

        foreach (var director in directors)
        {
            values[director.Author.name] = director.Author.photo.ToBitmap();
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
    protected virtual string FindStoredEntryPathById(int id)
    {
        if (id == -1)
        {
            return string.Empty;
        }

        using var ctx = new AriadnaEntities();
        var path = ctx.Libraries.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault()?.file_path;
        
        return !string.IsNullOrEmpty(path) ? path : string.Empty;
    }
    protected virtual void RemoveEntryFromDatabase(int id)
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Libraries.FirstOrDefault(r => r.Id == id);
        if (entry == null)
        {
            return;
        }

        ctx.LibraryAuthors.RemoveRange(ctx.LibraryAuthors.Where(r => (r.libraryId == id)));
        ctx.LibraryGenres.RemoveRange(ctx.LibraryGenres.Where(r => (r.libraryId == id)));
        ctx.Libraries.Remove(entry);
        ctx.SaveChanges();
    }
    protected virtual string FindNextEntryPathAutomatically()
    {
        using var ctx = new AriadnaEntities();
        Func<string, bool> isAlreadyInserted = path => IsAlreadyInserted(path, ctx);
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
    protected class LibraryQuerySource
    {
        public required IQueryable<Library> Entries { get; init; }
        public required Func<string, int?> FindAuthorId { get; init; }
        public required Func<string, int?> FindGenreId { get; init; }
    }
    // ReSharper disable once UnusedMember.Local
    private void DeleteUnusedGenres()
    {
        using var ctx = new AriadnaEntities();
        var genres = ctx.GenreOfLibraries.ToList();

        var bNeedToSaveChanges = false;
        foreach (var genre in genres)
        {
            var usedGenres = ctx.LibraryGenres.FirstOrDefault(r => (r.genreId == genre.Id));

            if (usedGenres == null)
            {
                ctx.GenreOfLibraries.Remove(genre);
                bNeedToSaveChanges = true;
            }
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    // ReSharper disable once UnusedMember.Local
    private void UpdateEntryData()
    {
        using var ctx = new AriadnaEntities();
        var entries = ctx.Libraries.ToList();

        foreach(var entry in entries)
        {
            entry.creation_time = File.GetLastWriteTimeUtc(entry.file_path);

            try
            {
                ctx.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                MessageBox.Show(entry.title, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}