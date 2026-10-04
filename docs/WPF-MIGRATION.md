# WPF migration

Implemented 2026-10-04 as Ariadna.Wpf 1.0.0. Production acceptance and retirement
of the WinForms frontend have not occurred. The existing executable remains
available while the new frontend is tested.

## Architecture and compatibility

The WPF application references Ariadna.Storage directly. It does not reference
the WinForms application or host WinForms controls. XAML windows and controls
replace the shell, browser, detail editors, people editors, and metadata chooser.
Observable models hold catalog/filter/selection state; views handle native
dialogs, focus, and scrolling. Storage owns SQL, validation, and persistence.

Both frontends use the same SQLite schema and matched external assets. No
export/import, ID conversion, or image rename is needed. Saves continue through
CatalogStore.Save and CatalogAssets. Image edits stay in memory until Save;
unchanged image files are not rewritten. Nullable values and loaded relationships
survive unchanged editing. Storage 2.0.2 also retains unchanged legacy duplicate
relationships and their IDs instead of rebuilding them during a save.
Configured game preview suffixes are retained; safe filename characters are
validated before staging, with the existing `_preview` default for old callers.

The build copies configuration from `Ariadna/App.config`. WPF reads application
settings and default user settings; it does not import the WinForms executable's
separate per-user configuration file. `ARIADNA_CATALOG_PATH` and
`ARIADNA_ASSET_ROOT` support disposable testing with the existing collection
subdirectories. Window geometry is stored separately under the local application
data directory in `Ariadna/Wpf/window.json`.

## Browser and lifecycle

The browser binds rows of poster cards to a recycling VirtualizingStackPanel.
Each visible row contains a limited number of cards. Resizing changes the column
count while retaining selection and anchoring the viewport. Pixel scrolling stays
virtualized; the UI does not construct a card for every entry. See
[Microsoft's virtualization guidance](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-controls).

Four workers decode thumbnails off the UI thread. A shared cache retains at most
256 images, decoded to 240 pixels wide. Images are frozen, source streams close
after decoding, and unloaded/recycled cards cancel requests and discard stale
results. Missing/unsupported posters use a fallback. Save/refresh invalidates the
cache, including in-flight cache generations.

Filters debounce for 180 milliseconds and run SQLite queries on workers. Replaced
requests cannot publish stale results. Four retained views keep independent
filters, palettes, selection, and scroll position. Initial selection is random.
Remaining catalogs preload after the initial window is shown. Closure cancels
view/query/discovery/file/metadata work. Synchronous SQLite/native calls already
running finish before canceled results are discarded.

The WPF frontend has its own catalog-specific single-instance mutex and
current-user named pipe. Subsequent WPF launches activate the original window;
named arguments select tabs, and requests wait for a modal editor to close.
Close WinForms before using WPF with the same live catalog during this transition.
Shared storage writer coordination remains in place.

## Implemented workflows

- Four permanent tabs in Movies/Games/Library/Documentaries order and their palettes.
- Applicable title/people/genre/subgenre/flag filters, clear buttons and suggestions.
- Alphabetical jumps, random selection, arrow/page/home/end navigation, Ctrl+Tab,
  Ctrl+Shift+Tab, and Ctrl+1 through Ctrl+4.
- Right-click/F2 details and double-click/Enter configured execution.
- Native file/folder selection, collection-specific discovery, and ignore paths.
- Collection-specific fields, raw descriptions, genre normalization/limits,
  people paste/rename/portraits, poster selection/paste, and four game previews.
- TMDb search/choice and poster retrieval; automatic lookup for new movie/series
  entries with a configured API key. Late results protect intervening manual edits.
- Background file size and bundled MediaInfo analysis; Escape cancellation,
  contextual Enter save, and Shift-confirm ignore.
- Confirmed catalog removal; Shift-remove additionally removes the media path.
- Separate persistent window geometry and native WPF scaling.

WPF uses text-based genre pickers and a fixed selection border. The WinForms
genre-icon picker, pixel layouts, and blinking renderer are not part of this
frontend. Visual acceptance remains a user check.

## Build and test

Close running Ariadna instances before rebuilding. Use the SDK/locked restore
commands in [README.md](../README.md). Launch the project-built executable:

```powershell
./Ariadna.Wpf/bin/Debug/net10.0-windows8.0/Ariadna.Wpf.exe
```

It accepts the existing `movies`, `games`, `library`, and `documentaries` arguments.
Copied configuration targets the configured catalog/assets by default. Initial
write tests should use a disposable matched copy and the environment overrides
described in [SQLite operations](SQLITE-OPERATIONS.md).

Acceptance on that copy:

1. Browse/filter/jump/scroll each tab and confirm retained state after switching.
2. Open/cancel, then edit/save/reopen an entry from every collection.
3. Replace posters, people/genres, and all four game previews.
4. Restart and confirm persisted data/images; exercise subsequent named launches.
5. Check TMDb, native file/image dialogs, clipboard, discovery, external tools,
   scaling, keyboard behavior, and appearance.
6. Verify matched snapshot/restore before switching daily use to WPF.

Checks actually run and remaining limits are in [repository memory](../MEMORY.md).
Synthetic checks do not establish production acceptance, live TMDb correctness,
or clean-machine installation.
