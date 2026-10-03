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

## Remaining acceptance and future work

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
- [ ] Decide whether to pin the SDK.
- [ ] Measure complete UI latency and profile image/discovery work before making
  speedup claims; direct-query measurements did not show a universal speedup.
- [ ] Optionally add a backup/export UI reusing the implemented package format.
  The CLI already supplies the recovery workflow.

Schema upgrades must start with a verified matched snapshot and advance the schema
version transactionally. Returning to the legacy SQL database after SQLite edits
requires separate data transfer; automatic reverse synchronization is not provided.
