using System;
using System.Collections.Generic;

namespace Ariadna.DatabaseStrategies;

internal static class EntryDiscoveryHelper
{
    public static bool TryOpenFirstNotInserted(string[] paths, Func<string[], string> findFirstNotInserted, Action<string> openPath)
    {
        var foundPath = findFirstNotInserted(paths);
        if (string.IsNullOrEmpty(foundPath))
        {
            return false;
        }

        openPath(foundPath);
        return true;
    }

    public static string FindFirstNotInserted(IEnumerable<string> paths, Func<string, bool> isAlreadyInserted)
    {
        foreach (var path in paths)
        {
            if (!isAlreadyInserted(path))
            {
                return path;
            }
        }

        return string.Empty;
    }
}