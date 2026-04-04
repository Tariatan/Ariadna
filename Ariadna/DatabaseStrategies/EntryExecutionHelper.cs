using System;
using System.Diagnostics;
using System.IO;

namespace Ariadna.DatabaseStrategies;

internal static class EntryExecutionHelper
{
    public static void ExecuteEntry(
        int id,
        Func<int, string> findStoredEntryPathById,
        Func<string, bool> fileExists,
        Func<string, bool> directoryExists,
        Action<string> openFile,
        Action<string> openDirectory,
        Action<string> showPathNotFound)
    {
        var path = findStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ExecutePath(path, fileExists, directoryExists, openFile, openDirectory, showPathNotFound);
    }

    public static void ExecuteEntry(
        int id,
        Func<int, string> findStoredEntryPathById,
        Func<string, bool> directoryExists,
        Action<string> openDirectory,
        Action<string> showPathNotFound)
    {
        var path = findStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ExecutePath(path, directoryExists, openDirectory, showPathNotFound);
    }

    public static void ExecutePath(
        string path,
        Func<string, bool> fileExists,
        Func<string, bool> directoryExists,
        Action<string> openFile,
        Action<string> openDirectory,
        Action<string> showPathNotFound)
    {
        if (fileExists(path))
        {
            openFile(path);
            return;
        }

        if (directoryExists(path))
        {
            openDirectory(path);
            return;
        }

        showPathNotFound(path);
    }

    public static void ExecutePath(
        string path,
        Func<string, bool> directoryExists,
        Action<string> openDirectory,
        Action<string> showPathNotFound)
    {
        if (directoryExists(path))
        {
            openDirectory(path);
            return;
        }

        showPathNotFound(path);
    }

    public static void OpenDirectoryInTotalCommander(string path, string totalCommanderPath, Action<ProcessStartInfo> startProcess)
    {
        startProcess(new ProcessStartInfo
        {
            FileName = totalCommanderPath,
            WorkingDirectory = Path.GetDirectoryName(totalCommanderPath)!,
            Arguments = $"/O /L=\"{path}\"",
        });
    }

    public static void OpenFileInMpc(string path, string mediaPlayerPath, Func<string, bool> fileExists, Action<string, string> startProcess)
    {
        if (!fileExists(mediaPlayerPath))
        {
            return;
        }

        startProcess(mediaPlayerPath, "\"" + path + "\"");
    }
}