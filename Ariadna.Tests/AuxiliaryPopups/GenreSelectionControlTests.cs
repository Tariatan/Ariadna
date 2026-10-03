using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests.AuxiliaryPopups;

[TestClass]
public sealed class GenreSelectionControlTests
{
    [TestMethod]
    public void AddGenre_SynonymAlreadySelected_DoesNotAddDuplicate()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new GenreSelectionControl();
            testee.Configure(["Drama"], _ => "Drama", _ => Resources.No_Image);
            testee.AddGenre("Drama");

            // Act
            testee.AddGenre("drama synonym");

            // Assert
            CollectionAssert.AreEqual(new[] { "Drama" }, testee.GetGenres());
        });
    }

    [TestMethod]
    public void LoadGenres_ExistingSelectionExceedsLimit_PreservesAllSavedGenres()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new GenreSelectionControl();
            var genres = Enumerable.Range(1, Settings.Default.MaxGenresCount + 2).Select(index => "Genre" + index).ToArray();

            // Act
            testee.LoadGenres(genres);
            testee.AddGenre("Additional");

            // Assert
            CollectionAssert.AreEquivalent(genres, testee.GetGenres());
        });
    }

    [TestMethod]
    public void LoadGenres_ExistingSynonymsAndDuplicates_PreservesSavedNames()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var testee = new GenreSelectionControl();
            testee.Configure(["Drama"], _ => "Drama", _ => Resources.No_Image);
            var genres = new[] { "drama synonym", "drama synonym", "Drama" };

            // Act
            testee.LoadGenres(genres);

            // Assert
            CollectionAssert.AreEquivalent(genres, testee.GetGenres());
        });
    }

    [TestMethod]
    public void OnListKeyUp_DeleteAtLimit_AllowsReplacementGenre()
    {
        UiTest.Run(() =>
        {
            // Arrange
            using var form = new Form();
            using var testee = new GenreSelectionControl();
            form.Controls.Add(testee);
            form.Show();
            testee.LoadGenres(Enumerable.Range(1, Settings.Default.MaxGenresCount).Select(index => "Genre" + index));
            var list = UiTest.Field<ListView>(testee, "list");
            list.Items[0].Focused = true;

            // Act
            UiTest.Key(list, "OnKeyUp", Keys.Delete);
            testee.AddGenre("Replacement");

            // Assert
            CollectionAssert.Contains(testee.GetGenres(), "Replacement");
            Assert.HasCount(Settings.Default.MaxGenresCount, testee.GetGenres());
        });
    }
}
