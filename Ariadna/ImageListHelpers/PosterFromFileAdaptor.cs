using Manina.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Ariadna
{
    public class PosterFromFileAdaptor : Manina.Windows.Forms.ImageListView.ImageListViewItemAdaptor
    {
        public string RootPath { get; set; }

        public override void Dispose() {}
        public override Utility.Tuple<ColumnType, string, object>[] GetDetails(object key) => null;
        public override string GetSourceImage(object key) => RootPath + (string) key;
        public bool ThumbnailCallback() => false;
        public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails thmb, bool useExif)
        {
            string filename = RootPath + (string)key;
            if (File.Exists(filename))
            {
                Bitmap bmp = new Bitmap(filename);
                return bmp.GetThumbnailImage(size.Width, size.Height, null, System.IntPtr.Zero);
            }
            else
            {
                return null;
            }
        }
        public override string GetUniqueIdentifier(object key, Size sz, UseEmbeddedThumbnails thmb, bool useExif) => (string)key;
    }
}
