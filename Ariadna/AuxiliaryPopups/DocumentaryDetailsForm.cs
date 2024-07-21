using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
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
        m_TxtTitle.Text = m_TxtTitle.Text.RemoveExtensions();
        var length = Utilities.GetVideoDuration(FilePath);
        m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

        using var ctx = new AriadnaEntities();
        var entry = ctx.Documentaries.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);

        if (entry != null)
        {
            StoredDbEntryId = entry.Id;
        }

        if (StoredDbEntryId != -1)
        {
            FillFieldsFromFile();
        }

        FillMediaInfo(FilePath);
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
    #endregion

    private bool StoreGenres()
    {
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;
        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.GenreOfDocumentaries.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                ctx.GenreOfDocumentaries.Add(new GenreOfDocumentary { name = item.Text });
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
            MessageBox.Show(Resources.Oops, Resources.FailedToSaveGenres, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(title, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
            bSuccess = false;
        }

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ctx.Documentaries.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault()!.Id;
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        try
        {
            m_PicPoster.Image.Save(Settings.Default.DocumentaryPostersRootPath + StoredDbEntryId, ImageFormat.Png);
        }
        catch (Exception)
        {
            MessageBox.Show(Resources.FailedToSavePoster, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return true;
    }
    private void StoreEntryGenres(int entryId)
    {
        if (m_GenresList.Items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        ctx.DocumentaryGenres.RemoveRange(ctx.DocumentaryGenres.Where(r => (r.documentaryId == entryId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.GenreOfDocumentaries.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                continue;
            }

            var entryGenre = ctx.DocumentaryGenres.FirstOrDefault(r => (r.documentaryId == entryId && r.genreId == genre.Id));
            if (entryGenre == null)
            {
                ctx.DocumentaryGenres.Add(new DocumentaryGenre { documentaryId = entryId, genreId = genre.Id });
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
        var entry = ctx.Documentaries.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        m_TxtYear.Text = (entry.year > 0) ? entry.year.ToString() : string.Empty;
        m_TxtTitle.Text = entry.title;
        m_TxtTitleOrig.Text = entry.title_original;
        m_TxtPath.Text = entry.file_path;
        m_TxtDescription.Text = Utilities.DecorateDescription(entry.description);
        m_WantToSee.Checked = Convert.ToBoolean(entry.want_to_see);

        var filename = Settings.Default.DocumentaryPostersRootPath + entry.Id;
        if (File.Exists(filename))
        {
            using var bmpTemp = new Bitmap(filename);
            m_PicPoster.Image = new Bitmap(bmpTemp);
        }

        var genresSet = ctx.DocumentaryGenres.AsNoTracking().ToArray().Where(r => (r.documentaryId == entry.Id));
        foreach (var genres in genresSet)
        {
            AddGenre(genres.GenreOfDocumentary.name);
        }
    }
}