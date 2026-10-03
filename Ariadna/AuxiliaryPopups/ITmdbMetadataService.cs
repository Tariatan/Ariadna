#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ariadna.AuxiliaryPopups;

internal interface ITmdbMetadataService : IDisposable
{
    Task<TmdbMetadata?> LoadAsync(int movieId, int tvShowId, CancellationToken cancellationToken);
    Task<byte[]?> GetPortraitAsync(string name, CancellationToken cancellationToken);
}
