#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
using Ariadna.Themes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.AuxiliaryPopups;

public partial class MovieDetailsForm : Form, IEntryDetailsDialog
{
    private readonly string initialPath;
    private readonly EntryEditorSession session;
    private readonly ITmdbMetadataService metadataService;
    private readonly CancellationTokenSource lifetime = new();
    private bool metadataEdited;
    private bool applyingMetadata;
    private bool resourcesDisposed;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int TmdbMovieIndex { get; set; } = -1;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int TmdbTvShowIndex { get; set; } = -1;

    public MovieDetailsForm() : this(string.Empty, NullLogger.Instance) { }

    public MovieDetailsForm(string filePath, ILogger logger) : this(filePath, logger, new TmdbMetadataService(logger)) { }

    internal MovieDetailsForm(string filePath, ILogger logger, ITmdbMetadataService metadataService)
    {
        initialPath = filePath;
        InitializeComponent();
        session = new EntryEditorSession(components, this, saveButton, CatalogKind.Movie, logger, SaveEntry, genres.DismissPicker);
        DetailTheme.Apply(this, Theme.Create(CatalogKind.Movie));
        fileSize.Configure(logger);
        videoInfo.Configure(logger);
        this.metadataService = metadataService;
        directors.Configure(PersonRole.Director);
        cast.Configure(PersonRole.Actor);
        directors.PeopleAdded += OnPeopleAdded;
        cast.PeopleAdded += OnPeopleAdded;
        originalTitleText.TextChanged += OnMetadataEdited;
        yearText.TextChanged += OnMetadataEdited;
        descriptionText.TextChanged += OnMetadataEdited;
        poster.ImageChanged += OnMetadataEdited;
        genres.GenresChanged += OnMetadataEdited;
        genres.Configure(Utilities.MovieGenres.Keys.ToArray(), Utilities.GetMovieGenreBySynonym, Utilities.GetMovieGenreImage);
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
            directors.LoadPeople(details.Directors);
            cast.LoadPeople(details.Actors);
        }
        session.FilePath = pathText.Text;
        metadataEdited = false;
        var sizeTask = fileSize.LoadPathAsync(pathText.Text);
        var videoTask = videoInfo.LoadPathAsync(pathText.Text);
        var metadataTask = session.Loaded == null ? LoadMetadataAsync() : Task.CompletedTask;
        await Task.WhenAll(sizeTask, videoTask, metadataTask);
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
        var details = new CatalogDetails(entry, genres.GetGenres(), directors.GetPeople(), cast.GetPeople());
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
        lifetime.Cancel();
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

    private void OnMetadataEdited(object? sender, EventArgs e)
    {
        if (!applyingMetadata)
        {
            metadataEdited = true;
        }
    }

    private async Task LoadMetadataAsync()
    {
        var token = lifetime.Token;
        try
        {
            var metadata = await metadataService.LoadAsync(TmdbMovieIndex, TmdbTvShowIndex, token);
            if (metadata == null || token.IsCancellationRequested || IsDisposed || metadataEdited)
            {
                return;
            }
            applyingMetadata = true;
            try
            {
                originalTitleText.Text = metadata.OriginalTitle;
                yearText.Text = metadata.Year.ToString(CultureInfo.InvariantCulture);
                descriptionText.Text = metadata.Description;
                if (metadata.Poster != null)
                {
                    using var image = metadata.Poster.ToBitmap();
                    poster.SetImage(image);
                }
                foreach (var genre in metadata.Genres)
                {
                    genres.AddGenre(genre);
                }
            }
            finally
            {
                applyingMetadata = false;
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    private async void OnPeopleAdded(object? sender, EventArgs e)
    {
        if (sender is not PeopleEditorControl editor)
        {
            return;
        }
        var token = lifetime.Token;
        try
        {
            var tasks = editor.GetMissingPhotoNames().Select(async name =>
            {
                var photo = await metadataService.GetPortraitAsync(name, token);
                if (photo != null && !token.IsCancellationRequested && !IsDisposed)
                {
                    editor.ApplyDownloadedPhoto(name, photo);
                }
            });
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
}
