# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Versioning rule:

- **Major** - behavior visible to other system components changes (breaking)
- **Minor** - new backward-compatible feature
- **Patch** - backward-compatible bug fix only

## Ariadna

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

### [1.0.0] - 2026-10-03

#### Added

- Direct SQLite catalog operations, explicit schema and Unicode comparison, complete-entry transactions, integrity/backup operations, and recoverable external image writes.

## Ariadna.Migration

### [1.0.0] - 2026-10-03

#### Added

- Lossless SQL export, staged SQLite import, every-value/hash verification, real-source query comparison, and matched SQLite/image snapshots.

## DbProvider

### [1.0.0] - 2026-10-03

#### Added

- Changelog introduced, tracking the current assembly/file version `1.0.0.0` as the `1.0.0` baseline.
