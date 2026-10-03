# SQLite catalog and recovery

Implemented on 2026-10-03. The desktop uses `Ariadna.Storage` and direct
`Microsoft.Data.Sqlite` commands. EF, EDMX/T4, SQL Server configuration, and the
legacy DbProvider project have been removed. The conversion tool is a separate
project and is not referenced by the desktop or its production solution.

## Catalog format

The SQLite file contains the original 19 data tables and a small internal image
commit table. Migration preserves every ID, identity high-water mark, scalar
value, NULL, duplicate relationship, and photo byte. External images retain
their extensionless `{id}` and `{id}_preview1` through `{id}_preview4` filenames.
The schema uses application ID `0x41524941`, schema version 1, foreign keys,
AUTOINCREMENT, explicit length constraints, and nonunique lookup/join indexes.
Startup refuses missing files, foreign databases, and unsupported schema versions.

The locked provider is Microsoft.Data.Sqlite 10.0.12 with
SQLitePCLRaw.bundle_e_sqlite3 3.0.5; the shipped engine reports SQLite 3.53.4.
Connections use WAL, synchronous FULL, a five-second busy timeout, and no
pooling. A named Windows mutex serializes complete-entry/image writes across
windows. A save replaces metadata and all selected relationships in one
transaction, including clearing a previously nonempty selection. The returned
insert ID is used directly.

Images are staged beside the catalog with durable old/new copies and a flushed
journal. Promotion precedes the database commit; a commit token lets startup
restore old images after an interrupted save or retain images after a successful
commit. The next save or startup removes completed journals. Database and image
updates are recoverable; independent readers may briefly see promoted images
before the database commit. Keep the catalog and its recovery journals on local
storage, with the configured asset folders accessible.

Text uses a registered `ARIADNA` collation backed by Windows/.NET en-US
case-insensitive comparison; literal substring search is Unicode aware. Cyrillic
case and literal `%`/`_` are covered by real-provider tests. This is not an exact
emulation of SQL Server's old collation: alphabetical positions changed for 141
of 2,965 movies, 20 of 471 games, and 9 of 669 books in the initial comparison.
Documentary ordering matched. All 50 sampled real-data query memberships matched.
Use the same Windows culture implementation when opening or changing indexed text;
generic SQLite tools do not register this custom collation automatically.

The comparison measured direct SQL Server queries against the new storage API,
not complete old/new UI latency. SQLite was not universally faster in those
single-run measurements. Grid image decoding, MediaInfo, and filesystem discovery
remain separate sources of latency. List/path/info reads do not load person photos;
detail forms and people suggestions request them explicitly.

## Configuration and isolated checks

`AriadnaCatalog` in `Ariadna/App.config` specifies an absolute SQLite filename
with `Mode=ReadWrite;Foreign Keys=True`. Existing poster-root settings still select
the four image folders. Environment variables override these for disposable checks:

```powershell
$env:ARIADNA_CATALOG_PATH = 'C:\Ariadna-QA\catalog.sqlite'
$env:ARIADNA_ASSET_ROOT = 'C:\Ariadna-QA\assets'
```

The asset root contains `movies`, `documentary`, `games`, and `library` folders.
Unset the overrides to return to normal configuration. Preserve the private TMDb
key and other existing settings; do not copy their values into reports.

## Backup and restore

Close every Ariadna window. The snapshot command refuses to run while an Ariadna
process is detected and holds the shared write mutex while copying the database
and assets. Use a new destination directory each time:

```powershell
dotnet run --project Ariadna.Migration -c Release -- snapshot $catalog $assetRoot $newBackupDirectory
```

The package includes a database snapshot, all 19 tables as lossless JSON Lines,
external assets, row counts, identity counters, and SHA-256 hashes in `manifest.json`.
The manifest is written last; incomplete packages cannot be imported. Verification
checks every cell, photo byte, count, counter, and asset hash, plus SQLite structural
and foreign-key integrity. Keep a copy of the active executable configuration
alongside the package separately.

