# SQLite migration investigation and plan

Investigated on 2026-10-03. This is a proposal, not an implemented migration. Application code and source records have not been changed. No recovery backup or SQLite conversion has been created yet.

## Recommendation

Migrate to SQLite with `Microsoft.Data.Sqlite` used directly through a small storage layer. Remove EF6, EDMX/T4 generation, and the .NET Framework `DbProvider` project from the production solution. Retain the existing four collection strategies and WinForms presentation, but move SQL, mapping, validation, and transactions out of forms and into storage services.

Direct conversion is practical for this database. The difficult part is replacing EF-dependent application behavior, not moving the records. A separate one-time migration utility should read a verified SQL Server backup restored under a separate database name and build a new SQLite database. Only that utility needs a SQL Server client and temporary access to LocalDB; the finished application needs neither SQL Server nor EF.

Do not make a general-purpose export/import UI a prerequisite. The migration utility can produce a durable, lossless export package as a recovery artifact before conversion. This makes the data recoverable independently of both application versions without delaying migration for UI work.

## Verified current state

The application targets `net9.0-windows8.0`. `DbProvider` targets .NET Framework 4.8. EF6 6.5.1 is referenced by both projects. The main application both references `DbProvider` and directly compiles its generated entity/context source files. It also links the EDMX. Warning CS0436 is suppressed in the main project; removing duplicate model compilation eliminates that particular source of type conflicts, but other warning suppressions need individual assessment.

The configured database is attached to `(localdb)\MSSQLLocalDB` from `ARIADNA.mdf` in the configured catalog directory. The database uses `SQL_Latin1_General_CP1_CI_AS`. Read-only SQL queries verified the actual schema and exact row counts below. `DBCC CHECKDB WITH NO_INFOMSGS` completed successfully without reporting integrity errors.

| Table | Rows |
|---|---:|
| Movie | 2,965 |
| Documentary | 78 |
| Game | 471 |
| Library | 669 |
| Actor | 17,596 |
| Director | 2,553 |
| Author | 645 |
| Genre | 25 |
| GenreOfDocumentary | 8 |
| GenreOfGame | 23 |
| GenreOfLibrary | 39 |
| Ignore | 77 |
| MovieCast | 29,212 |
| MovieDirector | 3,632 |
| MovieGenre | 8,209 |
| DocumentaryGenre | 81 |
| GameGenre | 1,178 |
| LibraryAuthor | 800 |
| LibraryGenre | 1,463 |
| **Total** | **69,724** |

There are 19 user tables, 19 primary keys, and 14 foreign keys. All foreign keys are enabled and trusted, with `NO_ACTION` deletion behavior. The inspected user-object inventory contains no stored procedures, views, or triggers. The current indexes are primary-key indexes only; relationship and path lookups have no additional indexes.

The MDF is 65 MiB and the LDF is 24 MiB. This is modest database volume for the proposed architecture, although the accumulated personal records are valuable. File allocation sizes do not predict the eventual SQLite file size.

### Images and identifiers

| Configured directory | Files | Meaning |
|---|---:|---|
| `MoviePostersRootPath` | 2,965 | One poster per Movie ID |
| `DocumentaryPostersRootPath` | 78 | One poster per Documentary ID |
| `GamePostersRootPath` | 2,355 | Poster plus four previews per Game ID |
| `LibraryPostersRootPath` | 669 | One poster per Library ID |
| **Total** | **6,067** | **2,846,768,317 bytes, approximately 2.65 GiB** |

Poster filenames are extensionless entry IDs. Game previews use `{id}_preview1` through `{id}_preview4`. The current save code writes PNG images; the architecture document's `{id}.jpg` description is inaccurate. A filename-to-database-ID comparison found no missing posters, missing game previews, or unmatched files in these four directories. Image decoding and content hashes have not been checked during this investigation.

The database also contains 18,045 non-null person-photo BLOBs: 15,098 actor photos, 2,302 director photos, and 645 author photos, totaling 28,423,356 bytes. All Movie, Documentary, and Library poster database fields are currently NULL. Preserve those nullable columns in the initial conversion anyway.

The configured catalog volume was verified as a local fixed NTFS volume. Keeping `ARIADNA.sqlite` in the configured catalog root beside the existing image directories is a reasonable target location. Backups must include the database and image directories together.

### Existing data that must survive unchanged

