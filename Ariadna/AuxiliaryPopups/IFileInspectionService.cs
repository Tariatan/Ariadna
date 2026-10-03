#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ariadna.AuxiliaryPopups;

internal interface IFileInspectionService
{
    Task<long> GetSizeAsync(string path, CancellationToken cancellationToken, IProgress<long> progress);
    Task<VideoInfoSnapshot?> GetVideoInfoAsync(string path, CancellationToken cancellationToken);
}
