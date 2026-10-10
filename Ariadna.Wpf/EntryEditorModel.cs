using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;
using Ariadna.Storage;

namespace Ariadna.Wpf;
internal sealed class EntryEditorModel : ObservableObject
{
    private readonly CatalogActions actions;
    private readonly CatalogDetails? loaded;
    private readonly Dictionary<string, byte[]> changedImages = new();
    private string title;
    private string originalTitle;
    private string path;
    private string year;
    private string description;
    private string version;
    private bool wanted;
    private bool vr;
    private BitmapSource? poster;
    internal EntryEditorModel(CatalogActions actions, CatalogKind kind, string path)
    {
        this.actions = actions;
        Kind = kind;
        var id = actions.Store.FindId(kind, path);
        loaded = id < 0 ? null : actions.Store.GetDetails(kind, id);
        StoredId = id;
        title = loaded?.Entry.Title ?? System.IO.Path.GetFileNameWithoutExtension(path);
        originalTitle = loaded?.Entry.OriginalTitle ?? (kind == CatalogKind.Movie ? title : string.Empty);
        this.path = loaded?.Entry.Path ?? path;
        year = loaded?.Entry.Year > 0 ? loaded.Entry.Year.ToString(CultureInfo.InvariantCulture) : string.Empty;
        description = loaded?.Entry.Description ?? string.Empty;
        version = loaded?.Entry.Version ?? string.Empty;
        wanted = loaded?.Entry.Wanted ?? false;
        vr = loaded?.Entry.Vr ?? false;
        Genres = new(loaded?.Genres ?? []);
        People = new((loaded?.Directors ?? []).Select(person => new PersonEditorModel(person)));
        Actors = new((loaded?.Actors ?? []).Select(person => new PersonEditorModel(person)));
        poster = ReadImage(string.Empty);
        Previews = Enumerable.Range(1, 4).Select(index => ReadImage(actions.Configuration.PreviewSuffix(index))).ToArray();
        Genres.CollectionChanged += (_, _) => Revision++;
    }

    public CatalogKind Kind { get; }
    public CatalogTheme Theme => CatalogTheme.For(Kind);
    public bool IsMovie => Kind == CatalogKind.Movie;
    public bool IsGame => Kind == CatalogKind.Game;
    public bool HasDescription => !IsGame;
    public bool HasPeople => IsMovie || Kind == CatalogKind.Library;
    public string PeopleLabel => Kind == CatalogKind.Library ? "Authors" : "Directors";
    public string DisplayTitle
    {
        get => IsMovie ? OriginalTitle : Title;
        set
        {
            if (IsMovie)
            {
                OriginalTitle = value;
            }
            else
            {
                Title = value;
            }
        }
    }

    public string SecondaryTitle
    {
        get => IsMovie ? Title : OriginalTitle;
        set
        {
            if (IsMovie)
            {
                Title = value;
            }
            else
            {
                OriginalTitle = value;
            }
        }
    }
    public string Caption => StoredId < 0 ? $"Add {Theme.Caption} entry" : $"{Theme.Caption} — {Title}";

    public string Title
    {
        get => title;
        set
        {
            if (Set(ref title, value))
            {
                Notify(nameof(DisplayTitle));
                Notify(nameof(SecondaryTitle));
                Revision++;
            }
        }
    }

    public string OriginalTitle
    {
        get => originalTitle;
        set
        {
            if (Set(ref originalTitle, value))
            {
                Notify(nameof(DisplayTitle));
                Notify(nameof(SecondaryTitle));
                Revision++;
            }
        }
    }

    public string Path
    {
        get => path;
        set
        {
            if (Set(ref path, value))
            {
                Revision++;
            }
        }
    }

    public string Year
    {
        get => year;
        set
        {
            if (Set(ref year, value))
            {
                Revision++;
            }
        }
    }

    public string Description
    {
        get => description;
        set
        {
            if (Set(ref description, value))
            {
                Revision++;
            }
        }
    }

    public string Version { get => version; set => Set(ref version, value); }
    public bool Wanted { get => wanted; set => Set(ref wanted, value); }
    public bool Vr { get => vr; set => Set(ref vr, value); }
    public BitmapSource? Poster => poster;
    public ObservableCollection<string> Genres { get; }
    public ObservableCollection<PersonEditorModel> People { get; }
    public ObservableCollection<PersonEditorModel> Actors { get; }
    public IReadOnlyCollection<string> AvailableGenres => GenreCatalog.For(Kind).Concat(actions.Store.GetGenres(Kind)).Distinct().Order().ToArray();
    public BitmapSource? [] Previews { get; }
    internal int StoredId { get; private set; }
    internal int Revision { get; private set; }

