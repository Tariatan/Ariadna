using Ariadna.Storage;

namespace Ariadna.Wpf;
internal static class GenreCatalog
{
    private static readonly string[] MovieGenres = ["Action", "Adventure", "Animation", "Biography", "Comedy", "Crime", "Detective", "Disaster", "Drama", "Fairytale", "Family", "Fantasy", "Historical", "Horror", "Child", "Music", "Mysticism", "Postapocalypse", "Romance", "Sci-Fi", "Sport", "Thriller", "War", "Western", "New Year"];
    private static readonly string[] GameGenres = ["Adventure", "Fighting", "Action", "Quest", "Platformer", "Postapocalypse", "RPG", "Turn-Based", "Simulator", "Strategy", "Tower Defense", "Racing", "Sport", "Sci-Fi", "Fantasy", "FPS", "3rd View", "Isometric", "Horror", "City Building", "Music", "Arcade", "Meditate"];
    private static readonly string[] DocumentaryGenres = ["Travel", "Art. History", "Universe", "Stars. Planets", "Astronautics", "Personality", "Science", "Misc"];
    private static readonly string[] LibraryGenres = ["Languages", "Literature", "Programming", "Misc"];
    private static readonly string[] LibraryLanguagesGenres = ["English", "French", "German", "Italian"];
    private static readonly string[] LibraryLiteratureGenres = ["Sci-Fi", "Action", "Fantasy", "Horror", "Adventure", "Fairytale", "Child"];
    private static readonly string[] LibraryProgrammingGenres = ["Testing", "Network", "OOP", "Concurrency", "Cryptography", "Algorithm", "UML", "Qt", "Python", "Project Management", "Linux", "Kanzi", "Java", "HTML", "Git", "GameDev", "DevOps", "Database", "CAN", "C#", "C++", "BigData", "Assembler", "Architecture", "Android", "Misc", "Rust", "Ai"];
    private static readonly string[] LibraryMiscGenres = ["Misc"];
    internal static IReadOnlyCollection<string> For(CatalogKind kind) => kind switch
    {
        CatalogKind.Movie => MovieGenres,
        CatalogKind.Game => GameGenres,
        CatalogKind.Documentary => DocumentaryGenres,
        CatalogKind.Library => LibraryGenres.Concat(LibraryLanguagesGenres).Concat(LibraryLiteratureGenres).Concat(LibraryProgrammingGenres).Concat(LibraryMiscGenres).Distinct().ToArray(),
        _ => [],
    };
    internal static IEnumerable<string> FilterGenres(CatalogKind kind, IEnumerable<string> storedGenres) =>
        kind == CatalogKind.Library ? LibraryGenres : For(kind).Concat(storedGenres);

    internal static IReadOnlyCollection<string> Subgenres(string genre)
    {
        var values = genre.Contains("Languages", StringComparison.InvariantCultureIgnoreCase) ? LibraryLanguagesGenres : genre.Contains("Literature", StringComparison.InvariantCultureIgnoreCase) ? LibraryLiteratureGenres : genre.Contains("Programming", StringComparison.InvariantCultureIgnoreCase) ? LibraryProgrammingGenres : LibraryMiscGenres;
        return values.Prepend(string.Empty).ToArray();
    }

    private static readonly Dictionary<string, string> MovieGenreTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Боевик"] = "Action",
        ["Приключение"] = "Adventure",
        ["Анимационный"] = "Animation",
        ["Биография"] = "Biography",
        ["Комедия"] = "Comedy",
        ["Криминал"] = "Crime",
        ["Детектив"] = "Detective",
        ["Катастрофа"] = "Disaster",
        ["Драма"] = "Drama",
        ["Сказка"] = "Fairytale",
        ["Семейный"] = "Family",
        ["Фэнтези"] = "Fantasy",
        ["Исторический"] = "Historical",
        ["Ужасы"] = "Horror",
        ["Детский"] = "Child",
        ["Музыка"] = "Music",
        ["Мистика"] = "Mysticism",
        ["Постапокалипсис"] = "Postapocalypse",
        ["Романтика"] = "Romance",
        ["Фантастика"] = "Sci-Fi",
        ["Спорт"] = "Sport",
        ["Триллер"] = "Thriller",
        ["Военный"] = "War",
        ["Вестерн"] = "Western",
        ["Новогодний"] = "New Year",
        ["Мультфильм"] = "Animation",
        ["Анимация"] = "Animation",
        ["Мелодрама"] = "Drama",
        ["Приключения"] = "Adventure",
    };

    internal static string Normalize(CatalogKind kind, string name) =>
        kind == CatalogKind.Movie && MovieGenreTranslations.TryGetValue(name, out var translated) ? translated : name;
}
