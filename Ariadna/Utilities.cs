using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace Ariadna;

public static class Utilities
{
    public const string EmptyDots = ". . .";

    public enum EFormCloseReason
    {
        NONE = 0,
        SUCCESS,
    }

    public static readonly Dictionary<string, Bitmap> MovieGenres = new()
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
        {"Сказка", Properties.Resources.fairytale},
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
    public static readonly Dictionary<string, Bitmap> DocumentaryGenres = new()
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
    public static readonly Dictionary<string, Bitmap> LibraryGenres = new()
    {
        {"Languages", Properties.Resources.languages},
        {"Literature", Properties.Resources.literature},
        {"Programming", Properties.Resources.programming},
        {"Misc", Properties.Resources.misc},
    };
    public static readonly Dictionary<string, Bitmap> LibraryLanguagesGenres = new()
    {
        {"English", Properties.Resources.en_language},
        {"French", Properties.Resources.fr_language},
        {"German", Properties.Resources.de_language},
        {"Italian", Properties.Resources.it_language},
    };
    public static readonly Dictionary<string, Bitmap> LibraryLiteratureGenres = new()
    {
        {"Sci-Fi", Properties.Resources.scifi},
        {"Action", Properties.Resources.action},
        {"Fantasy", Properties.Resources.fantasy},
        {"Horror", Properties.Resources.horror},
        {"Adventure", Properties.Resources.adventure},
        {"Fairytale", Properties.Resources.fairytale},
        {"Child", Properties.Resources.kid},
    };
    public static readonly Dictionary<string, Bitmap> LibraryProgrammingGenres = new()
    {
        {"Testing", Properties.Resources.testing},
        {"Network", Properties.Resources.network},
        {"OOP", Properties.Resources.oop},
        {"Concurrency", Properties.Resources.concurrency},
        {"Cryptography", Properties.Resources.cryptography},
        {"Algorithm", Properties.Resources.algorithm},
        {"UML", Properties.Resources.uml},
        {"Qt", Properties.Resources.qt},
        {"Python", Properties.Resources.python},
        {"Project Management", Properties.Resources.pm},
        {"Linux", Properties.Resources.linux},
        {"Kanzi", Properties.Resources.kanzi},
        {"Java", Properties.Resources.java},
        {"HTML", Properties.Resources.html},
        {"Git", Properties.Resources.git},
        {"GameDev", Properties.Resources.gamedev},
        {"DevOps", Properties.Resources.devops},
        {"Database", Properties.Resources.database},
        {"CAN", Properties.Resources.can},
        {"C#", Properties.Resources.csharp},
        {"C++", Properties.Resources.cpp},
        {"BigData", Properties.Resources.bigdata},
        {"Assembler", Properties.Resources.assembler},
        {"Architecture", Properties.Resources.architecture},
        {"Android", Properties.Resources.android},
        {"Misc", Properties.Resources.misc},
        {"Rust", Properties.Resources.rust},
    };
    public static readonly Dictionary<string, Bitmap> LibraryMiscGenres = new()
    {
        {"Misc", Properties.Resources.misc},
    };
    public static readonly Dictionary<string, Bitmap> GameGenres = new()
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
        return MovieGenres.TryGetValue(name, out var value) ? value : Properties.Resources.No_Image;
    }
    public static string GetMovieGenreBySynonym(string name)
    {
        return name switch
        {
            "Мультфильм" or "Анимация" => "Анимационный",
            "Мелодрама" => "Драма",
            "Приключения" => "Приключение",
            _ => name
        };
    }
    public static Bitmap GetGameGenreImage(string name)
    {
        return GameGenres.TryGetValue(name, out var value) ? value : Properties.Resources.No_Image;
    }
    public static string GetGameGenreBySynonym(string name)
    {
        return name;
    }
    public static Bitmap GetDocumentaryGenreImage(string name)
    {
        return DocumentaryGenres.TryGetValue(name, out var value) ? value : Properties.Resources.No_Image;
    }
    public static Bitmap GetLibraryGenreImage(string name)
    {
        return LibraryGenres.TryGetValue(name, out var value) ? value : Properties.Resources.No_Image;
    }
    public static Bitmap GetLibraryLanguagesGenreImage(string name)
    {
        return LibraryLanguagesGenres.TryGetValue(name, out var value) ? value : GetLibraryGenreImage(name);
    }
    public static Bitmap GetLibraryLiteratureGenreImage(string name)
    {
        return LibraryLiteratureGenres.TryGetValue(name, out var value) ? value : GetLibraryGenreImage(name);
    }
    public static Bitmap GetLibraryProgrammingGenreImage(string name)
    {
        return LibraryProgrammingGenres.TryGetValue(name, out var value) ? value : GetLibraryGenreImage(name);
    }
    public static Bitmap GetLibraryMiscGenreImage(string name)
    {
        return LibraryMiscGenres.TryGetValue(name, out var value) ? value : GetLibraryGenreImage(name);
    }
    public static string GetDocumentaryGenreBySynonym(string name)
    {
        return name;
    }
    public static string GetLibraryGenreBySynonym(string name)
    {
        return name;
    }
    private static readonly byte[] Empty = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 54, 0, 0, 0, 81, 8, 6, 0, 0, 0, 153, 180, 85, 63, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11, 252, 97, 5, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1, 199, 111, 168, 100, 0, 0];
    public static bool IsValidPreview(byte[] bytes)
    {
        if (bytes == null)
        {
            return false;
        }

        // Workaround: compare first ~85 bytes of NO_PREVIEW_IMAGE_SMALL to determine if no preview was set
        const int noPreviewImageSmallLength = 1119;
        const int noPreviewImageSmallLength2 = 1150;
        if ((bytes.Length != noPreviewImageSmallLength) &&
            (bytes.Length != noPreviewImageSmallLength2))
        {
            return true;
        }

        return Empty.Where((t, i) => bytes[i] != t).Any();
    }
    public static bool GetBitmapFromDisk(out Bitmap outBmp, string filter, int width, int height)
    {
        outBmp = null;

        // ReSharper disable once UsingStatementResourceInitialization
        using var openFileDialog = new OpenFileDialog()
        {
            InitialDirectory = Properties.Settings.Default.BitmapInitialSearchDir,
            Filter = filter,
            FilterIndex = 1,
            RestoreDirectory = true,
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return false;
        }

        var fileName = openFileDialog.FileName;
        var extStartPos = fileName.LastIndexOf('.') + 1;
        var ext = string.Empty;
        if (extStartPos > 0)
        {
            ext = fileName[extStartPos..].ToUpper();
        }

        // Check supported extensions
        var extensions = new HashSet<string> { "BMP", "GIF", "EXIF", "JPG", "JPEG", "PNG", "TIFF", "WEBP" };
        if ((ext.Length > 0) && !extensions.Contains(ext))
        {
            return false;
        }

        outBmp = new Bitmap(width, height);
        var graph = Graphics.FromImage(outBmp);

        if (ext == "WEBP")
        {
            // Use a library like SkiaSharp to load .webp files
            using var skBitmap = SkiaSharp.SKBitmap.Decode(fileName);
            using var skImage = SkiaSharp.SKImage.FromBitmap(skBitmap);
            using var skData = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            using var stream = skData.AsStream();
            using var webpImage = new Bitmap(stream);
            graph.DrawImage(webpImage, new Rectangle(0, 0, width, height));
        }
        else
        {
            using var bmpTemp = new Bitmap(fileName);
            Image image = new Bitmap(bmpTemp);
            graph.DrawImage(image, new Rectangle(0, 0, width, height));
        }

        return true;
    }
    public static TimeSpan GetVideoDuration(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return TimeSpan.Zero;
        }

        using var shell = ShellObject.FromParsingName(filePath);
        IShellProperty prop = shell.Properties.System.Media.Duration;
        return prop.ValueAsObject == null ? TimeSpan.Zero : TimeSpan.FromTicks((long)(ulong)prop.ValueAsObject);
    }
    public static string DecorateDescription(string description)
    {
        var paragraphs = description.Split('\n');
        var decoratedText = string.Empty;
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