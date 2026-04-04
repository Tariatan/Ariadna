using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public class FloatingPanelTests
{
    [TestMethod]
    public void UpdateListView_WithValues_ResetsStateAndPopulatesControls()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new FloatingPanel();
            using var poster = new Bitmap(10, 10);
            var values = ImmutableSortedDictionary<string, Bitmap>.Empty
                .Add("Aliens", poster)
                .Add("Terminator", null!);

            // Act
            testee.UpdateListView(values, FloatingPanel.EPanelContentType.CAST, checkBox: true, multiSelect: true, imageW: 80, imageH: 120);

            // Assert
            Assert.AreEqual(FloatingPanel.EPanelContentType.CAST, testee.PanelContentType);
            Assert.AreEqual(Utilities.EFormCloseReason.NONE, testee.FormCloseReason);
            Assert.AreEqual(0, testee.EntryNames.Count);
            Assert.AreEqual(2, GetPanelListView(testee).Items.Count);
            Assert.IsTrue(GetPanelListView(testee).CheckBoxes);
            Assert.IsTrue(GetPanelListView(testee).MultiSelect);
            Assert.AreEqual(new Size(80, 120), GetPanelImageView(testee).ImageSize);
            CollectionAssert.AreEqual(new[] { "Aliens", "Terminator" }, GetPanelListView(testee).Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
            Assert.IsNotNull(GetPanelImageView(testee).Images["Aliens"]);
            Assert.IsNotNull(GetPanelImageView(testee).Images["Terminator"]);
        });
    }

    [TestMethod]
    public void OnListItemChecked_ListNotFocused_DoesNotChangeSelectionOrRaiseEvent()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new FloatingPanel();
            testee.UpdateListView(ImmutableSortedDictionary<string, Bitmap>.Empty.Add("Aliens", new Bitmap(10, 10)), FloatingPanel.EPanelContentType.CAST, checkBox: true);
            var itemSelectedCallCount = 0;
            testee.ItemSelected += (_, _) => itemSelectedCallCount++;
            var item = GetPanelListView(testee).Items[0];
            item.Checked = true;

            // Act
            InvokeFloatingPanelMethod(testee, "OnListItemChecked", testee, new ItemCheckedEventArgs(item));

            // Assert
            Assert.AreEqual(0, testee.EntryNames.Count);
            Assert.AreEqual(0, itemSelectedCallCount);
        });
    }

    [TestMethod]
    public void OnListItemChecked_CheckedWhileFocused_AddsEntryAndRaisesEvent()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestFloatingPanel();
            testee.UpdateListView(ImmutableSortedDictionary<string, Bitmap>.Empty.Add("Aliens", new Bitmap(10, 10)), FloatingPanel.EPanelContentType.CAST, checkBox: true);
            var itemSelectedCallCount = 0;
            testee.ItemSelected += (_, _) => itemSelectedCallCount++;
            var item = GetPanelListView(testee).Items[0];
            testee.ForcePanelListFocus = true;
            item.Checked = true;

            // Act
            InvokeFloatingPanelMethod(testee, "OnListItemChecked", testee, new ItemCheckedEventArgs(item));

            // Assert
            CollectionAssert.AreEqual(new[] { "Aliens" }, testee.EntryNames);
            Assert.AreEqual(1, itemSelectedCallCount);
        });
    }

    [TestMethod]
    public void OnListItemChecked_UncheckedWhileFocused_RemovesEntryAndRaisesEvent()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new TestFloatingPanel();
            testee.UpdateListView(ImmutableSortedDictionary<string, Bitmap>.Empty.Add("Aliens", new Bitmap(10, 10)), FloatingPanel.EPanelContentType.CAST, checkBox: true);
            var itemSelectedCallCount = 0;
            testee.ItemSelected += (_, _) => itemSelectedCallCount++;
            var item = GetPanelListView(testee).Items[0];
            testee.ForcePanelListFocus = true;
            testee.EntryNames.Add("Aliens");
            item.Checked = false;

            // Act
            InvokeFloatingPanelMethod(testee, "OnListItemChecked", testee, new ItemCheckedEventArgs(item));

            // Assert
            Assert.AreEqual(0, testee.EntryNames.Count);
            Assert.AreEqual(1, itemSelectedCallCount);
        });
    }

    [TestMethod]
    public void OnListEntryDoubleClicked_WithFocusedItem_AddsEntrySetsSuccessAndHidesForm()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new FloatingPanel();
            testee.UpdateListView(ImmutableSortedDictionary<string, Bitmap>.Empty.Add("Aliens", new Bitmap(10, 10)), FloatingPanel.EPanelContentType.CAST);
            testee.Show();
            var item = GetPanelListView(testee).Items[0];
            item.Focused = true;

            // Act
            InvokeFloatingPanelMethod(testee, "OnListEntryDoubleClicked", testee, new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));

            // Assert
            CollectionAssert.AreEqual(new[] { "Aliens" }, testee.EntryNames);
            Assert.AreEqual(Utilities.EFormCloseReason.SUCCESS, testee.FormCloseReason);
            Assert.IsFalse(testee.Visible);
        });
    }

    [TestMethod]
    public void OnKeyDown_EscapePressed_HidesForm()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new FloatingPanel();
            testee.Show();

            // Act
            InvokeFloatingPanelMethod(testee, "OnKeyDown", testee, new KeyEventArgs(Keys.Escape));

            // Assert
            Assert.IsFalse(testee.Visible);
        });
    }

    private static ListView GetPanelListView(FloatingPanel panel)
    {
        return (ListView)typeof(FloatingPanel).GetField("m_PanelListView", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static ImageList GetPanelImageView(FloatingPanel panel)
    {
        return (ImageList)typeof(FloatingPanel).GetField("m_PanelImageView", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;
    }

    private static void InvokeFloatingPanelMethod(FloatingPanel panel, string methodName, params object[] arguments)
    {
        var parameterTypes = arguments.Select(argument => argument.GetType()).ToArray();
        typeof(FloatingPanel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, parameterTypes, null)!.Invoke(panel, arguments);
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

    private sealed class TestFloatingPanel : FloatingPanel
    {
        public bool ForcePanelListFocus { get; set; }

        protected override bool IsPanelListFocused()
        {
            return ForcePanelListFocus;
        }
    }
}