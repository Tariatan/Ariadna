# SQLite catalog and recovery

## Current storage contract

The WPF application uses `Ariadna.Storage` and direct `Microsoft.Data.Sqlite`
commands. The catalog has application ID `0x41524941` and schema version 2.
Startup refuses missing files, foreign databases and unsupported schema versions.
The shared `Person` table identifies external portraits; `Xxx` and `XxxCast`
provide the fifth collection. Current package versions are in project files
and their checked-in dependency locks.

Connections use WAL, synchronous FULL, a five-second busy timeout and no pooling.
A named mutex serializes complete-entry/image writes. Metadata and selected
relationships save in one transaction, preserving unchanged relationship identities.
Text uses the registered `ARIADNA` en-US case-insensitive collation and Unicode-aware
literal search. Generic SQLite tools do not register that collation automatically.

Poster PNGs have extensionless `{id}` filenames. Game previews append the configured
suffix and numbers 1–4; XXX has one screenshot. Shared portraits are 200×300 PNGs
named `{personId}.png`. Keep IDs, filenames and configured roots together.

## Interrupted saves

Images stage beside the catalog with durable old/new copies and a flushed journal.
Promotion precedes the database commit; a commit token lets startup restore old
images after an interrupted save or retain images after a successful commit.
Startup or the next save removes completed journals. Do not delete nonempty
journals manually: they may be required for rollback. Keep the catalog and recovery
journals on local storage with all configured asset folders accessible.

## Configuration and isolated checks

Use `Ariadna/App.config` for the catalog and asset roots. For disposable checks:

```powershell
$env:ARIADNA_CATALOG_PATH = 'C:\Ariadna-QA\catalog.sqlite'
$env:ARIADNA_ASSET_ROOT = 'C:\Ariadna-QA\assets'
```

The overridden asset root contains `movies`, `documentary`, `games`, `library`,
`people` and `xxx`. Remove the overrides to return to normal configuration.
Use synthetic catalogs and disposable directories for write tests.

## Backup and restore after retirement of the migration utility

The former CLI snapshot, import, verify and portrait-reset commands are retired.
There is no replacement backup CLI in this repository.

For a filesystem backup, close Ariadna and all other catalog connections after a
successful startup/recovery. Copy the complete catalog directory and every configured
asset root into a new backup directory, together with the application configuration.
Keep any SQLite WAL/SHM files and recovery journals with the database; never copy
only the live main `.sqlite` file. SQLite's backup API is an alternative for the
database, but does not capture external images: writers must remain stopped while
capturing the matched dataset. Automatic pre-upgrade database backups alone are
not complete image backups.

Restore into new, separate directories while the application is closed. Restore
the database and all matching assets together, then configure their paths together.
Do not merge restored images into existing asset folders. Retain the original
backup unchanged; open the restored copy and check all five catalogs, details,
posters, previews and portraits before accepting new edits. An old schema backup
requires a compatible retained executable; the current app does not open schema 1.

The storage integration suite checks real-provider queries, transactions, integrity
and interrupted-image recovery. It does not verify a user's filesystem backup.
