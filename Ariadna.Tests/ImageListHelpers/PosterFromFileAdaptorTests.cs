using System.Drawing;
using System.Drawing.Imaging;
using Ariadna.ImageListHelpers;
using Manina.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.ImageListHelpers;

[TestClass]
public class PosterFromFileAdaptorTests
{
    [TestMethod]
    public void GetSourceImage_KeyProvided_ReturnsCombinedPath()
    {
        // Arrange
        var testee = new PosterFromFileAdaptor
        {
            RootPath = @"A:\Posters\",
        };

        // Act
        var result = testee.GetSourceImage("42");

        // Assert
        Assert.AreEqual(@"A:\Posters\42", result);
    }

    [TestMethod]
    public void GetUniqueIdentifier_KeyProvided_ReturnsKey()
    {
        // Arrange
        var testee = new PosterFromFileAdaptor();

        // Act
        var result = testee.GetUniqueIdentifier("42", new Size(100, 100), UseEmbeddedThumbnails.Auto, false);

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void GetThumbnail_FileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var testee = new PosterFromFileAdaptor
        {
            RootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + Path.DirectorySeparatorChar,
        };

        // Act
        var result = testee.GetThumbnail("42", new Size(32, 48), UseEmbeddedThumbnails.Auto, false);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetThumbnail_FileExists_ReturnsThumbnailImage()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var imagePath = Path.Combine(tempDir, "42");
        using (var bitmap = new Bitmap(40, 60))
        {
            bitmap.Save(imagePath, ImageFormat.Png);
        }

        var testee = new PosterFromFileAdaptor
        {
            RootPath = tempDir + Path.DirectorySeparatorChar,
        };

        // Act
        using var result = testee.GetThumbnail("42", new Size(32, 48), UseEmbeddedThumbnails.Auto, false);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(32, result.Width);
        Assert.AreEqual(48, result.Height);

        File.Delete(imagePath);
        Directory.Delete(tempDir);
    }

    [TestMethod]
    public void GetThumbnail_FileExists_DoesNotKeepSourceFileLocked()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var imagePath = Path.Combine(tempDir, "42");
        using (var bitmap = new Bitmap(40, 60))
        {
            bitmap.Save(imagePath, ImageFormat.Png);
        }

        var testee = new PosterFromFileAdaptor
        {
            RootPath = tempDir + Path.DirectorySeparatorChar,
        };

        // Act
        using var result = testee.GetThumbnail("42", new Size(32, 48), UseEmbeddedThumbnails.Auto, false);
        File.Delete(imagePath);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(File.Exists(imagePath));

        Directory.Delete(tempDir);
    }
}