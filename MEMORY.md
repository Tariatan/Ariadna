# Ariadna project memory

Last updated: 2026-10-04. Scope: this repository only.

## Durable context

- Personal Windows catalog with one application instance and four permanent tabs:
  movies/series, documentaries, games, and library. Preserve each tab's fields,
  genres, palette, paths, filters, selection, and scroll position. Legacy arguments
  select a tab initially or activate that tab in the existing window.
- Current implementation: .NET 10 WinForms and direct SQLite through
  Ariadna.Storage. Legacy EF6/LocalDB and DbProvider were removed. Only the separate
  migration utility uses a SQL client for legacy export. Dependencies are locked;
  global.json pins stable SDK 10.0.401 with latestPatch updates in 10.0.4xx.
  Ariadna.slnx contains all six desktop/storage/migration/test projects; the
  desktop has no reference to the migration CLI.
- IDs identify extensionless posters. Game previews use the configured suffix
  and numbers 1 through 4. Preserve existing IDs, NULLs, relationships, ignore
  paths, and assets. Database and images form one recovery dataset.
- [Technical Design Description](docs/Technical_Design_Description.md) owns
  architecture; [PLAN.md](docs/PLAN.md) owns upcoming work; [SQLite operation and
  recovery](docs/SQLITE-OPERATIONS.md) owns migration/recovery evidence and limits.
- Language conventions are supplied by the session's global instructions.
  Repository rules, versioning, and context maintenance are in [AGENTS.md](AGENTS.md).

## Current evidence

- The approved SQLite migration and matched snapshot/restore tooling are
  implemented. PLAN records user acceptance on the copied catalog with 2.0.1 on
  2026-10-03; retain that dated evidence without inferring new live-data checks.
- 2026-10-03: implemented the accepted auxiliary popup refactor in Ariadna 2.1.0.
  Movie, game, documentary, and library detail forms inherit directly from Form,
  each with its own designer/resources. Focused controls/services provide reuse;
  EntryEditorSession coordinates commands and the existing atomic storage save.
  The DetailsForm base, template hooks, and old designer are removed.
- Locked restore, Release build (zero warnings/errors), and 167 desktop plus 38
  storage tests passed. Debug desktop build passed. All four forms opened in
  Visual Studio's WinForms designer; corrected an unsupported movie-icon cast.
- Characterization/regression tests use disposable real SQLite/image data. They
  cover four-mode save/reopen, nullable values and original descriptions, clearing
  relationships, Escape/ignore/validation, role-specific author portraits, image
  ownership, MediaInfo/file inspection, and cancellation/stale metadata/results.
- Native Windows checks used a disposable synthetic harness: reviewed all four
  layouts and save/reopen, author/cast F2 plus Enter, documentary keyboard save,
  and game version/VR mouse edit. A game layout overlap was corrected and checked.
  The genre picker displayed, but automation could not confirm its owned-window
  selection. Automated selection/cancel tests passed; exhaustive native clipboard,
  image-dialog, live TMDb, and clean-machine acceptance remain open.
- No personal catalog or image data was used or modified for this refactor.
  Storage/migration implementation and versions were unchanged by that refactor.

- 2026-10-03: upgraded all six projects to .NET 10 and replaced Ariadna.sln with
  Ariadna.slnx. Versions are Ariadna 3.0.0, Storage 2.0.0, and Migration 2.0.0
  because runtime/consumer requirements changed. Schema and recovery formats
  are unchanged. Direct package versions are unchanged; locks use net10 assets.
- Locked restore, Debug/Release across Any CPU/x64/x86 (zero warnings/errors),
  167 desktop + 38 storage + 5 migration tests, and self-contained win-x64
  publishing passed. Visual Studio 2026 loaded the SLNX with all six projects.
  ChoicePopup explicitly retains ManagerRenderMode for the changed WinForms
  status-bar default.
- Native Windows smoke checks of the published .NET 10 app used disposable
  synthetic catalogs: all four modes ran concurrently, rendered posters, and
  opened, saved unchanged, and reopened details; game previews also rendered.
  A synthetic choice-dialog harness displayed its status bar and Escape closed
  it. Clean-machine and exhaustive control/integration acceptance remain open.

