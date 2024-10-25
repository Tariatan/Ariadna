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
using DbProvider;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

public class LibraryDetailsForm(string filePath, ILogger logger) : DetailsForm(filePath, logger)
{
    private enum LibraryGenre
    {
        LANGUAGES,
        LITERATURE,
        PROGRAMMING,
        MISC,
        COMMON,
    }
    private LibraryGenre m_LibraryGenre;

    #region OVERRIDEN FUNCTIONS
    protected override void DoLoad()
    {
        #region Hide inappropriate fields
        m_CastList.Visible = false;
        m_CastPaste.Visible = false;
        m_LblCast.Visible = false;
        m_TxtLength.Visible = false;
        m_TxtDimension.Visible = false;
        m_LblDimensions.Visible = false;
        m_TxtBitrate.Visible = false;
        m_LblBitrate.Visible = false;
        m_LblAudioStreams.Visible = false;
        m_LblDuration.Visible = false;
        #endregion

        // Remove extension
        m_TxtTitle.Text = m_TxtTitle.Text.RemoveExtension();
        var length = Utilities.GetVideoDuration(FilePath);
        m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

        using var ctx = new AriadnaEntities();
        var entry = ctx.Libraries.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);

        #region Workaround
        m_LblDirector.Text = nameof(ctx.Authors);
        m_DirectorsList.Width = m_TxtDescription.Width;
        m_TxtDescription.Height += 100;
        m_DirectorsList.Top += 100;
        m_DirectorsList.Height = m_PicPoster.Bottom - m_DirectorsList.Top;
        m_LblDirector.Top += 100;
        m_DirectorPaste.Top += 100;
        #endregion

        m_LibraryGenre = GetGenreByPath(FilePath);

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

    protected override void DoAddListViewItemFromClipboard(ListView listView, ImageList imageList)
    {
        foreach (var item in Clipboard.GetText().Split(','))
        {
            AddNewListItem(listView, imageList, item.Capitalize());
        }
    }

    protected override bool DoStore()
    {
        var bSuccess = StoreGenres();
        bSuccess = bSuccess && StoreEntry();
        bSuccess = bSuccess && StoreAuthors();

        if (!bSuccess)
        {
            return false;
        }

        if (StoredDbEntryId == -1)
        {
            return true;
        }

        // Store tables with references
        StoreLibraryAuthors(StoredDbEntryId);
        StoreEntryGenres(StoredDbEntryId);

        return true;
    }
    protected override List<string> GetGenres()
    {
        switch (m_LibraryGenre)
        {
            case LibraryGenre.LANGUAGES:
                return Utilities.LibraryLanguagesGenres.Keys.ToList();
            case LibraryGenre.LITERATURE:
                return Utilities.LibraryLiteratureGenres.Keys.ToList();
            case LibraryGenre.PROGRAMMING:
                return Utilities.LibraryProgrammingGenres.Keys.ToList();
            case LibraryGenre.MISC:
                return Utilities.LibraryMiscGenres.Keys.ToList();
            case LibraryGenre.COMMON:
            default:
                return Utilities.LibraryGenres.Keys.ToList();
        }
    }
    protected override string GetGenreBySynonym(string name)
    {
        return Utilities.GetLibraryGenreBySynonym(name);
    }
    protected override Bitmap GetGenreImage(string name)
    {
        switch (m_LibraryGenre)
        {
            case LibraryGenre.LANGUAGES:
                return Utilities.GetLibraryLanguagesGenreImage(name);
            case LibraryGenre.LITERATURE:
                return Utilities.GetLibraryLiteratureGenreImage(name);
            case LibraryGenre.PROGRAMMING:
                return Utilities.GetLibraryProgrammingGenreImage(name);
            case LibraryGenre.MISC:
                return Utilities.GetLibraryMiscGenreImage(name);
            case LibraryGenre.COMMON:
            default:
                return Utilities.GetLibraryGenreImage(name);
        }
    }
    #endregion

