using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.Properties;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.DatabaseStrategies;

public class GamesDbStrategy : MediaDbStrategyBase
{
    protected override CatalogKind Kind => CatalogKind.Game;

    public GamesDbStrategy(ILogger logger) : base(logger, CatalogServices.GetPosterRoot(CatalogKind.Game))
    {
    }
        
    public override List<EntryDto> GetEntries() => QueryEntries(new QueryParams { Subgenre = Utilities.EmptyDots });
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        return Store.Query(CatalogKind.Game, CatalogServices.CreateQuery(values)).Select(entry => new EntryDto { Id = entry.Id, Title = entry.Title, Path = entry.Path }).ToList();
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var entry = Store.GetEntry(CatalogKind.Game, id);
        return entry == null ? new EntryInfo() : new EntryInfo { Path = entry.Path, Title = entry.Title, TitleOrig = entry.OriginalTitle };
    }
    public override void RemoveEntry(int id)
    {
        var posterPath = CatalogServices.GetPosterRoot(CatalogKind.Game) + id;
        var paths = new List<string> { posterPath };
        for (var i = 1u; i <= 4; ++i)
        {
            paths.Add(posterPath + Settings.Default.PreviewSuffix + i);
        }

        EntryRemovalHelper.RemoveFiles(() => RemoveEntryFromDatabase(id), paths, FileExists, DeleteFile);
    }
    public override bool FindNextEntryAutomatically()
    {
        if (TryOpenFirstNotInserted(GetDirectories(Settings.Default.DefaultGamesPath)))
        {
            return true;
        }

        if (TryOpenFirstNotInserted(GetDirectories(Settings.Default.DefaultGamesPathVR)))
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

    public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetGenres()
    {
        var values = new SortedDictionary<string, Bitmap>();
        foreach (var name in Store.GetGenres(CatalogKind.Game))
        {
            values[name] = Utilities.GetGameGenreImage(name);
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
    }
    protected virtual bool RemoveEntryFromDatabase(int id) => Store.Delete(CatalogKind.Game, id);
    protected override void ShowDataDialog(string path)
    {
        using var detailsForm = new GameDetailsForm(path, Logger);
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
    protected override string FindStoredEntryPathById(int id) => Store.GetEntry(CatalogKind.Game, id)?.Path ?? string.Empty;

}
