using System;
using System.Collections.Generic;
using System.Linq;
using Manina.Windows.Forms;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

namespace Ariadna.DBStrategies
{
    public class GamesDBStrategy : AbstractDBStrategy
    {
        private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new PosterFromFileAdaptor();

        public GamesDBStrategy()
        {
            m_PosterImageAdaptor.RootPath = Utilities.GAME_POSTERS_ROOT_PATH;
        }
        public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter()
        {
            return m_PosterImageAdaptor;
        }
        public override List<Utilities.EntryDto> GetEntries()
        {
            using (var ctx = new AriadnaEntities())
            {
                return ctx.Games.AsNoTracking().OrderBy(r => r.title).Select(x => new Utilities.EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
            }
        }
        public override List<Utilities.EntryDto> QueryEntries(QueryParams values)
        {
            using (var ctx = new AriadnaEntities())
            {
                IQueryable<Game> query = ctx.Games.AsNoTracking();

                // -- Search Name --
                if (!string.IsNullOrEmpty(values.Name))
                {
                    var toSearch = values.Name.ToUpper();
                    query = query.Where(r => r.title.ToUpper().Contains(toSearch) ||
                                             r.title_original.ToUpper().Contains(toSearch) ||
                                             r.file_path.ToUpper().Contains(toSearch));
                }
                // -- GENRE --
                if (!string.IsNullOrEmpty(values.Genre))
                {
                    var entry = ctx.GenreOfGames.AsNoTracking().Where(r => r.name == values.Genre).FirstOrDefault();
                    if (entry != null)
                    {
                        query = query.Where(r => r.GameGenres.Any(l => (l.genreId == entry.Id)));
                    }
                }
                // -- WISH LIST --
                if (values.IsWish)
                {
                    query = query.Where(r => (r.want_to_play == true));
                }
                // -- RECENTLY Added --
                if (values.IsRecent)
                {
                    var recentDateStart = DateTime.Now.AddMonths(-6);
                    query = query.Where(r => ((r.creation_time > recentDateStart)));
                }
                // -- NEW --
                if (values.IsNew)
                {
                    query = query.Where(r => ((r.year == (DateTime.Now.Year)) || r.year == (DateTime.Now.Year - 1)));
                }

                return query.OrderBy(r => r.title).Select(x => new Utilities.EntryDto { Path = x.file_path, Title = x.title, Id = x.Id }).ToList();
            }
        }
        public override Utilities.EntryInfo GetEntryInfo(int id)
        {
            Utilities.EntryInfo details = new Utilities.EntryInfo();
            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Games.Where(r => r.Id == id).FirstOrDefault();
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
                var entry = ctx.Games.Where(r => r.Id == id).FirstOrDefault();
                if (entry != null)
                {
                    ctx.GameGenres.RemoveRange(ctx.GameGenres.Where(r => (r.gameId == id)));

                    ctx.Games.Remove(entry);

                    ctx.SaveChanges();

                    string posterPath = Utilities.GAME_POSTERS_ROOT_PATH + id;
                    if (File.Exists(posterPath))
                    {
                        File.Delete(posterPath);
                    }
                    for (var i = 1u; i <= 4; ++i)
                    {
                        string name = posterPath + Utilities.PREVIEW_SUFFIX + i;
                        if (File.Exists(name))
                        {
                            File.Delete(name);
                        }
                    }
                }
            }
        }
        public override bool FindNextEntryAutomatically()
        {
            if (FindFirstNotInserted(Directory.GetDirectories(Utilities.DEFAULT_GAMES_PATH)))
            {
                return true;
            }
            
            return false;
        }
        public override void FindNextEntryManually()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Utilities.DEFAULT_GAMES_PATH;
//                 openFileDialog.Filter = "Видео файлы|*.avi;*.mkv;*.mpg;*.mp4;*.m4v;*.ts|All files (*.*)|*.*";
//                 openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                // Allow folders
                openFileDialog.ValidateNames = false;
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                const string folderFlag = "Choose folder";
                openFileDialog.FileName = folderFlag;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = openFileDialog.FileName;
                    if (path.Contains(folderFlag))
                    {
                        path = Path.GetDirectoryName(openFileDialog.FileName);
                    }
                    ShowDataDialog(path);
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

            GameDetailsForm detailsForm = new GameDetailsForm(path);
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

            // Open directory
            if (Directory.Exists(path))
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
            return null;
        }
        public override SortedDictionary<string, Bitmap> GetActors(string name, int limit)
        {
            return null;
        }
        public override SortedDictionary<string, Bitmap> GetGenres(string name)
        {
            SortedDictionary<string, Bitmap> values = new SortedDictionary<string, Bitmap>();
            using (var ctx = new AriadnaEntities())
            {
                var genres = ctx.GenreOfGames.AsNoTracking().ToList();

                foreach (var genre in genres)
                {
                    values[genre.name] = Utilities.GetGameGenreImage(genre.name);
                }
            }
            return values;
        }
        public override void FilterControls(MainPanel panel)
        {
            panel.m_ToolStrip_DirectorLbl.Visible = false;
            panel.m_ToolStrip_ActorLbl.Visible = false;
            panel.m_ToolStrip_ActorName.Visible = false;
            panel.m_ToolStrip_DirectorName.Visible = false;
            panel.m_ToolStrip_ClearActorBtn.Visible = false;
            panel.m_ToolStrip_ClearDirectorBtn.Visible = false;
            panel.m_ToolStrip_ClearDirectorBtn.Visible = false;
            panel.m_ToolStrip_DirectorSprt.Visible = false;
            panel.m_ToolStrip_ActorSprt.Visible = false;

            panel.Icon = Properties.Resources.AriadnaGames;
        }
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

                    if (ctx.Games.AsNoTracking().Where(r => r.file_path == path).Select(r => r.file_path).FirstOrDefault() == null)
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

            ShowDataDialog(foundPath);
            return true;
        }
        private void ShowDataDialog(string path)
        {
            GameDetailsForm detailsForm = new GameDetailsForm(path);
            detailsForm.FormClosed += new FormClosedEventHandler(OnDetailsFormClosed);
            detailsForm.ShowDialog();
        }
        private void OnDetailsFormClosed(object sender, FormClosedEventArgs e)
        {
            GameDetailsForm detailsForm = sender as GameDetailsForm;
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
                    var path = ctx.Games.AsNoTracking().Where(r => r.Id == id).Select(x => new { x.file_path }).FirstOrDefault().file_path;
                    if (!string.IsNullOrEmpty(path))
                    {
                        return path;
                    }
                }
            }

            return "";
        }
    }
}
