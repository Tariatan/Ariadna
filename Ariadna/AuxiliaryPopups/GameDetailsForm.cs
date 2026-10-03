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
            LoadCatalogEntry(CatalogKind.Game, CatalogServices.GetPosterRoot(CatalogKind.Game));
        }
    }
    protected override bool StorePreEntryData() => true;
    protected override bool StoreMainEntry() => SaveCatalogEntry(CatalogKind.Game, CatalogServices.GetPosterRoot(CatalogKind.Game));
    protected override void StoreRelatedData() { }
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
    protected virtual int GetStoredEntryId() => Store.FindId(CatalogKind.Game, FilePath);

    #endregion
}
