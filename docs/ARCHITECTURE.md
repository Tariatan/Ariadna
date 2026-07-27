# Ariadna

A personal media library management desktop application for Windows. Ariadna lets you browse, search, and organize a local collection of movies, documentaries, games, and books from a single window with poster thumbnails.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET-9.0-purple)
![Language](https://img.shields.io/badge/language-C%23-239120)

---

## Features

- **Poster thumbnail grid** — visual browsing of the full collection via a custom embedded ImageListView
- **Four media categories**, each with its own theme, color scheme, and icon:
  - Movies & TV Series
  - Documentaries
  - PC / VR Games
  - Books / Library
- **Filtering & search** — filter by title, director/author, actor, genre, subgenre, and flag-based toggles (wishlist, recently added, new releases, VR-only, series-only)
- **TMDb integration** — fetch movie metadata (title, year, poster, description, cast) from The Movie Database API
- **MediaInfo analysis** — read video resolution, bitrate, and audio track details from local files via MediaInfo
- **Quick-navigation bar** — alphabetical letter buttons for instant jump to any section of the list
- **Entry detail forms** — add, view, and edit full metadata for each entry including poster, genre tags, director/actor photos, and file path
- **File integration** — open video files in MPC-HC or browse directories in Total Commander directly from the UI
- **Ignore list** — shift-click to permanently skip a file during automatic discovery
- **Theming** — static theme system applied before UI construction; each category has a distinct palette

---

## Requirements

- **OS**: Windows 10 or later (x64)
- **Runtime**: .NET 9.0 (net9.0-windows8.0)
- **Database**: SQL Server LocalDB (ships with Visual Studio or can be installed separately via the SQL Server Express LocalDB installer)
- **External tools** (optional, paths configured in `App.config`):
  - [MPC-HC](https://github.com/clsid2/mpc-hc) — media player for opening video files
  - [Total Commander](https://www.ghisler.com/) — file manager for browsing entry directories
- **TMDb API key** — required for movie metadata lookup (set in `App.config`)

---

## Configuration

All paths and keys are set in `Ariadna/App.config`. Update these before building or running:

| Setting | Description |
|---|---|
| `connectionStrings` | Path to the `.mdf` database file (SQL Server LocalDB connection string) |
| `TmdbApiKey` | Your [TMDb API key](https://developer.themoviedb.org/docs/getting-started) |
| `MoviePostersRootPath` | Root directory where movie poster images are stored by entry ID |
| `GamePostersRootPath` | Root directory for game poster images |
| `DocumentaryPostersRootPath` | Root directory for documentary poster images |
| `LibraryPostersRootPath` | Root directory for library/book poster images |
| `DefaultMoviesPath` | Default directory scanned when discovering new movie files |
| `DefaultSeriesPath` | Default directory for TV series discovery |
| `DefaultGamesPath` | Default directory for game discovery |
| `DefaultDocumentariesPath` | Default directory for documentary discovery |
| `DefaultLibraryPath` | Default directory for book/document discovery |
| `TotalCommanderPath` | Full path to `TOTALCMD64.EXE` |
| `MediaPlayerPath` | Full path to `mpc-hc64.exe` |
| `PosterWidth` / `PosterHeight` | Poster image dimensions (default: 400×600) |
| `PreviewWidth` / `PreviewHeight` | Preview pane dimensions (default: 603×339) |
| `RecentInMonth` | How many months back counts as "recently added" (default: 3) |
| `VideoFilesFilter` | File extension filter for video file discovery (e.g. `*.mkv;*.mp4;*.avi`) |
| `ImdbLanguage` | Locale for TMDb API requests (e.g. `ru-RU`) |

Poster files are stored as `{id}.jpg` inside the configured root directory for each category.

---

## Building

Open `Ariadna.sln` in **JetBrains Rider** or **Visual Studio 2022+** and build normally, or use the .NET CLI:

```
dotnet build Ariadna.sln -c Release
```

The `DbProvider` project is an old-style (.NET Framework 4.8) project that provides the Entity Framework 6 data model. Its entity source files are linked directly into the main `Ariadna` project via `<Compile Include>` references, so no separate build step is required.

---

## Running

The application accepts a single command-line argument to select the media category:

```
Ariadna.exe movies          # Movies & TV series (default)
Ariadna.exe documentaries   # Documentaries
Ariadna.exe games           # PC / VR games
Ariadna.exe library         # Books & documents
```

Running without an argument defaults to the **movies** mode. Each invocation is an independent window — you can run all four simultaneously.

---

## Architecture

### Strategy Pattern

The core design uses the Strategy pattern to keep category-specific behavior separate from the shared UI:

```
AbstractDbStrategy (abstract contract)
└── MediaDbStrategyBase (abstract, adds logging, poster adaptor, file/process ops)
    ├── MoviesDbStrategy     — movies + TV series, TMDb API
    ├── DocumentariesDbStrategy
    └── GamesDbStrategy      — PC / VR games
LibraryDbStrategy            — books (implements AbstractDbStrategy directly)
```

`Program.cs` selects a strategy and theme pair based on the command-line argument, then passes the strategy to `MainPanel`. The `MainPanel` only talks to `AbstractDbStrategy` — it has no knowledge of which category is active.

Key strategy responsibilities:
- `GetEntries()` / `QueryEntries(QueryParams)` — data retrieval with filtering
- `ShowEntryDetails(int id)` — open the category-specific edit/detail dialog
- `ExecuteEntry(int id)` — open file in MPC-HC or directory in Total Commander
- `FindNextEntryAutomatically()` / `FindNextEntryManually()` — discover unregistered files on disk
- `RemoveEntry(int id)` — delete from DB and optionally from filesystem
- `FilterControls(MainPanel)` — show/hide toolbar controls irrelevant to the current category

### Data Layer

Entity Framework 6 Database-First targeting SQL Server LocalDB. The EDMX model lives in the `DbProvider` project and generates entity classes via T4 templates. The main entities are:

| Entity | Description |
|---|---|
| `Movie` | Movie / TV series entry with title, year, poster (byte[]), file path, description, cast, directors, genres |
| `Game` | PC or VR game entry |
| `Documentary` | Documentary entry |
| `Library` | Book / document entry with authors |
| `Actor` / `Director` / `Author` | People, with portrait photo stored as byte[] |
| `Genre` / `GameGenre` / `DocumentaryGenre` / `LibraryGenre` | Genre lookup tables |
| `Ignores` | Files permanently excluded from automatic discovery |

### Theming

`Theme` (abstract static class) holds static color fields used throughout the UI. Each category's concrete theme (`ThemeMovies`, `ThemeGames`, `ThemeDocumentaries`, `ThemeLibrary`) overrides `Init()` to populate these fields. `Theme.Init()` is called before the first window is constructed, so static colors are available during control initialization.

### Entry Detail Forms

`DetailsForm` (abstract base) provides shared detail/edit dialog behavior:
- Async directory size calculation with cancellation
- Async MediaInfo loading (resolution, bitrate, audio language detection)
- Genre picker via `FloatingPanel` overlay
- Director/actor photo management
- Templated DB save flow: `StorePreEntryData()` → `StoreMainEntry()` → `StorePostEntryData()` → `StoreRelatedData()`

Concrete subclasses: `MovieDetailsForm`, `GameDetailsForm`, `DocumentaryDetailsForm`, `LibraryDetailsForm`.

### Embedded ImageListView

The `Ariadna/ImageListView/` directory contains an embedded fork of the open-source [Manina Windows Forms ImageListView](https://github.com/oozcitak/imagelistview) control (license included). It provides:
- Background thumbnail extraction and caching
- Metadata extraction pipeline
- Custom renderer infrastructure
- Custom scrollbar

`ImageListViewAriadnaRenderer` extends the base renderer with a gradient background and an animated selection blink effect. `PosterFromFileAdaptor` is a custom `ImageListViewItemAdaptor` that loads poster images from the configured root directory using the entry ID as the filename.

---

## Project Structure

```
Ariadna.sln
├── Ariadna/                        # Main WinForms application (net9.0-windows)
│   ├── Program.cs                  # Entry point — selects strategy + theme by CLI arg
│   ├── MainPanel.cs/.Designer.cs   # Main application window
│   ├── Utilities.cs                # Genre dictionaries, image helpers, video duration
│   ├── App.config                  # Connection string and all application settings
│   ├── AuxiliaryPopups/            # Modal dialogs (DetailsForm, FloatingPanel, ChoicePopup)
│   ├── Data/                       # DTOs (EntryDto, EntryInfo, MovieChoiceDto)
│   ├── DatabaseStrategies/         # AbstractDbStrategy + 4 concrete strategies + helpers
│   ├── Extension/                  # Static extension methods
│   ├── ImageListHelpers/           # Custom renderer and poster adaptor
│   ├── ImageListView/              # Embedded fork of Manina ImageListView (~30 files)
│   ├── Properties/                 # App settings, resources
│   ├── Resources/                  # PNG/BMP/ICO assets, 100+ genre icons
│   ├── SplashScreen/               # SplashForm and Splasher helper
│   └── Themes/                     # Theme base + ThemeMovies/Games/Documentaries/Library
├── DbProvider/                     # EF6 Database-First data project (.NET Framework 4.8)
│   ├── AriadnaModel.edmx           # EF designer model
│   ├── AriadnaModel.Context.cs     # DbContext (AriadnaEntities)
│   └── *.cs / *.tt                 # T4-generated entity classes
└── Ariadna.Tests/                  # MSTest unit test project (net9.0-windows)
    ├── AuxiliaryPopups/
    ├── DatabaseStrategies/
    ├── ImageListHelpers/
    └── MainPanelTests.cs / UtilitiesTests.cs
```

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| EntityFramework | 6.5.1 | Database-First ORM (SQL Server LocalDB) |
| TMDbLib | 3.0.0 | The Movie Database REST API client |
| MediaInfo.Wrapper.Core | 26.1.0 | Video metadata (resolution, bitrate, audio) |
| Microsoft.WindowsAPICodePack-Shell | 1.1.0.0 | File duration via Windows Shell |
| SkiaSharp | 3.119.2 | Image processing |
| Microsoft.Extensions.Logging.Console | 10.0.5 | Console logging for strategies |
| MSTest.TestFramework / TestAdapter | 4.2.1 | Unit testing |

---

## Testing

Run the test suite with:

```
dotnet test Ariadna.Tests/Ariadna.Tests.csproj
```

Tests follow the `[MethodUnderTest]_[Precondition]_[ExpectedOutcome]` naming convention and use the `Arrange / Act / Assert` structure. See `AGENTS.md` for full coding and testing guidelines.

---

## Acknowledgements

- **[ImageListView](https://github.com/oozcitak/imagelistview)** by Ozgur Ozcitak — the thumbnail grid control embedded in `Ariadna/ImageListView/`. The source is included directly in the repository (with its license) and extended with a custom renderer and poster adaptor.