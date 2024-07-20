using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Ariadna.Extension;

public static class Extension
{
    public static void Capitalize(this ListView listView)
    {
        foreach (ListViewItem item in listView.Items)
        {
            item.Text = item.Text.Capitalize();
        }
    }

    public static string Capitalize(this string words)
    {
        var result = string.Empty;
        foreach (var word in words.Trim().Split(' '))
        {
            if (word.Length == 0)
            {
                continue;
            }

            var wordCapitalizes = word[0].ToString().ToUpper() + word[1..];
            if (result.Length > 0)
            {
                result += " ";
            }
            result += wordCapitalizes;
        }

        return result;
    }

    public static byte[] ToBytes(this Image img)
    {
        try
        {
            var converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        catch
        {
        }

        return null;
    }
    
    public static Bitmap ToBitmap(this byte[] bytes)
    {
        if ((bytes == null) || (bytes.Length == 0))
        {
            return null;
        }

        using var memoryStream = new MemoryStream(bytes);
        return new Bitmap(memoryStream);
    }

    public static int ToInt(this string sId)
    {
        int.TryParse(sId, out var id);
        return id;
    }

    public static void DeleteFocusedListItem(this ListView listView)
    {
        if (listView.SelectedItems.Count > 0)
        {
            listView.Items.Remove(listView.FocusedItem!);
        }
    }
}