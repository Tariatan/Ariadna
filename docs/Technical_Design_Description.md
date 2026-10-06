# Technical Design Description - Ariadna <!-- omit from toc -->

Last reviewed: 2026-10-05. This describes the current implementation unless a
section explicitly labels a proposal or an unverified target.

## Purpose

Ariadna is a personal media library management desktop application for Windows.
It lets the owner browse, search, and organize movies, documentaries, games, and
books in a poster grid. One application instance hosts four permanent catalog tabs.

This is the primary reference for developers and agents implementing or reviewing
Ariadna. It owns product behavior, architecture, and technical decisions; consult
source code for implementation details. Task status belongs in PLAN.md and dated
handoff evidence belongs in repository MEMORY.md.

## Scope

This document covers all eight projects in `Ariadna.slnx`:

- **Ariadna** — WinForms desktop application, targeting `net10.0-windows8.0`
- **Ariadna.Storage** — direct SQLite storage, targeting `net10.0`
- **Ariadna.Storage.Tests** — real-provider integration tests, targeting `net10.0`
- **Ariadna.Tests** — MSTest characterization/unit tests, targeting `net10.0-windows8.0`
- **Ariadna.Wpf** — native WPF frontend for acceptance testing, targeting `net10.0-windows8.0`
- **Ariadna.Wpf.Tests** — WPF/real-provider workflow and virtualization tests
- **Ariadna.Migration** — standalone migration/recovery CLI, targeting `net10.0`
- **Ariadna.Migration.Tests** — synthetic import/compare/snapshot tests, targeting `net10.0`

The production backend is SQLite. The migration CLI and its tests participate
in solution-wide builds/tests; the desktop does not reference the CLI. Only SQL
export/comparison needs a SQL Server client. The desktop has no EF or SQL Server
dependency.

## References

| Reference | Purpose |
|---|---|
| [README.md](../README.md) | Requirements and build/test/run guidance, with verification limits |
| [AGENTS.md](../AGENTS.md) | Operational instructions, editing rules, testing and review conventions |
| [MEMORY.md](../MEMORY.md) | Compact dated handoff and evidence |
| [PLAN.md](PLAN.md) | Remaining/upcoming tasks and completion gates |
| [SQLite operation and recovery](SQLITE-OPERATIONS.md) | Implemented storage format, migration evidence, and backup/restore |
| [SQLite migration investigation](SQLITE-MIGRATION-PLAN.md) | Original rationale and dated source-data investigation |
| [WPF migration](WPF-MIGRATION.md) | Native frontend implementation, compatibility, launch and acceptance |

## Abbreviations and Definitions

| Term | Meaning |
|---|---|
| TDD | Technical Design Description |
| EF6 | Entity Framework 6 |
| EDMX / T4 | Database-first model and entity/context generation templates |
| Collection mode | One of movies, documentaries, games, or library, selected by tab |
| Catalog assets | External posters/game previews and database-resident person photos |
| Matched recovery | Database, external images, and relevant configuration captured consistently |

## Unit Design Notes / Decisions

| Decision / current constraint | Reason and consequence |
|---|---|
| Windows WinForms desktop | Existing local media workflow; preserve native interactions and file/tool integration |
| One instance with four permanent tabs | Each retained catalog control owns its strategy, filters, grid state, and palette; subsequent launches activate the existing window |
| Category-specific strategies and detail forms | Collection fields, filters, discovery, and launch behavior differ; retain these differences |
| Direct SQLite commands in Ariadna.Storage | Explicit mapping, focused queries, complete-entry transactions, no server or ORM |
| External posters keyed by catalog ID | IDs connect rows to extensionless PNG image files; changing IDs or filenames breaks lookup |
| Embedded ImageListView fork | Existing custom grid/rendering/cache behavior; preserve its license and lifecycle handling |
| Recoverable image promotion | Durable staging journals and database commit tokens support startup recovery |

## Overview

