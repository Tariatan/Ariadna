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
public sealed class DetailsFormCatalogTests
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
            Field<TextBox>(form, "m_TxtTitle").Text = "Чудо";
            Field<TextBox>(form, "m_TxtTitleOrig").Text = "Original";
            Field<TextBox>(form, "m_TxtYear").Text = "2026";
            Field<TextBox>(form, "m_TxtDescription").Text = "Description";
            Field<ListView>(form, "m_GenresList").Items.Add("Drama");
            Field<CheckBox>(form, "m_WantToSee").Checked = true;
            Field<PictureBox>(form, "m_PicPoster").Image = new Bitmap(16, 16);
            if (kind == CatalogKind.Game)
            {
                foreach (var preview in (string[])["m_Preview1", "m_Preview2", "m_Preview3", "m_Preview4"])
                {
                    Field<PictureBox>(form, preview).Image = new Bitmap(16, 16);
                }
                Field<TextBox>(form, "m_TxtVersion").Text = "1.0";
                Field<CheckBox>(form, "m_VR").Checked = true;
            }
            if (kind is CatalogKind.Movie or CatalogKind.Library)
            {
                Field<ImageList>(form, "m_DirectorsPhotos").Images.Add("Person", new Bitmap(16, 16));
                Field<ListView>(form, "m_DirectorsList").Items.Add(new ListViewItem("Person", "Person"));
            }

            // Act
            Field<Button>(form, "m_BtnInsert").PerformClick();
            using var reopened = CreateForm(kind, mediaPath);
            reopened.Show();
            Application.DoEvents();

            // Assert
            Assert.AreEqual(Utilities.EFormCloseReason.SUCCESS, form.FormCloseReason);
            Assert.AreEqual("Чудо", Field<TextBox>(reopened, "m_TxtTitle").Text);
            Assert.AreEqual("Original", Field<TextBox>(reopened, "m_TxtTitleOrig").Text);
            Assert.IsTrue(Field<CheckBox>(reopened, "m_WantToSee").Checked);
            Assert.HasCount(1, Field<ListView>(reopened, "m_GenresList").Items);
            Assert.IsTrue(File.Exists(Path.Combine(CatalogServices.GetPosterRoot(kind), form.StoredDbEntryId.ToString())));
            if (kind == CatalogKind.Game)
            {
                Assert.AreEqual("1.0", Field<TextBox>(reopened, "m_TxtVersion").Text);
                Assert.IsTrue(Field<CheckBox>(reopened, "m_VR").Checked);
                foreach (var index in Enumerable.Range(1, 4))
                {
                    Assert.IsTrue(File.Exists(Path.Combine(CatalogServices.GetPosterRoot(kind), $"{form.StoredDbEntryId}_preview{index}")));
                }
            }
            if (kind is CatalogKind.Movie or CatalogKind.Library)
            {
                Assert.HasCount(1, Field<ListView>(reopened, "m_DirectorsList").Items);
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
            Assert.AreEqual(id, form.StoredDbEntryId);
            Assert.AreEqual("Existing", Field<TextBox>(form, "m_TxtTitle").Text);
            if (kind != CatalogKind.Game)
            {
                Assert.AreEqual("00:00:02", Field<TextBox>(form, "m_TxtLength").Text);
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
            Field<TextBox>(form, "m_TxtTitle").Text = "After";
            Field<ListView>(form, "m_GenresList").Items.Clear();
            Field<PictureBox>(form, "m_PicPoster").Image = new Bitmap(16, 16);

            // Act
            Field<Button>(form, "m_BtnInsert").PerformClick();

            // Assert
            Assert.AreEqual(id, form.StoredDbEntryId);
            Assert.AreEqual("After", store.GetDetails(CatalogKind.Documentary, id)!.Entry.Title);
            Assert.HasCount(0, store.GetDetails(CatalogKind.Documentary, id)!.Genres);
            CatalogServices.CreateDatabase().RecoverAssets();
        });
    }

    private DetailsForm CreateForm(CatalogKind kind, string path) => kind switch
    {
        CatalogKind.Movie => new MovieDetailsForm(path, loggerFactory.CreateLogger("FormTest")) { TmdbMovieIndex = -1, TmdbTvShowIndex = -1 },
        CatalogKind.Documentary => new DocumentaryDetailsForm(path, loggerFactory.CreateLogger("FormTest")),
        CatalogKind.Game => new GameDetailsForm(path, loggerFactory.CreateLogger("FormTest")),
        CatalogKind.Library => new LibraryDetailsForm(path, loggerFactory.CreateLogger("FormTest")),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static T Field<T>(DetailsForm form, string name) => (T)typeof(DetailsForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;

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
