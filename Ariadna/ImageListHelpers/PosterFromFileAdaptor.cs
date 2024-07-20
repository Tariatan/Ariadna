using System.Drawing;
using System.IO;
using Manina.Windows.Forms;

namespace Ariadna.ImageListHelpers;

public class PosterFromFileAdaptor : ImageListView.ImageListViewItemAdaptor
{
    public string RootPath { get; set; }

    public override void Dispose() {}
    public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key) => null;
    public override string GetSourceImage(object key) => RootPath + (string) key;
    public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails thmb, bool useExif)
    {
        var filename = RootPath + (string)key;
        if (!File.Exists(filename))
        {
            return null;
        }

        // Dispose bitmap to unlock the file
        using var bmpTmp = new Bitmap(filename);

        return bmpTmp.GetThumbnailImage(size.Width, size.Height, null, System.IntPtr.Zero);

    }
    public override string GetUniqueIdentifier(object key, Size sz, UseEmbeddedThumbnails thmb, bool useExif) => (string)key;
}