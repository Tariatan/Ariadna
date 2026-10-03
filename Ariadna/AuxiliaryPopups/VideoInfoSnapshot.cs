#nullable enable
using System;
using System.Collections.Generic;

namespace Ariadna.AuxiliaryPopups;

internal sealed record VideoInfoSnapshot(TimeSpan Duration, int Width, int Height, int Bitrate, IReadOnlyList<string> Languages);