Do not back up only the live main `.sqlite` file: committed data can still be in
its WAL. Use the snapshot command or the SQLite backup API.

To restore, import into a new absolute filename:

```powershell
dotnet run --project Ariadna.Migration -c Release -- import $backupDirectory $newCatalog
dotnet run --project Ariadna.Migration -c Release -- verify $backupDirectory $newCatalog
Copy-Item -LiteralPath (Join-Path $backupDirectory 'assets') -Destination $newAssetRoot -Recurse
```

Choose an absent `$newAssetRoot`; do not merge the restore into existing images.
Configure the new catalog and the copied asset root together, then open all four
modes. Import deliberately restores the database only; the original package
assets remain available for verification. Failed imports remain under a staging
filename and are never published to the final destination. Existing catalogs and
export directories are never overwritten. Verify against the package before
accepting new edits, because subsequent edits intentionally change the data.

## Initial SQL Server conversion and rollback

Only the separate migration tool's `export` and `compare` commands require SQL
Server. `snapshot`, `import`, and `verify` work without it:

```powershell
dotnet run --project Ariadna.Migration -c Release -- export $restoredSqlConnection $assetRoot $newExportDirectory
dotnet run --project Ariadna.Migration -c Release -- import $newExportDirectory $newCatalog
dotnet run --project Ariadna.Migration -c Release -- compare $restoredSqlConnection $newCatalog $comparisonReport
```

The initial migration used a copy-only checksum SQL backup, verified that backup,
restored it under a separate database name, and passed CHECKDB. A rehearsal and
a fresh final export/import both verified 69,724 rows, 18,045 non-NULL person
photos, and 6,067 external images (2,846,768,317 bytes). The original MDF/LDF,
matched asset copies, legacy executable/configuration, SQL backups, and export
packages were retained outside the source repository. Local recovery paths are
provided in the implementation handoff rather than committed public documentation.

Rollback to the retained legacy app uses the matching SQL database and original
image snapshot. Once SQLite accepts new edits, those edits are absent from the old
SQL database; there is no automatic reverse synchronization. Keep a fresh SQLite
snapshot before attempting rollback. Removing the machine-wide SQL installation
is a separate action, since other software may use it.

## Verification scope

All 203 release tests passed: 160 desktop/workflow tests, 38 storage tests, and
5 migration tests. Locked restores, Debug/Release builds, and a self-contained
win-x64 publish passed. The published package's dependency manifest contains no
EF, DbProvider, or SQL client; all four modes constructed their main catalog
windows against a restored full-data copy with LocalDB stopped. The smoke launch
was hidden and does not establish visual grid correctness or displayed counters.
A clean-machine installation was not tested.

The release test suites include legacy workflow characterization, real SQLite
queries/writes, concurrent connections, empty relationship replacement, image
failure rollback, interrupted-save recovery, damaged export rejection, ID-zero and
identity-counter preservation, and matched restore. Automated shown WinForms tests
exercise all four detail dialogs and all four grids with SQLite, including confirm,
reopen, editing, clearing genres, and the Enter-search handler. They do not constitute
a manual mouse/keyboard review of every control or external player/TMDb integration.

Version 2.0.1 fixes a WindowsBase type-load failure while opening details for an
existing file in the self-contained app. The old Windows API Code Pack duration
lookup and package were removed; duration uses the bundled MediaInfo reader. New
regressions cover a two-second synthetic video, file-handle release, non-media files,
and all four shown detail forms. Desktop, storage, and migration builds no longer
emit the Shell NU1701 warnings. See README for build/test/publish commands.

On 2026-10-03 the user reported all requested manual acceptance checks passed
with version 2.0.1 on the isolated full-data copy: browse/navigation, existing
details, metadata/genre edits, save/reopen/restart in all four collections,
posters, and game previews. A fresh matched production snapshot subsequently
passed every-value and image-hash verification for 69,724 rows and 6,067 images.
This acceptance does not establish every-control or external-integration coverage.
