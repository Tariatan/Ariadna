using System;
using System.Collections.Generic;
using System.Linq;
using Manina.Windows.Forms;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Data.Entity.Validation;

namespace Ariadna.DBStrategies
{
    public class MoviesDBStrategy : AbstractDBStrategy
    {
        private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new PosterFromFileAdaptor();

        public MoviesDBStrategy() => m_PosterImageAdaptor.RootPath = Utilities.MOVIE_POSTERS_ROOT_PATH;

        public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;
        
        public override List<Utilities.EntryDto> GetEntries()
        {
            //UpdateMovieData();

            using (var ctx = new AriadnaEntities())
            {
                return ctx.Movies.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
            }
        }
        public override List<Utilities.EntryDto> QueryEntries(QueryParams values)
        {
            using (var ctx = new AriadnaEntities())
            {
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
                    var entry = ctx.Directors.AsNoTracking().Where(r => r.name == values.Director).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieDirectors.Any(l => (l.directorId == entry.Id)));
                    }
                }
                // -- ACTOR NAME --
                if (!string.IsNullOrEmpty(values.Actor))
                {
                    var entry = ctx.Actors.AsNoTracking().Where(r => r.name == values.Actor).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.MovieCasts.Any(l => (l.actorId == entry.Id)));
                    }
                }
                // -- GENRE --
                if (!string.IsNullOrEmpty(values.Genre))
                {
                    var entry = ctx.Genres.AsNoTracking().Where(r => r.name == values.Genre).FirstOrDefault();
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
                    var recentDateStart = DateTime.Now.AddMonths(-Utilities.RECENT);
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
                    query = query.Where(r => (r.file_path.StartsWith(Utilities.DEFAULT_SERIES_PATH.Substring(0, 1))));
                }
                // -- MOVIES --
                if (values.IsMovies)
                {
                    query = query.Where(r => (r.file_path.StartsWith(Utilities.DEFAULT_MOVIES_PATH.Substring(0, 1)) ||
                        r.file_path.StartsWith(Utilities.DEFAULT_MOVIES_PATH_TMP.Substring(0, 1))));
                }

                return query.OrderBy(r => r.title).Select(x => new Utilities.EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
            }
        }
        public override Utilities.EntryInfo GetEntryInfo(int id)
        {
            Utilities.EntryInfo details = new Utilities.EntryInfo();
            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Movies.Where(r => r.Id == id).FirstOrDefault();
                if (entry != null)
                {
                    details.Path = entry.file_path;
                    details.Title = entry.title;
                    details.TitleOrig = entry.title_original;
                }
            }

            return details;
        }
        public override void RemoveEntry(int id)
        {
            using (var ctx = new AriadnaEntities())
            {
                Movie movie = ctx.Movies.Where(r => r.Id == id).FirstOrDefault();
                if (movie != null)
                {
                    ctx.MovieCasts.RemoveRange(ctx.MovieCasts.Where(r => (r.movieId == id)));
                    ctx.MovieDirectors.RemoveRange(ctx.MovieDirectors.Where(r => (r.movieId == id)));
                    ctx.MovieGenres.RemoveRange(ctx.MovieGenres.Where(r => (r.movieId == id)));

                    ctx.Movies.Remove(movie);

                    ctx.SaveChanges();
                }
            }

            string posterPath = Utilities.MOVIE_POSTERS_ROOT_PATH + id;
            if (File.Exists(posterPath))
            {
                try
                {
                    File.Delete(posterPath);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Source, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }
        public override bool FindNextEntryAutomatically()
        {
            if (FindFirstNotInserted(Directory.GetFiles(Utilities.DEFAULT_MOVIES_PATH)))
            {
                return true;
            }
            if (FindFirstNotInserted(Directory.GetFiles(Utilities.DEFAULT_MOVIES_PATH_TMP)))
            {
                return true;
            }

            if (FindFirstNotInserted(Directory.GetDirectories(Utilities.DEFAULT_SERIES_PATH)))
            {
                return true;
            }

            return false;
        }
        public override void FindNextEntryManually()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Utilities.DEFAULT_MOVIES_PATH;
                openFileDialog.Filter = "Видео файлы|*.avi;*.mkv;*.mpg;*.mp4;*.m4v;*.ts|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                // Allow folders
                openFileDialog.ValidateNames = false;
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                const string folderFlag = "File or folder";
                openFileDialog.FileName = folderFlag;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = openFileDialog.FileName;
                    if (path.Contains(folderFlag))
                    {
                        path = Path.GetDirectoryName(openFileDialog.FileName);
                    }
                    FetchMovieFromIMDB(path);
                }
            }
        }
        public override void ShowEntryDetails(int id)
        {
            var path = FindStoredEntryPathById(id);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            MovieDetailsForm detailsForm = new MovieDetailsForm(path);
            detailsForm.FormClosed += new FormClosedEventHandler(OnDetailsFormClosed);
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
                if (!File.Exists(Utilities.MEDIA_PLAYER_PATH))
                {
                    return;
                }

                // Enclose the path in quotes as required by MPC
                Process.Start(Utilities.MEDIA_PLAYER_PATH, "\"" + path + "\"");
            }
            // Checked if it is a directory
            else if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                MessageBox.Show(path, "Путь не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public override SortedDictionary<string, Bitmap> GetDirectors(string name, int limit)
        {
            var values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var directors = ctx.MovieDirectors.AsNoTracking().Where(r => r.Director.name.ToUpper().Contains(name)).Take(limit);

                foreach (var director in directors)
                {
                    values[director.Director.name] = Utilities.BytesToBitmap(director.Director.photo);
                }
            }

            return values;
        }
        public override SortedDictionary<string, Bitmap> GetActors(string name, int limit)
        {
            var values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var actors = ctx.MovieCasts.AsNoTracking().Where(r => (r.Actor.name.ToUpper().Contains(name))).Take(limit);

                foreach (var actor in actors)
                {
                    values[actor.Actor.name] = Utilities.BytesToBitmap(actor.Actor.photo);
                }
            }
            return values;
        }
        public override SortedDictionary<string, Bitmap> GetGenres(string name)
        {
            var values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var genres = ctx.Genres.AsNoTracking().ToList();

                foreach (var genre in genres)
                {
                    values[genre.name] = Utilities.GetMovieGenreImage(genre.name);
                }
            }
            return values;
        }
        public override void FilterControls(MainPanel panel) {}
        private bool FindFirstNotInserted(String[] paths)
        {
            string foundPath = null;
            using (var ctx = new AriadnaEntities())
            {
                foreach (var path in paths)
                {
                    if (ctx.Ignores.AsNoTracking().Where(r => r.path == path).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    if (ctx.Movies.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() == null)
                    {
                        foundPath = path;
                        break;
                    }
                }
            }

            if(string.IsNullOrEmpty(foundPath))
            {
                return false;
            }

            FetchMovieFromIMDB(foundPath);
            return true;
        }
        private bool isFetching = false;
        private async void FetchMovieFromIMDB(string path)
        {
            if(isFetching)
            {
                return;
            }

            isFetching = true;
            TMDbLib.Client.TMDbClient client = new TMDbLib.Client.TMDbClient(Utilities.TMDB_API_KEY);
            MovieDetailsForm detailsForm = new MovieDetailsForm(path)
            {
                TMDBMovieIndex = -1,
                TMDBTVShowIndex = -1
            };

            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                var tvShowsResults = await client.SearchTvShowAsync(dir.Name, "ru-RU");
                var tvShows = tvShowsResults.Results;
                if (tvShows.Count > 0)
                {
                    int choice = (tvShows.Count > 1) ? GetBestTVShowChoice(tvShows, path) : 0;
                    if (choice >= 0)
                    {
                        detailsForm.TMDBTVShowIndex = tvShows[choice].Id;
                    }
                }
            }
            else
            {
                string query = Path.GetFileNameWithoutExtension(path);
                var moviesResults = await client.SearchMovieAsync(query, "ru-RU");
                var movies = moviesResults.Results;

                if (movies.Count > 0)
                {
                    int choice = (movies.Count > 1) ? GetBestMovieChoice(movies, path) : 0;
                    if (choice >= 0)
                    {
                        detailsForm.TMDBMovieIndex = movies[choice].Id;
                    }
                }
            }

            detailsForm.FormClosed += new FormClosedEventHandler(OnDetailsFormClosed);
            detailsForm.ShowDialog();

            isFetching = false;
        }
        private int GetBestMovieChoice(List<TMDbLib.Objects.Search.SearchMovie> movies, string path)
        {
            List<Utilities.MovieChoiceDto> titles = new List<Utilities.MovieChoiceDto>();
            foreach (var result in movies.Take(20))
            {
                int y = (result.ReleaseDate != null) ? result.ReleaseDate.Value.Year : 0;
                titles.Add(new Utilities.MovieChoiceDto { Title = result.Title, TitleOrig = result.OriginalTitle, Year = y });
            }
            ChoicePopup choice = new ChoicePopup(path, titles);

            choice.ShowDialog(MainPanel.ActiveForm);
            return choice.index;
        }
        private int GetBestTVShowChoice(List<TMDbLib.Objects.Search.SearchTv> movies, string path)
        {
            List<Utilities.MovieChoiceDto> titles = new List<Utilities.MovieChoiceDto>();
            foreach (var result in movies.Take(20))
            {
                int y = (result.FirstAirDate != null) ? result.FirstAirDate.Value.Year : 0;
                titles.Add(new Utilities.MovieChoiceDto { Title = result.Name, TitleOrig = result.OriginalName, Year = y });
            }
            ChoicePopup choice = new ChoicePopup(path, titles);

            choice.ShowDialog(MainPanel.ActiveForm);
            return choice.index;
        }
        private void OnDetailsFormClosed(object sender, FormClosedEventArgs e)
        {
            MovieDetailsForm detailsForm = sender as MovieDetailsForm;
            if (detailsForm.FormCloseReason != Utilities.EFormCloseReason.SUCCESS)
            {
                return;
            }

            EntryInsertedEventArgs eventArgs = new EntryInsertedEventArgs(detailsForm.StoredDBEntryID);
            OnEntryInserted(eventArgs);
        }
        private string FindStoredEntryPathById(int id)
        {
            if (id != -1)
            {
                using (var ctx = new AriadnaEntities())
                {
                    var path = ctx.Movies.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault().file_path;
                    if (!string.IsNullOrEmpty(path))
                    {
                        return path;
                    }
                }
            }

            return "";
        }
        private void DeleteUnusedActors()
        {
            using (var ctx = new AriadnaEntities())
            {
                var actors = ctx.Actors.ToList();

                bool bNeedToSaveChanges = false;
                foreach (var actor in actors)
                {
                    var usedActor = ctx.MovieCasts.Where(r => (r.actorId == actor.Id)).FirstOrDefault();
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
        }
        private void DeleteUnusedGenres()
        {
            using (var ctx = new AriadnaEntities())
            {
                var genres = ctx.Genres.ToList();

                bool bNeedToSaveChanges = false;
                foreach (var genre in genres)
                {
                    var usedGenres = ctx.MovieGenres.Where(r => (r.genreId == genre.Id)).FirstOrDefault();
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
        }
        private void UpdateMovieData()
        {
            using (var ctx = new AriadnaEntities())
            {
                var movies = ctx.Movies.ToList();
                foreach(var movie in movies)
                {
                    movie.creation_time = File.GetLastWriteTimeUtc(movie.file_path);

                    try
                    {
                        ctx.SaveChanges();
                    }
                    catch (DbEntityValidationException)
                    {
                        MessageBox.Show(movie.title, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
