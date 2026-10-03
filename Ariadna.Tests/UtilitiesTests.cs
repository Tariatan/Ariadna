using System.Drawing;
using Ariadna.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public class UtilitiesTests
{
    [TestMethod]
    public void GetMovieGenreBySynonym_KnownSynonym_ReturnsNormalizedGenre()
    {
        // Arrange

        // Act
        var result = Utilities.GetMovieGenreBySynonym("Мультфильм");

        // Assert
        Assert.AreEqual("Анимационный", result);
    }

    [TestMethod]
    public void GetMovieGenreBySynonym_UnknownValue_ReturnsSameValue()
    {
        // Arrange

        // Act
        var result = Utilities.GetMovieGenreBySynonym("Comedy");

        // Assert
        Assert.AreEqual("Comedy", result);
    }

    [TestMethod]
    public void GetMovieGenreImage_KnownGenre_ReturnsMappedImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetMovieGenreImage("Боевик");

        // Assert
        AssertBitmapsEqual(Resources.fantasy, result);
    }

    [TestMethod]
    public void GetMovieGenreImage_UnknownGenre_ReturnsNoImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetMovieGenreImage("Unknown");

        // Assert
        AssertBitmapsEqual(Resources.No_Image, result);
    }

    [TestMethod]
    public void GetGameGenreImage_UnknownGenre_ReturnsNoImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetGameGenreImage("Unknown");

        // Assert
        AssertBitmapsEqual(Resources.No_Image, result);
    }

    [TestMethod]
    public void GetGameGenreImage_KnownGenre_ReturnsMappedImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetGameGenreImage("Adventure");

        // Assert
        AssertBitmapsEqual(Resources.adventure, result);
    }

    [TestMethod]
    public void GetDocumentaryGenreImage_UnknownGenre_ReturnsNoImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetDocumentaryGenreImage("Unknown");

        // Assert
        AssertBitmapsEqual(Resources.No_Image, result);
    }

    [TestMethod]
    public void GetDocumentaryGenreImage_KnownGenre_ReturnsMappedImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetDocumentaryGenreImage("Science");

        // Assert
        AssertBitmapsEqual(Resources.science, result);
    }

    [TestMethod]
    public void GetLibraryLanguagesGenreImage_UnknownSubgenre_FallsBackToLibraryGenreImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetLibraryLanguagesGenreImage("Languages");

        // Assert
        AssertBitmapsEqual(Resources.languages, result);
    }

    [TestMethod]
    public void GetLibraryProgrammingGenreImage_KnownSubgenre_ReturnsMappedImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetLibraryProgrammingGenreImage("Rust");

        // Assert
        AssertBitmapsEqual(Resources.rust, result);
    }

    [TestMethod]
    public void GetLibraryProgrammingGenreImage_UnknownSubgenre_FallsBackToLibraryGenreImage()
    {
        // Arrange

        // Act
        var result = Utilities.GetLibraryProgrammingGenreImage("Programming");

        // Assert
        AssertBitmapsEqual(Resources.programming, result);
    }

    [TestMethod]
    public void IsValidPreview_NullBytes_ReturnsFalse()
    {
        // Arrange

        // Act
        var result = Utilities.IsValidPreview(null);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidPreview_LengthDoesNotMatchKnownPlaceholder_ReturnsTrue()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3 };

        // Act
        var result = Utilities.IsValidPreview(bytes);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidPreview_KnownPlaceholderPrefix_ReturnsFalse()
    {
        // Arrange
        var bytes = new byte[1119];
        byte[] prefix = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 54, 0, 0, 0, 81, 8, 6, 0, 0, 0, 153, 180, 85, 63, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1, 199, 111, 168, 100, 0, 0];
        Array.Copy(prefix, bytes, prefix.Length);

        // Act
        var result = Utilities.IsValidPreview(bytes);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidPreview_KnownPlaceholderLengthButDifferentPrefix_ReturnsTrue()
    {
        // Arrange
        var bytes = new byte[1119];
        bytes[0] = 1;

        // Act
        var result = Utilities.IsValidPreview(bytes);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidPreview_AlternativePlaceholderLengthAndKnownPrefix_ReturnsFalse()
    {
        // Arrange
        var bytes = new byte[1150];
        byte[] prefix = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 54, 0, 0, 0, 81, 8, 6, 0, 0, 0, 153, 180, 85, 63, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1, 199, 111, 168, 100, 0, 0];
        Array.Copy(prefix, bytes, prefix.Length);

        // Act
        var result = Utilities.IsValidPreview(bytes);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetVideoDuration_FileDoesNotExist_ReturnsZero()
    {
        // Arrange
        var missingFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mkv");

        // Act
        var result = Utilities.GetVideoDuration(missingFilePath);

        // Assert
        Assert.AreEqual(TimeSpan.Zero, result);
    }

    [TestMethod]
    public void GetVideoDuration_ExistingVideo_ReturnsDurationAndReleasesFile()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".avi");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "media-two-seconds.avi"), path);
        try
        {
            // Act
            var result = Utilities.GetVideoDuration(path);

            // Assert
            Assert.AreEqual(TimeSpan.FromSeconds(2), result);
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.IsTrue(exclusive.Length > 0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void GetVideoDuration_ExistingNonMediaFile_ReturnsZero()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllText(path, "Not a media file");
        try
        {
            // Act
            var result = Utilities.GetVideoDuration(path);

            // Assert
            Assert.AreEqual(TimeSpan.Zero, result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void DecorateDescription_MultipleParagraphs_TrimsAndPrefixesEachParagraph()
    {
        // Arrange
        var description = "  First line  \nSecond line\n  Third line";

        // Act
        var result = Utilities.DecorateDescription(description);

        // Assert
        Assert.AreEqual("\tFirst line\r\n\tSecond line\r\n\tThird line", result);
    }

    [TestMethod]
    public void DecorateDescription_EmptyParagraphs_PreservesParagraphStructure()
    {
        // Arrange
        var description = "First line\n\n Third line ";

        // Act
        var result = Utilities.DecorateDescription(description);

        // Assert
        Assert.AreEqual("\tFirst line\r\n\t\r\n\tThird line", result);
    }

    private static void AssertBitmapsEqual(Bitmap expected, Bitmap actual)
    {
        Assert.AreEqual(expected.Size, actual.Size);
        Assert.AreEqual(expected.GetPixel(0, 0).ToArgb(), actual.GetPixel(0, 0).ToArgb());
        Assert.AreEqual(expected.GetPixel(expected.Width / 2, expected.Height / 2).ToArgb(), actual.GetPixel(actual.Width / 2, actual.Height / 2).ToArgb());
        Assert.AreEqual(expected.GetPixel(expected.Width - 1, expected.Height - 1).ToArgb(), actual.GetPixel(actual.Width - 1, actual.Height - 1).ToArgb());
    }
}
