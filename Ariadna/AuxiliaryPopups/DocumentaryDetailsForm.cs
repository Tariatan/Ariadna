#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
using Ariadna.Themes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.AuxiliaryPopups;

public partial class DocumentaryDetailsForm : Form, IEntryDetailsDialog
{
    private readonly string initialPath;
    private readonly EntryEditorSession session;

    public DocumentaryDetailsForm() : this(string.Empty, NullLogger.Instance) { }

    public DocumentaryDetailsForm(string filePath, ILogger logger)
    {
        initialPath = filePath;
        InitializeComponent();
        session = new EntryEditorSession(components, this, saveButton, CatalogKind.Documentary, logger, SaveEntry, genres.DismissPicker);
        DetailTheme.Apply(this, Theme.Create(CatalogKind.Documentary));
        fileSize.Configure(logger);
        videoInfo.Configure(logger);
        genres.Configure(Utilities.DocumentaryGenres.Keys.ToArray(), Utilities.GetDocumentaryGenreBySynonym, Utilities.GetDocumentaryGenreImage);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int StoredDbEntryId => session.StoredDbEntryId;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Utilities.EFormCloseReason FormCloseReason => session.FormCloseReason;

    private async void OnLoad(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(initialPath))
        {
            return;
        }
        session.Load(initialPath);
        titleText.Text = Path.GetFileName(initialPath).RemoveExtension();
        pathText.Text = initialPath;
        if (session.Loaded is { } details)
        {
            var entry = details.Entry;
            titleText.Text = entry.Title;
            originalTitleText.Text = entry.OriginalTitle;
            yearText.Text = entry.Year > 0 ? entry.Year.ToString(CultureInfo.InvariantCulture) : string.Empty;
            pathText.Text = entry.Path;
            wanted.Checked = entry.Wanted.GetValueOrDefault();
            genres.LoadGenres(details.Genres);
            poster.LoadImage(Path.Combine(session.PosterRoot, entry.Id.ToString(CultureInfo.InvariantCulture)));
            descriptionText.Text = Utilities.DecorateDescription(entry.Description ?? string.Empty);
        }
        session.FilePath = pathText.Text;
        var sizeTask = fileSize.LoadPathAsync(pathText.Text);
        var videoTask = videoInfo.LoadPathAsync(pathText.Text);
        await Task.WhenAll(sizeTask, videoTask);
    }

    private bool SaveEntry()
    {
        if (string.IsNullOrWhiteSpace(titleText.Text))
        {
            titleText.Focus();
            return false;
        }
        var entry = session.CreateEntry(titleText.Text, originalTitleText.Text, yearText.Text.ToInt(), pathText.Text, wanted.Checked);
        entry.Description = session.PreserveDescription(descriptionText.Text);
        var details = new CatalogDetails(entry, genres.GetGenres(), [], []);
        var images = new Dictionary<string, byte[]> { [string.Empty] = poster.GetPngBytes() };
        return session.Save(details, images);
    }

    private void OnPathChanged(object? sender, EventArgs e)
    {
        session.FilePath = pathText.Text;
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        fileSize.Cancel();
        videoInfo.Cancel();
        genres.DismissPicker();
    }

    private void OnDescriptionPaste(object? sender, EventArgs e)
        => descriptionText.Text = Utilities.DecorateDescription(Clipboard.GetText());

    private void OnDescriptionKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            descriptionText.Text = Utilities.DecorateDescription(descriptionText.Text);
        }
    }
}