- 2026-10-03: fixed a startup cleanup failure in Storage 2.0.1. An empty image
  recovery directory was read-only; cleanup now clears only that directory's
  read-only flag and retains commit/rollback behavior and other access failures.
  Four disposable filesystem regressions failed before the fix and pass afterward.
  Locked restore, Debug/Release builds (zero warnings/errors), 214 total tests,
  and self-contained win-x64 publishing passed. The reported empty directory's
  read-only flag was cleared; no catalog records or image contents were changed
  by this direct repair.
  The published app opened its native poster grid with a synthetic catalog and
  removed a deliberately read-only empty recovery directory during startup.

- 2026-10-03: Ariadna 4.0.0 replaces separate catalog processes with one instance
  and four permanent tabs (user-approved). MainWindow owns the shell, geometry,
  icons, and Ctrl+Tab / Ctrl+Shift+Tab / Ctrl+1 through Ctrl+4. Each lazy-created
  MainPanel user control retains its strategy, filters, selection, scroll, and
  instance palette. Closing disposes loaded views, timers, pickers, and listeners.
- WindowsFormsApplicationBase handles single-instance startup and splash lifetime.
  Legacy arguments select an initial tab or request that tab in the running app.
  Repeated launches without arguments preserve the active tab and restore a
  minimized window. Modal editors defer switching; the latest request wins.
- Locked restore, Debug/Release solution builds with zero warnings/errors,
  179 desktop + 42 storage + 5 migration tests, and self-contained win-x64 publish
  passed. Real SQLite/shown-form tests cover retained tab state and all four saves.
- Native Windows checks of the published app used disposable synthetic data:
  four palettes/filter sets and poster grids; mouse and keyboard tab selection;
  retained Movies title search and Games scroll; subsequent Library/Games launch
  requests with only the original process remaining; movie-modal deferral and
  minimized-window restoration without changing Games. Storage/Migration versions,
  database schema, and asset formats are unchanged; no personal catalog was used.
- Every-control, live TMDb/external-tool, and clean-machine acceptance remain open.
  The earlier concurrent-four-process smoke check is historical .NET 10 evidence,
  not the current launch behavior.

- 2026-10-03: Ariadna 4.0.1 fixes the reported smaller UI after tab refactoring.
  WindowsFormsApplicationBase defaults to SystemAware; CatalogApplication now
  explicitly selects DpiUnaware to preserve Windows scaling of existing pixel
  layouts and the custom poster grid. ApplicationHighDpiMode matches that policy;
  ForceDesignerDpiUnaware keeps designer layout serialization at 96 DPI.
- Locked restore, zero-warning/error Debug/Release builds, 226 tests, and
  self-contained win-x64 publishing passed. Native inspection at 150% scaling
  verified all four synthetic catalog grids/filter sets, keyboard tab switching,
  and the game editor with its poster/previews/save button visible. MSBuild
  evaluation confirms both DPI properties. Reopening the existing Visual Studio
  designer remains a user step; automation stopped there after detecting user
  input. No personal catalog data or storage/migration implementation changed.

## Resume here

2026-10-04: Ariadna 4.1.3 fixes slow filter rendering in MainPanel. The previous
update cleared/refilled the grid with intermediate paints and disposed/recreated
every quick-navigation button without suspending layout. Updates now suspend
grid painting until both results and navigation are ready, retain surviving
buttons, skip unchanged navigation, and batch changed letters into one layout.
One view-owned font replaces per-refresh button-font allocations.

Three shown-control regressions cover one layout for changed letters, reuse
without layout for unchanged letters, retained/removed button lifecycle, quick
jumps, and painting only completed results/navigation. The first two reproduced
the old behavior (28 layouts and recreated buttons) before the fix. Locked
restore, zero-warning/error Debug/Release solution builds, and all 237 tests
(190 desktop, 42 storage, 5 migration) passed. Debug executable is version 4.1.3.

