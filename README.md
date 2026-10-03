# Ariadna

A Windows desktop app for managing a personal media library — movies, documentaries, games, and books — with poster thumbnails and rich metadata.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET-9.0-purple)
![Language](https://img.shields.io/badge/language-C%23-239120)

**Status:** the WinForms catalog uses EF6 and SQL Server LocalDB. SQLite migration
has been investigated and proposed; conversion and cutover remain unimplemented.
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
- SQL Server LocalDB
- .NET 9 SDK and .NET Framework 4.8 build tooling for the legacy `DbProvider` project
- Paths to your media folders and a [TMDb API key](https://developer.themoviedb.org/docs/getting-started) configured in `Ariadna/App.config`

## Getting started

Open `Ariadna.sln` in Rider or Visual Studio with the required Windows/.NET
Framework tooling. Restore both SDK-style package references and DbProvider's
legacy `packages.config` dependencies before building.

The existing CLI build/test commands are below. They have not been rerun during
the 2026-10-03 documentation alignment; a clean-machine restore/build/test recipe
remains a task in docs/PLAN.md.

```powershell
dotnet build Ariadna.sln -c Release
dotnet test Ariadna.Tests/Ariadna.Tests.csproj -c Release
```

Close running instances before rebuilding. After a successful build, launch
`Ariadna/bin/Release/net9.0-windows8.0/Ariadna.exe` with one of the modes above;
no argument defaults to movies. Configure disposable catalog/image locations
before running write checks. There is currently no `global.json` or NuGet
lock-file setup; Balancia's `--locked-mode` commands are not established here.

## Documentation

- [Changelog](CHANGELOG.md) — notable changes and version history by project
- [Technical Design Description](docs/Technical_Design_Description.md) — primary
  project reference for product behavior, architecture, configuration, and decisions
- [Implementation plan](docs/PLAN.md) — remaining/upcoming tasks and completion gates
- [SQLite migration investigation](docs/SQLITE-MIGRATION-PLAN.md) — dated evidence
  and a detailed proposal; migration has not been implemented
- [AGENTS.md](AGENTS.md) / [MEMORY.md](MEMORY.md) — operational instructions and
  compact working handoff for contributors and agents

## Private data

The catalog, poster/preview directories, and TMDb credentials are local user
data. Committed fixtures and examples should use synthetic values. Configuration
currently contains local paths and an API-key setting: inspect it locally without
copying credential values into reports, screenshots, or logs. Database recovery
must include matching external images; ignore rules alone do not protect secrets.