- One duplicate Movie file-path group exists. Do not introduce a unique path constraint during conversion or merge records automatically.
- There are duplicate relationship groups in MovieCast (9), MovieGenre (51), and LibraryGenre (130). Preserve their individual rows and IDs; defer deduplication to a separate change.
- MovieCast, MovieDirector, and LibraryAuthor have nullable foreign-key columns, even though no current rows contain NULL references. Preserve schema nullability.
- One Movie wishlist flag is NULL. Preserve NULL separately from false.
- Ignore IDs range from 0 through 76. ID 0 is valid stored data, not a missing-value marker.
- Several identity counters exceed the maximum currently stored ID because rows have been deleted. Preserve the high-water marks when initializing SQLite sequences.
- 2,948 Movie titles, all 78 Documentary titles, and 360 Library titles contain non-ASCII characters. Unicode search and ordering are core compatibility requirements.

## Application changes required

Direct database access currently appears in 11 main-application source files:

- `DatabaseStrategies/MoviesDbStrategy.cs`, `DocumentariesDbStrategy.cs`, `GamesDbStrategy.cs`, and `LibraryDbStrategy.cs`: listing, query filters, people/genre suggestions, details, deletion, discovery, and maintenance methods.
- `DatabaseStrategies/MediaDbStrategyBase.cs`: shared discovery and ignore-list lookups.
- `AuxiliaryPopups/MovieDetailsForm.cs`, `DocumentaryDetailsForm.cs`, `GameDetailsForm.cs`, and `LibraryDetailsForm.cs`: loading, saving, people, genres, and relationship updates.
- `AuxiliaryPopups/DetailsForm.cs`: shared EF save helpers, ignore operations, photo lookups, and relationship replacement.
- `MainPanel.cs`: lookup cleanup logic.

`AbstractDbStrategy`, `EntryDto`, and `EntryInfo` already provide useful boundaries for the main list. Preserve those contracts where practical. Introduce storage operations for list/detail queries, people and genre lookups, complete-entry saves, deletion, and ignored paths. Pass plain data models to forms; do not expose a replacement DbContext, DbSet, IQueryable provider, or tracking system.

Recommended organization: an SDK-style `Ariadna.Storage` class library with plain models, a connection factory, versioned SQL schema, and focused repositories/services. It replaces the purpose of the legacy project without its framework or code-generation constraints. A separate migration console utility references the storage schema and a SQL Server client, but is not shipped or referenced by the desktop application. Storage tests can then run without constructing WinForms controls.

Use parameterized SQL. Translate relationship filters to `EXISTS`, detail navigation to explicit joins, and cleanup to set-based SQL. Add non-unique indexes for relationship lookups in both directions and frequently used paths. List queries should retrieve IDs, titles, and paths without loading photos. Discovery should load registered and ignored paths in batches rather than making two database queries per scanned file.

## Compatibility decisions

### Schema and types

Keep the initial 19-table layout, column meanings, original IDs, foreign-key nullability, and existing records. Avoid combining collections, externalizing person photos, renaming image files, or removing legacy columns in the same migration.

| SQL Server value | SQLite representation |
|---|---|
| `int` | `INTEGER` |
| `nvarchar`, including MAX | `TEXT` |
| `image` | `BLOB` |
| nullable `bit` | Nullable `INTEGER`, restricted to 0 or 1 when non-null |
| nullable `date` | Nullable ISO date text, `yyyy-MM-dd` |

Use `INTEGER PRIMARY KEY AUTOINCREMENT` for the existing generated IDs. Import explicit IDs, then initialize each sequence to at least the greater of the source identity high-water mark and the maximum imported ID. This avoids reusing identifiers associated with historical assets. SQLite supports explicit ID 0; test its round trip.

Replace EF's required-field and maximum-length validation with explicit storage-boundary validation and suitable database constraints. SQLite type declarations alone do not enforce SQL Server string-length limits. Empty strings and NULL must remain distinct.

### Unicode, dates, and filters

SQLite's default NOCASE and LIKE case handling is ASCII-only, and LIKE does not honor custom collations. Simply replacing the current `ToUpper().Contains(...)` queries with SQLite UPPER or LIKE would break important searches.

Define a named Unicode comparison policy for title ordering, people/genre equality, and Windows-path comparisons. Register the required collation/functions on every connection. For literal substring searches, use a Unicode-aware function or derived search fields with explicit normalization. Keep original text unchanged. Do not interpret `%` or `_` typed into a search field as SQL wildcards.

A .NET Unicode comparer is not automatically identical to SQL Server's existing collation. Compare the old and new results for Cyrillic and Latin case variants, accented characters, punctuation, trailing spaces, and representative title sorting. Document any unavoidable ordering differences before cutover.