`Program` runs `CatalogApplication`, which enforces one instance through the
WinForms application model and opens `MainWindow`. The shell hosts Movies,
Games, Library, and Documentaries as permanent tabs in that order. The initial
catalog loads under the splash screen. Startup waits for the splash to render
before catalog recovery; the splash remains through initial catalog initialization
and closes only after the main window is shown and rendered. Its lifetime follows
readiness, not a fixed display delay. Failed/canceled startup and early window
closure also dispose it. After the window is shown, the remaining
catalog reads run on worker threads; their `MainPanel` controls, strategies,
palettes, quick navigation, and initial random selections are prepared on the UI
thread and retained for the window lifetime. Existing thumbnail workers warm
only each hidden view's initial viewport. Strategies retrieve filtered entries and coordinate
discovery, details, and execution. Detail forms save catalog data and related
images; thumbnail workers render cached posters.

### Context Map

```mermaid
flowchart LR
    Desktop[CatalogApplication / MainWindow] --> Views[Four retained MainPanel controls]
    Views --> Strategies[Collection strategies]
    Strategies --> Forms[Detail forms]
    Strategies --> Store[Ariadna.Storage]
    Forms --> Store
    Store --> DB[(SQLite catalog)]
    Store --> Assets[External posters and previews]
    Views --> Grid[ImageListView / thumbnail workers]
    Grid --> Assets
    Strategies --> TMDb[TMDb]
    Strategies --> Tools[MediaInfo / player / file manager]
```

### Considerations for a Secure Design

Catalog records, file paths, external images, and the TMDb key are user data.
Keep credentials and personal content out of committed fixtures and diagnostics.
The configured API key is currently an application setting; this document does
not claim encrypted credential storage. TMDb metadata retrieval uses network
access; browsing local data depends on the configured database and image paths.

Database and external files are separate persistence resources. Complete-entry
saves use a SQLite transaction plus recoverable image promotion. Snapshot backups
pause shared writers and copy both resources; see the operations guide.

### Considerations for Localization

The application uses resources and collection-specific controls. `ImdbLanguage`
selects TMDb request language, not a verified runtime UI-language picker.
Unicode text is retained. A registered en-US case-insensitive collation and
literal Unicode search functions support Cyrillic searches. Alphabetical ordering
is close to, but does not exactly match, the old SQL Server collation; measured
differences are recorded in the operations guide.

## Features

- **Poster thumbnail grid** — visual browsing of the full collection via a custom embedded ImageListView
- **Four media categories**, each with its own theme, color scheme, and icon:
  - Movies & TV Series
  - Documentaries
  - PC / VR Games
  - Books / Library
- **Filtering & search** — filter by title, director/author, actor, genre, subgenre, and flag-based toggles (wishlist, recently added, new releases, VR-only, series-only)
  The Library Authors field retains the same adjacent X button as Movies Director;
  clearing it refreshes results while preserving the other active filters.
  Filter refreshes prepare the new entries before replacing the grid, suspend
  grid painting through the navigation update, and resume with the completed
  result set. Quick navigation reuses letter buttons, skips unchanged letters,
  and batches changed letters into one layout; its shared font is owned by the view.
- **TMDb integration** — fetch movie metadata (title, year, poster, description, cast) from The Movie Database API
- **MediaInfo analysis** — read video resolution, bitrate, and audio track details from local files via MediaInfo
- **Quick-navigation bar** — alphabetical letter buttons for instant jump to any section of the list
- **Entry detail forms** — add, view, and edit full metadata for each entry including poster, genre tags, director/actor photos, and file path
- **File integration** — open video files in MPC-HC or browse directories in Total Commander directly from the UI
- **Ignore list** — shift-click to permanently skip a file during automatic discovery
- **Permanent catalog tabs** — one instance, independent retained view state, and Ctrl+Tab / Ctrl+Shift+Tab / Ctrl+1 through Ctrl+4 navigation
- **Theming** — instance palettes keep each catalog grid, picker, and detail form independent

---

## Requirements

