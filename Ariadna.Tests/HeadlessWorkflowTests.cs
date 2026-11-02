using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Ariadna.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public sealed class HeadlessWorkflowTests
{
    [TestMethod]
    [DataRow(CatalogKind.Movie, "movies")]
    [DataRow(CatalogKind.Game, "games")]
    [DataRow(CatalogKind.Library, "library")]
    [DataRow(CatalogKind.Documentary, "documentaries")]
    [DataRow(CatalogKind.Xxx, "xxx")]
    public async Task SelectTab_CatalogHasFiltersAndSelection_RetainsState(CatalogKind kind, string argument) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        fixture.Add(kind, "Retained entry", true);
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, argument);
        try
        {
            await window.PrepareAsync(CancellationToken.None);
            HeadlessLayout.Arrange(window);
            var tabs = (TabControl)window.FindName("CatalogTabs");
            var view = (CatalogView)((TabItem)tabs.SelectedItem).Content;
            Assert.AreEqual(kind, view.Model.Kind);
            view.Model.Title = "Retained";
            view.Model.Wish = true;
            await view.Model.RefreshAsync(CancellationToken.None);
            var selected = view.Model.Selected;

            // Act
            tabs.SelectedIndex = (tabs.SelectedIndex + 1) % tabs.Items.Count;
            HeadlessLayout.Arrange(window);
            HeadlessLayout.SelectCatalog(window, argument);

            // Assert
            Assert.AreSame(view, ((TabItem)tabs.SelectedItem).Content);
            Assert.AreEqual("Retained", view.Model.Title);
            Assert.IsTrue(view.Model.Wish);
            Assert.AreSame(selected, view.Model.Selected);
            Assert.HasCount(1, view.Model.Entries);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    public async Task EditGenres_AddRemoveAndSave_PreservesLimitAndOtherMetadata(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Genre target");
        var model = new EntryEditorModel(fixture.Actions, kind, entry.Path);

        // Act
        model.Genres.Clear();
        string[] genres = ["One", "Two", "Three", "Four", "Five", "Six"];
        foreach (var genre in genres)
        {
            model.AddGenre(genre);
        }
        Assert.HasCount(5, model.Genres);
        Assert.IsFalse(model.Genres.Contains("Six"));
        model.Genres.Remove("Two");
        model.AddGenre("Replacement");
        model.Save();
        var saved = fixture.Store.GetDetails(kind, entry.Id)!;

        // Assert
        CollectionAssert.AreEqual(model.Genres.ToArray(), saved.Genres.ToArray());
        Assert.AreEqual(entry.Title, saved.Entry.Title);
        Assert.AreEqual(entry.Year, saved.Entry.Year);
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    [DataRow(CatalogKind.Xxx)]
    public async Task Close_UnsavedEditor_DiscardsMetadataChanges(CatalogKind kind) => await WpfThread.RunAsync(() =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var entry = fixture.Add(kind, "Unchanged entry");
        var window = new EntryDetailsWindow(fixture.Actions, kind, entry.Path);
        HeadlessLayout.Arrange(window);
        var model = (EntryEditorModel)window.DataContext;
        model.Title = "Discard this title";
        model.Year = "2026";

        // Act
        window.Close();
        var saved = fixture.Store.GetDetails(kind, entry.Id)!;

        // Assert
        Assert.AreEqual(entry.Title, saved.Entry.Title);
        Assert.AreEqual(entry.Year, saved.Entry.Year);
        Assert.AreEqual(entry.Wanted, saved.Entry.Wanted);
        return Task.CompletedTask;
    });

    [TestMethod]
    [DataRow("TitleSearch", true, false)]
    [DataRow("PersonSearch", true, false)]
    [DataRow("ActorSearch", true, false)]
    [DataRow("TitleSearch", true, true)]
    [DataRow("PersonSearch", true, true)]
    [DataRow("ActorSearch", true, true)]
    [DataRow("PosterRows", false, false)]
    [DataRow("PosterRows", false, true)]
    public async Task AddShortcut_EditingOrDisabled_DoesNotStartDiscovery(string name, bool enabled, bool numpad) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var window = new MainWindow(fixture.Store, fixture.Configuration, NullLogger.Instance, null);
        try
        {
            HeadlessLayout.Arrange(window);
            var view = (CatalogView)((TabItem)((TabControl)window.FindName("CatalogTabs")).SelectedItem).Content;
            ((Button)view.FindName("AddEntryButton")).IsEnabled = enabled;
            var control = (UIElement)view.FindName(name);
            InputEventArgs input = numpad
                ? new KeyEventArgs(Keyboard.PrimaryDevice, HeadlessLayout.Source, 0, Key.Add) { RoutedEvent = Keyboard.PreviewKeyDownEvent }
                : new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, control, "+")) { RoutedEvent = TextCompositionManager.PreviewTextInputEvent };

            // Act
            control.RaiseEvent(input);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Assert
            Assert.IsFalse(input.Handled);
            Assert.IsFalse(view.Model.Busy);
            Assert.IsFalse(((Popup)view.FindName("GenrePopup")).IsOpen);
            Assert.IsFalse(((Popup)view.FindName("PeoplePopup")).IsOpen);
        }
        finally
        {
            window.Close();
        }
    });

    [TestMethod]
    [DataRow(CatalogKind.Movie)]
    [DataRow(CatalogKind.Game)]
    [DataRow(CatalogKind.Library)]
    [DataRow(CatalogKind.Documentary)]
    public async Task Refresh_GenreChanged_UsesLatestFilter(CatalogKind kind) => await WpfThread.RunAsync(async () =>
    {
        // Arrange
        using var fixture = new CatalogFixture();
        var first = fixture.Add(kind, "First entry");
        var second = fixture.Add(kind, "Second entry");
        fixture.Store.Save(kind, new CatalogDetails(first, ["First genre"], [], []));
        fixture.Store.Save(kind, new CatalogDetails(second, ["Second genre"], [], []));
        using var model = new CatalogViewModel(kind, fixture.Store, fixture.Configuration, new ThumbnailCache(NullLogger.Instance), NullLogger.Instance);
        model.Genre = "First genre";
        await model.RefreshAsync(CancellationToken.None);
        Assert.AreEqual(first.Id, model.Entries.Single().Entry.Id);

        // Act
        model.Genre = "Second genre";
        await model.RefreshAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(second.Id, model.Entries.Single().Entry.Id);
        Assert.AreEqual("Second genre", model.Genre);
    });
}
