# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Versioning rule:

- **Major** - behavior visible to other system components changes (breaking)
- **Minor** - new backward-compatible feature
- **Patch** - backward-compatible bug fix only

## Ariadna.Wpf

### [3.2.0] - 2026-10-10

#### Changed

- Redesign popups.

### [3.1.0] - 2026-10-10

#### Changed

- Show Movies original titles on poster entries, with title fallback when absent; sort and navigate by the displayed titles.
- Show Movies original title in Details Title and stored title in Translation, preserving their storage mapping.

### [3.0.2] - 2026-10-10

#### Fixed

- Further Details form tweaks.

### [3.0.1] - 2026-10-10

#### Fixed

- Remove the Original title field and label from Games Details without leaving an empty row.

### [3.0.0] - 2026-10-10

#### Changed

- Switch to English.

### [2.0.1] - 2026-10-09

#### Fixed

- Place Games Details Version directly beneath Year with matching width, and show game sizes in GB.
- Replace numbered preview buttons with four image thumbnails; selecting shows the large preview and double-clicking opens the image replacement dialog. Remove the separate Replace preview and Paste preview buttons.

### [2.0.0] - 2026-10-09

#### Changed

- Replace page genre dropdowns with legacy-style, collection-themed icon panels beneath the toolbar on all four tabs. Double-click or Enter confirms; Escape, outside clicks, window changes and tab switches dismiss without applying an unconfirmed selection.
- Use catalog genres for Movies, Games and Documentaries, and category/subject icon panels for Library; retain the adjacent clear buttons.
- Release the genre field's text-selection mouse capture before opening the page picker, so genre tiles receive clicks and the popup can dismiss on outside clicks.

### [1.10.7] - 2026-10-09

#### Fixed

- Move the Details description field up 5px and fit one complete portrait row in the people panels.
- Scroll portrait panels by whole rows with the wheel, scrollbar and keyboard.
- Match inline people name editors to the original name's dimensions, font, alignment and wrapping, with a blue outline and no field padding.
- Keep people names aligned when entering inline editing, including Library authors; remove internal scrolling chrome and compensate for the text renderer's horizontal inset.

### [1.10.6] - 2026-10-09

#### Fixed

- Restore legacy live portrait search for Movies directors and actors and Library authors, with double-click/Enter confirmation and Escape dismissal.
- Stretch the Documentaries Details description to fill the available area above the footer as the window resizes.

### [1.10.5] - 2026-10-08

#### Fixed

- Open the poster file picker when double-clicking the missing-poster placeholder or empty poster area in Details.

### [1.10.4] - 2026-10-08

#### Changed

- Use ? for missing posters and portraits and unknown genre icons; remove the WPF No_Image.png resource.

### [1.10.3] - 2026-10-08

#### Fixed

- Preserve the poster list's scroll position, selection and keyboard focus when pressing Escape.

### [1.10.2] - 2026-10-08

#### Changed

- Cancel Details form on Escape.

### [1.10.1] - 2026-10-08

#### Fixed

- Fix Library and Documentaries Details background.

### [1.10.0] - 2026-10-08

#### Added

- Edit director, cast and author names with F2 or a double-click on the name.

### [1.9.5] - 2026-10-08

#### Fixed

- Use the active collection palette for the Details Save button instead of a fixed purple background and border.

### [1.9.4] - 2026-10-08

#### Changed

- Show Library authors as compact name-only items and give the Description field more space, preserving stored author photos and existing author controls.

### [1.9.3] - 2026-10-08

#### Fixed

- Select the saved entry after adding or editing.

### [1.9.2] - 2026-10-08

#### Changed

- Copy the application icon, mapped genre icons, audio language flags and fallback image into WPF-owned resources and embed them locally, removing resource links to the legacy project. Exclude unused legacy toolbar, collection and preview assets.
- Give WPF its own App.config copied from the legacy configuration, preserving existing settings and removing the build dependency on the legacy configuration file.

### [1.9.1] - 2026-10-06

#### Changed

- Place the clipboard paste button before Add in Details people headers.

### [1.9.0] - 2026-10-06

#### Changed

- Replace Details' Edit genres section with header paste and picker buttons. The floating icon picker hides selected genres and confirms on double-click or Enter; Delete removes selected genres.

### [1.8.2] - 2026-10-06

#### Changed

- Align Wishlist, Cancel and Save to the Details footer's right edge while keeping media information on the left.

### [1.8.1] - 2026-10-06

#### Fixed

- Correct the Details bitrate conversion to Mbps and round to whole numbers.

#### Changed

- Show audio track flags without text; retain language names in tooltips.

### [1.8.0] - 2026-10-06

#### Added

- Add clipboard paste buttons to Details people sections with WinForms name capitalization, duplicate handling and stored portraits.

#### Changed

- Hide Details from the taskbar, place Cancel and Save beside Wishlist in the information bar, and share the main window's rounded white scrollbar appearance.

### [1.7.0] - 2026-10-06

#### Changed

- Simplify Details form.

### [1.6.0] - 2026-10-06

#### Added

- Redesign Details form.

### [1.5.4] - 2026-10-06

#### Removed

- Remove the WPF startup splash window. Prepare the initial catalog before showing the main window, then preload the remaining catalogs; retain startup recovery and error handling.

### [1.5.3] - 2026-10-06

#### Fixed

- Allow poster titles to wrap across two lines, with an ellipsis when longer titles still overflow. Reserve the same two-line caption height for every card, retaining poster size and row-aligned scrolling.

### [1.5.2] - 2026-10-05

#### Fixed

- Focus the poster items list at startup and after switching catalog tabs instead of the Add entry button. Preserve selection and scroll restoration, and ignore queued activation callbacks for unloaded tabs.

### [1.5.1] - 2026-10-05

#### Changed

- Restyle QuickList.

### [1.5.0] - 2026-10-05

#### Added

- Redesign item hover and selection highlight.

### [1.4.1] - 2026-10-05

#### Changed

- Match the reference poster-grid scrollbar with a narrow catalog-colored track, rounded white thumb and no arrow buttons. Retain thumb dragging, track paging, row-aligned scrolling and virtualization; keep the style scoped to the poster grid.

### [1.4.0] - 2026-10-05

#### Added

- Map keyboard `+` to the active catalog's Add entry button when browsing. Preserve plus characters in text fields and the button's disabled guard during discovery.

### [1.3.1] - 2026-10-05

#### Fixed

- Keep the first visible poster row aligned to the viewport top at startup, during selection/navigation, scrollbar scrolling and tab restoration. Use WPF item scrolling for virtualized rows, retaining one-row wheel steps and resize anchoring while allowing a partial bottom row.

### [1.3.0] - 2026-10-05

#### Added

- Display the main application's assembly version at the far right of the tabs row in semi-transparent black text, with reserved space and no keyboard focus or mouse interception.

### [1.2.1] - 2026-10-04

#### Changed

- Match the catalog filters to the compact themed toolbar: white separators, outlined checkboxes after their labels, borderless tinted text fields, white clear icons, genre ellipses and a bold result count. Filter groups wrap together at narrower widths, and editable suggestion menus retain the collection palette.
- Restore the toolbar's plus button using the existing discovery action, with file and folder choices on its context menu.

### [1.2.0] - 2026-10-04

#### Added

- Show a bold "Found entries: <count>" indicator after the filters in every CatalogView, updating with the displayed result count after filtering, clearing filters and switching tabs.

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

### [2.0.3] - 2026-10-09

#### Fixed

- Add explicit strict genre matching for WPF queries, while preserving legacy unknown-lookup behavior and people-search semantics.

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