Preserve date-only values and the existing recent-filter boundary semantics. The source stores dates even though save code supplies DateTime values. Do not introduce a timezone conversion or regenerate dates from media files. Retain category differences: games currently use a six-month recent window and alphabetical order; movies/documentaries change ordering for recent/new filters. Also preserve the behavior where an unknown person/genre lookup leaves that filter unapplied, unless deliberately changed and tested separately.

### Saves, files, and simultaneous windows

Current saves span several DbContexts/SaveChanges calls. Relationship replacement commits deletion before inserting replacements, and returns immediately for empty selections. New storage operations should save the main record, people, and relationships in one transaction and handle an empty selection explicitly. These are deliberate reliability fixes that need workflow tests.

Return an inserted ID directly from the insert on the same connection. The current code re-queries by file path after saving, which is ambiguous for duplicate paths.

Filesystem image updates are not part of a SQL transaction. Stage images first, retain previous copies, and use a recoverable promotion/compensation procedure if a save or file replacement fails. Do not report a successful save while required images failed. Keep TMDb downloads and MediaInfo work outside database write transactions.

Support the existing ability to open all four modes simultaneously. Enable WAL on the database, explicitly enable foreign keys on each connection, retain durable synchronization settings, use short write transactions, and configure bounded lock timeouts. SQLite still has one writer at a time. Test competing writes and actionable timeout errors. Avoid shared-cache mode with WAL.

Pin a supported provider/native-engine combination and verify `sqlite_version()` in the shipped application. SQLite documents a WAL race fixed in 3.51.3 and later, with selected older-version backports; use a release containing that fix. Do not choose a NuGet version solely by its package number.

Microsoft.Data.Sqlite async methods perform synchronous database work. Put genuinely lengthy imports, backups, or queries on a controlled worker with its own connection so they do not freeze WinForms. Do not share a connection across concurrent workers.

## Implementation sequence and gates

### 1. Establish recovery before changing the application

1. Close all Ariadna windows and suspend catalog/image writes for the backup window.
2. Create a SQL Server full copy-only backup with checksum; back up all four asset directories and relevant configuration. Retain the old executable/source revision and document its runtime requirements. Do not raw-copy attached MDF/LDF files as the only backup.
3. Restore the backup under a separate database name/location and validate that restored database. Backup verification alone does not demonstrate that recovery works.
4. Build a source manifest from that restored snapshot: actual schema, table counts, primary-key sets, identity counters, canonical scalar values, and SHA-256 hashes for BLOBs and external files. Never include the TMDb API key in a public report.

Gate: a demonstrated restore and a matched database/assets backup. The live source remains available unchanged.

### 2. Build and rehearse lossless conversion

1. Create a separate utility that reads the restored source with SELECT-only database access.
2. Produce a versioned export package containing typed table records, photo bytes or explicit BLOB references, external assets, original identifiers, source schema/identity metadata, and hashes. JSON/JSONL plus binary files in a package is suitable; CSV-only export is insufficient for NULL/BLOB fidelity.
3. Build a fresh SQLite staging file from that package using prepared inserts in a transaction, importing parents before relationships. Never append a retry into a partially imported destination; restart with a new staging file.
4. Import all rows, including duplicate relationships and nullable legacy columns. Initialize identity sequences and add the non-unique indexes.
5. Verify all table counts and primary keys, every scalar value after defined type mapping, BLOB hashes, and asset-file hashes. Run `PRAGMA integrity_check` and `PRAGMA foreign_key_check`; the former does not replace the latter.
6. Close all connections and publish the validated candidate only when every comparison passes. Refuse an existing destination unless explicitly working on a new rehearsal candidate.

Gate: zero unexplained differences, a recoverable export, and repeatable conversion. Initial rehearsals never point the existing application at the SQLite candidate.

### 3. Replace EF in the application

Implement the storage layer, then migrate strategy queries and form load/save workflows collection by collection. Tests must exercise the real SQLite implementation, not only LINQ over lists. Keep the production release on LocalDB until all modes are complete; retaining two long-term backends is unnecessary.

Update `Program.cs` and constructors to supply storage dependencies. Replace EF validation handling and navigation loading. Remove SQL from the shared form/main-window helpers. Preserve external media paths, poster naming, discovery exclusions, filters, suggestions, and launch behavior.

Gate: all four modes have complete SQLite read/write behavior, and each complete-entry operation has tested transaction/failure handling.

### 4. Validate behavior and performance

