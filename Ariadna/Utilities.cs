using System;
using System.Collections.Generic;
using System.Drawing;

namespace Ariadna
{
    public static class Utilities
    {
        public enum EFormCloseReason
        {
            None = 0,
            SUCCESS,
        }
        // Movie Data Transfer Object
        public class MovieDto
        {
            public string path { get; set; }
            public string title { get; set; }
            public int id { get; set; }
        }
        public class MovieChoiceDto
        {
            public string titleRu { get; set; }
            public string titleOrig { get; set; }
            public int year { get; set; }
        }

        public static int GENRE_IMAGE_W = 60;
        public static int GENRE_IMAGE_H = 60;

        public static int POSTER_W = 400;
        public static int POSTER_H = 600;

        public static int PHOTO_W = 54;
        public static int PHOTO_H = 81;

        public static int MAX_GENRES_COUNT = 5;

        public static string TMDB_API_KEY = "ec3e7f0826eb6ef92dc4b1f69f1e1dd3";
        public static string POSTERS_ROOT_PATH = @"G:/Ariadna/posters/";

        public static List<string> GENRES_LIST = new List<string>
        {"Боевик", "Приключение", "Анимационный", "Биография", "Комедия", "Криминал", "Детектив", "Катастрофа", "Драма",
         "Сказка", "Семейный", "Фэнтези", "Исторический", "Ужасы", "Детский", "Музыка", "Мистика", "Постапокалипсис",
         "Романтика", "Фантастика", "Спорт", "Триллер", "Военный", "Вестерн", "Новогодний"
        };
        public static string GetGenreBySynonym(string name)
        {
            switch(name)
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
            if(bytes == null)
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

            for(int i = 0; i < empty.Length; ++i)
            {
                if(bytes[i] != empty[i])
                {
                    return true;
                }
            }

            return false;
        }
        public static Bitmap GetGenreImage(string name)
        {
            switch (name)
            {
                case "Боевик":              return new Bitmap(Properties.Resources.action);
                case "Приключение":         return new Bitmap(Properties.Resources.adventure);
                case "Анимационный":        return new Bitmap(Properties.Resources.animation);
                case "Биография":           return new Bitmap(Properties.Resources.biography);
                case "Комедия":             return new Bitmap(Properties.Resources.comedy);
                case "Криминал":            return new Bitmap(Properties.Resources.criminal);
                case "Детектив":            return new Bitmap(Properties.Resources.detective);
                case "Катастрофа":          return new Bitmap(Properties.Resources.disaster);
                case "Драма":               return new Bitmap(Properties.Resources.drama);
                case "Сказка":              return new Bitmap(Properties.Resources.fairytail);
                case "Семейный":            return new Bitmap(Properties.Resources.family);
                case "Фэнтези":             return new Bitmap(Properties.Resources.fantasy);
                case "Исторический":        return new Bitmap(Properties.Resources.historical);
                case "Ужасы":               return new Bitmap(Properties.Resources.horror);
                case "Детский":             return new Bitmap(Properties.Resources.kid);
                case "Музыка":              return new Bitmap(Properties.Resources.musical);
                case "Мистика":             return new Bitmap(Properties.Resources.mystic);
                case "Постапокалипсис":     return new Bitmap(Properties.Resources.postapocalypse);
                case "Романтика":           return new Bitmap(Properties.Resources.romance);
                case "Фантастика":          return new Bitmap(Properties.Resources.scifi);
                case "Спорт":               return new Bitmap(Properties.Resources.sport);
                case "Триллер":             return new Bitmap(Properties.Resources.thriller);
                case "Военный":             return new Bitmap(Properties.Resources.war);
                case "Вестерн":             return new Bitmap(Properties.Resources.western);
                case "Новогодний":          return new Bitmap(Properties.Resources.xmas);
                default:                    return new Bitmap(Properties.Resources.No_Image);
            }
        }
    }
}
