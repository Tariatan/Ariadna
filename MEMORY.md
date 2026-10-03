# Ariadna project memory

Last updated: 2026-10-03. Scope: this repository only.

## Durable context

- Personal Windows catalog with four independently launched modes: movies/series,
  documentaries, games, and library. Keep simultaneous processes and each mode's
  fields, genres, palette, and paths working.
- Current implementation: .NET 9 WinForms and direct SQLite through
  Ariadna.Storage. Legacy EF6/LocalDB and DbProvider were removed. Only the separate
  migration utility uses a SQL client for legacy export. Dependencies are locked;
  no SDK pin is configured.
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
  Storage/migration implementation and versions remain unchanged.

## Resume here

Complete the remaining native/integration review in [PLAN.md](docs/PLAN.md).
For an individual detail-form change, edit that form's designer and collection
mapping; change shared controls only when the behavior should apply to every
consumer. Keep save/reopen characterization and background-lifetime checks.

Use README's locked restore, Release build/test commands. Changes to storage or
schema require the matched recovery and comparison gates; build success alone
does not prove native interaction or recovery.
