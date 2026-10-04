using System.ComponentModel;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class DetailFormPresentationTests
{
    [TestMethod]
    public void ShowDialog_FirstPaint_RevealsBeforeNextModalCallback()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var components = new Container();
            using var form = new Form();
            _ = new DetailFormPresentation(components, form);
            double? paintOpacity = null;
            double? readyOpacity = null;
            form.Paint += (_, _) => paintOpacity ??= form.Opacity;
            form.Shown += (_, _) => form.BeginInvoke((Action)(() =>
            {
                readyOpacity = form.Opacity;
                form.DialogResult = DialogResult.Cancel;
            }));

            // Act
            var result = form.ShowDialog();

            // Assert
            Assert.AreEqual(0d, paintOpacity);
            Assert.AreEqual(1d, readyOpacity);
            Assert.AreEqual(DialogResult.Cancel, result);
        });
    }

    [TestMethod]
    public void Show_CloseBeforeQueuedReveal_DoesNotPaintOrReopenClosedForm()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var components = new Container();
            using var form = new Form();
            _ = new DetailFormPresentation(components, form);
            var painted = false;
            form.Paint += (_, _) => painted = true;
            form.Shown += (_, _) => form.Close();

            // Act
            form.Show();
            Application.DoEvents();

            // Assert
            Assert.IsTrue(form.IsDisposed);
            Assert.IsFalse(form.Visible);
            Assert.IsFalse(painted);
        });
    }

    [TestMethod]
    public void Dispose_QueuedReveal_DoesNotChangeOpacityOrPaint()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var components = new Container();
            using var form = new Form();
            var testee = new DetailFormPresentation(components, form);
            var opacityAfterDisposal = form.Opacity;
            var painted = false;
            form.Paint += (_, _) => painted = true;
            form.Shown += (_, _) =>
            {
                testee.Dispose();
                opacityAfterDisposal = form.Opacity;
                form.Hide();
            };

            // Act
            form.Show();
            Application.DoEvents();

            // Assert
            Assert.AreEqual(opacityAfterDisposal, form.Opacity);
            Assert.IsFalse(form.Visible);
            Assert.IsFalse(painted);
        });
    }
}
