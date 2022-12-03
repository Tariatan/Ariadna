using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        // Movie Data Transfer Object
        public class MovieDto
        {
            public string path { get; set; }
            public string title { get; set; }
        }

        public static int GENRE_IMAGE_W = 60;
        public static int GENRE_IMAGE_H = 60;

        public static int PHOTO_W = 54;
        public static int PHOTO_H = 81;

        public static int MAX_GENRES_COUNT = 5;

        public static List<string> GENRES_LIST = new List<string> 
        {"Боевик", "Приключение", "Анимационный", "Биография", "Комедия", "Криминал", "Детектив", "Катастрофа", "Драма",
         "Сказка", "Семейный", "Фэнтези", "Исторический", "Ужасы", "Детский", "Музыка", "Мистика", "Постапокалипсис",
         "Романтика", "Фантастика", "Спорт", "Триллер", "Военный", "Вестерн", "Новогодний" 
        };

        public static string CapitalizeWords(string words)
        {
            var wordList = words.Trim().Split(' ');
            var result = "";
            for (int i = 0; i < wordList.Length; ++i)
            {
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
            if(bytes == null)
            {
                return null;
            }

            using (var memoryStream = new System.IO.MemoryStream(bytes))
            {
                return new Bitmap(memoryStream);
            }
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
