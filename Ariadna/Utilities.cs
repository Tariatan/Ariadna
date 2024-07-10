using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

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
        public static Dictionary<string, Bitmap> DOCUMENTARY_GENRES = new Dictionary<string, Bitmap>
        {
            {"Travel", Properties.Resources.travel},
            {"Art. History", Properties.Resources.art},
            {"Universe", Properties.Resources.universe},
            {"Stars. Planets", Properties.Resources.planets},
            {"Astronautics", Properties.Resources.rocket},
            {"Personality", Properties.Resources.biography},
            {"Science", Properties.Resources.science},
            {"Misc", Properties.Resources.misc},
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
        }
        public static string GetGameGenreBySynonym(string name)
        {
            return name;
        }
        public static Bitmap GetDocumentaryGenreImage(string name)
        {
            if (DOCUMENTARY_GENRES.ContainsKey(name))
                return DOCUMENTARY_GENRES[name];

            return Properties.Resources.No_Image;
        }
        public static string GetDocumentaryGenreBySynonym(string name)
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

            using (var memoryStream = new MemoryStream(bytes))
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
                openFileDialog.InitialDirectory = Properties.Settings.Default.BitmapInitialSearchDir;
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
        public static TimeSpan GetVideoDuration(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return TimeSpan.Zero;
            }

            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                if (prop.ValueAsObject == null)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromTicks((long)(ulong)prop.ValueAsObject);
            }
        }
        public static string DecorateDescription(string description)
        {
            var paragraphs = description.Split('\n');
            var decoratedText = "";
            foreach (var paragraph in paragraphs)
            {
                if (decoratedText.Length > 0)
                {
                    decoratedText += "\r\n";
                }
                decoratedText += "\t" + paragraph.Trim();
            }

            return decoratedText;
        }
    }
}
