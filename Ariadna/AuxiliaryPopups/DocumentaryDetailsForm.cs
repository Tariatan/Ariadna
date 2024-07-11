using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Ariadna
{
    public partial class DocumentaryDetailsForm : DetailsForm
    {
        private readonly ILogger logger;

        public DocumentaryDetailsForm(string filePath, ILogger logger) : base(filePath, logger)
        {
            this.logger = logger;
        }
        #region OVERRIDEN FUNCTIONS
        protected override void DoLoad()
        {
            #region Hide inappropriate fields
            m_DirectorsList.Visible = false;
            m_CastList.Visible = false;
            m_DirectorPaste.Visible = false;
            m_CastPaste.Visible = false;
            m_LblDirector.Visible = false;
            m_LblCast.Visible = false;
            #endregion
            m_TxtDescription.Height = m_TxtDescription.Height * 5/2;

            // Remove extension
            m_TxtTitle.Text = m_TxtTitle.Text.Replace(".avi", "").Replace(".mkv", "").Replace(".m4v", "").Replace(".mp4", "").Replace(".mpg", "").Replace(".ts", "").Replace(".mpeg", "");
            var length = Utilities.GetVideoDuration(FilePath);
            m_TxtLength.Text = new TimeSpan(length.Hours, length.Minutes, length.Seconds).ToString(@"hh\:mm\:ss");

            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Documentaries.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (entry != null)
                {
                    StoredDBEntryID = entry.Id;
                }
            }

            if (StoredDBEntryID != -1)
            {
                FillFieldsFromFile();
            }

            FillMediaInfo(FilePath);
        }
        protected override bool DoStore()
        {
            bool bSuccess = StoreGenres();
            bSuccess = bSuccess && StoreEntry();

            if (bSuccess)
            {
                if (StoredDBEntryID != -1)
                {
                    // Store tables with references
                    bSuccess = bSuccess && StoreEntryGenres(StoredDBEntryID);
                }
            }

            return bSuccess;
        }
        protected override List<string> GetGenres()
        {
            return Utilities.DOCUMENTARY_GENRES.Keys.ToList();
        }
        protected override string GetGenreBySynonym(string name)
        {
            return Utilities.GetDocumentaryGenreBySynonym(name);
        }
        protected override Bitmap GetGenreImage(string name)
        {
            return Utilities.GetDocumentaryGenreImage(name);
        }
        #endregion

        private bool StoreGenres()
        {
            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;
                foreach (ListViewItem item in m_GenresList.Items)
                {
                    var genre = ctx.GenreOfDocumentaries.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        ctx.GenreOfDocumentaries.Add(new GenreOfDocumentary { name = item.Text });
                        bNeedToSaveChanges = true;
                    }
                }

                if (bNeedToSaveChanges)
                {
                    try
                    {
                        ctx.SaveChanges();
                    }
                    catch (DbEntityValidationException)
                    {
                        MessageBox.Show("Ой", "Ошибка сохранения списка жанров", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        bSuccess = false;
                    }
                }
            }
            return bSuccess;
        }
        private bool StoreEntry()
        {
            string title = m_TxtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                return false;
            }

            bool bSuccess = true;
            string path = "";
            using (var ctx = new AriadnaEntities())
            {
                Documentary entry = null;
                if (StoredDBEntryID != -1)
                {
                    entry = ctx.Documentaries.Where(r => r.Id == StoredDBEntryID).FirstOrDefault();
                }

                bool bAddEntry = false;
                if (entry == null)
                {
                    bAddEntry = true;
                    entry = new Documentary();
                }

                Int32.TryParse(m_TxtYear.Text, out int year);

                entry.title = title;
                entry.title_original = m_TxtTitleOrig.Text.Trim();
                entry.year = year;
                entry.file_path = m_TxtPath.Text.Trim();
                entry.description = m_TxtDescription.Text;
                entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
                entry.want_to_see = m_WanToSee.Checked;

                if (bAddEntry)
                {
                    ctx.Documentaries.Add(entry);
                }

                try
                {
                    ctx.SaveChanges();
                }
                catch (DbEntityValidationException)
                {
                    MessageBox.Show(title, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }

                path = entry.file_path;
            }

            if (bSuccess)
            {
                using (var ctx = new AriadnaEntities())
                {
                    StoredDBEntryID = ctx.Documentaries.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault().Id;
                    bSuccess = (StoredDBEntryID != -1);
                }
            }

            if (bSuccess)
            {
                try
                {
                    m_PicPoster.Image.Save(Properties.Settings.Default.DocumentaryPostersRootPath + StoredDBEntryID.ToString(), System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка сохранения постера", "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return bSuccess;
        }
        private bool StoreEntryGenres(int entryId)
        {
            if (m_GenresList.Items.Count == 0)
            {
                return true;
            }

            bool bSuccess = true;
            using (var ctx = new AriadnaEntities())
            {
                bool bNeedToSaveChanges = false;

                ctx.DocumentaryGenres.RemoveRange(ctx.DocumentaryGenres.Where(r => (r.documentaryId == entryId)));
                ctx.SaveChanges();

                foreach (ListViewItem item in m_GenresList.Items)
                {
                    GenreOfDocumentary genre = ctx.GenreOfDocumentaries.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        continue;
                    }

                    var entryGenre = ctx.DocumentaryGenres.Where(r => (r.documentaryId == entryId && r.genreId == genre.Id)).FirstOrDefault();
                    if (entryGenre == null)
                    {
                        ctx.DocumentaryGenres.Add(new DocumentaryGenre { documentaryId = entryId, genreId = genre.Id });
                        bNeedToSaveChanges = true;
                    }
                }

                if (bNeedToSaveChanges)
                {
                    ctx.SaveChanges();
                }
            }
            return bSuccess;
        }
        private void FillFieldsFromFile()
        {
            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Documentaries.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (entry == null)
                {
                    return;
                }

                m_TxtYear.Text = (entry.year > 0) ? entry.year.ToString() : "";
                m_TxtTitle.Text = entry.title;
                m_TxtTitleOrig.Text = entry.title_original;
                m_TxtPath.Text = entry.file_path;
                m_TxtDescription.Text = Utilities.DecorateDescription(entry.description);
                m_WanToSee.Checked = Convert.ToBoolean(entry.want_to_see);

                string filename = Properties.Settings.Default.DocumentaryPostersRootPath + entry.Id.ToString();
                if (File.Exists(filename))
                {
                    using (var bmpTemp = new Bitmap(filename))
                    {
                        m_PicPoster.Image = new Bitmap(bmpTemp);
                    }
                }

                var genresSet = ctx.DocumentaryGenres.AsNoTracking().ToArray().Where(r => (r.documentaryId == entry.Id));
                foreach (var genres in genresSet)
                {
                    AddGenre(genres.GenreOfDocumentary.name);
                }
            }
        }
        private void FillMediaInfo(string path)
        {
            if (Directory.Exists(path))
            {
                var firstFile = Directory.EnumerateFiles(path).FirstOrDefault();
                if(String.IsNullOrEmpty(firstFile))
                {
                    var firstSubDir = Directory.GetDirectories(path).FirstOrDefault();
                    firstFile = Directory.EnumerateFiles(firstSubDir).FirstOrDefault();
                }

                path = firstFile;
            }

            if(String.IsNullOrEmpty(path))
            {
                return;
            }

            var info = new MediaInfo.MediaInfoWrapper(path);
            m_TxtDimension.Text = info.Width.ToString() + "x" + info.Height.ToString();
            m_TxtBitrate.Text = (info.VideoRate / 1000000).ToString() + " Mbps";

            var audios = info.AudioStreams;
            List<PictureBox> flags = new List<PictureBox> { m_PicFlag1, m_PicFlag2, m_PicFlag3, m_PicFlag4 };
            foreach (var flag in flags)
            {
                flag.Image = null;
            }

            int index = 0;
            foreach (var stream in audios)
            {
                // Limit number of audio tracks
                if(index >= flags.Count)
                {
                    break;
                }

                if (stream.Language.Equals("Russian"))
                {
                    flags[index++].Image = Properties.Resources.ru;
                }
                else if (stream.Language.Equals("English"))
                {
                    flags[index++].Image = Properties.Resources.en;
                }
                else if (stream.Language.Equals("French"))
                {
                    flags[index++].Image = Properties.Resources.fr;
                }
                else if (stream.Language.Equals("Ukrainian"))
                {
                    flags[index++].Image = Properties.Resources.ua;
                }
            }
        }
    }
}