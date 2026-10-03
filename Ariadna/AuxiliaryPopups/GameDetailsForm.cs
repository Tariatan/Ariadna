#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
using Ariadna.Themes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.AuxiliaryPopups;

public partial class GameDetailsForm : Form, IEntryDetailsDialog
{
    private readonly string initialPath;
    private readonly EntryEditorSession session;

    public GameDetailsForm() : this(string.Empty, NullLogger.Instance) { }

    public GameDetailsForm(string filePath, ILogger logger)
    {
        initialPath = filePath;
        InitializeComponent();
        session = new EntryEditorSession(components, this, saveButton, CatalogKind.Game, logger, SaveEntry, genres.DismissPicker);
        DetailTheme.Apply(this, Theme.Create(CatalogKind.Game));
        fileSize.Configure(logger);
        genres.Configure(Utilities.GameGenres.Keys.ToArray(), Utilities.GetGameGenreBySynonym, Utilities.GetGameGenreImage);
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
        titleText.Text = Path.GetFileName(initialPath);
        pathText.Text = initialPath;
        vr.Checked = initialPath.Contains(Settings.Default.DefaultGamesPathVR, StringComparison.OrdinalIgnoreCase);
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
            versionText.Text = entry.Version ?? string.Empty;
            vr.Checked = entry.Vr.GetValueOrDefault();
            previews.LoadImages(session.PosterRoot, entry.Id);
        }
        session.FilePath = pathText.Text;
        await fileSize.LoadPathAsync(pathText.Text);
    }

    private bool SaveEntry()
    {
        if (string.IsNullOrWhiteSpace(titleText.Text))
        {
            titleText.Focus();
            return false;
        }
        var entry = session.CreateEntry(titleText.Text, originalTitleText.Text, yearText.Text.ToInt(), pathText.Text, wanted.Checked);
        entry.Version = session.Loaded != null && versionText.Text == (session.Loaded.Entry.Version ?? string.Empty)
            ? session.Loaded.Entry.Version : versionText.Text.Trim();
        entry.Vr = session.PreserveFlag(session.Loaded?.Entry.Vr, vr.Checked);
        var details = new CatalogDetails(entry, genres.GetGenres(), [], []);
        var images = new Dictionary<string, byte[]> { [string.Empty] = poster.GetPngBytes() };
        foreach (var image in previews.GetImages())
        {
            images[image.Key] = image.Value;
        }
        return session.Save(details, images);
    }

    private void OnPathChanged(object? sender, EventArgs e)
    {
        session.FilePath = pathText.Text;
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        fileSize.Cancel();
        genres.DismissPicker();
    }
}
