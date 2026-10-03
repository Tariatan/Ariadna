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

public class DocumentariesDbStrategy : MediaDbStrategyBase
{
    protected override CatalogKind Kind => CatalogKind.Documentary;

    public DocumentariesDbStrategy(ILogger logger) : base(logger, CatalogServices.GetPosterRoot(CatalogKind.Documentary))
    {
    }

    public override List<EntryDto> GetEntries() => QueryEntries(new QueryParams { Subgenre = Utilities.EmptyDots });
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        return Store.Query(CatalogKind.Documentary, CatalogServices.CreateQuery(values)).Select(entry => new EntryDto { Id = entry.Id, Title = entry.Title, Path = entry.Path }).ToList();
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var entry = Store.GetEntry(CatalogKind.Documentary, id);
        return entry == null ? new EntryInfo() : new EntryInfo { Path = entry.Path, Title = entry.Title, TitleOrig = entry.OriginalTitle };
    }
    public override void RemoveEntry(int id)
    {
        var posterPath = CatalogServices.GetPosterRoot(CatalogKind.Documentary) + id;
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
        using var detailsForm = new DocumentaryDetailsForm(path, Logger);
        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetGenres()
    {
        var values = new SortedDictionary<string, Bitmap>();
        foreach (var name in Store.GetGenres(CatalogKind.Documentary))
        {
            values[name] = Utilities.GetDocumentaryGenreImage(name);
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
    protected virtual void RemoveEntryFromDatabase(int id) { Store.Delete(CatalogKind.Documentary, id); }
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
    protected override string FindStoredEntryPathById(int id) => Store.GetEntry(CatalogKind.Documentary, id)?.Path ?? string.Empty;
    protected override bool SupportsFileExecution => true;

    // ReSharper disable once UnusedMember.Local
}
