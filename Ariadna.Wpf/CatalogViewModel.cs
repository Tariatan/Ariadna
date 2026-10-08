using System.IO;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
internal sealed class CatalogViewModel(CatalogKind kind, CatalogStore store, CatalogConfiguration configuration, ThumbnailCache thumbnails, ILogger logger) : ObservableObject, IDisposable
{
    private CancellationTokenSource? refresh;
    private bool disposed;
    private bool busy;
    private string status = "Loading catalog…";
    private string title = string.Empty;
    private string person = string.Empty;
    private string actor = string.Empty;
    private string genre = string.Empty;
    private string subgenre = string.Empty;
    private bool wish;
    private bool recent;
    private bool newEntries;
    private bool vr;
    private bool nonVr;
    private bool series;
    private bool movies;
    private int columns = 5;
    private PosterItem? selected;
    private List<PosterItem> entries = [];
    private IReadOnlyList<PosterRow> rows = [];
    private IReadOnlyList<string> letters = [];
    private IReadOnlyCollection<string> genres = [];
    public CatalogKind Kind { get; } = kind;
    public CatalogTheme Theme { get; } = CatalogTheme.For(kind);
    public bool IsMovie => Kind == CatalogKind.Movie;
    public bool IsGame => Kind == CatalogKind.Game;
    public bool IsLibrary => Kind == CatalogKind.Library;
    public bool HasPeople => IsMovie || IsLibrary;
    public bool HasSubgenre => IsLibrary && !string.IsNullOrWhiteSpace(Genre);
    public string PersonLabel => IsLibrary ? "Authors" : "Directors";
    public IReadOnlyList<PosterItem> Entries => entries;
    public IReadOnlyList<PosterRow> Rows => rows;
    public IReadOnlyList<string> Letters => letters;
    public IReadOnlyCollection<string> Genres => genres;
    public IReadOnlyCollection<string> Subgenres => GenreCatalog.Subgenres(Genre);
    public bool Busy { get => busy; private set => Set(ref busy, value); }
    public string Status { get => status; private set => Set(ref status, value); }
    public int Columns => columns;

    public string Title
    {
        get => title;
        set
        {
            if (Set(ref title, value))
            {
                ScheduleRefresh();
            }
        }
    }

    public string Person
    {
        get => person;
        set
        {
            if (Set(ref person, value))
            {
                ScheduleRefresh();
            }
        }
    }

    public string Actor
    {
        get => actor;
        set
        {
            if (Set(ref actor, value))
            {
                ScheduleRefresh();
            }
        }
    }

    public string Genre
    {
        get => genre;
        set
        {
            if (Set(ref genre, value ?? string.Empty))
            {
                subgenre = string.Empty;
                Notify(nameof(Subgenre));
                Notify(nameof(Subgenres));
                Notify(nameof(HasSubgenre));
                ScheduleRefresh();
            }
        }
    }

    public string Subgenre
    {
        get => subgenre;
        set
        {
            if (Set(ref subgenre, value ?? string.Empty))
            {
                ScheduleRefresh();
            }
        }
    }

    public bool Wish
    {
        get => wish;
        set
        {
            if (Set(ref wish, value))
            {
                ScheduleRefresh();
            }
        }
    }

    public bool Recent
    {
        get => recent;
        set
        {
            if (Set(ref recent, value))
            {
                ScheduleRefresh();
            }
        }
    }

    public bool New
    {
        get => newEntries;
        set
        {
            if (Set(ref newEntries, value))
            {
                ScheduleRefresh();
            }
        }
    }

    public bool Vr
    {
        get => vr;
        set
        {
            if (Set(ref vr, value))
            {
                if (value)
                {
                    nonVr = false;
                    Notify(nameof(NonVr));
                }

                ScheduleRefresh();
            }
        }
    }

    public bool NonVr
    {
        get => nonVr;
        set
        {
            if (Set(ref nonVr, value))
            {
                if (value)
                {
                    vr = false;
                    Notify(nameof(Vr));
                }

                ScheduleRefresh();
            }
        }
    }

    public bool Series
    {
        get => series;
        set
        {
            if (Set(ref series, value))
            {
                if (value)
                {
                    movies = false;
                    Notify(nameof(Movies));
                }

                ScheduleRefresh();
            }
        }
    }

