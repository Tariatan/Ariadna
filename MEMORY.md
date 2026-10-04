# Ariadna project memory

Last updated: 2026-10-04. Scope: this repository only.

## Durable context

- 2026-10-04: user designated Ariadna.Wpf as the default project for future work.
  Prefer its frontend and Ariadna.Wpf.Tests; leave the legacy WinForms Ariadna
  implementation unchanged unless explicitly requested. Shared storage changes
  remain task-dependent. This routing decision does not claim completion of the
  remaining acceptance checks or retirement of WinForms. PLAN is user-owned.
- Personal Windows catalog with one application instance and four permanent tabs:
  movies/series, documentaries, games, and library. Preserve each tab's fields,
  genres, palette, paths, filters, selection, and scroll position. Legacy arguments
  select a tab initially or activate that tab in the existing window.
- Current implementation: .NET 10 WinForms plus a native WPF frontend awaiting
  acceptance, and direct SQLite through
  Ariadna.Storage. Legacy EF6/LocalDB and DbProvider were removed. Only the separate
  migration utility uses a SQL client for legacy export. Dependencies are locked;
  global.json pins stable SDK 10.0.401 with latestPatch updates in 10.0.4xx.
  Ariadna.slnx contains all eight desktop/storage/migration/test projects; the
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

- 2026-10-04: Ariadna.Wpf 1.1.0 scrolls the poster grid to an adjacent row
  boundary per wheel notch in all four catalogs. Smaller deltas accumulate,
  rapid input retains every step, and offsets clamp at the content ends.
  Pixel virtualization, selection, scrollbar dragging and retained tab positions
  are preserved. Wheel, resize and keyboard page calculations now measure the
  rendered row height instead of assuming 300 pixels; current card styling is
  unchanged. WinForms/Storage/Migration and user-owned PLAN are unchanged.
- Locked WPF-test restore, zero-warning/error Debug and Release WPF builds,
  and all 39 Release WPF tests passed. Seven added cases cover four-catalog
  partial-row alignment, fine/rapid wheel input and boundaries, keyboard paging,
  and resize anchoring. Native checks with an isolated Debug WPF harness and
  disposable SQLite/posters confirmed 427-DIP wheel steps in every tab, reversal,
  keyboard Home/Page Down, partial-row alignment and retained Movies selection/
  offset after Ctrl+2/3/4/1. No personal catalog was used. The supplied video path
  was unavailable; video inspection, physical touchpad and changed-DPI acceptance
  are not claimed. Next: user acceptance with the normal project-built executable.
- 2026-10-04: Ariadna.Wpf 1.0.1 fixes main-window placement persistence. Saving
  now occurs in OnClosing after cancellation handlers, not Closed (when WPF's
  RestoreBounds is empty). The existing local application-data JSON store restores
  normal size/position and retains maximized closure. Unshown windows skip saving;
  canceled closure preserves the previous file. WinForms/Storage/Migration are
  unchanged, and PLAN remains user-owned and untouched.
- The placement regression reproduced invalid-infinity JSON serialization before
  the fix. Five new cases cover move/resize/reopen, normal/minimized/maximized
  closure, cancellation, and unshown-window protection. WPF test startup now
  explicitly loads resources without production startup. An initial native harness
  inadvertently started the production frontend alongside its synthetic window;
  it was stopped before UI editing, then corrected to use the isolated startup.
  No catalog edits were performed; no production recovery outcome is claimed.
  The corrected native check used disposable catalog and placement files: mouse
  movement, native keyboard resizing, Alt+F4, and reopening restored identical
  screenshot bounds. Personal-catalog acceptance, changed-monitor/DPI checks,
  and new publishing verification remain open for this patch.
- Final verification for 1.0.1: locked solution restore, zero-warning/error Debug
  and Release builds, and 292 Release tests passed (204 WinForms, 51 storage,
  32 WPF, 5 migration). PLAN's content hash is unchanged. Next: user acceptance
  of WPF move/resize/close/reopen with the normal startup executable.
- 2026-10-04: implemented Ariadna.Wpf 1.0.0 alongside Ariadna 4.1.7. The WPF
  frontend references only Storage and uses native XAML views, four retained tabs,
  a recycling poster-row browser, bounded background thumbnail decoding, worker
  queries, collection-specific editors, image staging, people/genres, discovery,
  MediaInfo, TMDb integration, and deferred single-instance activation. See
  [WPF migration](docs/WPF-MIGRATION.md) for launch, compatibility, and acceptance.
  WinForms remains the production fallback; no personal catalog/images were used
  or modified during implementation and synthetic tests. The database schema,
  recovery format, catalog IDs, and external asset names did not change.
- Storage 2.0.2 preserves unchanged relationship rows, including legacy duplicates
  and genre relationship IDs. Four characterization cases failed before the fix
  and pass after it; explicitly changed lists retain the existing replacement
  behavior. Both frontends use this fix. Migration remains 2.0.0.
- Locked restore, zero-warning/error Debug and Release builds, and 287 tests
  passed: 204 WinForms, 51 storage, 27 WPF, and 5 migration. WPF cases cover real
  SQLite filtering, canceled/replaced queries, retained view state, a 3,000-entry
  virtualized grid, the bounded thumbnail cache, editor save/reopen/cancel for all
  collections, configured preview suffixes, discovery/removal, and real AVI
  inspection. Storage cases cover duplicate preservation, custom preview recovery,
  unsafe suffix rejection, and the existing rollback/recovery workflows. A
  self-contained Release win-x64 WPF publish succeeded to a disposable directory.