    private bool StoreGenres()
    {
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;
        switch(m_LibraryGenre)
        {
            case LibraryGenre.LANGUAGES:
                m_GenresList.Items.Add(new ListViewItem("Languages"));
                break;
            case LibraryGenre.LITERATURE:
                m_GenresList.Items.Add(new ListViewItem("Literature"));
                break;
            case LibraryGenre.PROGRAMMING:
                m_GenresList.Items.Add(new ListViewItem("Programming"));
                break;
            case LibraryGenre.MISC:
                m_GenresList.Items.Add(new ListViewItem("Misc"));
                break;
        }

        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.GenreOfLibraries.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                ctx.GenreOfLibraries.Add(new GenreOfLibrary { name = item.Text });
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
    private bool StoreAuthors()
    {
        var bSuccess = true;
        using var ctx = new AriadnaEntities();
        foreach (ListViewItem item in m_DirectorsList.Items)
        {
            var bAddEntry = false;
            var author = ctx.Authors.FirstOrDefault(r => r.name == item.Text);
            if (author == null)
            {
                bAddEntry = true;
                author = new Author();
            }

            author.name = item.Text;
            author.photo = m_DirectorsPhotos.Images[item.Text].ToBytes();

            if (bAddEntry)
            {
                ctx.Authors.Add(author);
            }
        }

        try
        {
            ctx.SaveChanges();
        }
        catch (DbEntityValidationException)
        {
            MessageBox.Show(Resources.Oops, Resources.FailedToSaveDirectors, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        Library entry = null;
        if (StoredDbEntryId != -1)
        {
            entry = ctx.Libraries.FirstOrDefault(r => r.Id == StoredDbEntryId);
        }

        var bAddEntry = false;
        if (entry == null)
        {
            bAddEntry = true;
            entry = new Library();
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
            ctx.Libraries.Add(entry);
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
            StoredDbEntryId = ctx.Libraries.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault()!.Id;
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        try
        {
            m_PicPoster.Image.Save(Settings.Default.LibraryPostersRootPath + StoredDbEntryId, ImageFormat.Png);
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

        ctx.LibraryGenres.RemoveRange(ctx.LibraryGenres.Where(r => (r.libraryId == entryId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_GenresList.Items)
        {
            var genre = ctx.GenreOfLibraries.FirstOrDefault(r => r.name == item.Text);
            if (genre == null)
            {
                continue;
            }

            var entryGenre = ctx.LibraryGenres.FirstOrDefault(r => (r.libraryId == entryId && r.genreId == genre.Id));
            if (entryGenre == null)
            {
                ctx.LibraryGenres.Add(new DbProvider.LibraryGenre { libraryId = entryId, genreId = genre.Id });
                bNeedToSaveChanges = true;
            }
        }

        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    private void StoreLibraryAuthors(int libraryId)
    {
        if (m_DirectorsList.Items.Count == 0)
        {
            return;
        }

        using var ctx = new AriadnaEntities();
        var bNeedToSaveChanges = false;

        ctx.LibraryAuthors.RemoveRange(ctx.LibraryAuthors.Where(r => (r.libraryId == libraryId)));
        ctx.SaveChanges();

        foreach (ListViewItem item in m_DirectorsList.Items)
        {
            var author = ctx.Authors.FirstOrDefault(r => r.name == item.Text);
            if (author == null)
            {
                continue;
            }

            var libraryAuthor = ctx.LibraryAuthors.FirstOrDefault(r => (r.libraryId == libraryId && r.authorId == author.Id));
            if (libraryAuthor == null)
            {
                ctx.LibraryAuthors.Add(new LibraryAuthor { libraryId = libraryId, authorId = author.Id });
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
        var entry = ctx.Libraries.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
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

        var filename = Settings.Default.LibraryPostersRootPath + entry.Id;
        if (File.Exists(filename))
        {
            using var bmpTemp = new Bitmap(filename);
            m_PicPoster.Image = new Bitmap(bmpTemp);
        }

        var directorsSet = ctx.LibraryAuthors.AsNoTracking().ToArray().Where(r => (r.libraryId == entry.Id));
        foreach (var directors in directorsSet)
        {
            AddNewListItem(m_DirectorsList, m_DirectorsPhotos, directors.Author.name, directors.Author.photo.ToBitmap());
        }

        var genresSet = ctx.LibraryGenres.AsNoTracking().ToArray().Where(r => (r.libraryId == entry.Id));
        foreach (var genres in genresSet)
        {
            AddGenre(genres.GenreOfLibrary.name);
        }
    }

    private LibraryGenre GetGenreByPath(string path)
    {
        if (path.Contains("Languages", StringComparison.InvariantCultureIgnoreCase))
        {
            return LibraryGenre.LANGUAGES;
        }

        if (path.Contains("Literature", StringComparison.InvariantCultureIgnoreCase))
        {
            return LibraryGenre.LITERATURE;
        }

        if (path.Contains("Programming", StringComparison.InvariantCultureIgnoreCase))
        {
            return LibraryGenre.PROGRAMMING;
        }

        return LibraryGenre.MISC;
    }
}