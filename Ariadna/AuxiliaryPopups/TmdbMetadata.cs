#nullable enable
using System.Collections.Generic;

namespace Ariadna.AuxiliaryPopups;

internal sealed record TmdbMetadata(string OriginalTitle, string Description, int Year, IReadOnlyCollection<string> Genres, byte[]? Poster);