Existing `QueryEntriesBehaviorTests` use in-memory IQueryable sources, and store-workflow tests replace persistence steps. They help characterize intent but do not establish database-provider compatibility.

Add focused integration tests for exact conversion, original IDs and identity sequences, nullable flags, Cyrillic searches, case-insensitive paths, combined filters, unknown lookup values, date boundaries, relationship replacement including empty selections, delete cleanup, rollback after failures, and concurrent connections. Use a disposable database plus disposable image directories for write tests.

Run the normal build/test suite and manually exercise all four native Windows modes on copied data: browse, search, view details, add, edit, clear cast/genres, replace images, discover, ignore, delete, and reopen. Test simultaneous processes on a file-backed SQLite database.

Benchmark startup, full listing, representative combined filters, people suggestions, discovery, and complete-entry saves against the same restored/copy data. SQLite removes server setup and EF tracking overhead, but image decoding and filesystem scans can still dominate perceived speed. Do not promise a measured speedup before benchmarking.

Gate: data comparisons pass, workflow checks pass, and performance meets an agreed practical baseline.

### 5. Final conversion, cutover, and removal

1. Close every legacy app window, repeat the source backup/export/conversion against the latest data, and verify again. A rehearsal conversion becomes stale as soon as new legacy edits occur.
2. Publish the final SQLite file and configure the new executable with an absolute path. Normal startup should use `Mode=ReadWrite` and validate schema/application identity; a missing file must produce a clear recovery/setup error rather than silently create an empty catalog. Empty-library creation is an explicit operation.
3. Retain the original MDF/LDF, verified backup, export package, asset backup, and old executable. Rollback during acceptance uses this matched snapshot. Once edits are accepted in SQLite, returning to the old SQL database would lose those new edits unless separately transferred; never imply automatic reverse synchronization.
4. Remove EF package references, DbProvider references and linked source files, EntityDeploy/EDMX/T4 tooling, obsolete SQL connection configuration, and EF-specific tests/aliases. Update the solution, README, architecture documentation, and deployment requirements. Reassess warning suppressions individually.
5. Verify a clean build/publish and startup on a Windows environment without LocalDB or .NET Framework DbProvider tooling. Ship the SQLite native runtime dependencies.

Gate: the production application has no EF or SQL Server runtime dependency and opens the verified migrated catalog. Removing a machine-wide SQL Server installation is separate from removing Ariadna's dependency, since other software may still use it.

### 6. Keep recovery simple after migration

Use SQLite's backup API for database snapshots, combined with a brief application-wide pause on image/database mutations when capturing a matched backup of external assets. Test restore of those packages. A raw copy of the main SQLite file while WAL writes are active can miss committed data. Sync or archive completed snapshots, rather than treating a live database-file copy as a recovery backup.

Keep explicit schema versions and small transactional schema upgrades, taking a verified backup before future upgrades. A general export/import UI can later reuse the migration package format if it proves useful.

## Remaining verification

The live schema, counts, integrity check, and image filenames were checked. No actual export, restore rehearsal, SQLite import, content-hash comparison, performance benchmark, or migrated native UI test has been performed. These are implementation gates, not completed results. No implementation-time package version has been selected yet.

## Primary references

- [Microsoft.Data.Sqlite overview](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/) — standalone ADO.NET provider; EF is optional.
- [SQLite data mappings](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types) — scalar/BLOB types and unenforced string-length facets.
- [SQLite collation](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/collation) — Unicode custom collations and LIKE limitations.
- [SQLite connection settings](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings) — foreign keys, open modes, timeouts, and WAL/shared-cache guidance.
- [SQLite transactions](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/transactions) — transaction atomicity and single-writer concurrency.
- [SQLite WAL](https://www.sqlite.org/wal.html) — concurrency, file lifecycle, and the fixed WAL-reset race.
- [SQLite AUTOINCREMENT](https://www.sqlite.org/autoinc.html) — avoiding reuse of committed historical IDs.
- [SQLite integrity pragmas](https://www.sqlite.org/pragma.html#pragma_integrity_check) — separate structural and foreign-key checks.
- [Microsoft.Data.Sqlite backup](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/backup) — consistent database snapshots.
- [Microsoft.Data.Sqlite async limitations](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async) — synchronous execution of async ADO.NET methods.
- [SQL Server copy-only backups](https://learn.microsoft.com/en-us/sql/relational-databases/backup-restore/copy-only-backups-sql-server?view=sql-server-ver17) — isolated migration backups.
