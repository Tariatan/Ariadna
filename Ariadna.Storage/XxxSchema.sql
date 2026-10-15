CREATE TABLE IF NOT EXISTS [Xxx] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [title] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title]) <= 150),
    [title_original] TEXT NOT NULL COLLATE ARIADNA CHECK (length([title_original]) <= 150),
    [year] INTEGER NOT NULL,
    [file_path] TEXT NOT NULL COLLATE ARIADNA CHECK (length([file_path]) <= 256),
    [creation_time] TEXT,
    [want_to_see] INTEGER CHECK ([want_to_see] IN (0,1)),
    [vr] INTEGER CHECK ([vr] IN (0,1))
);
CREATE INDEX IF NOT EXISTS [IX_Xxx_file_path] ON [Xxx]([file_path]);
CREATE TABLE IF NOT EXISTS [XxxCast] (
    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [xxxId] INTEGER NOT NULL REFERENCES [Xxx]([Id]),
    [actorId] INTEGER NOT NULL REFERENCES [Actor]([Id])
);
CREATE INDEX IF NOT EXISTS [IX_XxxCast_xxxId] ON [XxxCast]([xxxId]);
CREATE INDEX IF NOT EXISTS [IX_XxxCast_actorId] ON [XxxCast]([actorId]);
