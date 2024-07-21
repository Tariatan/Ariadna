using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.AuxiliaryPopups;
using Ariadna.Data;
using Ariadna.Extension;
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;
using TMDbLib.Objects.Search;

namespace Ariadna.DBStrategies;

public class MoviesDbStrategy : AbstractDbStrategy
{
    private readonly ILogger m_Logger;
    private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new();

    public MoviesDbStrategy(ILogger logger)
    {
        m_Logger = logger;
        m_PosterImageAdaptor.RootPath = Settings.Default.MoviePostersRootPath;
    }

    public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;
        
    public override List<EntryDto> GetEntries()
    {
        //UpdateMovieData();

        using var ctx = new AriadnaEntities();
        return ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override List<EntryDto> QueryEntries(QueryParams values)
    {
        using var ctx = new AriadnaEntities();
        IQueryable<Movie> query = ctx.Movies.AsNoTracking();

        // -- Search Name --
        if (!string.IsNullOrEmpty(values.Name))
        {
            var toSearch = values.Name.ToUpper();
            query = query.Where(r => r.title.ToUpper().Contains(toSearch) ||
                                     r.title_original.ToUpper().Contains(toSearch) ||
                                     r.file_path.ToUpper().Contains(toSearch));
        }
        // -- DIRECTOR NAME --
        if (!string.IsNullOrEmpty(values.Director))
        {
            var entry = ctx.Directors.AsNoTracking().FirstOrDefault(r => r.name == values.Director);
            if (entry != null)
            {
                query = query.Where(r => r.MovieDirectors.Any(l => (l.directorId == entry.Id)));
            }
        }
        // -- ACTOR NAME --
        if (!string.IsNullOrEmpty(values.Actor))
        {
            var entry = ctx.Actors.AsNoTracking().FirstOrDefault(r => r.name == values.Actor);
            if (entry != null)
            {
                query = query.Where(r => r.MovieCasts.Any(l => (l.actorId == entry.Id)));
            }
        }
        // -- GENRE --
        if (!string.IsNullOrEmpty(values.Genre))
        {
            var entry = ctx.Genres.AsNoTracking().FirstOrDefault(r => r.name == values.Genre);
            if (entry != null)
            {
                query = query.Where(r => r.MovieGenres.Any(l => (l.genreId == entry.Id)));
            }
        }
        // -- WISH LIST --
        if (values.IsWish)
        {
            query = query.Where(r => (r.want_to_see == true));
        }
        // -- RECENTLY Added --
        if (values.IsRecent)
        {
            var recentDateStart = DateTime.Now.AddMonths(-Settings.Default.RecentInMonth);
            query = query.Where(r => ((r.creation_time > recentDateStart)));
        }
        // -- NEW --
        if (values.IsNew)
        {
            query = query.Where(r => ((r.year == (DateTime.Now.Year)) || r.year == (DateTime.Now.Year - 1)));
        }
        // -- SERIES --
        if (values.IsSeries)
        {
            query = query.Where(r => (r.file_path.StartsWith(Settings.Default.DefaultSeriesPath.Substring(0, 1))));
        }
        // -- MOVIES --
        if (values.IsMovies)
        {
            query = query.Where(r => (r.file_path.StartsWith(Settings.Default.DefaultMoviesPath.Substring(0, 1)) ||
                                      r.file_path.StartsWith(Settings.Default.DefaultMoviesPathTMP.Substring(0, 1))));
        }
                
        return query.OrderBy(r => r.title).Select(x => new EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
    }
    public override EntryInfo GetEntryInfo(int id)
    {
        var details = new EntryInfo();
        using var ctx = new AriadnaEntities();

        var entry = ctx.Movies.FirstOrDefault(r => r.Id == id);
        if (entry == null)
        {
            return details;
        }

        details.Path = entry.file_path;
        details.Title = entry.title;
        details.TitleOrig = entry.title_original;

        return details;
    }
    public override void RemoveEntry(int id)
    {
        using var ctx = new AriadnaEntities();
        var entry = ctx.Movies.FirstOrDefault(r => r.Id == id);
        if (entry != null)
        {
            ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == id)));
            ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == id)));
            ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == id)));

            ctx.Movies.Remove(entry);

            ctx.SaveChanges();
        }

        var posterPath = Settings.Default.MoviePostersRootPath + id;
        if (!File.Exists(posterPath))
        {
            return;
        }

        try
        {
            File.Delete(posterPath);
        }
        catch (IOException ex)
        {
            MessageBox.Show(ex.Source, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    public override bool FindNextEntryAutomatically()
    {
        if (FindFirstNotInserted(Directory.GetFiles(Settings.Default.DefaultMoviesPath)))
        {
            return true;
        }
        if (FindFirstNotInserted(Directory.GetFiles(Settings.Default.DefaultMoviesPathTMP)))
        {
            return true;
        }

        if (FindFirstNotInserted(Directory.GetDirectories(Settings.Default.DefaultSeriesPath)))
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
    public override void ShowEntryDetails(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var detailsForm = new MovieDetailsForm(path, m_Logger);
        detailsForm.FormClosed += OnDetailsFormClosed;
        detailsForm.ShowDialog();
    }
    public override void ExecuteEntry(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // Check if it is a file first
        if (File.Exists(path))
        {
            if (!File.Exists(Settings.Default.MediaPlayerPath))
            {
                return;
            }

            // Enclose the path in quotes as required by MPC
            Process.Start(Settings.Default.MediaPlayerPath, "\"" + path + "\"");
        }
        // Checked if it is a directory
        else if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "A:/PROGRAMS/UTILITIES/Total_Commander/Total Commander/Totalcmd64.exe",
                WorkingDirectory = Path.GetDirectoryName("A:/PROGRAMS/UTILITIES/Total_Commander/Total Commander")!,
                Arguments = $"/O /L=\"{path}\"",
            });
        }
        else
        {
            MessageBox.Show(path, Resources.PathNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    public override SortedDictionary<string, Bitmap> GetDirectors(string name, int limit)
    {
        var values = new SortedDictionary<string, Bitmap>();
        using var ctx = new AriadnaEntities();
        var directors = ctx.MovieDirectors.AsNoTracking().Where(r => r.Director.name.ToUpper().Contains(name)).Take(limit);

        foreach (var director in directors)
        {
            values[director.Director.name] = director.Director.photo.ToBitmap();
        }

        return values;
    }
    public override SortedDictionary<string, Bitmap> GetActors(string name, int limit)
    {
        var values = new SortedDictionary<string, Bitmap>();
        using var ctx = new AriadnaEntities();
        var actors = ctx.MovieCasts.AsNoTracking().Where(r => r.Actor.name.ToUpper().Contains(name)).Take(limit);

        foreach (var actor in actors)
        {
            values[actor.Actor.name] = actor.Actor.photo.ToBitmap();
        }

        return values;
    }
    public override SortedDictionary<string, Bitmap> GetGenres(string name)
    {
        var values = new SortedDictionary<string, Bitmap>();
        using var ctx = new AriadnaEntities();
        var genres = ctx.Genres.AsNoTracking().ToList();

        foreach (var genre in genres)
        {
            values[genre.name] = Utilities.GetMovieGenreImage(genre.name);
        }

        return values;
    }
    public override void FilterControls(MainPanel panel) {}
    private bool FindFirstNotInserted(string[] paths)
    {
        string foundPath = null;
        using var ctx = new AriadnaEntities();
        foreach (var path in paths)
        {
            // Skip ignored or already stored
            if (ctx.Ignores.AsNoTracking().FirstOrDefault(r => r.path == path) != null || 
                ctx.Movies.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() != null)
            {
                continue;
            }
                   
            foundPath = path;
            break;
        }

        if(string.IsNullOrEmpty(foundPath))
        {
            return false;
        }

        FetchMovieFromImdb(foundPath);
        return true;
    }
    private bool m_IsFetching;
    private async void FetchMovieFromImdb(string path)
    {
        if(m_IsFetching)
        {
            return;
        }

        m_IsFetching = true;
        var client = new TMDbClient(Settings.Default.TmdbApiKey);
        var detailsForm = new MovieDetailsForm(path, m_Logger)
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
    private string FindStoredEntryPathById(int id)
    {
        if (id == -1)
        {
            return string.Empty;
        }

        using var ctx = new AriadnaEntities();
        var path = ctx.Movies.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault()!.file_path;
        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }

        return string.Empty;
    }
    // ReSharper disable once UnusedMember.Local
    private void DeleteUnusedActors()
    {
        using var ctx = new AriadnaEntities();
        var actors = ctx.Actors.ToList();

        var bNeedToSaveChanges = false;
        foreach (var actor in actors)
        {
            var usedActor = ctx.MovieCasts.FirstOrDefault(r => (r.actorId == actor.Id));
            if (usedActor == null)
            {
                ctx.Actors.Remove(actor);
                bNeedToSaveChanges = true;
            }
        }
        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    // ReSharper disable once UnusedMember.Local
    private void DeleteUnusedGenres()
    {
        using var ctx = new AriadnaEntities();
        var genres = ctx.Genres.ToList();

        var bNeedToSaveChanges = false;
        foreach (var genre in genres)
        {
            var usedGenres = ctx.MovieGenres.FirstOrDefault(r => (r.genreId == genre.Id));
            if (usedGenres == null)
            {
                ctx.Genres.Remove(genre);
                bNeedToSaveChanges = true;
            }
        }
        if (bNeedToSaveChanges)
        {
            ctx.SaveChanges();
        }
    }
    // ReSharper disable once UnusedMember.Local
    private void UpdateEntryData()
    {
        using var ctx = new AriadnaEntities();
        var entries = ctx.Movies.ToList();
        foreach(var entry in entries)
        {
            entry.creation_time = File.GetLastWriteTimeUtc(entry.file_path);

            try
            {
                ctx.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                MessageBox.Show(entry.title, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}