- **OS**: Windows 10 or later (x64)
- **Runtime**: .NET 10.0 (net10.0-windows8.0); self-contained publishing includes it
- **Database**: a verified SQLite catalog plus matching external image directories
- **External tools** (optional, paths configured in `App.config`):
  - [MPC-HC](https://github.com/clsid2/mpc-hc) — media player for opening video files
  - [Total Commander](https://www.ghisler.com/) — file manager for browsing entry directories
- **TMDb API key** — required for movie metadata lookup (set in `App.config`)

---

## Configuration

All paths and keys are set in `Ariadna/App.config`. Update these before building or running:

| Setting | Description |
|---|---|
| `AriadnaCatalog` connection string | Absolute SQLite filename; normal startup uses ReadWrite and validates application/schema identity |
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

Poster files are PNG images stored under extensionless `{id}` filenames inside
the configured root for each category. Game previews append `PreviewSuffix` and
numbers 1 through 4 (currently `_preview1` through `_preview4`). Person photos
are stored as byte arrays in the database. These identifiers and filenames are
part of the persistence contract.

Strategies/adaptors concatenate configured root paths with IDs; preserve the
required path separators. Typed settings are generated from
`Ariadna/Properties/Settings.settings`; runtime values are read through
`Settings.Default` and configuration. Do not publish actual credential values.

---

## Building

Use [README.md](../README.md) for build/test guidance and its verification limits.
All production projects are SDK-style and use locked NuGet dependencies. The
legacy DbProvider project, generated/linked models, EF packages, and EDMX/T4
build tooling have been removed. `Ariadna.slnx` includes all eight projects and retains
the Debug/Release and Any CPU/x64/x86 solution configurations. `global.json` pins
SDK 10.0.401 with `latestPatch` roll-forward within 10.0.4xx and excludes previews.
Framework-dependent runs and consumers of the storage library now require .NET 10;
the schema and matched database/image recovery format are unchanged.

---

## Running

The application accepts an optional command-line argument to select a catalog tab:

```
Ariadna.exe movies          # Movies & TV series (default)
Ariadna.exe documentaries   # Documentaries
Ariadna.exe games           # PC / VR games
Ariadna.exe library         # Books & documents
```

The first launch without an argument opens **Movies**. A subsequent launch
activates the existing window; a named argument selects its tab, while no
argument preserves the active tab. A minimized window is restored. If a modal
editor is open, a requested switch waits until it closes.

The four tabs cannot be closed. Ctrl+Tab and Ctrl+Shift+Tab cycle with wrapping;
Ctrl+1 through Ctrl+4 select Movies, Games, Library, and Documentaries. Filters,
selection, scroll position, and palette persist when switching tabs. A pending
title search is applied before hiding its tab; transient pickers close. Closing
the main window disposes every loaded catalog view and its thumbnail resources.

---

## Architecture

### WPF frontend

The approved WPF migration is implemented as a separate native frontend in
`Ariadna.Wpf`, alongside the retained WinForms application for acceptance testing.
The user designated Ariadna.Wpf as the default development target on 2026-10-04;
new frontend work, builds, runs, and tests focus on WPF unless requested otherwise.
WPF starts without a splash window. Recovery and the initial catalog query finish
before the main window is shown; remaining catalogs preload after its first render.
Startup errors retain the existing error dialog and unsuccessful exit code.
The tabs row reserves space at its far right for the main application's assembly
version (major.minor.patch), shown in semi-transparent black text. The label stays
visible across catalog switches and does not participate in keyboard or mouse input.
Main-window geometry is saved on accepted Closing and restored on the next launch
from the local application-data file `Ariadna/Wpf/window.json`. Normal restore
bounds are retained when closing minimized or maximized; the existing maximized
state is also persisted. Canceled closure and unshown windows leave saved geometry
unchanged. WPF tests initialize application resources without production startup.
Startup and catalog tab activation focus the poster items list after restoring
the view state, rather than the Add entry button. Existing selection and scroll
positions are retained; unloaded views cannot receive deferred activation focus.
QuickList uses two columns in an 86-DIP sidebar on the active catalog background, with
24-DIP-high square buttons, thin white outlines and bold white letters.
Hover, keyboard focus and pressing provide a translucent-white highlight.
Letters read left to right, then downward. Button rows stay compact at the top,
leaving the catalog background below them. The sidebar scrolls independently
when letters exceed the available height;
letter clicks and keyboard activation retain the existing jump behavior.
Poster titles reserve 36 DIPs for two 18-DIP lines, wrapping with an ellipsis
when text exceeds two lines. Short titles remain centered in the same caption
area; poster height stays 380 DIPs and each card's total height is 438 DIPs.
Poster cards use square, one-DIP gray outlines and a vertical highlight gradient
from translucent white to dark gray. Hover is subtler than selection. Selecting
a card flashes its outline white three times over 420 ms, then leaves it gray;
deselection removes the animation. Images, captions and card dimensions remain
steady throughout highlighting.
The poster grid uses WPF item scrolling, with each virtualized item representing
one poster row. Startup selection, navigation, scrollbar movement and restored
tab positions keep the first visible row aligned to the viewport top; only the
bottom row may be partial. Each wheel notch moves one row, accumulating smaller
deltas and retaining selection. Resize anchoring preserves the first visible
entry's new row. Page Up/Page Down uses the rendered row height to determine how
many entries to move; row recycling and the final row's reachability are retained.
The poster grid's vertical scrollbar has a narrow catalog-colored track and a
rounded white thumb without arrow buttons. Thumb size follows the viewport,
with a minimum visible height of 50 DIPs. The thumb moves continuously under
the mouse and posters scroll live during dragging, with a whole top row.
The thumb retains its fractional drag position independently of WPF's rounded
content offset, then restores normal offset binding on release or unloading.
Track clicks retain immediate
page scrolling, and wheel/keyboard scrolling remains immediate. Its styles are
scoped to the poster list so editor and filter dropdown scrollbars are unaffected.
The shared filter panel retains its layout while showing only applicable fields:
Title, Wishlist, Recent, New and Genre in all catalogs; Series/Movies and
Directors/Actors in Movies; VR/Non-VR in Games; Authors in Library.
Library alone reveals the Subgenre label, selector and clear button after any
nonblank Genre is chosen. Changing Genre resets Subgenre; clearing Genre also
collapses that group. Filters and conditional visibility persist across tabs.
Each catalog's filter panel ends with a bold "Found entries: <count>" indicator,
bound to its displayed entries and updated after completed filter refreshes.
The compact filter toolbar uses the catalog palette, white separators and clear
icons, tinted borderless fields and outlined checkboxes after their labels.
Each field and its clear button wrap as one group when space is limited. Scoped
XAML styles preserve editable themed suggestion menus and visible keyboard focus
without changing editor controls. The plus button invokes existing discovery;
its context menu offers manual file and folder selection. Typing `+` while browsing
the catalog invokes the same Add entry button, including its disabled-state guard.
Plus characters in editable fields and Control/Alt/Windows combinations are not
treated as the shortcut.
Library's Genre dropdown contains the categories Languages, Literature,
Programming and Misc. Its Subgenre dropdown contains subjects for the selected
category (for example, Programming -> C++); stored subject tags are not added to
the category dropdown. The details editor retains the combined category/subject
tag list and custom stored tags. Storage and existing genre-query semantics are
unchanged.
The WPF Details editor reserves the left column for a large, uncropped poster.
Double-clicking the poster opens its image picker; there are no buttons below it.
The right column scrolls independently and contains themed metadata fields,
WinForms genre pictograms, description and portrait cards with name labels.
Directors and Cast remain separate for Movies; Library uses a full-width Authors
section. Details is owned by the main window and has no separate taskbar entry.
Each people section has a header `↓` button to paste clipboard names, normalize
word initials as in WinForms, skip duplicates and reuse stored portraits.
Each people header orders the buttons `↓`, then `+`. The `+` button adds a selected placeholder
named New Entry. Repeated clicks add separate cards; existing storage rules
consolidate identical names on save. People have no text fields or toolbars;
photo double-click and Ctrl+V remain available, and Delete removes a selected card.
Games retain VR, version and four preview slots; Documentaries omit people.
The Genres header has right-aligned clipboard `↓` and picker `...` buttons.
The old Edit genres section is removed. The floating icon picker opens beneath
selected genres, excludes already selected tags and confirms on double-click or
Enter. Escape or clicking outside dismisses it. Delete removes a selected genre;
Ctrl+V pastes names. Genre editing retains stored custom tags, clipboard input
and existing limits.
The fixed footer presents the editable path with a file/folder picker, separate
size, duration, resolution and bitrate icons, audio language flags with name
tooltips,
and Wishlist followed by Cancel and Save in a group anchored to the footer's
right edge. Media information stays left-aligned and wraps within its available
space without overlapping the action group. Details
scrollbars share the main window's rounded white thumb and unobtrusive track,
with normal continuous pixel scrolling. Media inspection runs asynchronously;
stale path results and results after close are discarded. Non-video entries show size only. Unknown
languages retain their names in tooltips with a generic icon. Bitrate converts
bits per second to whole-number Mbps. Footer groups wrap when space
is limited. Holding either Shift key changes Save to Ignore with gold text.
Shift release or window deactivation restores Save; confirming Ignore registers
the path without saving entry changes. Shift+Enter uses the same ignore workflow.
The manual TMDb button is removed; automatic lookup for newly discovered Movies
with a configured API key remains. Transactional saves retain existing metadata,
relationships and image behavior.
It uses the same direct SQLite storage, schema, IDs and image files. XAML and
observable models replace the shell, browser and editor controls; no WinForms
controls are hosted. Row virtualization, asynchronous bounded thumbnail caching,
filter cancellation, retained tabs, native editors and application lifetime are
described in [WPF migration](WPF-MIGRATION.md). WinForms architecture below remains
the current fallback; production cutover has not been accepted.

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

`MainWindow` creates a strategy and theme pair for each tab, then passes both
to its `MainPanel` user control. The main collection operations use
`AbstractDbStrategy`. Strategies and detail forms call focused `CatalogStore`
operations; SQL and mapping belong to the storage project.

The designer owns exactly four named tab pages. `MainWindow` maps their control
references to `CatalogKind` in runtime code; tab `Tag` values and displayed
captions do not identify catalogs. Designer saves therefore cannot remove a
required identity or leave a second set of runtime-created pages. Keyboard
shortcuts still follow the visible tab order.

Background tab preparation captures the database and default query configuration
before dispatching a read. Prepared entries seed the hidden page, so its first
activation does not repeat the initial query or random selection. UI controls
are constructed and attached only on the owning UI thread. Movies' filters,
focus, selection, scroll, and palette are retained while other pages load; the
same behavior applies when a legacy argument selects a different initial tab.
Ctrl+1 through Ctrl+4 follow the visible Movies/Games/Library/Documentaries order.

Selecting a pending tab displays `Loading catalog...` and joins its existing
preload rather than creating another page. Preload failures are logged, other
pages continue loading, and selecting the failed tab again retries normal
initialization. Accepted window closure cancels pending work and discards late
results; a synchronous SQLite read already running finishes and releases its
connection before its canceled result is discarded. Loaded controls own and
dispose their existing thumbnail workers, timers, and pickers.

Key strategy responsibilities:
- `GetEntries()` / `QueryEntries(QueryParams)` — data retrieval with filtering
- `ShowEntryDetails(int id)` — open the category-specific edit/detail dialog
- `ExecuteEntry(int id)` — open file in MPC-HC or directory in Total Commander
- `FindNextEntryAutomatically()` / `FindNextEntryManually()` — discover unregistered files on disk
- `RemoveEntry(int id)` — delete from DB and optionally from filesystem
- `FilterControls(MainPanel)` — show/hide toolbar controls irrelevant to the current category

### Data Layer

Direct `Microsoft.Data.Sqlite` queries with explicit scalar/BLOB mapping. The
original 19 data tables are retained, plus internal image commit metadata:

| Entity | Description |
|---|---|
| `Movie` | Movie / TV series entry with title, year, poster (byte[]), file path, description, cast, directors, genres |
| `Game` | PC or VR game entry |
| `Documentary` | Documentary entry |
| `Library` | Book / document entry with authors |
| `Actor` / `Director` / `Author` | People, with portrait photo stored as byte[] |
| `Genre` / `GenreOfGame` / `GenreOfDocumentary` / `GenreOfLibrary` | Genre lookup entities |
| `MovieCast` / `MovieDirector` / `MovieGenre` / `GameGenre` / `DocumentaryGenre` / `LibraryAuthor` / `LibraryGenre` | Collection-to-person/genre relationship entities |
| `Ignore` (`Ignores` context set) | Files permanently excluded from automatic discovery |

`CatalogStore` owns queries, people lookups, discovery/ignore paths, complete-entry
saves, and relationship-aware deletes. `CatalogDatabase` owns validated connections,
backup, integrity checks, writer coordination, and image-journal recovery. Lists and
entry information avoid loading photos; details and suggestions request them.
Recovery clears the read-only attribute on its own staging directories before
cleanup so empty leftovers do not block startup or later writes. Commit/rollback
decisions are unchanged; other filesystem access failures still propagate.

Complete-entry saves compare loaded genre/people sequences before replacing
relationships. Unchanged sequences, including legacy duplicates and photo bytes,
retain their existing relationship rows and IDs. Explicit relationship edits
retain the previous replacement/normalization behavior. The schema is unchanged.

### Theming

`Theme` is an abstract instance palette created by `Theme.Create(CatalogKind)`.
Each concrete theme (`ThemeMovies`, `ThemeGames`, `ThemeDocumentaries`,
`ThemeLibrary`) initializes its own colors. Catalog controls and grid renderers
retain their palette; scrollbars and floating pickers receive those colors.
Independent detail forms explicitly apply their collection palette. Switching
tabs cannot change colors in an already loaded view. The shared splash screen
uses a fixed branding color.

`CatalogTabControl` paints catalog-colored headers inside the native tab frame.
The selected header's background is inset by four logical pixels at the top and
sides so selecting Movies preserves the upper-left rim. Its bottom remains
joined to the page; caption alignment uses the original drawing bounds.
The unused header area to the right of the tabs is green. The control paints
that strip after native painting, including client printing, while leaving the
tab headers and page frame to their existing renderers.

### Display scaling

The existing layouts and embedded poster grid use pixel-based sizes. Runtime
startup explicitly uses `HighDpiMode.DpiUnaware`, preserving Windows bitmap
scaling and the pre-tab-refactor UI size on high-DPI displays. The application
model otherwise defaults to SystemAware, which changes those existing metrics.
`ApplicationHighDpiMode` records the same policy in the desktop project; the
custom application model explicitly sets its runtime property before startup.

`ForceDesignerDpiUnaware` runs WinForms designers at a 96-DPI baseline even when
Visual Studio is on a 150% display. This prevents monitor-dependent layout
serialization and does not itself configure runtime scaling. Native per-monitor
DPI rendering would require a separate conversion of custom pixel-based controls.

### Entry Detail Forms

`MovieDetailsForm`, `GameDetailsForm`, `DocumentaryDetailsForm`, and
`LibraryDetailsForm` each inherit directly from WinForms `Form`, with their own
designer and resources. Each owns its layout, loading, validation, and explicit
mapping to `CatalogDetails`. There is no shared detail-form base or save-hook chain.
The small `IEntryDetailsDialog` contract exposes the stored ID and close reason.

Shared behavior is composed into the forms:

- `GenreSelectionControl` owns genre normalization, duplicate/limit handling,
  paste/delete, and a disposable `FloatingPanel` picker. Existing lists above the
  configured limit remain intact; the limit applies to new additions.
- `PeopleEditorControl` owns paste, rename, delete, and portrait replacement,
  configured explicitly for director, actor, or author lookup. Enter commits a
  label edit without saving the dialog; F2 acts on that editor's selected person.
- `ImageEditorControl` owns cloned images and releases source file handles;
  `GamePreviewsControl` composes four numbered previews and the selected view.
  Loading all four images selects the first once; later image edits and clicks
  still update the selected view immediately. People/genre loading batches native
  list updates without normalizing or dropping saved relationships.
- `FileSizeControl` and `VideoInfoControl` use `IFileInspectionService` for
  cancellable size and bundled MediaInfo work. Only movies/documentaries request
  video information; replaced or completed requests cannot apply stale results.
- `MovieDetailsForm` uses `ITmdbMetadataService` for movie/series metadata and
  missing portraits. Closing cancels requests; late metadata does not overwrite
  manual edits, and downloaded portraits do not replace manual portraits.
- `EntryEditorSession`, owned by the form's component container, handles lookup,
  save/ignore, keyboard commands, and save failure recovery without owning any
  collection controls. Forms are disposed by their modal callers.
- `DetailFormPresentation`, also owned by the component container, keeps each
  runtime form transparent through local loading and its first paint. A callback
  queued after `Shown` refreshes the form and child windows before restoring its
  opacity. No fixed delay is added, and background file inspection/TMDb requests
  do not gate visibility. Closing/disposal cancels the queued reveal; designer
  construction leaves visibility unchanged.

Save still uses one existing `CatalogStore.Save` operation for metadata, genres,
people, and recoverable image promotion. IDs, unchanged nullable flags/dates,
undecorated descriptions, and configured poster/preview filenames are preserved.
Escape cancels editing; Shift-confirm ignores the path. TMDb choices accept Enter
or double-click and clear a selected index on cancellation.

The choice dialog explicitly uses `ManagerRenderMode` for its status bar to
preserve its existing appearance under .NET 10's changed WinForms default.

Automated shown-form checks use disposable real SQLite/image data, including
save/reopen, relationship clearing, cancellation, and delayed-result behavior.
The current verification scope and remaining native checks are recorded in
[PLAN.md](PLAN.md).

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
Ariadna.slnx
├── Ariadna/                        # Main WinForms application (net10.0-windows)
│   ├── Program.cs                  # Entry point — starts the single-instance application model
│   ├── CatalogApplication.cs       # Startup, splash, subsequent launch activation
│   ├── MainWindow.cs/.Designer.cs  # Four-tab shell, navigation, window geometry
│   ├── MainPanel.cs/.Designer.cs   # Retained collection user control
│   ├── Utilities.cs                # Genre dictionaries, image helpers, video duration
│   ├── App.config                  # Connection string and all application settings
│   ├── AuxiliaryPopups/            # Independent detail forms, composed editors/services, picker/choice dialogs
│   ├── Data/                       # DTOs (EntryDto, EntryInfo, MovieChoiceDto)
│   ├── DatabaseStrategies/         # AbstractDbStrategy + 4 concrete strategies + helpers
│   ├── Extension/                  # Static extension methods
│   ├── ImageListHelpers/           # Custom renderer and poster adaptor
│   ├── ImageListView/              # Embedded fork of Manina ImageListView (~30 files)
│   ├── Properties/                 # App settings, resources
│   ├── Resources/                  # PNG/BMP/ICO assets, 100+ genre icons
│   ├── SplashScreen/               # SplashForm managed by the application model
│   └── Themes/                     # Theme base + ThemeMovies/Games/Documentaries/Library
├── Ariadna.Storage/                # Focused SQLite operations, schema, image recovery
├── Ariadna.Storage.Tests/          # Real SQLite integration tests
├── Ariadna.Migration/              # Standalone export/import/compare/snapshot CLI
├── Ariadna.Migration.Tests/        # Synthetic migration/recovery tests
└── Ariadna.Tests/                  # MSTest unit test project (net10.0-windows)
    ├── AuxiliaryPopups/
    ├── DatabaseStrategies/
    ├── ImageListHelpers/
    └── MainWindowTests.cs / MainPanelTests.cs / UtilitiesTests.cs
```

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| Microsoft.Data.Sqlite | 10.0.12 | Direct SQLite provider |
| SQLitePCLRaw.bundle_e_sqlite3 | 3.0.5 | Native SQLite 3.53.4 runtime |
| TMDbLib | 3.0.0 | The Movie Database REST API client |
| MediaInfo.Wrapper.Core | 26.1.0 | Video metadata (resolution, bitrate, audio) |
| SkiaSharp | 3.119.2 | Image processing |
| Microsoft.Extensions.Logging.Console | 10.0.5 | Console logging for strategies |
| System.Runtime.Serialization.Formatters | 10.0.5 | Legacy serialization support |
| Microsoft.NET.Test.Sdk | 18.4.0 | Test host |
| MSTest.TestFramework / TestAdapter | 4.2.1 | Unit testing |

Versions reflect project files and locked packages on 2026-10-03. Locked restores
use the SDK pin in global.json. The separate migration tool uses
Microsoft.Data.SqlClient 7.1.1 for SQL export/comparison only.

---

## Testing

Use [README.md](../README.md) for test commands. Tests follow the
`[MethodUnderTest]_[Precondition]_[ExpectedOutcome]` naming convention and the
Arrange / Act / Assert structure; see [AGENTS.md](../AGENTS.md) for the complete
repository testing rules.

Tests include retained workflow characterization, real SQLite query/write and
recovery integration, conversion/package failures, and automated shown WinForms
grid/detail checks in all four modes. See the operations guide for measured results
and the limits of automated native checks.

### Acceptance Scenarios

These are verification scenarios, not claims of completed checks. Run write
scenarios against disposable catalog/image copies.

| Workflow | Expected behavior / verification |
|---|---|
| Launch | First launch opens the requested tab or Movies; second launch activates the same process, restores a minimized window, and defers switching during modal editing |
| Tab navigation | Four permanent tabs retain their filters, selection, scroll position, controls, and colors; mouse selection and Ctrl+Tab / Ctrl+Shift+Tab / Ctrl+1 through Ctrl+4 work |
| Browse and filter | Poster grid and alphabetical navigation work; title, people, genre/subgenre, and applicable flags retain collection-specific results and ordering |
| Details and edits | Open an existing entry, edit metadata/images/relationships, save, reopen, and confirm persisted values; cancel retains saved data |
| Discovery and ignore | Manual/automatic discovery retain collection-specific paths/exclusions; ignore prevents later rediscovery |
| Execute and remove | Configured player/file manager receives the expected path; cancellation prevents deletion; optional filesystem removal respects the chosen action |
| Form lifecycle | Closing a form during directory/MediaInfo work cancels it without stale UI updates; images are released for replacement |
| SQLite migration | Compare every converted value/ID/hash, real-provider searches and failures, concurrent storage writers, and matched restore before production cutover |

## Non-Functional Considerations

Thumbnail decoding/caching, filesystem discovery, and MediaInfo work affect
perceived responsiveness independently of database performance. Long-running
work must respect form lifetime and keep UI updates on the owning thread.
There is no measured latency or migration speedup guarantee in this document.

The embedded ImageListView source and license remain part of the application.
Tests and builds do not prove native rendering, mouse/keyboard behavior, external
tool availability, or recovery of a production catalog.

## Storage evolution and verification limits

The approved SQLite migration is implemented. The initial source investigation
is retained in [SQLITE-MIGRATION-PLAN.md](SQLITE-MIGRATION-PLAN.md); current
storage/recovery behavior and migration evidence belong in
[SQLITE-OPERATIONS.md](SQLITE-OPERATIONS.md). IDs, NULLs, duplicate relationships,
photo bytes, and external filenames were verified before cutover.

A clean-machine installation and manual review of every control remain outside
the verified scope. Native automated checks exercise key browse/save/reopen paths.
On 2026-10-03 the user reported successful manual browse/details/edit/restart
checks across all four collections, including posters and game previews, using
the isolated full-data copy with version 2.0.1; see the operations guide.
Thumbnail decoding and external tool integration still affect responsiveness.
Future schema upgrades must take a matched backup first and advance the schema
version transactionally. Track future work in [PLAN.md](PLAN.md).

---

## Acknowledgements

- **[ImageListView](https://github.com/oozcitak/imagelistview)** by Ozgur Ozcitak — the thumbnail grid control embedded in `Ariadna/ImageListView/`. The source is included directly in the repository (with its license) and extended with a custom renderer and poster adaptor.