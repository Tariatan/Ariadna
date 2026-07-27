# Ariadna

A Windows desktop app for managing a personal media library — movies, documentaries, games, and books — with poster thumbnails and rich metadata.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Framework](https://img.shields.io/badge/.NET-9.0-purple)
![Language](https://img.shields.io/badge/language-C%23-239120)

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
- Paths to your media folders and a [TMDb API key](https://developer.themoviedb.org/docs/getting-started) configured in `Ariadna/App.config`

## Documentation

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for a full description of the project structure, architecture, configuration reference, and dependency list.