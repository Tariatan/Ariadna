# Ariadna project memory

Last updated: 2026-10-03. Scope: this repository only.

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

Complete the remaining native/integration review in [PLAN.md](docs/PLAN.md).
For an individual detail-form change, edit that form's designer and collection
mapping; change shared controls only when the behavior should apply to every
consumer. Keep save/reopen characterization and background-lifetime checks.

Use README's locked restore, Release build/test commands. Changes to storage or
schema require the matched recovery and comparison gates; build success alone
does not prove native interaction or recovery.
