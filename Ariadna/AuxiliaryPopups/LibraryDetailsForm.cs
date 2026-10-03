using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
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
        m_LblDirector.Text = "Authors";
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
            LoadCatalogEntry(CatalogKind.Library, CatalogServices.GetPosterRoot(CatalogKind.Library));
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

    protected override bool StorePreEntryData() => true;
    protected override bool StoreMainEntry() => SaveCatalogEntry(CatalogKind.Library, CatalogServices.GetPosterRoot(CatalogKind.Library));
    protected override bool StorePostEntryData() => true;
    protected override void StoreRelatedData() { }
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
    protected virtual int GetStoredEntryId() => Store.FindId(CatalogKind.Library, FilePath);
    #endregion


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
