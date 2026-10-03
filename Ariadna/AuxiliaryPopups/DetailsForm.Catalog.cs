#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

public partial class DetailsForm
{
    protected CatalogStore Store => CatalogServices.CreateStore();
    private CatalogDetails? loadedCatalogDetails;

    protected bool SaveCatalogEntry(CatalogKind kind, string posterRoot)
    {
        if (string.IsNullOrWhiteSpace(m_TxtTitle.Text))
        {
            return false;
        }

        try
        {
            var previous = loadedCatalogDetails?.Entry;
            var entry = new CatalogEntry
            {
                Id = StoredDbEntryId,
                Title = m_TxtTitle.Text.Trim(),
                OriginalTitle = m_TxtTitleOrig.Text.Trim(),
                Year = m_TxtYear.Text.ToInt(),
                Path = m_TxtPath.Text.Trim(),
                Description = kind == CatalogKind.Game ? null : previous != null && m_TxtDescription.Text == Utilities.DecorateDescription(previous.Description ?? string.Empty) ? previous.Description : m_TxtDescription.Text,
                CreationDate = previous != null ? previous.CreationDate : DateOnly.FromDateTime(File.GetLastWriteTimeUtc(FilePath)),
                Wanted = PreserveNullableFlag(previous?.Wanted, m_WantToSee.Checked, previous != null),
                Vr = kind == CatalogKind.Game ? PreserveNullableFlag(previous?.Vr, m_VR.Checked, previous != null) : null,
                Version = kind == CatalogKind.Game ? m_TxtVersion.Text.Trim() : null,
            };
            var genres = m_GenresList.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray();
            var directors = kind is CatalogKind.Movie or CatalogKind.Library ? ReadPeople(m_DirectorsList, m_DirectorsPhotos) : [];
            var actors = kind == CatalogKind.Movie ? ReadPeople(m_CastList, m_CastPhotos) : [];
            var details = new CatalogDetails(entry, genres, directors, actors);
            var images = new Dictionary<string, byte[]>
            {
                [string.Empty] = PngBytes(m_PicPoster.Image),
            };
            if (kind == CatalogKind.Game)
            {
                images["_preview1"] = PngBytes(m_Preview1.Image);
                images["_preview2"] = PngBytes(m_Preview2.Image);
                images["_preview3"] = PngBytes(m_Preview3.Image);
                images["_preview4"] = PngBytes(m_Preview4.Image);
            }

            StoredDbEntryId = Store.Save(kind, details, new CatalogAssets(posterRoot, images));
            entry.Id = StoredDbEntryId;
            loadedCatalogDetails = details;
            return true;
        }
        catch (Exception exception)
        {
            m_Logger.LogError(exception, "Failed to save '{Collection}' entry '{Id}'", kind, StoredDbEntryId);
            MessageBox.Show(exception.Message, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    protected void LoadCatalogEntry(CatalogKind kind, string posterRoot)
    {
        loadedCatalogDetails = Store.GetDetails(kind, StoredDbEntryId);
        if (loadedCatalogDetails == null)
        {
            return;
        }

        var entry = loadedCatalogDetails.Entry;
        LoadBaseEntryFields(entry.Title, entry.OriginalTitle, entry.Year, entry.Path);
        if (kind == CatalogKind.Game)
        {
            m_WantToSee.Checked = entry.Wanted.GetValueOrDefault();
            m_VR.Checked = entry.Vr.GetValueOrDefault();
            m_TxtVersion.Text = entry.Version ?? string.Empty;
            LoadPosterImage(Path.Combine(posterRoot, $"{entry.Id}_preview1"), m_Preview1);
            LoadPosterImage(Path.Combine(posterRoot, $"{entry.Id}_preview2"), m_Preview2);
            LoadPosterImage(Path.Combine(posterRoot, $"{entry.Id}_preview3"), m_Preview3);
            LoadPosterImage(Path.Combine(posterRoot, $"{entry.Id}_preview4"), m_Preview4);
            if (m_Preview1.Image != null)
            {
                m_PreviewFull.Image = new Bitmap(m_Preview1.Image);
            }
        }
        else
        {
            LoadDescribedEntryFields(entry.Description ?? string.Empty, entry.Wanted.GetValueOrDefault());
        }
        LoadPosterImage(Path.Combine(posterRoot, entry.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)), m_PicPoster);
        LoadGenres(loadedCatalogDetails.Genres, name => name);
        LoadNamedItems(m_DirectorsList, m_DirectorsPhotos, loadedCatalogDetails.Directors, person => person.Name, person => person.Photo);
        LoadNamedItems(m_CastList, m_CastPhotos, loadedCatalogDetails.Actors, person => person.Name, person => person.Photo);
    }

    private static PersonPhoto[] ReadPeople(ListView list, ImageList photos) => list.Items.Cast<ListViewItem>()
        .Select(item => new PersonPhoto(item.Text, photos.Images[item.Text]?.ToBytes())).ToArray();

    private static byte[] PngBytes(Image? image)
    {
        if (image == null)
        {
            throw new InvalidOperationException("Select an image before saving.");
        }
        using var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private static bool? PreserveNullableFlag(bool? previous, bool current, bool existing)
        => existing && previous.GetValueOrDefault() == current ? previous : current;
}
