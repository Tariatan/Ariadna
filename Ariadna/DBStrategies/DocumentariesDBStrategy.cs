using System;
using System.Collections.Generic;
using System.Linq;
using Manina.Windows.Forms;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Data.Entity.Validation;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.ImageListHelpers;
using Microsoft.Extensions.Logging;

namespace Ariadna.DBStrategies;

public class DocumentariesDbStrategy : AbstractDbStrategy
{
    private readonly ILogger logger;
    private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new PosterFromFileAdaptor();

    public DocumentariesDbStrategy(ILogger logger)
    {
        this.logger = logger;
        m_PosterImageAdaptor.RootPath = Properties.Settings.Default.DocumentaryPostersRootPath;
    }

    public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;
        
    public override List<EntryDto> GetEntries()
    {
        using var ctx = new AriadnaEntities();
        return ctx.Documentaries.AsNoTracking().OrderBy(r => r.title).
            Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        using var ctx = new AriadnaEntities();
        IQueryable<Documentary> query = ctx.Documentaries.AsNoTracking();

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
            var entry = ctx.GenreOfDocumentaries.AsNoTracking().FirstOrDefault(r => r.name == values.Genre);
            if (entry != null)
            {
                query = query.Where(r => r.DocumentaryGenres.Any(l => (l.genreId == entry.Id)));
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
            var recentDateStart = DateTime.Now.AddMonths(-Properties.Settings.Default.RecentInMonth);
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
        using var ctx = new AriadnaEntities();
        var entry = ctx.Documentaries.FirstOrDefault(r => r.Id == id);
        if (entry != null)
        {
            ctx.DocumentaryGenres.RemoveRange(ctx.DocumentaryGenres.Where(r => (r.documentaryId == id)));

            ctx.Documentaries.Remove(entry);

            ctx.SaveChanges();
        }

        var posterPath = Properties.Settings.Default.DocumentaryPostersRootPath + id;
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
        foreach(var baseDir in Directory.GetDirectories(Properties.Settings.Default.DefaultDoocumentariesPath))
        {
            if (FindFirstNotInserted(Directory.GetDirectories(baseDir)))
            {
                return true;
            }

            if (FindFirstNotInserted(Directory.GetFiles(baseDir)))
            {
                return true;
            }

        }

        return false;
    }
    public override void FindNextEntryManually()
    {
        const string folderFlag = "File or folder";
        using var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Properties.Settings.Default.DefaultDoocumentariesPath,
            Filter = Properties.Settings.Default.VideoFilesFilter,
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
        var detailsForm = new DocumentaryDetailsForm(path, logger);
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

        // Check if it is a file first
        if (File.Exists(path))
        {
            if (!File.Exists(Properties.Settings.Default.MediaPlayerPath))
            {
                return;
            }

            // Enclose the path in quotes as required by MPC
            Process.Start(Properties.Settings.Default.MediaPlayerPath, "\"" + path + "\"");
        }
        // Checked if it is a directory
        else if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Properties.Settings.Default.TotalCommanderPath,
                WorkingDirectory = Path.GetDirectoryName(Properties.Settings.Default.TotalCommanderPath)!,
                Arguments = $"/O /L=\"{path}\"",
            });
        }
        else
        {
            MessageBox.Show(path, "Путь не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    public override SortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => null;
    public override SortedDictionary<string, Bitmap> GetActors(string name, int limit) => null;
    public override SortedDictionary<string, Bitmap> GetGenres(string name)
    {
        var values = new SortedDictionary<string, Bitmap>();
        using var ctx = new AriadnaEntities();
        var genres = ctx.GenreOfDocumentaries.AsNoTracking().ToList();

        foreach (var genre in genres)
        {
            values[genre.name] = Utilities.GetDocumentaryGenreImage(genre.name);
        }

        return values;
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

        panel.Icon = Properties.Resources.AriadnaDocumentaries;
    }
    private bool FindFirstNotInserted(string[] paths)
    {
        string foundPath = null;
        using var ctx = new AriadnaEntities();
        foreach (var path in paths)
        {
            if (ctx.Ignores.AsNoTracking().FirstOrDefault(r => r.path == path) != null)
            {
                continue;
            }

            if (ctx.Documentaries.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() == null)
            {
                foundPath = path;
                break;
            }
        }

        if(string.IsNullOrEmpty(foundPath))
        {
            return false;
        }

        ShowDataDialog(foundPath);

        return true;
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
    private string FindStoredEntryPathById(int id)
    {
        if (id == -1)
        {
            return string.Empty;
        }

        using var ctx = new AriadnaEntities();
        var path = ctx.Documentaries.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault()?.file_path;
        
        return !string.IsNullOrEmpty(path) ? path : string.Empty;
    }
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
                MessageBox.Show(entry.title, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}