# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Versioning rule:

- **Major** - behavior visible to other system components changes (breaking)
- **Minor** - new backward-compatible feature
- **Patch** - backward-compatible bug fix only

## Ariadna.Wpf

### [1.1.2] - 2026-10-04

#### Fixed

- Separate Library filter choices: Genre offers Languages, Literature, Programming and Misc; Subgenre offers the chosen category's subjects, such as C++ under Programming. Keep the combined tag list in the details editor and stored tags intact.
- Populate language subjects when the Languages category is selected.

### [1.1.1] - 2026-10-04

#### Fixed

- Restore collection-specific filters in the revised panel: Series/Movies only for Movies, VR/Non-VR only for Games, Directors/Actors for Movies and Authors for Library. Keep the user's layout and sizing.
- Show Library's Subgenre label, selector and clear button only after a Genre is chosen. Clearing or changing Genre resets Subgenre; clearing Genre hides the entire Subgenre group.

### [1.1.0] - 2026-10-04

#### Added

- Scroll the poster grid by one complete row per mouse-wheel notch in all four catalogs, aligning partial rows and accumulating smaller wheel deltas. Keep pixel virtualization and the selected entry unchanged.

#### Fixed

- Use the rendered poster-row height for resizing and Page Up/Page Down instead of the outdated 300-pixel assumption.

### [1.0.2] - 2026-10-04

#### Changed

- Enlarge catalog poster cards.

### [1.0.1] - 2026-10-04

#### Fixed

- Save the main window's size and position on accepted Closing, before WPF clears RestoreBounds. Restore the normal bounds on the next launch, including after minimized/maximized closure; retain the existing maximized-state setting. Canceled closure and windows never shown do not overwrite the previous placement.
- Keep WPF test/resource-only application instances from running production startup, reading personal configuration, recovering its catalog, or opening an extra main window.

### [1.0.0] - 2026-10-04

#### Added

- Native .NET 10 WPF frontend alongside WinForms for acceptance: four retained catalog tabs, asynchronous SQLite filtering, recycling poster rows, bounded background thumbnails, navigation and single-instance activation.
- Native collection editors, genre/people/image editing, four game previews, TMDb metadata and portraits, file/MediaInfo inspection, discovery/ignore, configured execution and confirmed removal. Reuse existing catalog IDs, image filenames, complete-entry transactions and recovery.
- Project-built Debug launch, locked dependencies, synthetic SQLite/image workflow tests, and migration/acceptance documentation. Production acceptance and WinForms retirement remain pending.

## Ariadna

### [4.1.7] - 2026-10-04

#### Changed

- Paint the unused header area to the right of the catalog tabs green, retaining the collection-colored headers and native frame when switching tabs or resizing the window.

### [4.1.6] - 2026-10-04

#### Fixed

- Keep the native tab frame visible when the first catalog tab is selected by insetting the selected header's background along its top and sides while retaining its connection to the page below.

### [4.1.5] - 2026-10-04

#### Fixed

- Fix startup after saving MainWindow in the designer: retain exactly four named tab pages, remove duplicate page creation/event subscription, and map catalog identities in runtime code instead of relying on designer-serialized `Tag` values. Catalog selection, preloading, and themes retain the same identities when captions or tab order change.

### [4.1.4] - 2026-10-04

#### Fixed

- Mitigate the white flash when opening Details: paint local fields and images while the form is transparent, then reveal it after `Shown` without a fixed delay or waiting for background file inspection/TMDb. Batch people/genre list updates and select the first game preview once after loading all four images.

### [4.1.3] - 2026-10-04

#### Fixed

- Batch filter refreshes so the poster grid renders after both the filtered results and quick navigation are ready. Reuse letter buttons, skip unchanged navigation, and lay out changed letters once instead of disposing and arranging every button individually.

### [4.1.2] - 2026-10-04

#### Fixed

- Restore the X button beside the Library Authors filter, using the existing Director clear action to reset the author search and refresh the filtered catalog.

### [4.1.1] - 2026-10-04

#### Fixed

- Show the splash before catalog recovery and retain it through initial catalog initialization. Dismiss it only after the main window is shown and rendered, before inactive-tab preloading, rather than during the Load event. Startup cancellation, recovery failure, and early window closure also release the splash.

### [4.1.0] - 2026-10-04

#### Added

- Preload the remaining catalog tabs after the initial page is shown. Catalog reads run on worker threads; page controls, quick navigation, and initial selection are prepared on the UI thread without changing the active tab. Existing thumbnail workers warm each hidden page's initial viewport.
- Selecting a tab during preload shows a loading message and reuses the pending page. Closing cancels pending work and discards late results; a failed preload is logged and can be retried by selecting the tab again.

### [4.0.1] - 2026-10-03

#### Fixed

- Restored the existing Windows-scaled UI size on high-DPI displays. The single-instance application model now explicitly uses DpiUnaware rather than silently changing to its SystemAware default, which shrank pixel-based posters and controls.
- Set ForceDesignerDpiUnaware so editing forms at 150% display scaling retains their 96-DPI layout metrics. The designer setting is separate from runtime DPI configuration.

