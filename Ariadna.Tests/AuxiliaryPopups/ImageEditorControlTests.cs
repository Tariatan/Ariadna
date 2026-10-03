using System.Drawing;
using System.Drawing.Imaging;
using Ariadna.AuxiliaryPopups;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class ImageEditorControlTests
{
    [TestMethod]
    public void LoadImage_FileThenReplacement_ReleasesFileAndOwnsImageCopy()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            using var image = new Bitmap(16, 16);
            image.SetPixel(0, 0, Color.Red);
            image.Save(path, ImageFormat.Png);
            using var testee = new ImageEditorControl();
            try
            {
                // Act
                testee.LoadImage(path);
                using (var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Assert.IsTrue(exclusive.Length > 0);
                }
                using (var replacement = new Bitmap(16, 16))
                {
                    replacement.SetPixel(0, 0, Color.Blue);
                    testee.SetImage(replacement);
                }
                using var saved = new MemoryStream(testee.GetPngBytes());
                using var bitmap = new Bitmap(saved);

                // Assert
                Assert.AreEqual(Color.Blue.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
                Assert.AreEqual(Color.Red.ToArgb(), image.GetPixel(0, 0).ToArgb());
            }
            finally
            {
                File.Delete(path);
            }
        });
    }
}
