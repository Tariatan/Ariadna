using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Ariadna.Storage;

public sealed class CatalogStore(CatalogDatabase database, string? peopleRoot = null)
{
    private readonly string portraitRoot = GetPortraitRoot(peopleRoot ?? Path.Combine(database.DirectoryPath, "people"));

    private static string GetPortraitRoot(string root)
    {
        if (!Path.IsPathFullyQualified(root))
        {
            throw new ArgumentException("The shared portrait directory must be an absolute path.", nameof(root));
        }
        return Path.GetFullPath(root);
    }
    public IReadOnlyCollection<CatalogEntry> Query(CatalogKind kind, CatalogQuery query)
    {
        var layout = CatalogLayout.For(kind);
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        var conditions = new List<string>();
        if (!string.IsNullOrEmpty(query.Name))
        {
            conditions.Add("(contains_text(e.title,$name) OR contains_text(e.title_original,$name) OR contains_text(e.file_path,$name))");
            command.Parameters.AddWithValue("$name", query.Name);
        }

        if (kind != CatalogKind.Xxx)
        {
            AddLookupFilter(command, conditions, layout.GenreTable, layout.GenreRelation, layout.EntryKey, "genreId", query.Genre, "$genre", !query.RequireGenreMatch);
        }
        if (kind == CatalogKind.Xxx)
        {
            AddLookupFilter(command, conditions, "Actor", "XxxCast", "xxxId", "actorId", query.Actor, "$actor");
        }
        if (kind == CatalogKind.Movie)
        {
            AddLookupFilter(command, conditions, "Director", "MovieDirector", "movieId", "directorId", query.Director, "$director");
            AddLookupFilter(command, conditions, "Actor", "MovieCast", "movieId", "actorId", query.Actor, "$actor");
        }
        else if (kind == CatalogKind.Library)
        {
            AddLookupFilter(command, conditions, "Author", "LibraryAuthor", "libraryId", "authorId", query.Director, "$author");
        }

        if (query.Wish)
        {
            conditions.Add($"e.[{layout.WantedColumn}]=1");
        }

        if (query.Recent)
        {
            conditions.Add("e.creation_time > $recent");
            // Source dates compare as midnight DateTimes, including the cutoff's time of day.
            command.Parameters.AddWithValue("$recent", query.Now.AddMonths(kind == CatalogKind.Game ? -6 : -query.RecentMonths).ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture));
        }

        if (query.New)
        {
            conditions.Add("e.year IN ($year,$previousYear)");
            command.Parameters.AddWithValue("$year", query.Now.Year);
            command.Parameters.AddWithValue("$previousYear", query.Now.Year - 1);
        }

        if (kind is CatalogKind.Game or CatalogKind.Xxx)
        {
            if (query.Vr)
            {
                conditions.Add("e.vr=1");
            }
            if (query.NonVr)
            {
                conditions.Add("e.vr=0");
            }
        }

        if (kind == CatalogKind.Movie && (query.Series || query.Movies))
        {
            command.Parameters.AddWithValue("$temporary", query.TemporaryMoviesPath);
            if (query.Series)
            {
                conditions.Add("(starts_text(e.file_path,$series) AND NOT contains_text(e.file_path,$temporary))");
                command.Parameters.AddWithValue("$series", query.SeriesDrive);
            }
            if (query.Movies)
            {
                conditions.Add("(starts_text(e.file_path,$movies) OR contains_text(e.file_path,$temporary))");
                command.Parameters.AddWithValue("$movies", query.MoviesDrive);
            }
        }

