PRAGMA journal_mode=WAL;
BEGIN IMMEDIATE;
CREATE TABLE [Actor] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50),
    [photo] BLOB
);
CREATE INDEX [IX_Actor_Id] ON [Actor]([Id]);
CREATE INDEX [IX_Actor_name] ON [Actor]([name]);
CREATE TABLE [Author] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50),
    [photo] BLOB
);
CREATE INDEX [IX_Author_Id] ON [Author]([Id]);
CREATE INDEX [IX_Author_name] ON [Author]([name]);
CREATE TABLE [Director] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50),
    [photo] BLOB
);
CREATE INDEX [IX_Director_Id] ON [Director]([Id]);
CREATE INDEX [IX_Director_name] ON [Director]([name]);
CREATE TABLE [Documentary] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [title] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title]) <= 150),
    [title_original] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title_original]) <= 150),
    [year] INTEGER NOT NULL,
    [file_path] TEXT NOT NULL COLLATE ARIADNA CHECK (length([file_path]) <= 256),
    [poster] BLOB,
    [description] TEXT COLLATE ARIADNA,
    [creation_time] TEXT,
    [want_to_see] INTEGER CHECK ([want_to_see] IN (0,1))
);
CREATE INDEX [IX_Documentary_Id] ON [Documentary]([Id]);
CREATE INDEX [IX_Documentary_file_path] ON [Documentary]([file_path]);
CREATE TABLE [DocumentaryGenre] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [documentaryId] INTEGER NOT NULL,
    [genreId] INTEGER NOT NULL,
    FOREIGN KEY ([documentaryId]) REFERENCES [Documentary]([Id]),
    FOREIGN KEY ([genreId]) REFERENCES [GenreOfDocumentary]([Id])
);
CREATE INDEX [IX_DocumentaryGenre_Id] ON [DocumentaryGenre]([Id]);
CREATE INDEX [IX_DocumentaryGenre_documentaryId] ON [DocumentaryGenre]([documentaryId]);
CREATE INDEX [IX_DocumentaryGenre_genreId] ON [DocumentaryGenre]([genreId]);
CREATE TABLE [Game] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [title] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title]) <= 150),
    [title_original] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title_original]) <= 150),
    [year] INTEGER NOT NULL,
    [file_path] TEXT NOT NULL COLLATE ARIADNA CHECK (length([file_path]) <= 256),
    [version] TEXT COLLATE ARIADNA CHECK (length([version]) <= 64),
    [creation_time] TEXT,
    [want_to_play] INTEGER CHECK ([want_to_play] IN (0,1)),
    [vr] INTEGER CHECK ([vr] IN (0,1))
);
CREATE INDEX [IX_Game_Id] ON [Game]([Id]);
CREATE INDEX [IX_Game_file_path] ON [Game]([file_path]);
CREATE TABLE [GameGenre] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [gameId] INTEGER NOT NULL,
    [genreId] INTEGER NOT NULL,
    FOREIGN KEY ([gameId]) REFERENCES [Game]([Id]),
    FOREIGN KEY ([genreId]) REFERENCES [GenreOfGame]([Id])
);
CREATE INDEX [IX_GameGenre_Id] ON [GameGenre]([Id]);
CREATE INDEX [IX_GameGenre_gameId] ON [GameGenre]([gameId]);
CREATE INDEX [IX_GameGenre_genreId] ON [GameGenre]([genreId]);
CREATE TABLE [Genre] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50)
);
CREATE INDEX [IX_Genre_Id] ON [Genre]([Id]);
CREATE INDEX [IX_Genre_name] ON [Genre]([name]);
CREATE TABLE [GenreOfDocumentary] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50)
);
CREATE INDEX [IX_GenreOfDocumentary_Id] ON [GenreOfDocumentary]([Id]);
CREATE INDEX [IX_GenreOfDocumentary_name] ON [GenreOfDocumentary]([name]);
CREATE TABLE [GenreOfGame] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50)
);
CREATE INDEX [IX_GenreOfGame_Id] ON [GenreOfGame]([Id]);
CREATE INDEX [IX_GenreOfGame_name] ON [GenreOfGame]([name]);
CREATE TABLE [GenreOfLibrary] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] TEXT NOT NULL COLLATE ARIADNA CHECK (length([name]) <= 50)
);
CREATE INDEX [IX_GenreOfLibrary_Id] ON [GenreOfLibrary]([Id]);
CREATE INDEX [IX_GenreOfLibrary_name] ON [GenreOfLibrary]([name]);
CREATE TABLE [Ignore] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [path] TEXT NOT NULL COLLATE ARIADNA CHECK (length([path]) <= 256)
);
CREATE INDEX [IX_Ignore_Id] ON [Ignore]([Id]);
CREATE INDEX [IX_Ignore_path] ON [Ignore]([path]);
CREATE TABLE [Library] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [title] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title]) <= 150),
    [title_original] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title_original]) <= 150),
    [year] INTEGER NOT NULL,
    [poster] BLOB,
    [file_path] TEXT NOT NULL COLLATE ARIADNA CHECK (length([file_path]) <= 256),
    [description] TEXT COLLATE ARIADNA,
    [creation_time] TEXT,
    [want_to_see] INTEGER CHECK ([want_to_see] IN (0,1))
);
CREATE INDEX [IX_Library_Id] ON [Library]([Id]);
CREATE INDEX [IX_Library_file_path] ON [Library]([file_path]);
CREATE TABLE [LibraryAuthor] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [libraryId] INTEGER,
    [authorId] INTEGER,
    FOREIGN KEY ([authorId]) REFERENCES [Author]([Id]),
    FOREIGN KEY ([libraryId]) REFERENCES [Library]([Id])
);
CREATE INDEX [IX_LibraryAuthor_Id] ON [LibraryAuthor]([Id]);
CREATE INDEX [IX_LibraryAuthor_libraryId] ON [LibraryAuthor]([libraryId]);
CREATE INDEX [IX_LibraryAuthor_authorId] ON [LibraryAuthor]([authorId]);
CREATE TABLE [LibraryGenre] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [libraryId] INTEGER NOT NULL,
    [genreId] INTEGER NOT NULL,
    FOREIGN KEY ([genreId]) REFERENCES [GenreOfLibrary]([Id]),
    FOREIGN KEY ([libraryId]) REFERENCES [Library]([Id])
);
CREATE INDEX [IX_LibraryGenre_Id] ON [LibraryGenre]([Id]);
CREATE INDEX [IX_LibraryGenre_libraryId] ON [LibraryGenre]([libraryId]);
CREATE INDEX [IX_LibraryGenre_genreId] ON [LibraryGenre]([genreId]);
CREATE TABLE [Movie] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [title] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title]) <= 150),
    [title_original] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title_original]) <= 150),
    [year] INTEGER NOT NULL,
    [poster] BLOB,
    [file_path] TEXT NOT NULL COLLATE ARIADNA CHECK (length([file_path]) <= 256),
    [description] TEXT COLLATE ARIADNA,
    [creation_time] TEXT,
    [want_to_see] INTEGER CHECK ([want_to_see] IN (0,1))
);
CREATE INDEX [IX_Movie_Id] ON [Movie]([Id]);
CREATE INDEX [IX_Movie_file_path] ON [Movie]([file_path]);
CREATE TABLE [MovieCast] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [movieId] INTEGER,
    [actorId] INTEGER,
    FOREIGN KEY ([actorId]) REFERENCES [Actor]([Id]),
    FOREIGN KEY ([movieId]) REFERENCES [Movie]([Id])
);
CREATE INDEX [IX_MovieCast_Id] ON [MovieCast]([Id]);
CREATE INDEX [IX_MovieCast_movieId] ON [MovieCast]([movieId]);
CREATE INDEX [IX_MovieCast_actorId] ON [MovieCast]([actorId]);
CREATE TABLE [MovieDirector] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [movieId] INTEGER,
    [directorId] INTEGER,
    FOREIGN KEY ([directorId]) REFERENCES [Director]([Id]),
    FOREIGN KEY ([movieId]) REFERENCES [Movie]([Id])
);
CREATE INDEX [IX_MovieDirector_Id] ON [MovieDirector]([Id]);
CREATE INDEX [IX_MovieDirector_movieId] ON [MovieDirector]([movieId]);
CREATE INDEX [IX_MovieDirector_directorId] ON [MovieDirector]([directorId]);
CREATE TABLE [MovieGenre] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [movieId] INTEGER NOT NULL,
    [genreId] INTEGER NOT NULL,
    FOREIGN KEY ([genreId]) REFERENCES [Genre]([Id]),
    FOREIGN KEY ([movieId]) REFERENCES [Movie]([Id])
);
CREATE INDEX [IX_MovieGenre_Id] ON [MovieGenre]([Id]);
CREATE INDEX [IX_MovieGenre_movieId] ON [MovieGenre]([movieId]);
CREATE INDEX [IX_MovieGenre_genreId] ON [MovieGenre]([genreId]);
PRAGMA application_id=1095911745;
CREATE TABLE CatalogAssetCommit (token TEXT PRIMARY KEY NOT NULL);
PRAGMA user_version=1;
COMMIT;
