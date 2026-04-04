using System.Collections.Immutable;
using System.Drawing;
using System.Runtime.ExceptionServices;
using Ariadna.Data;
using Ariadna.DatabaseStrategies;
using Ariadna.ImageListHelpers;
using Manina.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.DatabaseStrategies;

[TestClass]
public class StrategyUiBehaviorTests
{
    [TestMethod]
    public void FilterControls_DocumentariesStrategy_HidesMovieSpecificControls()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var testee = new DocumentariesDbStrategy(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            panel.Show();

            // Act
            testee.FilterControls(panel);

            // Assert
            Assert.IsFalse(panel.m_ToolStrip_DirectorLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_DirectorName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_DirectorSprt.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorSprt.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesBtn.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesSprtr.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesBtn.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesSprtr.Visible);
            Assert.IsNotNull(panel.Icon);
        });
    }

    [TestMethod]
    public void FilterControls_GamesStrategy_ShowsVrControlsAndHidesMovieSpecificControls()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var testee = new GamesDbStrategy(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            panel.Show();

            // Act
            testee.FilterControls(panel);

            // Assert
            Assert.IsFalse(panel.m_ToolStrip_DirectorLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_DirectorName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_DirectorSprt.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorSprt.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesBtn.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesSprtr.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesBtn.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesSprtr.Visible);
            Assert.IsTrue(panel.m_ToolStrip_VrLbl.Visible);
            Assert.IsTrue(panel.m_ToolStrip_VRBtn.Visible);
            Assert.IsTrue(panel.m_ToolStrip_VRSprtr.Visible);
            Assert.IsTrue(panel.m_ToolStrip_nonVRLbl.Visible);
            Assert.IsTrue(panel.m_ToolStrip_nonVRBtn.Visible);
            Assert.IsTrue(panel.m_ToolStrip_nonVRSprtr.Visible);
            Assert.IsNotNull(panel.Icon);
        });
    }

    [TestMethod]
    public void FilterControls_LibraryStrategy_HidesMovieSpecificControlsAndRenamesDirectorLabel()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var testee = new LibraryDbStrategy(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            panel.Show();

            // Act
            testee.FilterControls(panel);

            // Assert
            Assert.IsFalse(panel.m_ToolStrip_ActorLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ActorSprt.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesBtn.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SeriesSprtr.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesBtn.Visible);
            Assert.IsFalse(panel.m_ToolStrip_MoviesSprtr.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SubgenreNameLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SubgenreName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ClearSubgenreBtn.Visible);
            Assert.AreEqual("Authors", panel.m_ToolStrip_DirectorLbl.Text);
            Assert.IsNotNull(panel.Icon);
        });
    }

    [TestMethod]
    public void UpdateSubgenre_LibraryStrategy_GenreSelected_ShowsSubgenreControls()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var testee = new LibraryDbStrategy(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            panel.Show();
            panel.m_ToolStrip_GenreName.Text = "Programming";
            panel.m_ToolStrip_SubgenreNameLbl.Visible = false;
            panel.m_ToolStrip_SubgenreName.Visible = false;
            panel.m_ToolStrip_ClearSubgenreBtn.Visible = false;

            // Act
            testee.UpdateSubgenre(panel);

            // Assert
            Assert.IsTrue(panel.m_ToolStrip_SubgenreNameLbl.Visible);
            Assert.IsTrue(panel.m_ToolStrip_SubgenreName.Visible);
            Assert.IsTrue(panel.m_ToolStrip_ClearSubgenreBtn.Visible);
        });
    }

    [TestMethod]
    public void UpdateSubgenre_LibraryStrategy_NoGenreSelected_HidesSubgenreControls()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var testee = new LibraryDbStrategy(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
            panel.Show();
            panel.m_ToolStrip_GenreName.Text = Utilities.EmptyDots;
            panel.m_ToolStrip_SubgenreNameLbl.Visible = true;
            panel.m_ToolStrip_SubgenreName.Visible = true;
            panel.m_ToolStrip_ClearSubgenreBtn.Visible = true;

            // Act
            testee.UpdateSubgenre(panel);

            // Assert
            Assert.IsFalse(panel.m_ToolStrip_SubgenreNameLbl.Visible);
            Assert.IsFalse(panel.m_ToolStrip_SubgenreName.Visible);
            Assert.IsFalse(panel.m_ToolStrip_ClearSubgenreBtn.Visible);
        });
    }

    private static MainPanel CreatePanel()
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        return new MainPanel(new StubDbStrategy());
    }

    private static void RunInSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private sealed class StubDbStrategy : AbstractDbStrategy
    {
        private readonly PosterFromFileAdaptor m_Adaptor = new();

        public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_Adaptor;
        public override List<EntryDto> GetEntries() => [];
        public override List<EntryDto> QueryEntries(QueryParams values) => [];
        public override EntryInfo GetEntryInfo(int id) => new();
        public override void ShowEntryDetails(int id) {}
        public override void ExecuteEntry(int id) {}
        public override void RemoveEntry(int id) {}
        public override bool FindNextEntryAutomatically() => false;
        public override void FindNextEntryManually() {}
        public override void UpdateSubgenre(MainPanel panel) {}
        public override string[] QuickListFilter() => [];
        public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override ImmutableSortedDictionary<string, Bitmap> GetGenres() => ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override void FilterControls(MainPanel panel) {}
    }
}