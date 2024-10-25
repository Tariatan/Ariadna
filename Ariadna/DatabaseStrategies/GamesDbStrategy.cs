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
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using DbProvider;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Ariadna.DatabaseStrategies;

public class GamesDbStrategy : AbstractDbStrategy
{
    private readonly ILogger m_Logger;
    private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new();

    public GamesDbStrategy(ILogger logger)
    {
        m_Logger = logger;
        m_PosterImageAdaptor.RootPath = Settings.Default.GamePostersRootPath;
    }

    public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;
        
    public override List<EntryDto> GetEntries()
    {
        using var ctx = new AriadnaEntities();
        return ctx.Games.AsNoTracking().OrderBy(r => r.title).Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        using var ctx = new AriadnaEntities();
        IQueryable<Game> query = ctx.Games.AsNoTracking();

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
            var entry = ctx.GenreOfGames.AsNoTracking().FirstOrDefault(r => r.name == values.Genre);
            if (entry != null)
            {
                query = query.Where(r => r.GameGenres.Any(l => (l.genreId == entry.Id)));
            }
        }
        // -- WISH LIST --
        if (values.IsWish)
        {
            query = query.Where(r => (r.want_to_play == true));
        }
        // -- RECENTLY Added --
        if (values.IsRecent)
        {
            var recentDateStart = DateTime.Now.AddMonths(-6);
            query = query.Where(r => ((r.creation_time > recentDateStart)));
        }
        // -- NEW --
        if (values.IsNew)
        {
            query = query.Where(r => ((r.year == (DateTime.Now.Year)) || r.year == (DateTime.Now.Year - 1)));
        }
        // -- VR --
        if (values.IsVr)
        {
            query = query.Where(r => (r.vr == true));
        }
        // -- nonVR --
        if (values.IsNonVr)
        {
            query = query.Where(r => (r.vr == false));
        }

        return query.OrderBy(r => r.title).Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var details = new EntryInfo();
        using var ctx = new AriadnaEntities();

        var entry = ctx.Games.FirstOrDefault(r => r.Id == id);
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
        var entry = ctx.Games.FirstOrDefault(r => r.Id == id);
        if (entry == null)
        {
            return;
        }

        ctx.GameGenres.RemoveRange(ctx.GameGenres.Where(r => (r.gameId == id)));

        ctx.Games.Remove(entry);

        ctx.SaveChanges();

        var posterPath = Settings.Default.GamePostersRootPath + id;
        if (File.Exists(posterPath))
        {
            File.Delete(posterPath);
        }
        for (var i = 1u; i <= 4; ++i)
        {
            var name = posterPath + Settings.Default.PreviewSuffix + i;
            if (File.Exists(name))
            {
                File.Delete(name);
            }
        }
    }
    public override bool FindNextEntryAutomatically()
    {
        if (FindFirstNotInserted(Directory.GetDirectories(Settings.Default.DefaultGamesPath)))
        {
            return true;
        }

        if (FindFirstNotInserted(Directory.GetDirectories(Settings.Default.DefaultGamesPathVR)))
        {
            return true;
        }

        return false;
    }
    public override void FindNextEntryManually()
    {
        const string folderFlag = "Choose folder";
        // ReSharper disable once UsingStatementResourceInitialization
        using var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Settings.Default.DefaultGamesPath,
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

    public override void ShowEntryDetails(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ShowDataDialog(path);
    }
    public override void ExecuteEntry(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // Open directory
        if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Settings.Default.TotalCommanderPath,
                WorkingDirectory = Path.GetDirectoryName(Settings.Default.TotalCommanderPath)!,
                Arguments = $"/O /L=\"{path}\"",
            });
        }
        else
        {
            MessageBox.Show(path, Resources.PathNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetGenres()
    {
        var values = new SortedDictionary<string, Bitmap>();
        using var ctx = new AriadnaEntities();
        var genres = ctx.GenreOfGames.AsNoTracking().ToList();

        foreach (var genre in genres)
        {
            values[genre.name] = Utilities.GetGameGenreImage(genre.name);
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

        panel.m_ToolStrip_VRSprtr.Visible = true;
        panel.m_ToolStrip_VrLbl.Visible = true;
        panel.m_ToolStrip_VRBtn.Visible = true;
        panel.m_ToolStrip_nonVRSprtr.Visible = true;
        panel.m_ToolStrip_nonVRLbl.Visible = true;
        panel.m_ToolStrip_nonVRBtn.Visible = true;
        panel.Icon = Resources.AriadnaGames;
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

            if (ctx.Games.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() == null)
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
    private void ShowDataDialog(string path)
    {
        var detailsForm = new GameDetailsForm(path, m_Logger);
        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();
    }
    private void OnDetailsFormClosed(object sender, FormClosedEventArgs e)
    {
        var detailsForm = sender as GameDetailsForm;
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
        var path = ctx.Games.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault()?.file_path;

        return !string.IsNullOrEmpty(path) ? path : string.Empty;
    }
}