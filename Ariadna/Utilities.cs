using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Ariadna
{
    public static class Utilities
    {
        public enum EFormCloseReason
        {
            None = 0,
            SUCCESS,
        }
        // Entry Data Transfer Object
        public class EntryDto
        {
            public string Path { get; set; }
            public string Title { get; set; }
            public int Id { get; set; }
        }
        public class MovieChoiceDto
        {
            public string Title { get; set; }
            public string TitleOrig { get; set; }
            public int Year { get; set; }
        }
        public class EntryInfo
        {
            public string Title { get; set; }
            public string TitleOrig { get; set; }
            public string Path { get; set; }
        }

        public static readonly int GENRE_IMAGE_W = 60;
        public static readonly int GENRE_IMAGE_H = 60;

        public static readonly int POSTER_W = 400;
        public static readonly int POSTER_H = 600;

        public static readonly int PHOTO_W = 54;
        public static readonly int PHOTO_H = 81;

        public static readonly int PREVIEW_W = 603;
        public static readonly int PREVIEW_H = 339;

        public static readonly int PREVIEW_SMALL_W = 137;
        public static readonly int PREVIEW_SMALL_H = 77;

        public static readonly int MAX_GENRES_COUNT = 5;

        public static readonly string TMDB_API_KEY = "ec3e7f0826eb6ef92dc4b1f69f1e1dd3";
        public static readonly string MOVIE_POSTERS_ROOT_PATH = @"G:/Ariadna/movies/";
        public static readonly string GAME_POSTERS_ROOT_PATH = @"G:/Ariadna/games/";
        public static readonly string DEFAULT_GAMES_PATH = @"A:\GAMES\";
        public static readonly string DEFAULT_GAMES_PATH_VR = @"A:\GAMES\_VR_\";
        public static readonly string DEFAULT_MOVIES_PATH = @"M:\";
        public static readonly string DEFAULT_MOVIES_PATH_TMP = @"A:\MOVIES\";
        public static readonly string DEFAULT_SERIES_PATH = @"S:\";
        public static readonly string MEDIA_PLAYER_PATH = "C:/Program Files/MEDIA/K-Lite Codec Pack/MPC-HC64/mpc-hc64.exe";
        public static readonly string PREVIEW_SUFFIX = "_preview";

        public static Dictionary<string, Bitmap> MOVIE_GENRES = new Dictionary<string, Bitmap>
        {
            {"Боевик", Properties.Resources.action},
            {"Приключение", Properties.Resources.adventure},
            {"Анимационный", Properties.Resources.animation},
            {"Биография", Properties.Resources.biography},
            {"Комедия", Properties.Resources.comedy},
            {"Криминал", Properties.Resources.criminal},
            {"Детектив", Properties.Resources.detective},
            {"Катастрофа", Properties.Resources.disaster},
            {"Драма", Properties.Resources.drama},
            {"Сказка", Properties.Resources.fairytail},
            {"Семейный", Properties.Resources.family},
            {"Фэнтези", Properties.Resources.fantasy},
            {"Исторический", Properties.Resources.historical},
            {"Ужасы", Properties.Resources.horror},
            {"Детский", Properties.Resources.kid},
            {"Музыка", Properties.Resources.musical},
            {"Мистика", Properties.Resources.mystic},
            {"Постапокалипсис", Properties.Resources.postapocalypse},
            {"Романтика", Properties.Resources.romance},
            {"Фантастика", Properties.Resources.scifi},
            {"Спорт", Properties.Resources.sport},
            {"Триллер", Properties.Resources.thriller},
            {"Военный", Properties.Resources.war},
            {"Вестерн", Properties.Resources.western},
            {"Новогодний", Properties.Resources.xmas},
        };
        public static Dictionary<string, Bitmap> GAME_GENRES = new Dictionary<string, Bitmap>
        {
            {"Adventure", Properties.Resources.adventure},
            {"Fighting", Properties.Resources.fighting},
            {"Action", Properties.Resources.action},
            {"Quest", Properties.Resources.detective},
            {"Platformer", Properties.Resources.platformer},
            {"Postapocalypse", Properties.Resources.postapocalypse},
            {"RPG", Properties.Resources.historical},
            {"Turn-Based", Properties.Resources.turnbased},
            {"Simulator", Properties.Resources.simulator},
            {"Strategy", Properties.Resources.strategy},
            {"Tower Defense", Properties.Resources.towerdefense},
            {"Racing", Properties.Resources.racing},
            {"Sport", Properties.Resources.sport},
            {"Sci-Fi", Properties.Resources.scifi},
            {"Fantasy", Properties.Resources.fantasy},
            {"FPS", Properties.Resources.fps},
            {"3rd View", Properties.Resources.tps},
            {"Isometric", Properties.Resources.isometric},
            {"Horror", Properties.Resources.horror},
            {"City Building", Properties.Resources.city_building},
            {"Music", Properties.Resources.musical},
            {"Arcade", Properties.Resources.arcade},
            {"Meditate", Properties.Resources.meditate},
        };
        public static Bitmap GetMovieGenreImage(string name)
        {
            if(MOVIE_GENRES.ContainsKey(name))
                return MOVIE_GENRES[name];

            return Properties.Resources.No_Image;
            //return new Bitmap(Properties.Resources.No_Image);
        }
        public static string GetMovieGenreBySynonym(string name)
        {
            switch (name)
            {
                case "Мультфильм":
                case "Анимация":
                    return "Анимационный";
                case "Мелодрама":
                    return "Драма";
                case "Приключения":
                    return "Приключение";
            }
            return name;
        }
        public static Bitmap GetGameGenreImage(string name)
        {
            if (GAME_GENRES.ContainsKey(name))
                return GAME_GENRES[name];

            return Properties.Resources.No_Image;
            //return new Bitmap(Properties.Resources.No_Image);
        }
        public static string GetGameGenreBySynonym(string name)
        {
            return name;
        }
        public static string CapitalizeWords(string words)
        {
            var wordList = words.Trim().Split(' ');
            var result = "";
            for (int i = 0; i < wordList.Length; ++i)
            {
                if (wordList[i].Length == 0)
                {
                    continue;
                }

                wordList[i] = wordList[i][0].ToString().ToUpper() + wordList[i].Substring(1);
                if (result.Length > 0)
                {
                    result += " ";
                }
                result += wordList[i];
            }

            return result;
        }
        public static byte[] ImageToBytes(Image img)
        {
            try
            {
                ImageConverter converter = new ImageConverter();
                return (byte[])converter.ConvertTo(img, typeof(byte[]));
            }
            catch (Exception)
            {
            }

            return null;
        }
        public static Bitmap BytesToBitmap(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length == 0))
            {
                return null;
            }

            using (var memoryStream = new System.IO.MemoryStream(bytes))
            {
                return new Bitmap(memoryStream);
            }
        }
        private static byte[] empty = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 54, 0, 0, 0, 81, 8, 6, 0, 0, 0, 153, 180, 85, 63, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1, 199, 111, 168, 100, 0, 0 };
        public static bool IsValidPreview(byte[] bytes)
        {
            if (bytes == null)
            {
                return false;
            }

            // Workaround: compare first ~85 bytes of NO_PREVIEW_IMAGE_SMALL to determine if no preview was set
            const int NO_PREVIEW_IMAGE_SMALL_length = 1119;
            const int NO_PREVIEW_IMAGE_SMALL_length2 = 1150;
            if ((bytes.Length != NO_PREVIEW_IMAGE_SMALL_length) &&
                (bytes.Length != NO_PREVIEW_IMAGE_SMALL_length2))
            {
                return true;
            }

            for (int i = 0; i < empty.Length; ++i)
            {
                if (bytes[i] != empty[i])
                {
                    return true;
                }
            }

            return false;
        }
        public static int ConvertId(string sId)
        {
            Int32.TryParse(sId, out int id);
            return id;
        }
        public static bool GetBitmapFromDisk(out Bitmap outBmp, string filter, int width, int height)
        {
            outBmp = null;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "T:\\Downloads\\";
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                var fileName = openFileDialog.FileName;
                var extStartPos = fileName.LastIndexOf('.') + 1;
                string ext = "";
                if (extStartPos > 0)
                {
                    ext = fileName.Substring(extStartPos).ToUpper();
                }

                // Check supported extensions
                HashSet<string> exts = new HashSet<string> { "BMP", "GIF", "EXIF", "JPG", "JPEG", "PNG", "TIFF" };
                if ((ext.Length > 0) && !exts.Contains(ext))
                {
                    return false;
                }

                outBmp = new Bitmap(width, height);
                Graphics graph = Graphics.FromImage(outBmp);

                using (var bmpTemp = new Bitmap(fileName))
                {
                    Image image = new Bitmap(bmpTemp);
                    graph.DrawImage(image, new Rectangle(0, 0, width, height));
                }

                return true;
            }
        }
    }
}
