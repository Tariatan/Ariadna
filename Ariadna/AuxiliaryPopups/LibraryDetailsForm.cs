using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
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

        #region Workaround
        m_LblDirector.Text = nameof(AriadnaEntities.Authors);
        m_DirectorsList.Width = m_TxtDescription.Width;
        m_TxtDescription.Height += 100;
        m_DirectorsList.Top += 100;
        m_DirectorsList.Height = m_PicPoster.Bottom - m_DirectorsList.Top;
        m_LblDirector.Top += 100;
        m_DirectorPaste.Top += 100;
        #endregion

        m_LibraryGenre = GetGenreByPath(FilePath);
        StoredDbEntryId = GetStoredEntryId();

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

    protected override bool StorePreEntryData()
    {
        return StoreGenres();
    }
    protected override bool StoreMainEntry()
    {
        return StoreEntry();
    }
    protected override bool StorePostEntryData()
    {
        return StoreAuthors();
    }
    protected override void StoreRelatedData()
    {
        StoreLibraryAuthors(StoredDbEntryId);
        StoreEntryGenres(StoredDbEntryId);
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
    protected virtual int GetStoredEntryId()
    {
        using var ctx = new AriadnaEntities();
        return ResolveStoredEntryId(ctx.Libraries.AsNoTracking().Where(r => r.file_path == FilePath).Select(r => (int?)r.Id));
    }
    #endregion

    private bool StoreGenres()
    {
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

        return SaveMissingListEntries(
            m_GenresList.Items,
            (innerCtx, name) =>
            {
                if (innerCtx.GenreOfLibraries.FirstOrDefault(r => r.name == name) != null)
                {
                    return false;
                }

                innerCtx.GenreOfLibraries.Add(new GenreOfLibrary { name = name });
                return true;
            },
            Resources.Oops,
            Resources.FailedToSaveGenres);
    }
    private bool StoreAuthors()
    {
        return SaveNamedPhotoEntries(
            m_DirectorsList.Items,
            m_DirectorsPhotos,
            (ctx, name, photo) =>
            {
                var bAddEntry = false;
                var author = ctx.Authors.FirstOrDefault(r => r.name == name);
                if (author == null)
                {
                    bAddEntry = true;
                    author = new Author();
                }

                author.name = name;
                author.photo = photo;

                if (bAddEntry)
                {
                    ctx.Authors.Add(author);
                }

                return true;
            },
            Resources.Oops,
            Resources.FailedToSaveDirectors);
    }
    private bool StoreEntry()
    {
        var title = m_TxtTitle.Text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

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

        var bSuccess = TrySaveChanges(ctx, title, Resources.FailedToSaveEntry);

        var path = entry.file_path;

        if (bSuccess)
        {
            StoredDbEntryId = ResolveStoredEntryId(ctx.Libraries.AsNoTracking().Where(r => r.file_path == path).Select(r => (int?)r.Id));
            bSuccess = (StoredDbEntryId != -1);
        }

        if (!bSuccess)
        {
            return false;
        }

        TrySavePng(m_PicPoster.Image, Settings.Default.LibraryPostersRootPath + StoredDbEntryId, Resources.FailedToSaveEntry);

        return true;
    }
    private void StoreEntryGenres(int entryId)
    {
        ReplaceListRelations(
            m_GenresList.Items,
            ctx => ctx.LibraryGenres.RemoveRange(ctx.LibraryGenres.Where(r => r.libraryId == entryId)),
            (ctx, name) =>
            {
                var genre = ctx.GenreOfLibraries.FirstOrDefault(r => r.name == name);
                if (genre == null)
                {
                    return false;
                }

                if (ctx.LibraryGenres.FirstOrDefault(r => r.libraryId == entryId && r.genreId == genre.Id) != null)
                {
                    return false;
                }

                ctx.LibraryGenres.Add(new DbProvider.LibraryGenre { libraryId = entryId, genreId = genre.Id });
                return true;
            });
    }
    private void StoreLibraryAuthors(int libraryId)
    {
        ReplaceListRelations(
            m_DirectorsList.Items,
            ctx => ctx.LibraryAuthors.RemoveRange(ctx.LibraryAuthors.Where(r => r.libraryId == libraryId)),
            (ctx, name) =>
            {
                var author = ctx.Authors.FirstOrDefault(r => r.name == name);
                if (author == null)
                {
                    return false;
                }

                if (ctx.LibraryAuthors.FirstOrDefault(r => r.libraryId == libraryId && r.authorId == author.Id) != null)
                {
                    return false;
                }

                ctx.LibraryAuthors.Add(new LibraryAuthor { libraryId = libraryId, authorId = author.Id });
                return true;
            });
    }
    private void FillFieldsFromFile()
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Libraries.AsNoTracking().FirstOrDefault(r => r.file_path == FilePath);
        if (entry == null)
        {
            return;
        }

        LoadBaseEntryFields(entry.title, entry.title_original, entry.year, entry.file_path);
        LoadDescribedEntryFields(entry.description, Convert.ToBoolean(entry.want_to_see));

        LoadPosterImage(Settings.Default.LibraryPostersRootPath + entry.Id, m_PicPoster);

        LoadNamedItems(
            m_DirectorsList,
            m_DirectorsPhotos,
            ctx.LibraryAuthors
                .AsNoTracking()
                .Include(r => r.Author)
                .Where(r => r.libraryId == entry.Id)
                .ToArray(),
            directors => directors.Author.name,
            directors => directors.Author.photo);

        LoadGenres(
            ctx.LibraryGenres
                .AsNoTracking()
                .Include(r => r.GenreOfLibrary)
                .Where(r => r.libraryId == entry.Id)
                .ToArray(),
            genres => genres.GenreOfLibrary.name);
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
