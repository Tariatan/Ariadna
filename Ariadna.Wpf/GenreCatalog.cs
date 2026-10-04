using Ariadna.Storage;

namespace Ariadna.Wpf;
internal static class GenreCatalog
{
    private static readonly string[] MovieGenres = ["Боевик", "Приключение", "Анимационный", "Биография", "Комедия", "Криминал", "Детектив", "Катастрофа", "Драма", "Сказка", "Семейный", "Фэнтези", "Исторический", "Ужасы", "Детский", "Музыка", "Мистика", "Постапокалипсис", "Романтика", "Фантастика", "Спорт", "Триллер", "Военный", "Вестерн", "Новогодний"];
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
    internal static IReadOnlyCollection<string> Subgenres(string genre)
    {
        var values = genre.Contains("English", StringComparison.InvariantCultureIgnoreCase) ? LibraryLanguagesGenres : genre.Contains("Literature", StringComparison.InvariantCultureIgnoreCase) ? LibraryLiteratureGenres : genre.Contains("Programming", StringComparison.InvariantCultureIgnoreCase) ? LibraryProgrammingGenres : LibraryMiscGenres;
        return values.Prepend(string.Empty).ToArray();
    }

    internal static string Normalize(CatalogKind kind, string name) => kind == CatalogKind.Movie ? name switch
    {
        "Мультфильм" or "Анимация" => "Анимационный",
        "Мелодрама" => "Драма",
        "Приключения" => "Приключение",
        _ => name,
    } : name;
}
