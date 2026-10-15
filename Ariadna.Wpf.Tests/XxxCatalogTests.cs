using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Wpf.Tests;

[TestClass]
public sealed class XxxCatalogTests
{
    [TestMethod]
    public async Task Save_XxxEntry_ReopensCastVrWishlistAndBothImages() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var editor = new EntryEditorModel(fixture.Actions, CatalogKind.Xxx, Path.Combine(fixture.Root, "sample.mp4"))
        {
            Title = "Sample title",
            Year = "2026",
            Vr = true,
            Wanted = true,
        };
        editor.AddPeople("First Actor; Second Actor; Third Actor; Fourth Actor", true);
        editor.AddPlaceholderPerson(true);
        var png = Images.Png(BitmapSource.Create(2, 2, 96, 96, PixelFormats.Gray8, null, new byte[4], 2));
        editor.ReplaceImage(string.Empty, png);
        editor.ReplaceImage(fixture.Configuration.PreviewSuffix(1), png);

        // Act
        var id = editor.Save();
        var reopened = new EntryEditorModel(fixture.Actions, CatalogKind.Xxx, editor.Path);
        var matching = fixture.Store.Query(CatalogKind.Xxx, new CatalogQuery { Actor = "Second", Vr = true, Wish = true });

        // Assert
        Assert.AreEqual(id, reopened.StoredId);
        Assert.AreEqual("Sample title", reopened.DisplayTitle);
        Assert.AreEqual("2026", reopened.Year);
        Assert.IsTrue(reopened.Vr && reopened.Wanted);
        CollectionAssert.AreEqual(new[] { "First Actor", "Second Actor", "Third Actor" }, reopened.Actors.Select(actor => actor.Name).ToArray());
        Assert.IsNotNull(reopened.Poster);
        Assert.IsNotNull(reopened.Previews[0]);
        Assert.HasCount(1, matching);
        Assert.HasCount(0, fixture.Store.Query(CatalogKind.Movie, new CatalogQuery()));
        Assert.HasCount(0, fixture.Store.Query(CatalogKind.Xxx, new CatalogQuery { NonVr = true }));
        Assert.IsTrue(fixture.Store.Delete(CatalogKind.Xxx, id));
        new CatalogDatabase(fixture.Configuration.DatabasePath).CheckIntegrity();
        return Task.CompletedTask;
    });

    [TestMethod]
    public async Task Layout_XxxDetails_ShowsCompactCastAboveSingleScreenshot() => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var window = new EntryDetailsWindow(fixture.Actions, CatalogKind.Xxx, Path.Combine(fixture.Root, "sample.mp4"));
        try
        {
            // Act
            window.Show();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var cast = (PeopleEditorView)window.FindName("XxxCastEditor");
            var year = (TextBox)window.FindName("XxxYearText");
            var vr = (CheckBox)window.FindName("XxxVr");
            var screenshot = (Button)window.FindName("XxxScreenshotButton");

            // Assert
            foreach (var name in new[] { "SecondaryTitleText", "GenreList", "DescriptionText", "PeopleEditor", "CastEditor", "AudioLanguagesSection", "SelectedPreview" })
            {
                Assert.IsFalse(((FrameworkElement)window.FindName(name)).IsVisible, name);
            }
            Assert.IsTrue(cast.IsVisible && year.IsVisible && vr.IsVisible && screenshot.IsVisible);
            Assert.IsTrue(cast.ActualWidth <= 300);
            Assert.AreEqual(year.TranslatePoint(new Point(), window).Y, vr.TranslatePoint(new Point(), window).Y, 15);
            Assert.IsTrue(screenshot.TranslatePoint(new Point(), window).Y >= cast.TranslatePoint(new Point(0, cast.ActualHeight), window).Y);
            var editor = (EntryEditorModel)window.DataContext;
            editor.AddPeople("One; Two; Three; Four", true);
            Assert.HasCount(3, editor.Actors);
            window.UpdateLayout();
            var people = (ListBox)cast.FindName("People");
            var firstCard = (ListBoxItem)people.ItemContainerGenerator.ContainerFromIndex(0);
            var thirdCard = (ListBoxItem)people.ItemContainerGenerator.ContainerFromIndex(2);
            Assert.AreEqual(firstCard.TranslatePoint(new Point(), people).Y, thirdCard.TranslatePoint(new Point(), people).Y, 1);
            Assert.IsTrue(thirdCard.TranslatePoint(new Point(thirdCard.ActualWidth, 0), people).X <= people.ActualWidth);
            Assert.IsFalse(((Button)cast.FindName("AddButton")).IsEnabled);
            Assert.IsFalse(((Button)cast.FindName("PasteButton")).IsEnabled);
            editor.Actors.RemoveAt(0);
            Assert.IsTrue(((Button)cast.FindName("AddButton")).IsEnabled);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    public async Task MainWindow_XxxArgument_SelectsFifthTabAfterDocumentaries() => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, "xxx");
        try
        {
            // Act
            var tabs = (TabControl)window.FindName("CatalogTabs");

            // Assert
            Assert.HasCount(5, tabs.Items);
            Assert.AreEqual("Documentaries", ((TabItem)tabs.Items[3]).Header);
            Assert.AreEqual("XXX", ((TabItem)tabs.Items[4]).Header);
            Assert.AreEqual(4, tabs.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
        return Task.CompletedTask;
    });
}
