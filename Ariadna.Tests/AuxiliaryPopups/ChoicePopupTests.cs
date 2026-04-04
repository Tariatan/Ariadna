using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public class ChoicePopupTests
{
    [TestMethod]
    public void Constructor_WithResults_PopulatesListAndInitialState()
    {
        RunInSta(() =>
        {
            // Arrange
            var results = new List<MovieChoiceDto>
            {
                new() { Title = "Terminator", TitleOrig = "The Terminator", Year = 1984 },
                new() { Title = "Aliens", TitleOrig = "Aliens", Year = 1986 },
            };

            using var testee = new ChoicePopup(@"A:\Media\Movies\Terminator.mkv", results);

            // Act
            testee.Show();

            // Assert
            Assert.AreEqual(-1, testee.Index);
            Assert.AreEqual(@"A:\Media\Movies\Terminator.mkv", GetPathLabel(testee).Text);
            Assert.AreEqual(2, GetResultList(testee).Items.Count);
            CollectionAssert.AreEqual(new[] { "Terminator", "The Terminator", "1984" }, GetResultList(testee).Items[0].SubItems.Cast<ListViewItem.ListViewSubItem>().Select(subItem => subItem.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "Aliens", "Aliens", "1986" }, GetResultList(testee).Items[1].SubItems.Cast<ListViewItem.ListViewSubItem>().Select(subItem => subItem.Text).ToArray());
        });
    }

    [TestMethod]
    public void OnSelectedIndexChanged_WithFocusedItem_UpdatesIndex()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new ChoicePopup(@"A:\Media\Movies\Terminator.mkv",
            [
                new MovieChoiceDto { Title = "Terminator", TitleOrig = "The Terminator", Year = 1984 },
                new MovieChoiceDto { Title = "Aliens", TitleOrig = "Aliens", Year = 1986 },
            ]);
            testee.Show();
            var resultList = GetResultList(testee);
            resultList.Items[1].Focused = true;

            // Act
            InvokeChoicePopupMethod(testee, "OnSelectedIndexChanged", testee, EventArgs.Empty);

            // Assert
            Assert.AreEqual(1, testee.Index);
        });
    }

    [TestMethod]
    public void OnDoubleClick_WhenInvoked_ClosesForm()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new ChoicePopup(@"A:\Media\Movies\Terminator.mkv",
            [
                new MovieChoiceDto { Title = "Terminator", TitleOrig = "The Terminator", Year = 1984 },
            ]);
            testee.Show();

            // Act
            InvokeChoicePopupMethod(testee, "OnDoubleClick", testee, EventArgs.Empty);

            // Assert
            Assert.IsTrue(testee.IsDisposed);
        });
    }

    [TestMethod]
    public void OnKeyDown_EscapePressed_ClosesForm()
    {
        RunInSta(() =>
        {
            // Arrange
            using var testee = new ChoicePopup(@"A:\Media\Movies\Terminator.mkv",
            [
                new MovieChoiceDto { Title = "Terminator", TitleOrig = "The Terminator", Year = 1984 },
            ]);
            testee.Show();

            // Act
            InvokeChoicePopupMethod(testee, "OnKeyDown", testee, new KeyEventArgs(Keys.Escape));

            // Assert
            Assert.IsTrue(testee.IsDisposed);
        });
    }

    private static ListView GetResultList(ChoicePopup popup)
    {
        return (ListView)typeof(ChoicePopup).GetField("m_ResultList", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(popup)!;
    }

    private static ToolStripStatusLabel GetPathLabel(ChoicePopup popup)
    {
        return (ToolStripStatusLabel)typeof(ChoicePopup).GetField("m_ToolStripPath", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(popup)!;
    }

    private static void InvokeChoicePopupMethod(ChoicePopup popup, string methodName, params object[] arguments)
    {
        var parameterTypes = arguments.Select(argument => argument.GetType()).ToArray();
        typeof(ChoicePopup).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, parameterTypes, null)!.Invoke(popup, arguments);
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
}