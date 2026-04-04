using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Ariadna.DatabaseStrategies;

internal static class EntryRemovalHelper
{
    public static void RemoveSingleFile(Action removeEntryFromDatabase, string path, Func<string, bool> fileExists, Action<string> deleteFile, Action<string, string, MessageBoxButtons, MessageBoxIcon> showMessage)
    {
        removeEntryFromDatabase();

        if (!fileExists(path))
        {
            return;
        }

        try
        {
            deleteFile(path);
        }
        catch (IOException ex)
        {
            showMessage(ex.Source, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static void RemoveFiles(Func<bool> removeEntryFromDatabase, IEnumerable<string> paths, Func<string, bool> fileExists, Action<string> deleteFile)
    {
        if (!removeEntryFromDatabase())
        {
            return;
        }

        foreach (var path in paths)
        {
            if (fileExists(path))
            {
                deleteFile(path);
            }
        }
    }
}