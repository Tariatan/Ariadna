using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Microsoft.Extensions.Logging;
using static System.Drawing.Imaging.ImageFormat;

namespace Ariadna.AuxiliaryPopups;

public class GameDetailsForm(string filePath, ILogger logger) : DetailsForm(filePath, logger)
{
    private readonly ILogger logger = logger;

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
        m_VR.Checked = FilePath.Contains(Properties.Settings.Default.DefaultGamesPathVR);

        Icon = Properties.Resources.AriadnaGames;
        #endregion

        using var ctx = new AriadnaEntities();
        var entry = ctx.Games.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry != null)
        {
            StoredDbEntryId = entry.Id;
        }

        if (StoredDbEntryId != -1)
        {
            FillFieldsFromFile();
        }
    }
    protected override bool DoStore()
    {
        var bSuccess = StoreGenres();
        bSuccess = bSuccess && StoreEntry();

        if (!bSuccess)
        {
            return false;
        }
            
        if (StoredDbEntryId != -1)
        {
            // Store tables with references
            StoreEntryGenres(StoredDbEntryId);
        }

        return true;
    }
    protected override List<string> GetGenres()
    {
        return Utilities.GAME_GENRES.Keys.ToList();
    }
    protected override string GetGenreBySynonym(string name)
    {
        return Utilities.GetGameGenreBySynonym(name);
    }
    protected override Bitmap GetGenreImage(string name)
    {
        return Utilities.GetGameGenreImage(name);
    }

    #endregion
    private bool StoreGenres()
    {
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;
        foreach (ListViewItem item in m_GenresList.Items)
        {
            if (ctx.GenreOfGames.FirstOrDefault(r => r.name == item.Text) == null)
            {
                ctx.GenreOfGames.Add(new GenreOfGame { name = item.Text });
                bNeedToSaveChanges = true;
            }
        }

        if (!bNeedToSaveChanges)
        {
            return true;
        }

        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show("Ой", "Ошибка сохранения списка жанров", MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        return bSuccess;
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
        entry.want_to_play = m_WanToSee.Checked;
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
        catch (DbEntityValidationException ex)
        {
            MessageBox.Show(title + ":\n" + ex.Message, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ctx.Games.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault()!.Id;
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        try
        {
            var name = Properties.Settings.Default.GamePostersRootPath + StoredDbEntryId;
            m_PicPoster.Image.Save(name, Png);

            m_Preview1.Image.Save(name + Properties.Settings.Default.PreviewSuffix + 1, Png);
            m_Preview2.Image.Save(name + Properties.Settings.Default.PreviewSuffix + 2, Png);
            m_Preview3.Image.Save(name + Properties.Settings.Default.PreviewSuffix + 3, Png);
            m_Preview4.Image.Save(name + Properties.Settings.Default.PreviewSuffix + 4, Png);
        }
        catch (Exception)
        {
            MessageBox.Show("Ошибка сохранения постера", "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        return bSuccess;
    }
    private void StoreEntryGenres(int entryId)
    {
        if (m_GenresList.Items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        ctx.GameGenres.RemoveRange(ctx.GameGenres.Where(r => (r.gameId == entryId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.GenreOfGames.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                continue;
            }

            var entryGenre = ctx.GameGenres.FirstOrDefault(r => (r.gameId == entryId && r.genreId == genre.Id));
            if (entryGenre == null)
            {
                ctx.GameGenres.Add(new GameGenre { gameId = entryId, genreId = genre.Id });
                bNeedToSaveChanges = true;
            }
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    private void FillFieldsFromFile()
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Games.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        m_TxtYear.Text = (entry.year > 0) ? entry.year.ToString() : string.Empty;
        m_TxtTitle.Text = entry.title;
        m_TxtTitleOrig.Text = entry.title_original;
        m_TxtPath.Text = entry.file_path;
        m_WanToSee.Checked = Convert.ToBoolean(entry.want_to_play);
        m_VR.Checked = Convert.ToBoolean(entry.vr);
        m_TxtVersion.Text = entry.version;

        var genresSet = ctx.GameGenres.AsNoTracking().ToArray().Where(r => (r.gameId == entry.Id));
        foreach (var genres in genresSet)
        {
            AddGenre(genres.GenreOfGame.name);
        }

        var filename = Properties.Settings.Default.GamePostersRootPath + entry.Id;
        if (File.Exists(filename))
        {
            using var bmpTemp = new Bitmap(filename);
            m_PicPoster.Image = new Bitmap(bmpTemp);
        }

        for (var i = 1u; i <= 4; ++i)
        {
            var name = filename + Properties.Settings.Default.PreviewSuffix + i;
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