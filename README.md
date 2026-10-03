# Ariadna

A Windows desktop app for managing a personal media library — movies, documentaries, games, and books — with poster thumbnails and rich metadata.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET-9.0-purple)
![Language](https://img.shields.io/badge/language-C%23-239120)

**Status:** the WinForms catalog now uses SQLite through a focused storage layer.
Entity Framework, SQL Server LocalDB, and the legacy DbProvider project are removed.
See [docs/PLAN.md](docs/PLAN.md) for remaining/upcoming tasks.

---

![Ariadna poster grid](docs/view.png)

---

Launch with one argument to pick your collection:

```
Ariadna.exe movies          # Movies & TV series
Ariadna.exe documentaries   # Documentaries
Ariadna.exe games           # PC / VR games
Ariadna.exe library         # Books & documents
```

Each mode has its own color theme, toolbar, and genre set. You can run all four simultaneously.

## What it does

- Browse the full collection as a scrollable poster grid
- Filter by title, director/author, actor, genre, or quick toggles (wishlist, new, series, VR)
- Open video files in MPC-HC or browse directories in Total Commander with one click
- Fetch movie metadata and posters automatically from TMDb
- Add entries manually or let the app discover new files on disk
- Edit full metadata in a detail form — cast, genres, file info, MediaInfo stats

## Requirements

- Windows 10+ (x64), .NET 9.0
- .NET 9 SDK for development; the self-contained Windows publish includes its runtime
- A verified SQLite catalog and its matching image folders
- Paths to your media folders and a [TMDb API key](https://developer.themoviedb.org/docs/getting-started) configured in `Ariadna/App.config`

## Getting started

Open `Ariadna.sln` in Rider or Visual Studio with the .NET 9 SDK. All production
projects use SDK-style package references and checked-in NuGet lock files:

```powershell
dotnet restore Ariadna.sln --locked-mode
dotnet build Ariadna.sln -c Release --no-restore
dotnet test Ariadna.sln -c Release --no-build --no-restore
dotnet restore Ariadna.Migration.Tests/Ariadna.Migration.Tests.csproj --locked-mode
dotnet test Ariadna.Migration.Tests/Ariadna.Migration.Tests.csproj -c Release --no-restore
dotnet publish Ariadna/Ariadna.csproj -c Release -r win-x64 --self-contained true -o publish
```

Close running instances before rebuilding. Launch
`Ariadna/bin/Release/net9.0-windows8.0/Ariadna.exe` with a mode above; no argument
defaults to movies. A missing catalog produces a recovery error instead of creating
an empty database. The separate `Ariadna.Migration` utility provides lossless export,
import, verification, and matched SQLite/image snapshots. See
[SQLite operation and recovery](docs/SQLITE-OPERATIONS.md) for configuration,
disposable test overrides, and backup/restore commands.

Release builds/tests and a self-contained win-x64 publish were verified during the
migration. Version 2.0.1 removes Windows API Code Pack and uses the bundled MediaInfo
reader for duration; existing-video regression tests cover details opening. There
is no SDK pin or clean-machine installation test.

## Documentation

- [Changelog](CHANGELOG.md) — notable changes and version history by project
- [Technical Design Description](docs/Technical_Design_Description.md) — primary
  project reference for product behavior, architecture, configuration, and decisions
- [Implementation plan](docs/PLAN.md) — remaining/upcoming tasks and completion gates
- [SQLite operation and recovery](docs/SQLITE-OPERATIONS.md) — implemented format, configuration, verification, and backup/restore
- [SQLite migration investigation](docs/SQLITE-MIGRATION-PLAN.md) — original dated evidence and migration rationale
- [AGENTS.md](AGENTS.md) / [MEMORY.md](MEMORY.md) — operational instructions and
  compact working handoff for contributors and agents

## Private data

The catalog, poster/preview directories, and TMDb credentials are local user
data. Committed fixtures and examples should use synthetic values. Configuration
currently contains local paths and an API-key setting: inspect it locally without
copying credential values into reports, screenshots, or logs. Database recovery
must include matching external images; ignore rules alone do not protect secrets.