    public bool Movies
    {
        get => movies;
        set
        {
            if (Set(ref movies, value))
            {
                if (value)
                {
                    series = false;
                    Notify(nameof(Series));
                }

                ScheduleRefresh();
            }
        }
    }

    public PosterItem? Selected
    {
        get => selected;
        set
        {
            if (selected == value)
            {
                return;
            }

            if (selected != null)
            {
                selected.Selected = false;
            }

            selected = value;
            if (selected != null)
            {
                selected.Selected = true;
            }

            Notify();
        }
    }

    private async void ScheduleRefresh() => await RefreshAsync(CancellationToken.None, true);
    internal async Task RefreshAsync(CancellationToken cancellationToken, bool debounce = false, int? selectedEntryId = null)
    {
        if (disposed)
        {
            return;
        }

        refresh?.Cancel();
        using var request = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        refresh = request;
        var query = configuration.Query(new CatalogQuery
        {
            Name = Title,
            Director = Person,
            Actor = Actor,
            Genre = IsLibrary && !string.IsNullOrEmpty(Subgenre) ? Subgenre : Genre,
            Wish = Wish,
            Recent = Recent,
            New = New,
            Vr = Vr,
            NonVr = NonVr,
            Series = Series,
            Movies = Movies,
        });
        Busy = true;
        try
        {
            if (debounce)
            {
                await Task.Delay(180, request.Token);
            }

            var result = await Task.Run(() => (Entries: store.Query(Kind, query), Genres: store.GetGenres(Kind)), request.Token);
            request.Token.ThrowIfCancellationRequested();
            var selectedId = selectedEntryId ?? Selected?.Entry.Id;
            entries = result.Entries.Select(entry => new PosterItem(entry, Path.Combine(configuration.PosterRoot(Kind), entry.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)), thumbnails)).ToList();
            genres = GenreCatalog.FilterGenres(Kind, result.Genres).Distinct(StringComparer.OrdinalIgnoreCase).Order().Prepend(string.Empty).ToArray();
            letters = entries.Where(entry => entry.Entry.Title.Length > 0).Select(entry => entry.Entry.Title[..1].ToUpperInvariant()).Where(letter => letter is not ("}" or "«") && (!IsLibrary || letter is not ("(" or "9"))).Distinct().ToArray();
            BuildRows();
            Selected = entries.FirstOrDefault(entry => entry.Entry.Id == selectedId) ?? (selectedId == null && entries.Count > 0 ? entries[Random.Shared.Next(entries.Count)] : entries.FirstOrDefault());
            Status = $"{entries.Count:N0} entries";
            Notify(nameof(Entries));
            Notify(nameof(Genres));
            Notify(nameof(Letters));
        }
        catch (OperationCanceledException)when (request.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (!request.IsCancellationRequested)
            {
                logger.LogError("Catalog query failed for '{Collection}', error type '{ErrorType}'", Kind, exception.GetType().Name);
                Status = "Could not load catalog. Use Refresh to retry.";
            }
        }
        finally
        {
            if (refresh == request)
            {
                refresh = null;
                Busy = false;
            }
        }
    }

    internal void SetColumns(int value)
    {
        value = Math.Max(1, value);
        if (columns != value)
        {
            columns = value;
            Notify(nameof(Columns));
            BuildRows();
        }
    }

    private void BuildRows()
    {
        rows = entries.Chunk(columns).Select(items => new PosterRow(items)).ToArray();
        Notify(nameof(Rows));
    }

    internal void Move(int delta)
    {
        var index = Selected == null ? 0 : entries.IndexOf(Selected);
        if (entries.Count > 0)
        {
            Selected = entries[Math.Clamp(index + delta, 0, entries.Count - 1)];
        }
    }

    internal void Jump(string letter) => Selected = entries.FirstOrDefault(entry => entry.Entry.Title.StartsWith(letter, StringComparison.OrdinalIgnoreCase)) ?? Selected;
    internal void Randomize()
    {
        if (entries.Count > 0)
        {
            Selected = entries[Random.Shared.Next(entries.Count)];
        }
    }

    public void Dispose()
    {
        disposed = true;
        refresh?.Cancel();
    }
}
