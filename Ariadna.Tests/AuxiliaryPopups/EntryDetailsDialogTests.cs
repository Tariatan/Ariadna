using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.DatabaseStrategies;
using Ariadna.Storage;
using Ariadna.Themes;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
[DoNotParallelize]
public sealed class EntryDetailsDialogTests
{
    private string directory = string.Empty;
    private string? previousCatalog;
    private string? previousAssets;
    private ILoggerFactory loggerFactory = null!;

    [TestInitialize]
    public void Initialize()
    {
        directory = Path.Combine(Path.GetTempPath(), "Ariadna-form-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var catalog = Path.Combine(directory, "catalog.sqlite");
        CatalogDatabase.Create(catalog);
        previousCatalog = Environment.GetEnvironmentVariable("ARIADNA_CATALOG_PATH");
        previousAssets = Environment.GetEnvironmentVariable("ARIADNA_ASSET_ROOT");
        Environment.SetEnvironmentVariable("ARIADNA_CATALOG_PATH", catalog);
        Environment.SetEnvironmentVariable("ARIADNA_ASSET_ROOT", directory);
        loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable("ARIADNA_CATALOG_PATH", previousCatalog);
        Environment.SetEnvironmentVariable("ARIADNA_ASSET_ROOT", previousAssets);
        Directory.Delete(directory, true);
        loggerFactory.Dispose();
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void SaveCatalogEntry_ConfirmButtonThenReopen_PersistsMetadataRelationsAndImages(CatalogKind kind)
    {
        RunInSta(() =>
        {
            // Arrange
            var mediaPath = Path.Combine(directory, "sample.mp4");
            using var form = CreateForm(kind, mediaPath);
            form.Show();
            Application.DoEvents();
            Field<TextBox>(form, "titleText").Text = "Чудо";
            Field<TextBox>(form, "originalTitleText").Text = "Original";
            Field<TextBox>(form, "yearText").Text = "2026";
            if (kind != CatalogKind.Game)
            {
                Field<TextBox>(form, "descriptionText").Text = "Description";
            }
            Field<GenreSelectionControl>(form, "genres").LoadGenres(["Drama"]);
            Field<CheckBox>(form, "wanted").Checked = true;
            using var poster = new Bitmap(16, 16);
            Field<ImageEditorControl>(form, "poster").SetImage(poster);
            if (kind == CatalogKind.Game)
            {
                var previews = Field<GamePreviewsControl>(form, "previews");
                foreach (var index in Enumerable.Range(1, 4))
                {
                    var preview = (ImageEditorControl)previews.Controls.Find($"preview{index}", true).Single();
                    preview.SetImage(poster);
                }
                Field<TextBox>(form, "versionText").Text = "1.0";
                Field<CheckBox>(form, "vr").Checked = true;
            }
            if (kind is CatalogKind.Movie or CatalogKind.Library)
            {
                Field<PeopleEditorControl>(form, kind == CatalogKind.Library ? "authors" : "directors").AddPerson(new PersonPhoto("Person", null));
            }

            // Act
            Field<Button>(form, "saveButton").PerformClick();
            using var reopened = CreateForm(kind, mediaPath);
            reopened.Show();
            Application.DoEvents();

            // Assert
            Assert.AreEqual(Utilities.EFormCloseReason.SUCCESS, ((IEntryDetailsDialog)form).FormCloseReason);
            Assert.AreEqual("Чудо", Field<TextBox>(reopened, "titleText").Text);
            Assert.AreEqual("Original", Field<TextBox>(reopened, "originalTitleText").Text);
            Assert.IsTrue(Field<CheckBox>(reopened, "wanted").Checked);
            Assert.HasCount(1, Field<GenreSelectionControl>(reopened, "genres").GetGenres());
            Assert.IsTrue(File.Exists(Path.Combine(CatalogServices.GetPosterRoot(kind), ((IEntryDetailsDialog)form).StoredDbEntryId.ToString())));
            if (kind == CatalogKind.Game)
            {
                Assert.AreEqual("1.0", Field<TextBox>(reopened, "versionText").Text);
                Assert.IsTrue(Field<CheckBox>(reopened, "vr").Checked);
                foreach (var index in Enumerable.Range(1, 4))
                {
                    Assert.IsTrue(File.Exists(Path.Combine(CatalogServices.GetPosterRoot(kind), $"{((IEntryDetailsDialog)form).StoredDbEntryId}{Ariadna.Properties.Settings.Default.PreviewSuffix}{index}")));
                }
            }
            if (kind is CatalogKind.Movie or CatalogKind.Library)
            {
                Assert.HasCount(1, Field<PeopleEditorControl>(reopened, kind == CatalogKind.Library ? "authors" : "directors").GetPeople());
            }
            reopened.Close();
            CatalogServices.CreateDatabase().RecoverAssets();
        });
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void QueryEntries_VisibleGridAndEnterSearch_DisplaysMatchingSQLiteEntries(CatalogKind kind)
    {
        RunInSta(() =>
        {
            // Arrange
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            new ThemeMovies().Init();
            var store = CatalogServices.CreateStore();
            foreach (var title in (string[])["Чудо", "Other"])
            {
                store.Save(kind, new CatalogDetails(new CatalogEntry { Title = title, Path = title }, [], [], []));
            }
            var logger = loggerFactory.CreateLogger("GridTest");
            AbstractDbStrategy strategy = kind switch
            {
                CatalogKind.Movie => new MoviesDbStrategy(logger),
                CatalogKind.Documentary => new DocumentariesDbStrategy(logger),
                CatalogKind.Game => new GamesDbStrategy(logger),
                CatalogKind.Library => new LibraryDbStrategy(logger),
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
            using var panel = new MainPanel(strategy);
            panel.Show();
            Application.DoEvents();
            var grid = (ImageListView)typeof(MainPanel).GetField("m_ImageListView", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
            Assert.HasCount(2, grid.Items);
            var search = (ToolStripTextBox)typeof(MainPanel).GetField("m_ToolStrip_EntryName", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
            search.Text = "чУд";

            // Act
            typeof(MainPanel).GetMethod("OnEntryNameConfirmed", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(panel, [search, new KeyEventArgs(Keys.Enter)]);

            // Assert
            Assert.HasCount(1, grid.Items);
            Assert.AreEqual("Чудо", grid.Items[0].Text);
        });
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void LoadCatalogEntry_ExistingVideo_OpensDetailsAndReadsDuration(CatalogKind kind)
    {
        RunInSta(() =>
        {
            // Arrange
            var path = Path.Combine(directory, "existing.avi");
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "media-two-seconds.avi"), path);
            var id = CatalogServices.CreateStore().Save(kind, new CatalogDetails(new CatalogEntry { Title = "Existing", Path = path }, [], [], []));
            using var form = CreateForm(kind, path);

            // Act
            form.Show();
            Application.DoEvents();

            // Assert
            Assert.AreEqual(id, ((IEntryDetailsDialog)form).StoredDbEntryId);
            Assert.AreEqual("Existing", Field<TextBox>(form, "titleText").Text);
            if (kind is CatalogKind.Movie or CatalogKind.Documentary)
            {
                var duration = (Label)Field<VideoInfoControl>(form, "videoInfo").Controls.Find("duration", true).Single();
                UiTest.PumpUntil(() => duration.Text == "00:00:02");
                Assert.AreEqual("00:00:02", duration.Text);
            }
            form.Close();
        });
    }

    [TestMethod]
    public void SaveCatalogEntry_EditAndClearGenres_UpdatesOriginalIdAndClearsSelection()
    {
        RunInSta(() =>
        {
            // Arrange
            var path = Path.Combine(directory, "documentary.mp4");
            var store = CatalogServices.CreateStore();
            var id = store.Save(CatalogKind.Documentary, new CatalogDetails(new CatalogEntry { Title = "Before", Path = path }, ["Drama"], [], []));
            using var form = CreateForm(CatalogKind.Documentary, path);
            form.Show();
            Field<TextBox>(form, "titleText").Text = "After";
            Field<GenreSelectionControl>(form, "genres").LoadGenres([]);
            using var poster = new Bitmap(16, 16);
            Field<ImageEditorControl>(form, "poster").SetImage(poster);

            // Act
            Field<Button>(form, "saveButton").PerformClick();

            // Assert
            Assert.AreEqual(id, ((IEntryDetailsDialog)form).StoredDbEntryId);
            Assert.AreEqual("After", store.GetDetails(CatalogKind.Documentary, id)!.Entry.Title);
            Assert.HasCount(0, store.GetDetails(CatalogKind.Documentary, id)!.Genres);
            CatalogServices.CreateDatabase().RecoverAssets();
        });
    }


    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void SaveEntry_UnchangedNullableValues_PreservesNullsAndOriginalDescription(CatalogKind kind)
    {
        UiTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(directory, "nullable.mp4");
            var store = CatalogServices.CreateStore();
            var id = store.Save(kind, new CatalogDetails(new CatalogEntry { Title = "Nullable", Path = path, Description = kind == CatalogKind.Game ? null : " First\nSecond ", Wanted = null, Vr = null, CreationDate = null }, [], [], []));
            using var form = CreateForm(kind, path);
            form.Show();

            // Act
            Field<Button>(form, "saveButton").PerformClick();
            var saved = store.GetDetails(kind, id)!.Entry;

            // Assert
            Assert.AreEqual(id, ((IEntryDetailsDialog)form).StoredDbEntryId);
            Assert.IsNull(saved.Wanted);
            Assert.IsNull(saved.Vr);
            Assert.IsNull(saved.CreationDate);
            Assert.IsNull(saved.Version);
            Assert.AreEqual(kind == CatalogKind.Game ? null : " First\nSecond ", saved.Description);
        });
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void Close_EditedEntry_DoesNotSaveMetadataOrImages(CatalogKind kind)
    {
        UiTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(directory, "cancel.mp4");
            var store = CatalogServices.CreateStore();
            var id = store.Save(kind, new CatalogDetails(new CatalogEntry { Title = "Before", Path = path }, ["Drama"], [], []));
            using var form = CreateForm(kind, path);
            form.Show();
            Field<TextBox>(form, "titleText").Text = "Cancelled";
            Field<GenreSelectionControl>(form, "genres").LoadGenres([]);

            // Act
            UiTest.Key(form, "OnKeyDown", Keys.Escape);

            // Assert
            Assert.AreEqual("Before", store.GetDetails(kind, id)!.Entry.Title);
            Assert.HasCount(1, store.GetDetails(kind, id)!.Genres);
            Assert.AreNotEqual(Utilities.EFormCloseReason.SUCCESS, ((IEntryDetailsDialog)form).FormCloseReason);
            Assert.IsFalse(File.Exists(Path.Combine(CatalogServices.GetPosterRoot(kind), id.ToString())));
        });
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void SaveEntry_BlankTitle_KeepsDialogOpenAndDoesNotWrite(CatalogKind kind)
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var form = CreateForm(kind, Path.Combine(directory, "blank.mp4"));
            form.Show();
            Field<TextBox>(form, "titleText").Text = "  ";

            // Act
            Field<Button>(form, "saveButton").PerformClick();

            // Assert
            Assert.IsFalse(form.IsDisposed);
            Assert.AreEqual(-1, ((IEntryDetailsDialog)form).StoredDbEntryId);
            Assert.IsTrue(Field<Button>(form, "saveButton").Enabled);
            Assert.IsFalse(form.UseWaitCursor);
            Assert.AreEqual(-1, CatalogServices.CreateStore().FindId(kind, Path.Combine(directory, "blank.mp4")));
        });
    }

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    public void SaveEntry_ShiftConfirm_IgnoresPathWithoutCreatingEntry(CatalogKind kind)
    {
        UiTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(directory, "ignored.mp4");
            using var form = CreateForm(kind, path);
            form.Show();
            UiTest.Key(form, "OnKeyDown", Keys.Shift | Keys.ShiftKey);

            // Act
            Field<Button>(form, "saveButton").PerformClick();

            // Assert
            Assert.AreEqual(-1, CatalogServices.CreateStore().FindId(kind, path));
            CollectionAssert.Contains(CatalogServices.CreateStore().GetRegisteredPaths(kind).ToArray(), path);
            Assert.AreNotEqual(Utilities.EFormCloseReason.SUCCESS, ((IEntryDetailsDialog)form).FormCloseReason);
        });
    }

    [TestMethod]
    public void SaveEntry_MoviePeopleCleared_RemovesRelationsAndRetainsOriginalId()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(directory, "people.mp4");
            var store = CatalogServices.CreateStore();
            var id = store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie", Path = path }, [], [new PersonPhoto("Director", null)], [new PersonPhoto("Actor", null)]));
            using var form = CreateForm(CatalogKind.Movie, path);
            form.Show();
            Field<PeopleEditorControl>(form, "directors").LoadPeople([]);
            Field<PeopleEditorControl>(form, "cast").LoadPeople([]);

            // Act
            Field<Button>(form, "saveButton").PerformClick();

            // Assert
            Assert.AreEqual(id, ((IEntryDetailsDialog)form).StoredDbEntryId);
            Assert.HasCount(0, store.GetDetails(CatalogKind.Movie, id)!.Directors);
            Assert.HasCount(0, store.GetDetails(CatalogKind.Movie, id)!.Actors);
        });
    }

    [TestMethod]
    public void OnRenamed_LibraryAuthorWithMatchingDirector_LoadsAuthorPortrait()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var store = CatalogServices.CreateStore();
            using var red = new Bitmap(16, 16);
            using var blue = new Bitmap(16, 16);
            red.SetPixel(0, 0, Color.Red);
            blue.SetPixel(0, 0, Color.Blue);
            using var encoder = new ImageEditorControl();
            encoder.SetImage(red);
            var authorPhoto = encoder.GetPngBytes();
            encoder.SetImage(blue);
            var directorPhoto = encoder.GetPngBytes();
            store.Save(CatalogKind.Library, new CatalogDetails(new CatalogEntry { Title = "Book", Path = "book" }, [], [new PersonPhoto("Shared Name", authorPhoto)], []));
            store.Save(CatalogKind.Movie, new CatalogDetails(new CatalogEntry { Title = "Movie", Path = "movie" }, [], [new PersonPhoto("Shared Name", directorPhoto)], []));
            using var form = CreateForm(CatalogKind.Library, "new-book.pdf");
            form.Show();
            var authors = Field<PeopleEditorControl>(form, "authors");
            authors.AddPerson(new PersonPhoto("New Name", null));

            // Act
            typeof(PeopleEditorControl).GetMethod("OnRenamed", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(authors, [authors, new LabelEditEventArgs(0, "Shared Name")]);

            // Assert
            Assert.AreEqual("Shared Name", authors.GetPeople()[0].Name);
            CollectionAssert.AreEqual(authorPhoto, authors.GetPeople()[0].Photo!);
        });
    }

    [TestMethod]
    public void LoadMetadataAsync_ManualEditWhileRequestPending_DoesNotOverwriteEdit()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var service = new ControlledMetadata();
            using var form = new MovieDetailsForm(Path.Combine(directory, "tmdb.mp4"), loggerFactory.CreateLogger("FormTest"), service) { TmdbMovieIndex = 1 };
            form.Show();
            Field<TextBox>(form, "originalTitleText").Text = "Manual edit";

            // Act
            service.Completion.SetResult(new TmdbMetadata("Downloaded", "Description", 2026, [], null));
            UiTest.PumpUntil(() => service.Returned);
            Application.DoEvents();

            // Assert
            Assert.AreEqual("Manual edit", Field<TextBox>(form, "originalTitleText").Text);
        });
    }

    [TestMethod]
    public void LoadMetadataAsync_ManualGenreWhileRequestPending_DoesNotOverwriteEdit()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var service = new ControlledMetadata();
            using var form = new MovieDetailsForm(Path.Combine(directory, "tmdb-genre.mp4"), loggerFactory.CreateLogger("FormTest"), service) { TmdbMovieIndex = 1 };
            form.Show();
            var genres = Field<GenreSelectionControl>(form, "genres");
            genres.AddGenre("Manual genre");

            // Act
            service.Completion.SetResult(new TmdbMetadata("Downloaded", "Description", 2026, ["Another genre"], null));
            UiTest.PumpUntil(() => service.Returned);
            Application.DoEvents();

            // Assert
            CollectionAssert.AreEqual(new[] { "Manual Genre" }, genres.GetGenres());
            Assert.AreEqual(string.Empty, Field<TextBox>(form, "originalTitleText").Text);
        });
    }

    [TestMethod]
    public void Close_PendingTmdbRequest_CancelsAndIgnoresLateCompletion()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var service = new ControlledMetadata();
            using var form = new MovieDetailsForm(Path.Combine(directory, "tmdb-close.mp4"), loggerFactory.CreateLogger("FormTest"), service) { TmdbMovieIndex = 1 };
            form.Show();

            // Act
            form.Close();
            service.Completion.SetResult(new TmdbMetadata("Late result", "Description", 2026, [], null));
            UiTest.PumpUntil(() => service.Returned);
            Application.DoEvents();

            // Assert
            Assert.IsTrue(service.Token.IsCancellationRequested);
            Assert.IsTrue(service.Disposed);
            Assert.IsTrue(form.IsDisposed);
        });
    }

    private sealed class ControlledMetadata : ITmdbMetadataService
    {
        internal TaskCompletionSource<TmdbMetadata?> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationToken Token { get; private set; }
        internal bool Returned { get; private set; }
        internal bool Disposed { get; private set; }

        public async Task<TmdbMetadata?> LoadAsync(int movieId, int tvShowId, CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            var value = await Completion.Task;
            Returned = true;
            return value;
        }
        public Task<byte[]?> GetPortraitAsync(string name, CancellationToken cancellationToken) => Task.FromResult<byte[]?>(null);
        public void Dispose() => Disposed = true;
    }

    private Form CreateForm(CatalogKind kind, string path) => kind switch
    {
        CatalogKind.Movie => new MovieDetailsForm(path, loggerFactory.CreateLogger("FormTest")) { TmdbMovieIndex = -1, TmdbTvShowIndex = -1 },
        CatalogKind.Documentary => new DocumentaryDetailsForm(path, loggerFactory.CreateLogger("FormTest")),
        CatalogKind.Game => new GameDetailsForm(path, loggerFactory.CreateLogger("FormTest")),
        CatalogKind.Library => new LibraryDetailsForm(path, loggerFactory.CreateLogger("FormTest")),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static T Field<T>(Form form, string name) => (T)form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;

    private static void RunInSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException, true);
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)), "The native form workflow timed out.");
        if (failure != null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