A separate native harness used real SQLite and synthetic posters: 3,000 movies
plus 200 entries in each other collection. Ten alternating Wishlist toggles
averaged 977.6 ms in 4.1.2 and 227.9 ms in 4.1.3, with identical 1,000/3,000
result counts. Quick-list layouts fell from 720 to 10; direct SQLite reads
averaged about 5 ms in both runs. These are local sequential synthetic timings,
not a personal-catalog or general latency guarantee. Native mouse checks covered
Wishlist in Movies/Library/Documentaries, Games VR, and Movies quick jump;
keyboard Title/Enter and tab navigation retained the Movies search/Wishlist.
The genre picker displayed and Escape closed it, but owned-item selection could
not be automated, so native genre selection is not claimed. No personal catalog
or image data and no Storage/Migration implementation changed. PLAN is user-owned
and was left unchanged. Personal-size filter acceptance is the next check.

2026-10-04: Ariadna 4.1.2 restores the Library Authors X button by removing
the strategy's duplicate visibility overrides. It reuses the existing Director
icon and clear handler, preserving other active filters. The existing Library
toolbar test reproduced the missing button before the fix and passes afterward.
Locked restore, zero-warning/error Release solution build, all 234 tests
(187 desktop, 42 storage, 5 migration), and diff whitespace checks passed.
Native Windows checks used disposable synthetic SQLite/poster data: clicking X
cleared Authors, refreshed one result to two, and retained the Title filter;
keyboard author typing and Ctrl+1/Ctrl+3 retained the cleared Library state.
Storage/Migration versions and implementations are unchanged. No personal catalog
was used. New publishing and personal-catalog acceptance are not claimed.

2026-10-04: Ariadna 4.1.0 preloads the remaining tabs after the initial page
is shown. Database/default-query configuration is captured before worker reads;
controls, quick lists, and random selections are prepared on the UI thread.
Existing image workers warm only the initial viewport. Early selection reuses
pending work; closure cancels and discards late results. Synchronous reads already
running finish and release connections. Failed preloads are logged and retried
on subsequent selection. Storage and Migration implementations/versions are
unchanged. Preserved the user's local tab styling/order (Movies, Games, Library,
Documentaries) and resource/designer edits; shortcuts follow that visible order.

Locked restore, zero-warning/error Debug/Release solution builds, and 231 tests
(184 desktop, 42 storage, 5 migration) passed. Native synthetic harness confirmed
hidden pages with cached initial posters while Movies stayed active, tab mouse
messages across all four pages, and SendKeys Ctrl+2 / Ctrl+1 retaining Movies.
Reviewed the synthetic grid render. No personal catalog/image data was used.
Personal-size responsiveness acceptance remains open; no latency benchmark or
new publishing verification is claimed for this slice.

2026-10-04: Ariadna 4.1.1 corrects splash lifetime. CatalogApplication waits for
the splash to render before recovery and supplies MainForm before the application
model's OnRun to bypass its premature Load-event dismissal. After Shown, a queued
callback renders the initial window and dismisses the splash before inactive-tab
preloading. No artificial minimum delay is used. Canceled startup, recovery errors,
and early window closure also release the splash.

Zero-warning/error Debug and Release builds and 234 tests (187 desktop, 42 storage,
5 migration) passed. Three new splash workflow tests cover load/display/render
ordering, early closure, and no-splash operation. A native synthetic harness using
the real splash thread confirmed cold-start ordering, Ctrl+2/Ctrl+1 afterward,
and cancellation cleanup. No personal catalog or image data was used. Personal
startup acceptance, recovery-error dialog interaction, and new publishing checks
remain unverified for this patch. Storage/Migration implementations are unchanged.

Next: user acceptance of splash lifetime and tab readiness on the personal catalog,
then remaining integration review in PLAN. The preload does not warm an entire
collection or perform background form/control construction.

Complete the remaining native/integration review in [PLAN.md](docs/PLAN.md).
For an individual detail-form change, edit that form's designer and collection
mapping; change shared controls only when the behavior should apply to every
consumer. Keep save/reopen characterization and background-lifetime checks.

Use README's locked restore, Release build/test commands. Changes to storage or
schema require the matched recovery and comparison gates; build success alone
does not prove native interaction or recovery.