### [4.0.0] - 2026-10-03

#### Changed

- Replaced independently launched catalog processes with one application instance containing four permanent tabs: Movies, Documentaries, Games, and Library. Each tab retains its filters, selection, scroll position, and collection-specific controls.
- Existing collection arguments select the initial tab or activate that tab in the running window. Launching without an argument opens Movies initially and preserves the active tab on subsequent launches. Requests made during modal editing wait until the dialog closes.
- Replaced global mutable theme colors with per-catalog palettes for grids, scrollbars, pickers, and independent detail forms. The main window owns geometry and catalog icons; retained catalog controls release their timers, pickers, and subscriptions on shutdown.

#### Added

- Browser-style keyboard navigation: Ctrl+Tab / Ctrl+Shift+Tab to cycle, and Ctrl+1 through Ctrl+4 to select a catalog. Tabs load when first selected and remain available for the window lifetime.

### [3.0.0] - 2026-10-03

#### Changed

- Upgraded the desktop to .NET 10; framework-dependent deployments now require the .NET 10 Windows Desktop runtime. Self-contained win-x64 publishing includes the runtime.
- Replaced Ariadna.sln with Ariadna.slnx, including all six desktop, storage, migration, and test projects. Pinned SDK 10.0.401 with stable patch updates within the 10.0.4xx feature band; refreshed dependency locks without changing direct package versions.
- Explicitly retained the TMDb choice dialog's status-bar renderer after the WinForms default changed in .NET 10.

### [2.1.0] - 2026-10-03

#### Changed

- Replaced the shared DetailsForm hierarchy with independently designed movie, game, documentary, and library forms. Reuse is through focused genre, people, image, preview, size, and video controls plus metadata/file services.
- Removed templated save hooks; each form explicitly maps its collection fields into the existing complete-entry SQLite/image operation. Modal callers dispose their dialogs, and background requests cancel on close.

#### Fixed

- Author portrait lookup uses the author role, and cast F2 renaming uses the cast selection. Enter commits people edits without saving the form.
- Canceling a selected TMDb choice clears it; late metadata/inspection results cannot overwrite manual edits or update closed controls.
- Preserved unchanged nullable flags/dates and undecorated descriptions during edits; loaded genre lists above the configured limit remain intact.

### [2.0.1] - 2026-10-03

#### Fixed

- Fixed a WindowsBase type-load exception when opening details for an existing file in the self-contained app. Duration now uses the bundled MediaInfo reader; the legacy Windows API Code Pack dependency is removed.
- Added regression coverage with an existing two-second video, non-media files, file-handle release, and all four shown detail forms.

### [2.0.0] - 2026-10-03

#### Changed

- Replaced EF6/SQL Server LocalDB with direct SQLite storage across all four collections; removed the legacy DbProvider project and model-generation tooling.
- Added complete-entry transactions, empty relationship replacement, Unicode searches, and recoverable poster/preview writes with startup recovery.
- Added lossless migration, verification, and matched SQLite/image snapshot tools; preserved existing record IDs, identity counters, NULLs, relationships, and image data.
- Added locked dependencies, real-provider/recovery tests, and shown WinForms browse/search/save/reopen checks.


### [1.0.0] - 2026-10-03

#### Added

- Changelog introduced, tracking the current project version before the planned rework.

## Ariadna.Storage

### [2.0.2] - 2026-10-04

#### Fixed

- Retain unchanged legacy genre/people relationships, including duplicate rows and their IDs, when saving other entry fields. Compare photo bytes before skipping unchanged people; explicit relationship edits retain their prior behavior.
- Honor configured game preview suffixes during recoverable image saves, validating filename characters before staging. Existing callers keep the `_preview` default.

### [2.0.1] - 2026-10-03

#### Fixed

- Clear the read-only attribute on image recovery directories before cleanup, preventing an empty leftover save directory from blocking startup or subsequent saves. Preserve commit/rollback decisions and surface other access failures.

### [2.0.0] - 2026-10-03

#### Changed

- Retargeted the storage library to .NET 10, raising the runtime requirement for consumers. Catalog schema, data format, and recovery behavior are unchanged.

### [1.0.0] - 2026-10-03

#### Added

- Direct SQLite catalog operations, explicit schema and Unicode comparison, complete-entry transactions, integrity/backup operations, and recoverable external image writes.

## Ariadna.Migration

### [2.0.0] - 2026-10-03

#### Changed

- Retargeted the migration utility to .NET 10 and included it and its tests in Ariadna.slnx. Framework-dependent CLI runs require .NET 10; export, verification, and snapshot formats are unchanged.

### [1.0.0] - 2026-10-03

#### Added

- Lossless SQL export, staged SQLite import, every-value/hash verification, real-source query comparison, and matched SQLite/image snapshots.

## DbProvider

### [1.0.0] - 2026-10-03

#### Added

- Changelog introduced, tracking the current assembly/file version `1.0.0.0` as the `1.0.0` baseline.