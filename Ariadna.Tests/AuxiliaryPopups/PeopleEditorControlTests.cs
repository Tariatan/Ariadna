using System.Drawing;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class PeopleEditorControlTests
{
    [TestMethod]
    public void LoadPeople_DuplicateSavedNames_PreservesRelationshipsAndPhotos()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new PeopleEditorControl();
            using var image = new Bitmap(16, 16);
            using var encoder = new ImageEditorControl();
            encoder.SetImage(image);
            var photo = encoder.GetPngBytes();
            var people = new[] { new PersonPhoto("Same Name", null), new PersonPhoto("Same Name", photo) };

            // Act
            testee.LoadPeople(people);
            var saved = testee.GetPeople();

            // Assert
            Assert.HasCount(2, saved);
            Assert.IsNull(saved[0].Photo);
            CollectionAssert.AreEqual(photo, saved[1].Photo!);
        });
    }

    [TestMethod]
    public void AddPerson_BeforeControlShown_CopiesPortraitIntoNativeImageList()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new PeopleEditorControl();

            // Act
            testee.AddPerson(new PersonPhoto("Synthetic Person", null));
            var photos = UiTest.Field<ImageList>(testee, "photos");
            using var portrait = photos.Images[0];

            // Assert
            Assert.IsTrue(portrait.Width > 0);
            Assert.IsTrue(portrait.Height > 0);
        });
    }
}