- Native mouse/keyboard checks used the actual project-built Debug WPF executable
  with 3,000 synthetic movies and 200 entries in each other collection. Verified
  wishlist/VR filters, alphabetic jump, F2/details, all collection editor layouts,
  a title save/reopen, game preview selection, retained movie filter/selection/
  scroll after switching tabs, Ctrl+1 through Ctrl+4, and second-launch activation
  deferred until a modal editor closed. Corrected selected-tab caption contrast
  and inspected the rebuilt browser. No native destructive removal was performed.
- Remaining acceptance: the personal matched dataset, live TMDb, configured
  external tools, native image/clipboard/discovery paths, monitor scaling changes,
  and clean-machine installation. WPF reads copied default configuration without
  importing legacy per-user settings. Genre pickers are text based and selection
  uses a fixed border; these visual differences need user acceptance. Next: follow
  the WPF acceptance sequence on a disposable matched copy, then switch daily use
  after that succeeds. WinForms retirement has not occurred. PLAN remains unchanged.

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

2026-10-04: Ariadna 4.1.7 paints the unused tab-header strip green after native
WM_PAINT/WM_PRINTCLIENT rendering. Header palettes, the user's four-pixel selected
header inset, native page frame, and existing display scaling are retained.
Locked restore, zero-warning/error Debug and Release solution builds, all 251
Release tests (204 desktop, 42 storage, 5 migration), and diff checks passed.
Native checks used the actual control in an isolated window (including maximize)
and MainWindow with disposable synthetic SQLite/images: all four catalog colors,
mouse Games selection, keyboard Library/Documentaries/Movies selection, green
strip persistence, and retained Movies filters were confirmed. No personal
catalog data was used. The prior icon-path build block is resolved in the user's
current checkout. PLAN was left unchanged. Next: user acceptance of the color;
publishing and a new designer round trip were not checked.

2026-10-04: Ariadna 4.1.6 preserves the native tab frame at the upper-left corner
when Movies is selected. CatalogTabControl insets only the selected header's
background by two logical pixels at the top and sides, leaving its bottom joined
to the page and using the original bounds for caption alignment.

Native Windows checks reproduced the original border overlap and inspected the
updated control in an isolated window using the actual source and existing
tab dimensions, palettes, and DpiUnaware setting. Mouse switching to Games and
Library, keyboard return to Movies, and keyboard selection of Documentaries
passed. The isolated build and locked solution restore passed. Full application
build/test verification is blocked by unrelated in-progress icon moves: resource
entries still reference the removed root icons. No current suite pass or full
application rendering is claimed. The designer/resource edits and icon moves
were left intact; PLAN remains user-owned. Next: build/test the solution and
check the actual shell after the icon references are updated.

2026-10-04: Ariadna 4.1.5 fixes startup after the MainWindow designer was saved.
The designer had added four untagged field-backed pages beside four inline
runtime-created pages and duplicated the Selecting handler. MainWindow now maps
four named designer page references to CatalogKind in runtime code. Themes,
selection, preloading, and view attachment no longer read page.Tag or captions.
The designer creates each page once and subscribes each navigation handler once.
Visible order and the user's unrelated toolbar/resource edits are preserved.

Four new initial-catalog regressions reproduced the NullReferenceException before
the fix. All 251 Release tests (204 desktop, 42 storage, 5 migration), locked
restore, and zero-warning/error Debug/Release builds passed. The fifth regression
clears tags and changes captions/order before showing the window, then checks
identity, palette, retained views, and visible-order shortcuts. It intentionally
does not exercise removal of live permanent tabs.

Native synthetic SQLite/image checks confirmed startup with exactly four tabs,
mouse Games selection, Ctrl+3 to Library, Ctrl+Tab to Documentaries, and Ctrl+1
returning to Movies with its title/director filters and result count retained.
No personal catalog/images or Storage/Migration implementation changed. A new
Visual Studio designer save/reopen round trip and publishing were not performed.
PLAN remains user-owned and unchanged. Next: user startup/design-save acceptance.

2026-10-04: Ariadna 4.1.4 mitigates the initial Details white flash. All four
independent forms compose a container-owned DetailFormPresentation: runtime
construction sets opacity to zero, and a callback after Shown refreshes the form
and child windows before revealing them. Local fields/images load first; pending
file inspection/TMDb does not gate visibility. Closing/disposal suppresses the
queued reveal, and designer construction skips transparency. People/genre loads
use BeginUpdate/EndUpdate; loading four game previews replaces the selected full
image once rather than five times, with later clicks/edits unchanged.

Before the fix, new regressions showed all four forms visible before their first
poster paint and five full-preview replacements. Locked restore, zero-warning/
error Debug and Release builds, and all 246 Release tests (199 desktop, 42 storage,
5 migration) passed. Tests cover existing-entry local content/paint ordering,
pending metadata, modal reveal, early close/disposal, preview selection/editing,
and existing save/reopen/relationship/image/cancellation behavior.

A native synthetic SQLite/image harness confirmed opacity 0 at Shown and first
poster paint, then opacity 1 for all four forms. Mouse preview selection and Movie
edit/save/reopen, initial text focus, and Escape cancellation worked. Native
interaction used modeless test windows with ShowInTaskbar enabled solely so the
UI tool could target them; the unchanged owned Game modal also rendered, but its
inputs could not be targeted. Automated modal message-loop checks passed. No
personal catalog/images or Storage/Migration implementation changed. No updated
high-frame-rate recording comparison or publishing check is claimed. PLAN remains
user-owned and unchanged. Next: personal cold-open acceptance of the flash fix.

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