        var order = kind is CatalogKind.Movie or CatalogKind.Documentary && (query.Recent || query.New)
            ? "e.creation_time DESC,e.Id" : "e.title COLLATE ARIADNA,e.Id";
        var where = conditions.Count == 0 ? string.Empty : $" WHERE {string.Join(" AND ", conditions)}";
        command.CommandText = $"SELECT e.Id,e.title,e.title_original,e.file_path,e.year,e.creation_time,e.[{layout.WantedColumn}] FROM [{layout.Table}] e{where} ORDER BY {order}";
        using var reader = command.ExecuteReader();
        var entries = new List<CatalogEntry>();
        while (reader.Read())
        {
            entries.Add(ReadEntry(reader));
        }
        return entries;
    }

    public CatalogEntry? GetEntry(CatalogKind kind, int id)
    {
        using var connection = database.Open();
        return GetEntry(connection, kind, id);
    }

    private static CatalogEntry? GetEntry(SqliteConnection connection, CatalogKind kind, int id)
    {
        var layout = CatalogLayout.For(kind);
        using var command = connection.CreateCommand();
        var extraColumns = kind == CatalogKind.Game ? "version,vr" : kind == CatalogKind.Xxx ? "vr" : "description";
        command.CommandText = $"SELECT Id,title,title_original,file_path,year,creation_time,[{layout.WantedColumn}],{extraColumns} FROM [{layout.Table}] WHERE Id=$id";
        command.Parameters.AddWithValue("$id", id);
        CatalogEntry entry;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                return null;
            }
            entry = ReadEntry(reader);
            if (kind == CatalogKind.Game)
            {
                entry.Version = reader.IsDBNull(7) ? null : reader.GetString(7);
                entry.Vr = reader.IsDBNull(8) ? null : reader.GetBoolean(8);
            }
            else if (kind == CatalogKind.Xxx)
            {
                entry.Vr = reader.IsDBNull(7) ? null : reader.GetBoolean(7);
            }
            else
            {
                entry.Description = reader.IsDBNull(7) ? null : reader.GetString(7);
            }
        }

        return entry;
    }

    public CatalogDetails? GetDetails(CatalogKind kind, int id)
    {
        using var connection = database.Open();
        var entry = GetEntry(connection, kind, id);
        if (entry == null)
        {
            return null;
        }
        var layout = CatalogLayout.For(kind);
        using var command = connection.CreateCommand();
        command.Parameters.AddWithValue("$id", id);
        command.CommandText = kind == CatalogKind.Xxx ? "SELECT NULL WHERE 0" : $"SELECT g.name FROM [{layout.GenreRelation}] r JOIN [{layout.GenreTable}] g ON g.Id=r.genreId WHERE r.[{layout.EntryKey}]=$id ORDER BY r.Id";
        var genres = new List<string>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                genres.Add(reader.GetString(0));
            }
        }

        var directors = kind == CatalogKind.Movie ? ReadPeople(connection, "Director", "MovieDirector", "movieId", "directorId", id)
            : kind == CatalogKind.Library ? ReadPeople(connection, "Author", "LibraryAuthor", "libraryId", "authorId", id) : [];
        var actors = kind == CatalogKind.Movie ? ReadPeople(connection, "Actor", "MovieCast", "movieId", "actorId", id)
            : kind == CatalogKind.Xxx ? ReadPeople(connection, "Actor", "XxxCast", "xxxId", "actorId", id) : [];
        return new CatalogDetails(entry, genres, directors, actors);
    }

    public int FindId(CatalogKind kind, string path)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT Id FROM [{CatalogLayout.For(kind).Table}] WHERE file_path=$path COLLATE ARIADNA ORDER BY Id LIMIT 1";
        command.Parameters.AddWithValue("$path", path);
        return command.ExecuteScalar() is long id ? checked((int)id) : -1;
    }

    public IReadOnlyCollection<string> GetRegisteredPaths(CatalogKind kind)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT file_path FROM [{CatalogLayout.For(kind).Table}] UNION ALL SELECT path FROM [Ignore]";
        using var reader = command.ExecuteReader();
        var paths = new List<string>();
        while (reader.Read())
        {
            paths.Add(reader.GetString(0));
        }
        return paths;
    }

    public static bool PathsEqual(string left, string right) => CatalogDatabase.Compare(left, right) == 0;

    public IReadOnlyCollection<string> GetGenres(CatalogKind kind)
    {
        if (kind == CatalogKind.Xxx)
        {
            return [];
        }
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM [{CatalogLayout.For(kind).GenreTable}] ORDER BY name COLLATE ARIADNA";
        using var reader = command.ExecuteReader();
        var names = new List<string>();
        while (reader.Read())
        {
            names.Add(reader.GetString(0));
        }
        return names;
    }

    public PersonPhoto? FindPerson(string name, bool actor = false, bool author = false)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        var table = actor ? "Actor" : author ? "Author" : "Director";
        command.CommandText = $"SELECT r.name,r.Id,p.Id,p.tmdbId FROM [{table}] r JOIN Person p ON p.Id=r.personId WHERE r.name=$name COLLATE ARIADNA ORDER BY r.Id LIMIT 1";
        command.Parameters.AddWithValue("$name", name);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadPerson(reader) : null;
    }

    public PersonPhoto? FindPerson(int tmdbId, bool actor = false, bool author = false)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        var table = actor ? "Actor" : author ? "Author" : "Director";
        command.CommandText = $"SELECT COALESCE(r.name,''),COALESCE(r.Id,-1),p.Id,p.tmdbId FROM Person p LEFT JOIN [{table}] r ON r.personId=p.Id WHERE p.tmdbId=$tmdbId ORDER BY r.Id LIMIT 1";
        command.Parameters.AddWithValue("$tmdbId", tmdbId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadPerson(reader) : null;
    }

    public IReadOnlyCollection<PersonPhoto> SuggestPeople(bool actors, bool authors, string name, int limit)
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        var table = actors ? "Actor" : authors ? "Author" : "Director";
        var relation = actors ? "MovieCast" : authors ? "LibraryAuthor" : "MovieDirector";
        var key = actors ? "actorId" : authors ? "authorId" : "directorId";
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='XxxCast'";
        var xxxActors = actors && (long)command.ExecuteScalar()! != 0
            ? " OR EXISTS (SELECT 1 FROM XxxCast r WHERE r.actorId=p.Id)" : string.Empty;
        command.CommandText = $"SELECT p.name,p.Id,i.Id,i.tmdbId FROM [{table}] p JOIN Person i ON i.Id=p.personId WHERE contains_text(p.name,$name) AND (EXISTS (SELECT 1 FROM [{relation}] r WHERE r.[{key}]=p.Id){xxxActors}) ORDER BY p.name COLLATE ARIADNA,p.Id LIMIT $limit";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$limit", limit);
        using var reader = command.ExecuteReader();
        var people = new List<PersonPhoto>();
        while (reader.Read())
        {
            people.Add(ReadPerson(reader));
        }
        return people;
    }

    public void Ignore(string path)
    {
        ValidateText(path, "Ignored path", 256);
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO [Ignore](path) SELECT $path WHERE NOT EXISTS (SELECT 1 FROM [Ignore] WHERE path=$path COLLATE ARIADNA)";
        command.Parameters.AddWithValue("$path", path);
        command.ExecuteNonQuery();
    }

    public int Save(CatalogKind kind, CatalogDetails details, CatalogAssets? assets = null)
    {
        Validate(details);
        if (kind == CatalogKind.Xxx && details.Actors.Count > 3)
        {
            throw new ArgumentException("XXX entries support at most three actors.", nameof(details));
        }
        assets ??= new CatalogAssets(database.DirectoryPath, new Dictionary<string, byte[]>());
        using var writeLock = database.AcquireWriteLock();
        database.RecoverAssetsWhileLocked();
        var layout = CatalogLayout.For(kind);
        using var connection = database.Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var entry = details.Entry;
        var columns = new List<string> { "title", "title_original", "file_path", "year", "creation_time", layout.WantedColumn };
        var values = new List<object?> { entry.Title, entry.OriginalTitle, entry.Path, entry.Year, entry.CreationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), entry.Wanted };
        if (kind == CatalogKind.Game)
        {
            columns.AddRange(["version", "vr"]);
            values.AddRange([entry.Version, entry.Vr]);
        }
        else if (kind == CatalogKind.Xxx)
        {
            columns.Add("vr");
            values.Add(entry.Vr);
        }
        else
        {
            columns.Add("description");
            values.Add(entry.Description);
        }

        foreach (var (value, index) in values.Select((value, index) => (value, index)))
        {
            command.Parameters.AddWithValue($"$p{index}", value ?? DBNull.Value);
        }
        var id = entry.Id;
        if (id < 0)
        {
            command.CommandText = $"INSERT INTO [{layout.Table}]({string.Join(",", columns.Select(column => $"[{column}]"))}) VALUES ({string.Join(",", values.Select((_, index) => $"$p{index}"))}) RETURNING Id";
            id = checked((int)(long)command.ExecuteScalar()!);
        }
        else
        {
            command.Parameters.AddWithValue("$id", id);
            command.CommandText = $"UPDATE [{layout.Table}] SET {string.Join(",", columns.Select((column, index) => $"[{column}]=$p{index}"))} WHERE Id=$id";
            if (command.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException("The entry was removed by another window. Reopen the collection before saving.");
            }
        }

        if (kind != CatalogKind.Xxx)
        {
            ReplaceGenres(connection, transaction, layout, id, details.Genres);
        }
        else
        {
            ReplacePeople(connection, transaction, "Actor", "XxxCast", "xxxId", "actorId", id, details.Actors, assets);
        }
        if (kind == CatalogKind.Movie)
        {
            ReplacePeople(connection, transaction, "Actor", "MovieCast", "movieId", "actorId", id, details.Actors, assets);
            ReplacePeople(connection, transaction, "Director", "MovieDirector", "movieId", "directorId", id, details.Directors, assets);
        }
        else if (kind == CatalogKind.Library)
        {
            ReplacePeople(connection, transaction, "Author", "LibraryAuthor", "libraryId", "authorId", id, details.Directors, assets);
        }

        try
        {
            if (assets.HasImages)
            {
                assets.Prepare(database.DirectoryPath, id);
                Execute(connection, transaction, "INSERT INTO CatalogAssetCommit(token) VALUES ($token)", ("$token", assets.Token));
                assets.Promote();
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            assets?.Rollback();
            throw;
        }
        // The next save or startup finalizes committed journals. Cleanup cannot turn a
        // successful commit into a reported failure or cause a duplicate insert on retry.
        return id;
    }

    public bool Delete(CatalogKind kind, int id)
    {
        using var writeLock = database.AcquireWriteLock();
        database.RecoverAssetsWhileLocked();
        var layout = CatalogLayout.For(kind);
        using var connection = database.Open();
        using var transaction = connection.BeginTransaction();
        if (kind != CatalogKind.Xxx)
        {
            Execute(connection, transaction, $"DELETE FROM [{layout.GenreRelation}] WHERE [{layout.EntryKey}]=$id", ("$id", id));
        }
        else
        {
            Execute(connection, transaction, "DELETE FROM XxxCast WHERE xxxId=$id", ("$id", id));
        }
        if (kind == CatalogKind.Movie)
        {
            Execute(connection, transaction, "DELETE FROM MovieCast WHERE movieId=$id; DELETE FROM MovieDirector WHERE movieId=$id", ("$id", id));
        }
        else if (kind == CatalogKind.Library)
        {
            Execute(connection, transaction, "DELETE FROM LibraryAuthor WHERE libraryId=$id", ("$id", id));
        }
        var deleted = Execute(connection, transaction, $"DELETE FROM [{layout.Table}] WHERE Id=$id", ("$id", id)) == 1;
        transaction.Commit();
        return deleted;
    }

    public void DeleteUnusedMovieGenres()
    {
        using var connection = database.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Genre WHERE NOT EXISTS (SELECT 1 FROM MovieGenre WHERE genreId=Genre.Id)";
        command.ExecuteNonQuery();
    }

    private static void AddLookupFilter(SqliteCommand command, List<string> conditions, string lookup, string relation, string entryKey, string lookupKey, string? value, string parameter, bool ignoreUnknown = true)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        command.Parameters.AddWithValue(parameter, value);
        var unknownLookup = ignoreUnknown ? $"NOT EXISTS (SELECT 1 FROM [{lookup}] WHERE name={parameter} COLLATE ARIADNA) OR " : string.Empty;
        conditions.Add($"({unknownLookup}EXISTS (SELECT 1 FROM [{relation}] r WHERE r.[{entryKey}]=e.Id AND r.[{lookupKey}]=(SELECT Id FROM [{lookup}] WHERE name={parameter} COLLATE ARIADNA ORDER BY Id LIMIT 1)))");
    }

    private static CatalogEntry ReadEntry(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        OriginalTitle = reader.GetString(2),
        Path = reader.GetString(3),
        Year = reader.GetInt32(4),
        CreationDate = reader.IsDBNull(5) ? null : DateOnly.ParseExact(reader.GetString(5), "yyyy-MM-dd", CultureInfo.InvariantCulture),
        Wanted = reader.IsDBNull(6) ? null : reader.GetBoolean(6),
    };

    private PersonPhoto ReadPerson(SqliteDataReader reader)
    {
        var personId = reader.GetInt32(2);
        var file = Path.Combine(portraitRoot, $"{personId}.png");
        return new PersonPhoto(reader.GetString(0), File.Exists(file) ? File.ReadAllBytes(file) : null)
        {
            Id = reader.GetInt32(1),
            PersonId = personId,
            TmdbId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
        };
    }

    private IReadOnlyCollection<PersonPhoto> ReadPeople(SqliteConnection connection, string table, string relation, string entryKey, string personKey, int id, SqliteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT p.name,p.Id,i.Id,i.tmdbId FROM [{relation}] r JOIN [{table}] p ON p.Id=r.[{personKey}] JOIN Person i ON i.Id=p.personId WHERE r.[{entryKey}]=$id ORDER BY r.Id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        var people = new List<PersonPhoto>();
        while (reader.Read())
        {
            people.Add(ReadPerson(reader));
        }
        return people;
    }

    private static void ReplaceGenres(SqliteConnection connection, SqliteTransaction transaction, CatalogLayout layout, int id, IEnumerable<string> names)
    {
        var values = names.ToArray();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT g.name FROM [{layout.GenreRelation}] r JOIN [{layout.GenreTable}] g ON g.Id=r.genreId WHERE r.[{layout.EntryKey}]=$id ORDER BY r.Id";
        command.Parameters.AddWithValue("$id", id);
        var existing = new List<string>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                existing.Add(reader.GetString(0));
            }
        }
        // Unedited imported relationships may contain duplicates and retain their original IDs.
        if (existing.SequenceEqual(values, StringComparer.Ordinal))
        {
            return;
        }
        Execute(connection, transaction, $"DELETE FROM [{layout.GenreRelation}] WHERE [{layout.EntryKey}]=$id", ("$id", id));
        foreach (var name in values.Distinct(StringComparer.Ordinal))
        {
            Execute(connection, transaction, $"INSERT INTO [{layout.GenreTable}](name) SELECT $name WHERE NOT EXISTS (SELECT 1 FROM [{layout.GenreTable}] WHERE name=$name COLLATE ARIADNA)", ("$name", name));
            Execute(connection, transaction, $"INSERT INTO [{layout.GenreRelation}]([{layout.EntryKey}],genreId) SELECT $id,Id FROM [{layout.GenreTable}] WHERE name=$name COLLATE ARIADNA ORDER BY Id LIMIT 1", ("$id", id), ("$name", name));
        }
    }

    private void ReplacePeople(SqliteConnection connection, SqliteTransaction transaction, string table, string relation, string entryKey, string personKey, int id, IEnumerable<PersonPhoto> people, CatalogAssets assets)
    {
        var values = people.ToArray();
        var existing = ReadPeople(connection, table, relation, entryKey, personKey, id, transaction);
        var resolvedIds = values.Select(person => ResolvePerson(connection, transaction, table, person, assets)).ToArray();
        if (existing.Select(person => person.Id).SequenceEqual(resolvedIds))
        {
            return;
        }
        Execute(connection, transaction, $"DELETE FROM [{relation}] WHERE [{entryKey}]=$id", ("$id", id));
        foreach (var personId in resolvedIds.Distinct())
        {
            Execute(connection, transaction, $"INSERT INTO [{relation}]([{entryKey}],[{personKey}]) VALUES ($id,$personId)", ("$id", id), ("$personId", personId));
        }
    }

    private int ResolvePerson(SqliteConnection connection, SqliteTransaction transaction, string table, PersonPhoto person, CatalogAssets assets)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.Parameters.AddWithValue("$name", person.Name);
        command.Parameters.AddWithValue("$id", person.Id);
        command.Parameters.AddWithValue("$personId", person.PersonId);
        command.Parameters.AddWithValue("$tmdbId", (object?)person.TmdbId ?? DBNull.Value);
        command.CommandText = $"""
            SELECT r.Id,r.personId FROM [{table}] r JOIN Person p ON p.Id=r.personId
            WHERE ($tmdbId IS NOT NULL AND p.tmdbId=$tmdbId)
                OR ($tmdbId IS NULL AND r.personId=$personId)
                OR (r.Id=$id AND ($tmdbId IS NULL OR p.tmdbId IS NULL OR p.tmdbId=$tmdbId))
                OR ($id<0 AND ($personId<0 OR $tmdbId IS NOT NULL)
                    AND r.name=$name COLLATE ARIADNA AND ($tmdbId IS NULL OR p.tmdbId IS NULL))
            ORDER BY CASE WHEN p.tmdbId=$tmdbId THEN 0
                WHEN r.personId=$personId THEN 1 WHEN r.Id=$id THEN 2 ELSE 3 END,r.Id
            LIMIT 1
            """;
        var roleId = -1;
        var identityId = -1;
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                roleId = reader.GetInt32(0);
                identityId = reader.GetInt32(1);
            }
        }

        if (person.TmdbId != null)
        {
            command.CommandText = "SELECT Id FROM Person WHERE tmdbId=$tmdbId";
            if (command.ExecuteScalar() is long knownId)
            {
                identityId = checked((int)knownId);
            }
            else if (identityId >= 0)
            {
                command.Parameters["$personId"].Value = identityId;
                command.CommandText = "UPDATE Person SET tmdbId=$tmdbId WHERE Id=$personId AND tmdbId IS NULL";
                command.ExecuteNonQuery();
            }
            else
            {
                command.CommandText = "INSERT INTO Person(tmdbId) VALUES ($tmdbId) RETURNING Id";
                identityId = checked((int)(long)command.ExecuteScalar()!);
            }
        }
        else if (identityId < 0 && person.PersonId >= 0)
        {
            command.CommandText = "SELECT Id FROM Person WHERE Id=$personId";
            identityId = command.ExecuteScalar() is long knownId ? checked((int)knownId) : -1;
        }

        if (identityId < 0)
        {
            command.CommandText = "INSERT INTO Person DEFAULT VALUES RETURNING Id";
            identityId = checked((int)(long)command.ExecuteScalar()!);
        }

        command.Parameters.Clear();
        command.Parameters.AddWithValue("$name", person.Name);
        command.Parameters.AddWithValue("$personId", identityId);
        if (roleId < 0)
        {
            command.CommandText = $"INSERT INTO [{table}](name,personId) VALUES ($name,$personId) RETURNING Id";
            roleId = checked((int)(long)command.ExecuteScalar()!);
        }
        else
        {
            command.Parameters.AddWithValue("$id", roleId);
            command.CommandText = $"UPDATE [{table}] SET name=$name,personId=$personId WHERE Id=$id";
            command.ExecuteNonQuery();
        }

        if (person.Photo != null)
        {
            var file = Path.Combine(portraitRoot, $"{identityId}.png");
            if (!File.Exists(file) || !person.Photo.AsSpan().SequenceEqual(File.ReadAllBytes(file)))
            {
                assets.AddPortrait(portraitRoot, identityId, person.Photo);
            }
        }
        return roleId;
    }

    private static int Execute(SqliteConnection connection, SqliteTransaction? transaction, string sql, params (string Name, object? Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }
        return command.ExecuteNonQuery();
    }

    private static void Validate(CatalogDetails details)
    {
        ValidateText(details.Entry.Title, "Title", 150);
        ValidateText(details.Entry.OriginalTitle, "Original title", 150);
        ValidateText(details.Entry.Path, "File path", 256);
        if (details.Entry.Version?.Length > 64)
        {
            throw new ArgumentException("Version must contain at most 64 characters.");
        }
        foreach (var genre in details.Genres)
        {
            ValidateText(genre, "Genre", 50);
        }
        foreach (var person in details.Directors.Concat(details.Actors))
        {
            ValidateText(person.Name, "Person name", 50);
        }
    }

    private static void ValidateText(string value, string name, int maximum)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length > maximum)
        {
            throw new ArgumentException($"{name} must contain at most {maximum} characters.");
        }
    }
}
