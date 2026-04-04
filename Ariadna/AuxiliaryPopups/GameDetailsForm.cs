using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using DbProvider;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

public class GameDetailsForm(string filePath, ILogger logger) : DetailsForm(filePath, logger)
{
    #region OVERRIDEN FUNCTIONS
    protected override void DoLoad()
    {
        #region Hide inappropriate fields
        m_TxtLength.Visible = false;
        m_TxtDescription.Visible = false;
        m_TxtDimension.Visible = false;
        m_TxtBitrate.Visible = false;
        m_DirectorsList.Visible = false;
        m_CastList.Visible = false;
        m_DescriptionPaste.Visible = false;
        m_DirectorPaste.Visible = false;
        m_CastPaste.Visible = false;
        m_LblDirector.Visible = false;
        m_LblCast.Visible = false;
        m_LblDescr.Visible = false;
        m_PicFlag1.Visible = false;
        m_PicFlag2.Visible = false;
        m_PicFlag3.Visible = false;
        m_PicFlag4.Visible = false;
        m_LblDuration.Visible = false;
        m_LblDimensions.Visible = false;
        m_LblBitrate.Visible = false;
        m_LblAudioStreams.Visible = false;
        #endregion
        #region Show my fields
        m_PreviewFull.Visible = true;
        m_Preview1.Visible = true;
        m_Preview2.Visible = true;
        m_Preview3.Visible = true;
        m_Preview4.Visible = true;
        m_VR.Visible = true;
        m_LblVersion.Visible = true;
        m_TxtVersion.Visible = true;
        m_VR.Checked = FilePath.Contains(Settings.Default.DefaultGamesPathVR);

        Icon = Resources.AriadnaGames;
        #endregion

        StoredDbEntryId = GetStoredEntryId();

        if (StoredDbEntryId != -1)
        {
            FillFieldsFromFile();
        }
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
        return Utilities.GameGenres.Keys.ToList();
    }
    protected override string GetGenreBySynonym(string name)
    {
        return Utilities.GetGameGenreBySynonym(name);
    }
    protected override Bitmap GetGenreImage(string name)
    {
        return Utilities.GetGameGenreImage(name);
    }
    protected virtual int GetStoredEntryId()
    {
        using var ctx = new AriadnaEntities();
        return ResolveStoredEntryId(ctx.Games.AsNoTracking().Where(r => r.file_path == FilePath).Select(r => (int?)r.Id));
    }

    #endregion
    private bool StoreGenres()
    {
        return SaveMissingListEntries(
            m_GenresList.Items,
            (ctx, name) =>
            {
                if (ctx.GenreOfGames.FirstOrDefault(r => r.name == name) != null)
                {
                    return false;
                }

                ctx.GenreOfGames.Add(new GenreOfGame { name = name });
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

        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        Game entry = null;
        if (StoredDbEntryId != -1)
        {
            entry = ctx.Games.FirstOrDefault(r => r.Id == StoredDbEntryId);
        }

        var bAdd = false;
        if (entry == null)
        {
            bAdd = true;
            entry = new Game();
        }

        entry.title = title.Trim();
        entry.title_original = m_TxtTitleOrig.Text.Trim();
        entry.year = m_TxtYear.Text.ToInt();
        entry.file_path = m_TxtPath.Text.Trim();
        entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
        entry.want_to_play = m_WantToSee.Checked;
        entry.vr = m_VR.Checked;
        entry.version = m_TxtVersion.Text.Trim();

        if (bAdd)
        {
            ctx.Games.Add(entry);
        }

        try
        {
            ctx.SaveChanges();
        }
        catch (Exception ex) when (ex is DbEntityValidationException)
        {
            MessageBox.Show(title + Resources.Colon + '\n' + ex.Message, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ResolveStoredEntryId(ctx.Games.AsNoTracking().Where(r => r.file_path == path).Select(r => (int?)r.Id));
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        var name = Settings.Default.GamePostersRootPath + StoredDbEntryId;
        bSuccess = TrySavePng(m_PicPoster.Image, name, Resources.FailedToSaveEntry);
        bSuccess = bSuccess && TrySavePng(m_Preview1.Image, name + Settings.Default.PreviewSuffix + 1, Resources.FailedToSaveEntry);
        bSuccess = bSuccess && TrySavePng(m_Preview2.Image, name + Settings.Default.PreviewSuffix + 2, Resources.FailedToSaveEntry);
        bSuccess = bSuccess && TrySavePng(m_Preview3.Image, name + Settings.Default.PreviewSuffix + 3, Resources.FailedToSaveEntry);
        bSuccess = bSuccess && TrySavePng(m_Preview4.Image, name + Settings.Default.PreviewSuffix + 4, Resources.FailedToSaveEntry);

        return bSuccess;
    }
    private void StoreEntryGenres(int entryId)
    {
        ReplaceListRelations(
            m_GenresList.Items,
            ctx => ctx.GameGenres.RemoveRange(ctx.GameGenres.Where(r => r.gameId == entryId)),
            (ctx, name) =>
            {
                var genre = ctx.GenreOfGames.FirstOrDefault(r => r.name == name);
                if (genre == null)
                {
                    return false;
                }

                if (ctx.GameGenres.FirstOrDefault(r => r.gameId == entryId && r.genreId == genre.Id) != null)
                {
                    return false;
                }

                ctx.GameGenres.Add(new GameGenre { gameId = entryId, genreId = genre.Id });
                return true;
            });
    }
    private void FillFieldsFromFile()
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Games.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        LoadBaseEntryFields(entry.title, entry.title_original, entry.year, entry.file_path);
        m_WantToSee.Checked = Convert.ToBoolean(entry.want_to_play);
        m_VR.Checked = Convert.ToBoolean(entry.vr);
        m_TxtVersion.Text = entry.version;

        LoadGenres(
            ctx.GameGenres.AsNoTracking().ToArray().Where(r => r.gameId == entry.Id),
            genres => genres.GenreOfGame.name);

        var filename = Settings.Default.GamePostersRootPath + entry.Id;
        LoadPosterImage(filename, m_PicPoster);

        for (var i = 1u; i <= 4; ++i)
        {
            var name = filename + Settings.Default.PreviewSuffix + i;
            if (!File.Exists(name))
            {
                continue;
            }

            using var bmpTemp = new Bitmap(name);
            switch (i)
            {
                case 1: m_Preview1.Image = new Bitmap(bmpTemp);
                    m_PreviewFull.Image = new Bitmap(m_Preview1.Image); break;
                case 2: m_Preview2.Image = new Bitmap(bmpTemp); break;
                case 3: m_Preview3.Image = new Bitmap(bmpTemp); break;
                case 4: m_Preview4.Image = new Bitmap(bmpTemp); break;
            }
        }
    }
}