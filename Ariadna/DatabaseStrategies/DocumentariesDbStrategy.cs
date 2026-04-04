using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.Properties;
using DbProvider;
using Microsoft.Extensions.Logging;

namespace Ariadna.DatabaseStrategies;

public class DocumentariesDbStrategy : MediaDbStrategyBase
{
    public DocumentariesDbStrategy(ILogger logger) : base(logger, Settings.Default.DocumentaryPostersRootPath)
    {
    }

    public override List<EntryDto> GetEntries()
    {
        using var ctx = new AriadnaEntities();
        return ctx.Documentaries.AsNoTracking().OrderBy(r => r.title).
            Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        using var ctx = new AriadnaEntities();
        return QueryEntries(values, CreateQuerySource(ctx));
    }
    protected List<EntryDto> QueryEntries(QueryParams values, DocumentaryQuerySource source)
    {
        IQueryable<Documentary> query = source.Entries;

        // -- Search Name --
        if (!string.IsNullOrEmpty(values.Name))
        {
            var toSearch = values.Name.ToUpper();
            query = query.Where(r => r.title.ToUpper().Contains(toSearch) ||
                                     r.title_original.ToUpper().Contains(toSearch) ||
                                     r.file_path.ToUpper().Contains(toSearch));
        }
        // -- GENRE --
        if (!string.IsNullOrEmpty(values.Genre))
        {
            var genreId = source.FindGenreId(values.Genre);
            if (genreId.HasValue)
            {
                query = query.Where(r => r.DocumentaryGenres.Any(l => l.genreId == genreId.Value));
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
    protected virtual DocumentaryQuerySource CreateQuerySource(AriadnaEntities ctx)
    {
        return new DocumentaryQuerySource
        {
            Entries = ctx.Documentaries.AsNoTracking(),
            FindGenreId = name => ctx.GenreOfDocumentaries.AsNoTracking().Where(r => r.name == name).Select(r => (int?)r.Id).FirstOrDefault(),
        };
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var details = new EntryInfo();
        using var ctx = new AriadnaEntities();

        var entry = ctx.Documentaries.FirstOrDefault(r => r.Id == id);
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
        var posterPath = Settings.Default.DocumentaryPostersRootPath + id;
        EntryRemovalHelper.RemoveSingleFile(() => RemoveEntryFromDatabase(id), posterPath, FileExists, DeleteFile, ShowMessage);
    }
    public override bool FindNextEntryAutomatically()
    {
        foreach(var baseDir in GetDirectories(Settings.Default.DefaultDoocumentariesPath))
        {
            if (TryOpenFirstNotInserted(GetDirectories(baseDir)))
            {
                return true;
            }

            if (TryOpenFirstNotInserted(GetFiles(baseDir)))
            {
                return true;
            }

        }

        return false;
    }
    public override void FindNextEntryManually()
    {
        const string folderFlag = "File or folder";
        // ReSharper disable once UsingStatementResourceInitialization
        using var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Settings.Default.DefaultDoocumentariesPath,
            Filter = Settings.Default.VideoFilesFilter,
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

    public override void UpdateSubgenre(MainPanel panel) {}
    public override string[] QuickListFilter() => ["}", "«"];

    protected override void ShowDataDialog(string path)
    {
        var detailsForm = new DocumentaryDetailsForm(path, Logger);
        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetGenres()
    {
        var values = new SortedDictionary<string, Bitmap>();
        using var ctx = new AriadnaEntities();
        var genres = ctx.GenreOfDocumentaries.AsNoTracking().ToList();

        foreach (var genre in genres)
        {
            values[genre.name] = Utilities.GetDocumentaryGenreImage(genre.name);
        }

        return values.ToImmutableSortedDictionary();
    }
    public override void FilterControls(MainPanel panel)
    {
        panel.m_ToolStrip_DirectorLbl.Visible = false;
        panel.m_ToolStrip_ActorLbl.Visible = false;
        panel.m_ToolStrip_ActorName.Visible = false;
        panel.m_ToolStrip_DirectorName.Visible = false;
        panel.m_ToolStrip_ClearActorBtn.Visible = false;
        panel.m_ToolStrip_ClearDirectorBtn.Visible = false;
        panel.m_ToolStrip_ClearDirectorBtn.Visible = false;
        panel.m_ToolStrip_DirectorSprt.Visible = false;
        panel.m_ToolStrip_ActorSprt.Visible = false;
        panel.m_ToolStrip_SeriesBtn.Visible = false;
        panel.m_ToolStrip_SeriesLbl.Visible = false;
        panel.m_ToolStrip_SeriesSprtr.Visible = false;
        panel.m_ToolStrip_MoviesBtn.Visible = false;
        panel.m_ToolStrip_MoviesLbl.Visible = false;
        panel.m_ToolStrip_MoviesSprtr.Visible = false;

        panel.Icon = Resources.AriadnaDocumentaries;
    }
    protected virtual void RemoveEntryFromDatabase(int id)
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Documentaries.FirstOrDefault(r => r.Id == id);
        if (entry == null)
        {
            return;
        }

        ctx.DocumentaryGenres.RemoveRange(ctx.DocumentaryGenres.Where(r => (r.documentaryId == id)));
        ctx.Documentaries.Remove(entry);
        ctx.SaveChanges();
    }
    private void OnDetailsFormClosed(object sender, FormClosedEventArgs e)
    {
        var detailsForm = sender as DocumentaryDetailsForm;
        if (detailsForm!.FormCloseReason != Utilities.EFormCloseReason.SUCCESS)
        {
            return;
        }

        var eventArgs = new EntryInsertedEventArgs(detailsForm.StoredDbEntryId);
        OnEntryInserted(eventArgs);
    }
    protected override string FindStoredEntryPathById(int id)
    {
        if (id == -1)
        {
            return string.Empty;
        }

        using var ctx = new AriadnaEntities();
        var path = ctx.Documentaries.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault()?.file_path;
        
        return !string.IsNullOrEmpty(path) ? path : string.Empty;
    }
    protected override bool SupportsFileExecution => true;
    protected override bool IsStoredPath(string path, AriadnaEntities ctx)
        => ctx.Documentaries.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() is not null;
    protected class DocumentaryQuerySource
    {
        public required IQueryable<Documentary> Entries { get; init; }
        public required Func<string, int?> FindGenreId { get; init; }
    }
    // ReSharper disable once UnusedMember.Local
    private void DeleteUnusedGenres()
    {
        using var ctx = new AriadnaEntities();
        var genres = ctx.GenreOfDocumentaries.ToList();

        var bNeedToSaveChanges = false;
        foreach (var genre in genres)
        {
            var usedGenres = ctx.DocumentaryGenres.FirstOrDefault(r => (r.genreId == genre.Id));
            if (usedGenres == null)
            {
                ctx.GenreOfDocumentaries.Remove(genre);
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
        var entries = ctx.Documentaries.ToList();

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