    private BitmapSource? ReadImage(string suffix)
    {
        if (StoredId < 0)
        {
            return null;
        }

        var file = System.IO.Path.Combine(actions.Configuration.PosterRoot(Kind), $"{StoredId}{suffix}");
        return File.Exists(file) ? Images.Decode(File.ReadAllBytes(file), suffix.Length == 0 ? 400 : 603) : null;
    }

    internal void ReplaceImage(string suffix, byte[] bytes)
    {
        var preview = suffix.Length > 0;
        var width = actions.Configuration.GetInt(preview ? "PreviewWidth" : "PosterWidth", preview ? 603 : 400);
        var height = actions.Configuration.GetInt(preview ? "PreviewHeight" : "PosterHeight", preview ? 339 : 600);
        var png = Images.Resize(bytes, width, height);
        changedImages[suffix] = png;
        if (preview)
        {
            Previews[int.Parse(suffix[^1..], CultureInfo.InvariantCulture) - 1] = Images.Decode(png);
            Notify(nameof(Previews));
        }
        else
        {
            poster = Images.Decode(png);
            Notify(nameof(Poster));
        }

        Revision++;
    }

    internal void AddGenre(string name)
    {
        name = GenreCatalog.Normalize(Kind, name.Trim());
        if (name.Length > 0 && !Genres.Contains(name, StringComparer.OrdinalIgnoreCase) && Genres.Count < actions.Configuration.GetInt("MaxGenresCount", 5))
        {
            Genres.Add(name);
        }
    }

    internal void AddPlaceholderPerson(bool actors) => (actors ? Actors : People).Add(new PersonEditorModel(new PersonPhoto("New Entry", null)));

    internal void AddPeople(string text, bool actors)
    {
        var target = actors ? Actors : People;
        foreach (var value in text.Split(['\r', '\n', ';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var name = string.Join(" ", value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => char.ToUpper(word[0], CultureInfo.CurrentCulture) + word[1..]));
            if (name == "↓" || target.Any(person => person.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var person = actions.Store.FindPerson(name, actors, Kind == CatalogKind.Library) ?? new PersonPhoto(name, null);
            target.Add(new PersonEditorModel(person));
        }
    }

    internal CatalogDetails BuildDetails()
    {
        if ((string.IsNullOrWhiteSpace(Title) && (!IsMovie || string.IsNullOrWhiteSpace(OriginalTitle))) || string.IsNullOrWhiteSpace(Path))
        {
            throw new InvalidDataException("Title and media path are required.");
        }

        if (!string.IsNullOrEmpty(Year) && (!int.TryParse(Year, CultureInfo.InvariantCulture, out var parsed) || parsed < 0))
        {
            throw new InvalidDataException("Year must be a nonnegative number or empty.");
        }

        var entry = new CatalogEntry
        {
            Id = StoredId,
            Title = loaded != null && Title == loaded.Entry.Title ? loaded.Entry.Title : Title.Trim(),
            OriginalTitle = loaded != null && OriginalTitle == loaded.Entry.OriginalTitle ? loaded.Entry.OriginalTitle : OriginalTitle.Trim(),
            Path = loaded != null && Path == loaded.Entry.Path ? loaded.Entry.Path : Path.Trim(),
            Year = int.TryParse(Year, CultureInfo.InvariantCulture, out var value) ? value : 0,
            CreationDate = loaded != null ? loaded.Entry.CreationDate : DateOnly.FromDateTime(File.GetLastWriteTimeUtc(Path)),
            Wanted = PreserveFlag(loaded?.Entry.Wanted, Wanted),
            Description = loaded != null && Description == (loaded.Entry.Description ?? string.Empty) ? loaded.Entry.Description : Description,
            Version = loaded != null && Version == (loaded.Entry.Version ?? string.Empty) ? loaded.Entry.Version : Version.Trim(),
            Vr = PreserveFlag(loaded?.Entry.Vr, Vr),
        };
        return new CatalogDetails(entry, Genres.ToArray(), People.Select(person => person.ToPerson()).ToArray(), Actors.Select(person => person.ToPerson()).ToArray());
    }

    private bool? PreserveFlag(bool? previous, bool current) => loaded != null && previous.GetValueOrDefault() == current ? previous : current;
    internal int Save()
    {
        var details = BuildDetails();
        StoredId = actions.Store.Save(Kind, details, changedImages.Count == 0 ? null : new CatalogAssets(actions.Configuration.PosterRoot(Kind), changedImages, actions.Configuration.PreviewPrefix));
        return StoredId;
    }
}
