using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Ariadna.Data;
using Ariadna.DatabaseStrategies;
using Ariadna.ImageListHelpers;
using Manina.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.ImageListHelpers;

[TestClass]
public class ImageListViewAriadnaRendererTests
{
    [TestMethod]
    public void MeasureItem_RendererAttachedToListView_ReturnsThumbnailSizeWithPadding()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var renderer = GetRenderer(panel);
            var imageListView = GetImageListView(panel);

            // Act
            var result = (Size)renderer.GetType().GetMethod("MeasureItem")!.Invoke(renderer, [View.Thumbnails])!;

            // Assert
            Assert.AreEqual(imageListView.ThumbnailSize.Width + 40, result.Width);
            Assert.AreEqual(imageListView.ThumbnailSize.Height + 18 + (2 * imageListView.Font.Height), result.Height);
        });
    }

    [TestMethod]
    public void Blink_Invoked_StartsTimerAndResetsBlinkState()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var renderer = GetRenderer(panel);

            // Act
            renderer.GetType().GetMethod("Blink", Type.EmptyTypes)!.Invoke(renderer, null);

            // Assert
            Assert.AreEqual("TICK", GetPrivateField(renderer, "m_BlinkState")!.ToString());
            Assert.AreEqual(5, (int)GetPrivateField(renderer, "m_BlinkCount")!);
            Assert.IsTrue(((System.Windows.Forms.Timer)GetPrivateField(renderer, "m_BlinkTimer")!).Enabled);
        });
    }

    [TestMethod]
    public void Blink_TimerTicksPastConfiguredCount_StopsTimerAndClearsBlinkState()
    {
        RunInSta(() =>
        {
            // Arrange
            using var panel = CreatePanel();
            var renderer = GetRenderer(panel);
            var blinkTickMethod = renderer.GetType().GetMethod("Blink", BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(object), typeof(EventArgs)], null)!;
            renderer.GetType().GetMethod("Blink", Type.EmptyTypes)!.Invoke(renderer, null);

            // Act
            for (var i = 0; i <= 5; i++)
            {
                blinkTickMethod.Invoke(renderer, [null, EventArgs.Empty]);
            }

            // Assert
            Assert.AreEqual("NONE", GetPrivateField(renderer, "m_BlinkState")!.ToString());
            Assert.IsFalse(((System.Windows.Forms.Timer)GetPrivateField(renderer, "m_BlinkTimer")!).Enabled);
        });
    }

    private static MainPanel CreatePanel()
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        return new MainPanel(new StubDbStrategy());
    }

    private static object GetRenderer(MainPanel panel)
    {
        return typeof(MainPanel).GetField("m_ListViewRenderer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static ImageListView GetImageListView(MainPanel panel)
    {
        return (ImageListView)typeof(MainPanel).GetField("m_ImageListView", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static object? GetPrivateField(object instance, string name)
    {
        return instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance);
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
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit) => System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit) => System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap> GetGenres() => System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => System.Collections.Immutable.ImmutableSortedDictionary<string, Bitmap>.Empty;
        public override void FilterControls(MainPanel panel) {}
    }
}