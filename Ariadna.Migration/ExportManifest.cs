namespace Ariadna.Migration;

internal sealed record ExportManifest(int FormatVersion, DateTime CreatedUtc, ExportTable[] Tables, ExportAsset[] Assets);
internal sealed record ExportTable(string Name, ExportColumn[] Columns, long Identity, long Rows, string Sha256);
internal sealed record ExportColumn(string Name, string Type, bool Nullable, int MaximumLength);
internal sealed record ExportAsset(string Path, long Bytes, string Sha256);
