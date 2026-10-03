# Implementation plan and checkpoint

Last updated: 2026-10-03.

The approved SQLite migration is implemented. The desktop uses direct SQLite
storage; EF6, LocalDB configuration, EDMX/T4 tooling, and DbProvider are removed.
The [Technical Design Description](Technical_Design_Description.md) owns current
architecture; [SQLite operation and recovery](SQLITE-OPERATIONS.md) owns storage,
backup/restore, migration evidence, and verification limits.

## Completed migration work

- [x] Establish matched SQL/image/configuration recovery and restore a checksum
  backup under a separate SQL database name with CHECKDB passing.
- [x] Build a lossless export/import utility preserving IDs, identity counters,
  NULLs, duplicates, photo bytes, and external filenames; verify every value/hash.
- [x] Implement focused SQLite queries and complete-entry transactions across all
  four collections, with Unicode searches, existing filters, discovery, and ignore.
- [x] Add durable image journals, commit markers, concurrent-writer coordination,
  rollback/recovery tests, and explicit empty relationship replacement.
- [x] Verify real-provider queries and shown WinForms browse/search/save/reopen
  paths using disposable catalog/image data; compare 50 real-source queries.
- [x] Repeat conversion from a fresh final SQL backup/export; publish the verified
  SQLite catalog while retaining SQL/asset/legacy-app recovery artifacts.
- [x] Demonstrate matched SQLite/database-image snapshot and import/restore
  without requiring SQL Server for the ongoing backup workflow.
- [x] Configure dependency locks and verify restore, Release builds/tests, and a
  self-contained win-x64 publish.

## Completed auxiliary popup refactor

- [x] Replace `DetailsForm` inheritance and template save hooks with four direct
  `Form` subclasses, each with an independent designer/resource pair and explicit
  collection mapping. Compose shared editor controls and inspection/TMDb services.
- [x] Open all four independent forms in Visual Studio's WinForms designer. Fixed
  an unsupported movie-icon resource cast found during this check.
- [x] Preserve complete-entry SQLite/image saves, IDs, nullable values, existing
  relationships, and configured asset filenames; characterize shown-form
  save/reopen, relationship clearing, cancel/ignore, and delayed request behavior.
- [x] Native Windows checks on disposable synthetic data: inspect all four layouts;
  save/reopen in all four modes; author/cast F2 plus Enter commit; documentary
  keyboard save; game version/VR mouse edit and save/reopen. Corrected a game
  layout overlap found during this review. Verified on 2026-10-03.
- [x] Locked restore, Release build with zero warnings/errors, and 167 desktop
  plus 38 storage tests pass. Debug desktop build also passes for designer loading.
- [ ] Complete every-control native review, including picker confirmation,
  clipboard paths, image-file dialogs, and live TMDb. The owned genre picker was
  displayed, but the automation target did not allow confirming its selection;
  selection/cancellation events are covered by shown-form automated tests.

## .NET 10 and SLNX upgrade

- [x] Retarget all six projects to .NET 10; refresh dependency locks while keeping
  direct package versions. Raise Ariadna to 3.0.0 and Storage/Migration to 2.0.0
  for their changed runtime/consumer requirements. No catalog/schema change.
- [x] Replace the legacy solution with Ariadna.slnx and include migration and
  its tests. Preserve Debug/Release and Any CPU/x64/x86 solution mappings.
- [x] Pin stable SDK 10.0.401 with latestPatch roll-forward in global.json.
- [x] Locked restore, all six configuration/platform builds with zero warnings
  or errors, 167 desktop + 38 storage + 5 migration tests, and self-contained
  win-x64 publishing passed on 2026-10-03.
- [x] Open the SLNX solution in Visual Studio 2026 with all six projects loaded.
- [x] Native Windows smoke checks of the published .NET 10 app with disposable
  synthetic catalogs: run all four modes concurrently, render posters and game
  previews, open details, save unchanged, and reopen. A synthetic choice-dialog
  harness displayed its status bar and Escape closed it. Verified on 2026-10-03;
  exhaustive control/integration and clean-machine checks remain open below.

## Remaining acceptance and future work

- [x] Fix read-only image recovery directories blocking startup or subsequent
  writes. Four new real-filesystem regression cases reproduced the failure before
  the fix and pass afterward, covering empty leftovers and committed/interrupted
  image saves. Locked restore, Debug/Release builds with zero warnings/errors,
  167 desktop + 42 storage + 5 migration tests, and self-contained publishing
  passed on 2026-10-03. Storage version is 2.0.1; no schema/format change.
  Native startup of the published app showed the synthetic poster grid and
  removed a deliberately read-only empty recovery directory.
- [x] User acceptance on the copied catalog: browse/navigation, existing-entry
  details, metadata/genre edits, save/reopen/restart across all four collections,
  poster changes, and game previews. User reported all requested checks passed
  on 2026-10-03 with version 2.0.1.
- [ ] Manual Windows mouse/keyboard review of every control, image rendering,
  and optional external player/file-manager/TMDb integration.
- [ ] Clean-machine installation test; current deployment checks use the existing
  Windows machine with LocalDB stopped rather than an unconfigured machine.
- [x] Replace Windows API Code Pack duration lookup with bundled MediaInfo and
  verify details opening using an existing video; the Shell dependency is removed.
- [ ] Measure complete UI latency and profile image/discovery work before making
  speedup claims; direct-query measurements did not show a universal speedup.
- [ ] Optionally add a backup/export UI reusing the implemented package format.
  The CLI already supplies the recovery workflow.

Schema upgrades must start with a verified matched snapshot and advance the schema
version transactionally. Returning to the legacy SQL database after SQLite edits
requires separate data transfer; automatic reverse synchronization is not provided.
