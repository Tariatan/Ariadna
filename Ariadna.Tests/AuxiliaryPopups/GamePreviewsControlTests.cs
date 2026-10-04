using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class GamePreviewsControlTests
{
    [TestMethod]
    public void LoadImages_FourSavedPreviews_SelectsFirstOnceAndAllowsLaterEdits()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var directory = Path.Combine(Path.GetTempPath(), "Ariadna-preview-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Gold };
                for (var index = 0; index < colors.Length; index++)
                {
                    using var image = new Bitmap(16, 16);
                    using var graphics = Graphics.FromImage(image);
                    graphics.Clear(colors[index]);
                    image.Save(Path.Combine(directory, $"1{Settings.Default.PreviewSuffix}{index + 1}"), ImageFormat.Png);
                }
                using var host = new Form();
                using var testee = new GamePreviewsControl { Dock = DockStyle.Fill };
                host.Controls.Add(testee);
                host.Show();
                Application.DoEvents();
                var full = UiTest.Field<PictureBox>(testee, "full");
                var replacements = new List<Image>();
                var previous = full.Image;
                full.Invalidated += (_, _) =>
                {
                    if (full.Image != previous)
                    {
                        previous = full.Image;
                        replacements.Add(full.Image!);
                    }
                };

                // Act
                testee.LoadImages(directory, 1);

                // Assert
                Assert.HasCount(1, replacements);
                Assert.AreEqual(Color.Red.ToArgb(), ((Bitmap)full.Image!).GetPixel(0, 0).ToArgb());
                Assert.HasCount(4, testee.GetImages());
                var third = (ImageEditorControl)testee.Controls.Find("preview3", true).Single();
                using var edited = new Bitmap(16, 16);
                edited.SetPixel(0, 0, Color.Purple);
                third.SetImage(edited);
                Assert.AreEqual(Color.Purple.ToArgb(), ((Bitmap)full.Image!).GetPixel(0, 0).ToArgb());
                var second = (ImageEditorControl)testee.Controls.Find("preview2", true).Single();
                typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(UiTest.Field<PictureBox>(second, "picture"), [EventArgs.Empty]);
                Assert.AreEqual(Color.Green.ToArgb(), ((Bitmap)full.Image!).GetPixel(0, 0).ToArgb());
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        });
    }
}
