using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Ariadna.Data;
using Ariadna.DatabaseStrategies;
using Ariadna.ImageListHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.ImageListViewControls;

[TestClass]
public class AriadnaVScrollBarTests
{
    [TestMethod]
    public void Constructor_CreatedThroughImageListView_UsesAriadnaVerticalScrollBar()
    {
        RunInSta(() =>
        {
            // Arrange
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            using var panel = new MainPanel(new StubDbStrategy());
            var imageListView = (Manina.Windows.Forms.ImageListView)typeof(MainPanel)
                .GetField("m_ImageListView", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(panel)!;

            // Act
            var verticalScrollBar = imageListView.GetType()
                .GetField("vScrollBar", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(imageListView);

            // Assert
            Assert.IsNotNull(verticalScrollBar);
            Assert.AreEqual("AriadnaVScrollBar", verticalScrollBar.GetType().Name);
        });
    }

    [TestMethod]
    public void Value_SetAboveScrollableRange_ClampsToUpperBound()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = CreateScrollBar();
            SetProperty(testee, "Minimum", 0);
            SetProperty(testee, "Maximum", 99);
            SetProperty(testee, "LargeChange", 20);

            // Act
            SetProperty(testee, "Value", 200);

            // Assert
            Assert.AreEqual(80, (int)GetProperty(testee, "Value")!);
        });
    }

    [TestMethod]
    public void OnMouseDown_ClickedBelowThumb_RaisesLargeIncrementScroll()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = CreateScrollBar();
            testee.Height = 200;
            SetProperty(testee, "Minimum", 0);
            SetProperty(testee, "Maximum", 199);
            SetProperty(testee, "LargeChange", 40);
            var scrollRaised = false;
            var newValue = 0;
            var scrollEvent = testee.GetType().GetEvent("Scroll")!;
            ScrollEventHandler scrollEventHandler = (_, e) =>
            {
                scrollRaised = true;
                newValue = e.NewValue;
            };
            scrollEvent.AddEventHandler(testee, scrollEventHandler);

            // Act
            InvokeProtectedMethod(testee, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, 7, 180, 0));

            // Assert
            Assert.IsTrue(scrollRaised);
            Assert.AreEqual(40, newValue);
            Assert.AreEqual(40, (int)GetProperty(testee, "Value")!);
        });
    }

    private static Control CreateScrollBar()
    {
        var type = typeof(MainPanel).Assembly.GetType("Manina.Windows.Forms.AriadnaVScrollBar", true)!;
        return (Control)Activator.CreateInstance(type)!;
    }

    private static object? GetProperty(object instance, string propertyName)
    {
        return instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!.GetValue(instance);
    }

    private static void SetProperty(object instance, string propertyName, object? value)
    {
        instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!.SetValue(instance, value);
    }

    private static void InvokeProtectedMethod(object instance, string methodName, params object[] args)
    {
        instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, args);
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

        public override Manina.Windows.Forms.ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_Adaptor;
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
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap> GetDirectors(string name, int limit) => System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap>.Empty;
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap> GetActors(string name, int limit) => System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap>.Empty;
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap> GetGenres() => System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap>.Empty;
        public override System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap> GetSubgenres(string name) => System.Collections.Immutable.ImmutableSortedDictionary<string, System.Drawing.Bitmap>.Empty;
        public override void FilterControls(MainPanel panel) {}
    }
}
