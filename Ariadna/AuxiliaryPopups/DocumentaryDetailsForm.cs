using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Ariadna.Extension;
using Ariadna.Properties;
using DbProvider;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

public class DocumentaryDetailsForm(string filePath, ILogger logger) : DetailsForm(filePath, logger)
{
    #region OVERRIDEN FUNCTIONS
    protected override void DoLoad()
    {
        #region Hide inappropriate fields
        m_DirectorsList.Visible = false;
        m_CastList.Visible = false;
        m_DirectorPaste.Visible = false;
        m_CastPaste.Visible = false;
        m_LblDirector.Visible = false;
        m_LblCast.Visible = false;
        #endregion
        m_TxtDescription.Height = m_TxtDescription.Height * 5/2;

        // Remove extension
        m_TxtTitle.Text = m_TxtTitle.Text.RemoveExtension();
        var length = Utilities.GetVideoDuration(FilePath);
        m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

        StoredDbEntryId = GetStoredEntryId();

        if (StoredDbEntryId != -1)
        {
            FillFieldsFromFile();
        }

        FillMediaInfo(FilePath);
    }
    protected override bool StorePreEntryData()
    {
        return StoreGenres();
    }
    protected override bool StoreMainEntry()
    {
        return StoreEntry();
    }
    protected override void StoreRelatedData()
    {
        StoreEntryGenres(StoredDbEntryId);
    }
    protected override List<string> GetGenres()
    {
        return Utilities.DocumentaryGenres.Keys.ToList();
    }
    protected override string GetGenreBySynonym(string name)
    {
        return Utilities.GetDocumentaryGenreBySynonym(name);
    }
    protected override Bitmap GetGenreImage(string name)
    {
        return Utilities.GetDocumentaryGenreImage(name);
    }
    protected virtual int GetStoredEntryId()
    {
        using var ctx = new AriadnaEntities();
        return ResolveStoredEntryId(ctx.Documentaries.AsNoTracking().Where(r => r.file_path == FilePath).Select(r => (int?)r.Id));
    }
    #endregion

    private bool StoreGenres()
    {
        return SaveMissingListEntries(
            m_GenresList.Items,
            (ctx, name) =>
            {
                if (ctx.GenreOfDocumentaries.FirstOrDefault(r => r.name == name) != null)
                {
                    return false;
                }

                ctx.GenreOfDocumentaries.Add(new GenreOfDocumentary { name = name });
                return true;
            },
            Resources.Oops,
            Resources.FailedToSaveGenres);
    }
    private bool StoreEntry()
    {
        var title = m_TxtTitle.Text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        using var ctx = new AriadnaEntities();
        Documentary entry = null;
        if (StoredDbEntryId != -1)
        {
            entry = ctx.Documentaries.FirstOrDefault(r => r.Id == StoredDbEntryId);
        }

        var bAddEntry = false;
        if (entry == null)
        {
            bAddEntry = true;
            entry = new Documentary();
        }

        entry.title = title;
        entry.title_original = m_TxtTitleOrig.Text.Trim();
        entry.year = m_TxtYear.Text.ToInt();
        entry.file_path = m_TxtPath.Text.Trim();
        entry.description = m_TxtDescription.Text;
        entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
        entry.want_to_see = m_WantToSee.Checked;

        if (bAddEntry)
        {
            ctx.Documentaries.Add(entry);
        }

        var bSuccess = TrySaveChanges(ctx, title, Resources.FailedToSaveEntry);

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ResolveStoredEntryId(ctx.Documentaries.AsNoTracking().Where(r => r.file_path == path).Select(r => (int?)r.Id));
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        TrySavePng(m_PicPoster.Image, Settings.Default.DocumentaryPostersRootPath + StoredDbEntryId, Resources.FailedToSaveEntry);

        return true;
    }
    private void StoreEntryGenres(int entryId)
    {
        ReplaceListRelations(
            m_GenresList.Items,
            ctx => ctx.DocumentaryGenres.RemoveRange(ctx.DocumentaryGenres.Where(r => r.documentaryId == entryId)),
            (ctx, name) =>
            {
                var genre = ctx.GenreOfDocumentaries.FirstOrDefault(r => r.name == name);
                if (genre == null)
                {
                    return false;
                }

                if (ctx.DocumentaryGenres.FirstOrDefault(r => r.documentaryId == entryId && r.genreId == genre.Id) != null)
                {
                    return false;
                }

                ctx.DocumentaryGenres.Add(new DocumentaryGenre { documentaryId = entryId, genreId = genre.Id });
                return true;
            });
    }
    private void FillFieldsFromFile()
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Documentaries.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        LoadBaseEntryFields(entry.title, entry.title_original, entry.year, entry.file_path);
        LoadDescribedEntryFields(entry.description, Convert.ToBoolean(entry.want_to_see));

        LoadPosterImage(Settings.Default.DocumentaryPostersRootPath + entry.Id, m_PicPoster);

        LoadGenres(
            ctx.DocumentaryGenres.AsNoTracking().ToArray().Where(r => r.documentaryId == entry.Id),
            genres => genres.GenreOfDocumentary.name);
    }
}