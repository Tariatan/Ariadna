using Manina.Windows.Forms;
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
    public partial class GameDetailsForm : DetailsForm
    {
        public GameDetailsForm(string filePath) : base(filePath) { }
        #region OVERRIDEN FUNCTIONS
        protected override void DoLoad()
        {
            #region Hide inappropriate fields
            m_TxtLength.Visible = false;
            m_TxtDescription.Visible = false;
            m_TxtDimension.Visible = false;
            m_TxtBitrate.Visible = false;
            m_DirectorsList.Visible = false;
            m_CastList.Visible = false;
            m_DescriptionPaste.Visible = false;
            m_DirectorPaste.Visible = false;
            m_CastPaste.Visible = false;
            m_LblDirector.Visible = false;
            m_LblCast.Visible = false;
            m_LblDescr.Visible = false;
            m_PicFlag1.Visible = false;
            m_PicFlag2.Visible = false;
            m_PicFlag3.Visible = false;
            m_PicFlag4.Visible = false;
            m_LblDuration.Visible = false;
            m_LblDimensions.Visible = false;
            m_LblBitrate.Visible = false;
            m_LblAudioStreams.Visible = false;
            #endregion
            #region Show my fields
            m_PreviewFull.Visible = true;
            m_Preview1.Visible = true;
            m_Preview2.Visible = true;
            m_Preview3.Visible = true;
            m_Preview4.Visible = true;
            m_VR.Visible = true;
            m_LblVersion.Visible = true;
            m_TxtVersion.Visible = true;
            m_VR.Checked = FilePath.Contains(Utilities.DEFAULT_GAMES_PATH_VR);

            Icon = Properties.Resources.AriadnaGames;
            #endregion

            using (var ctx = new AriadnaEntities())
            {
                var entry = ctx.Games.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (entry != null)
                {
                    StoredDBEntryID = entry.Id;
                }
            }

            if (StoredDBEntryID != -1)
            {
                FillFieldsFromFile();
            }
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
            return Utilities.GAME_GENRES.Keys.ToList();
        }
        protected override string GetGenreBySynonym(string name)
        {
            return Utilities.GetGameGenreBySynonym(name);
        }
        protected override Bitmap GetGenreImage(string name)
        {
            return Utilities.GetGameGenreImage(name);
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
                    var genre = ctx.GenreOfGames.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        ctx.GenreOfGames.Add(new GenreOfGame { name = item.Text });
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
                Game entry = null;
                if (StoredDBEntryID != -1)
                {
                    entry = ctx.Games.Where(r => r.Id == StoredDBEntryID).FirstOrDefault();
                }

                bool bAdd = false;
                if (entry == null)
                {
                    bAdd = true;
                    entry = new Game();
                }

                Int32.TryParse(m_TxtYear.Text, out int year);

                entry.title = title.Trim();
                entry.title_original = m_TxtTitleOrig.Text.Trim();
                entry.year = year;
                entry.file_path = m_TxtPath.Text.Trim();
                entry.creation_time = File.GetLastWriteTimeUtc(FilePath);
                entry.want_to_play = m_WanToSee.Checked;
                entry.vr = m_VR.Checked;
                entry.version = m_TxtVersion.Text.Trim();

                if (bAdd)
                {
                    ctx.Games.Add(entry);
                }

                try
                {
                    ctx.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    MessageBox.Show(title + ":\n" + ex.Message, "Ошибка сохранения записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }

                path = entry.file_path;
            }

            if (bSuccess)
            {
                using (var ctx = new AriadnaEntities())
                {
                    StoredDBEntryID = ctx.Games.AsNoTracking().Where(r => r.file_path == path).Select(x => new { x.Id }).FirstOrDefault().Id;
                    bSuccess = (StoredDBEntryID != -1);
                }
            }

            if (bSuccess)
            {
                try
                {
                    string name = Utilities.GAME_POSTERS_ROOT_PATH + StoredDBEntryID.ToString();
                    m_PicPoster.Image.Save(name, System.Drawing.Imaging.ImageFormat.Png);

                    m_Preview1.Image.Save(name + Utilities.PREVIEW_SUFFIX + 1, System.Drawing.Imaging.ImageFormat.Png);
                    m_Preview2.Image.Save(name + Utilities.PREVIEW_SUFFIX + 2, System.Drawing.Imaging.ImageFormat.Png);
                    m_Preview3.Image.Save(name + Utilities.PREVIEW_SUFFIX + 3, System.Drawing.Imaging.ImageFormat.Png);
                    m_Preview4.Image.Save(name + Utilities.PREVIEW_SUFFIX + 4, System.Drawing.Imaging.ImageFormat.Png);
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

                ctx.GameGenres.RemoveRange(ctx.GameGenres.Where(r => (r.gameId == entryId)));
                ctx.SaveChanges();

                foreach (ListViewItem item in m_GenresList.Items)
                {
                    GenreOfGame genre = ctx.GenreOfGames.Where(r => r.name == item.Text).FirstOrDefault();
                    if (genre == null)
                    {
                        continue;
                    }

                    var entryGenre = ctx.GameGenres.Where(r => (r.gameId == entryId && r.genreId == genre.Id)).FirstOrDefault();
                    if (entryGenre == null)
                    {
                        ctx.GameGenres.Add(new GameGenre { gameId = entryId, genreId = genre.Id });
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
                var entry = ctx.Games.AsNoTracking().Where(r => r.file_path == FilePath).FirstOrDefault();
                if (entry == null)
                {
                    return;
                }

                m_TxtYear.Text = (entry.year > 0) ? entry.year.ToString() : "";
                m_TxtTitle.Text = entry.title;
                m_TxtTitleOrig.Text = entry.title_original;
                m_TxtPath.Text = entry.file_path;
                m_WanToSee.Checked = Convert.ToBoolean(entry.want_to_play);
                m_VR.Checked = Convert.ToBoolean(entry.vr);
                m_TxtVersion.Text = entry.version;

                var genresSet = ctx.GameGenres.AsNoTracking().ToArray().Where(r => (r.gameId == entry.Id));
                foreach (var genres in genresSet)
                {
                    AddGenre(genres.GenreOfGame.name);
                }

                string filename = Utilities.GAME_POSTERS_ROOT_PATH + entry.Id.ToString();
                if (File.Exists(filename))
                {
                    using (var bmpTemp = new Bitmap(filename))
                    {
                        m_PicPoster.Image = new Bitmap(bmpTemp);
                    }
                }

                for (var i = 1u; i <= 4; ++i)
                {
                    string name = filename + Utilities.PREVIEW_SUFFIX + i;
                    if (!File.Exists(name))
                    {
                        continue;
                    }

                    using (var bmpTemp = new Bitmap(name))
                    {
                        switch (i)
                        {
                            case 1: m_Preview1.Image = new Bitmap(bmpTemp);
                                    m_PreviewFull.Image = new Bitmap(m_Preview1.Image); break;
                            case 2: m_Preview2.Image = new Bitmap(bmpTemp); break;
                            case 3: m_Preview3.Image = new Bitmap(bmpTemp); break;
                            case 4: m_Preview4.Image = new Bitmap(bmpTemp); break;
                        }
                    }
                }
            }
        }
    }
}