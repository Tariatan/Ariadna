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

namespace Ariadna.DbStrategies;

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
        IQueryable<Library> query = ctx.Libraries.AsNoTracking();

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
            var entry = ctx.Authors.AsNoTracking().FirstOrDefault(r => r.name == values.Director);
            if (entry != null)
            {
                query = query.Where(r => r.LibraryAuthors.Any(l => (l.authorId == entry.Id)));
            }
        }
        // -- GENRE --
        var genre = values.Subgenre != Utilities.EmptyDots ? values.Subgenre : values.Genre;
        if (!string.IsNullOrEmpty(genre))
        {
            var entry = ctx.GenreOfLibraries.AsNoTracking().FirstOrDefault(r => r.name == genre);
            if (entry != null)
            {
                query = query.Where(r => r.LibraryGenres.Any(l => (l.genreId == entry.Id)));
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
        using var ctx = new AriadnaEntities();
        var entry = ctx.Libraries.FirstOrDefault(r => r.Id == id);
        if (entry != null)
        {
            ctx.LibraryAuthors.RemoveRange(ctx.LibraryAuthors.Where(r => (r.libraryId == id)));
            ctx.LibraryGenres.RemoveRange(ctx.LibraryGenres.Where(r => (r.libraryId == id)));

            ctx.Libraries.Remove(entry);

            ctx.SaveChanges();
        }

        var posterPath = Settings.Default.LibraryPostersRootPath + id;
        if (!File.Exists(posterPath))
        {
            return;
        }

        try
        {
            File.Delete(posterPath);
        }
        catch (IOException ex)
        {
            MessageBox.Show(ex.Source, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    public override bool FindNextEntryAutomatically()
    {
        using var ctx = new AriadnaEntities();
        var foundPath = string.Empty;
        var found = false;
        foreach (var baseDir in Directory.GetDirectories(Settings.Default.DefaultLibraryPath))
        {
            foreach (var subDir in Directory.GetDirectories(baseDir, "*", SearchOption.AllDirectories))
            {
                if (Path.GetDirectoryName(subDir)!.Any(char.IsLower)) continue;
                if (Path.GetFileName(subDir).Any(char.IsLower))
                {
                    if (IsAlreadyInserted(subDir, ctx)) continue;

                    foundPath = subDir;
                    found = true;
                    break;
                }

                foreach (var file in Directory.GetFiles(subDir))
                {
                    if (IsAlreadyInserted(file, ctx)) continue;

                    foundPath = file;
                    found = true;
                    break;
                }

                if (found) break;
            }

            if (found) break;

            foreach (var file in Directory.GetFiles(baseDir))
            {
                if (IsAlreadyInserted(file, ctx)) continue;

                foundPath = file;
                found = true;
                break;
            }
        }

        if (found is false)
        {
            return false;
        }

        ShowDataDialog(foundPath);

        return true;
    }
    private bool IsAlreadyInserted(string subDir, AriadnaEntities ctx)
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
    private void ShowDataDialog(string path)
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

        Process.Start(new ProcessStartInfo
        {
            FileName = Settings.Default.TotalCommanderPath,
            WorkingDirectory = Path.GetDirectoryName(Settings.Default.TotalCommanderPath)!,
            Arguments = $"/O /L=\"{path}\"",
        });
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
    private string FindStoredEntryPathById(int id)
    {
        if (id == -1)
        {
            return string.Empty;
        }

        using var ctx = new AriadnaEntities();
        var path = ctx.Libraries.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault()?.file_path;
        
        return !string.IsNullOrEmpty(path) ? path : string.Empty;
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