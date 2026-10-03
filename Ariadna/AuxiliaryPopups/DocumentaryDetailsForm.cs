using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
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
            LoadCatalogEntry(CatalogKind.Documentary, CatalogServices.GetPosterRoot(CatalogKind.Documentary));
        }

        FillMediaInfo(FilePath);
    }
    protected override bool StorePreEntryData() => true;
    protected override bool StoreMainEntry() => SaveCatalogEntry(CatalogKind.Documentary, CatalogServices.GetPosterRoot(CatalogKind.Documentary));
    protected override void StoreRelatedData() { }
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
    protected virtual int GetStoredEntryId() => Store.FindId(CatalogKind.Documentary, FilePath);
    #endregion

}
