using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;
using TMDbLib.Objects.Search;

namespace Ariadna.DatabaseStrategies;

public class MoviesDbStrategy : MediaDbStrategyBase
{
    protected override CatalogKind Kind => CatalogKind.Movie;

    public MoviesDbStrategy(ILogger logger) : base(logger, CatalogServices.GetPosterRoot(CatalogKind.Movie))
    {
    }

    public override List<EntryDto> GetEntries() => QueryEntries(new QueryParams { Subgenre = Utilities.EmptyDots });
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        return Store.Query(CatalogKind.Movie, CatalogServices.CreateQuery(values)).Select(entry => new EntryDto { Id = entry.Id, Title = entry.Title, Path = entry.Path }).ToList();
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var entry = Store.GetEntry(CatalogKind.Movie, id);
        return entry == null ? new EntryInfo() : new EntryInfo { Path = entry.Path, Title = entry.Title, TitleOrig = entry.OriginalTitle };
    }
    public override void RemoveEntry(int id)
    {
        var posterPath = CatalogServices.GetPosterRoot(CatalogKind.Movie) + id;
        EntryRemovalHelper.RemoveSingleFile(() => RemoveEntryFromDatabase(id), posterPath, FileExists, DeleteFile, ShowMessage);
    }
    public override bool FindNextEntryAutomatically()
    {
        if (TryOpenFirstNotInserted(GetFiles(Settings.Default.DefaultMoviesPath)))
        {
            return true;
        }
        if (TryOpenFirstNotInserted(GetFiles(Settings.Default.DefaultMoviesPathTMP2)))
        {
            return true;
        }

        if (TryOpenFirstNotInserted(GetDirectories(Settings.Default.DefaultSeriesPath)))
        {
            return true;
        }

        return false;
    }
    public override void FindNextEntryManually()
    {
        const string folderFlag = "File or folder";
        // ReSharper disable once UsingStatementResourceInitialization
        using var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Settings.Default.DefaultMoviesPath,
            Filter = Settings.Default.VideoFilesFilter,
            FilterIndex = 1,
            RestoreDirectory = true,

            // Allow folders
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = folderFlag,
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var path = openFileDialog.FileName;
        if (path.Contains(folderFlag))
        {
            path = Path.GetDirectoryName(openFileDialog.FileName);
        }

        FetchMovieFromImdb(path);
    }
    public override void UpdateSubgenre(MainPanel panel){}
    public override string[] QuickListFilter() => ["}", "«"];

    public override ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit)
    {
        var values = new SortedDictionary<string, Bitmap>();
        foreach (var person in Store.SuggestPeople(false, false, name, limit))
        {
            values[person.Name] = person.Photo.ToBitmap();
        }
        return values.ToImmutableSortedDictionary();
    }
    public override ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit)
    {
        var values = new SortedDictionary<string, Bitmap>();
        foreach (var person in Store.SuggestPeople(true, false, name, limit))
        {
            values[person.Name] = person.Photo.ToBitmap();
        }
        return values.ToImmutableSortedDictionary();
    }

    public override ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name) => null;
    public override ImmutableSortedDictionary<string, Bitmap> GetGenres()
    {
        var values = new SortedDictionary<string, Bitmap>();
        foreach (var name in Store.GetGenres(CatalogKind.Movie))
        {
            values[name] = Utilities.GetMovieGenreImage(name);
        }
        return values.ToImmutableSortedDictionary();
    }
    public override void FilterControls(MainPanel panel) {}
    protected virtual void RemoveEntryFromDatabase(int id) { Store.Delete(CatalogKind.Movie, id); }
    protected override void ShowDataDialog(string path)
    {
        var detailsForm = new MovieDetailsForm(path, Logger);
        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();
    }
    protected override void OpenDiscoveredPath(string path) => FetchMovieFromImdb(path);
    private bool m_IsFetching;
    private async void FetchMovieFromImdb(string path)
    {
        if(m_IsFetching)
        {
            return;
        }

        m_IsFetching = true;
        var client = new TMDbClient(Settings.Default.TmdbApiKey);
        var detailsForm = new MovieDetailsForm(path, Logger)
        {
            TmdbMovieIndex = -1,
            TmdbTvShowIndex = -1
        };

        if (Directory.Exists(path))
        {
            var dir = new DirectoryInfo(path);
            var tvShowsResults = await client.SearchTvShowAsync(dir.Name, "ru-RU");
            var tvShows = tvShowsResults.Results;
            if (tvShows.Count > 0)
            {
                var choice = (tvShows.Count > 1) ? GetBestTvShowChoice(tvShows, path) : 0;
                if (choice >= 0)
                {
                    detailsForm.TmdbTvShowIndex = tvShows[choice].Id;
                }
            }
        }
        else
        {
            var query = Path.GetFileNameWithoutExtension(path);
            var moviesResults = await client.SearchMovieAsync(query, "ru-RU");
            var movies = moviesResults.Results;

            if (movies.Count > 0)
            {
                var choice = (movies.Count > 1) ? GetBestMovieChoice(movies, path) : 0;
                if (choice >= 0)
                {
                    detailsForm.TmdbMovieIndex = movies[choice].Id;
                }
            }
        }

        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();

        m_IsFetching = false;
    }
    private int GetBestMovieChoice(List<SearchMovie> movies, string path)
    {
        var titles = new List<MovieChoiceDto>();
        foreach (var result in movies.Take(20))
        {
            var y = result.ReleaseDate?.Year ?? 0;
            titles.Add(new MovieChoiceDto { Title = result.Title, TitleOrig = result.OriginalTitle, Year = y });
        }

        var choice = new ChoicePopup(path, titles);
        choice.ShowDialog(Form.ActiveForm);
        return choice.Index;
    }
    private int GetBestTvShowChoice(List<SearchTv> movies, string path)
    {
        var titles = new List<MovieChoiceDto>();
        foreach (var result in movies.Take(20))
        {
            var y = result.FirstAirDate?.Year ?? 0;
            titles.Add(new MovieChoiceDto { Title = result.Name, TitleOrig = result.OriginalName, Year = y });
        }
        var choice = new ChoicePopup(path, titles);

        choice.ShowDialog(Form.ActiveForm);
        return choice.Index;
    }
    private void OnDetailsFormClosed(object sender, FormClosedEventArgs e)
    {
        var detailsForm = sender as MovieDetailsForm;
        if (detailsForm!.FormCloseReason != Utilities.EFormCloseReason.SUCCESS)
        {
            return;
        }

        var eventArgs = new EntryInsertedEventArgs(detailsForm.StoredDbEntryId);
        OnEntryInserted(eventArgs);
    }
    protected override string FindStoredEntryPathById(int id) => Store.GetEntry(CatalogKind.Movie, id)?.Path ?? string.Empty;
    protected override bool SupportsFileExecution => true;

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedMember.Local
}
