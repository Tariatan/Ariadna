using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ariadna.ImageListHelpers;
using Ariadna.Properties;
using Ariadna.Storage;
using Manina.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Ariadna.DatabaseStrategies;

public abstract class MediaDbStrategyBase : AbstractDbStrategy
{
    private readonly PosterFromFileAdaptor m_PosterImageAdaptor = new();

    protected MediaDbStrategyBase(ILogger logger, string posterRootPath)
    {
        Logger = logger;
        m_PosterImageAdaptor.RootPath = posterRootPath;
    }

    protected ILogger Logger { get; }
    protected CatalogStore Store => CatalogServices.CreateStore();
    protected abstract CatalogKind Kind { get; }

    public override ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter() => m_PosterImageAdaptor;

    public override void ShowEntryDetails(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ShowDataDialog(path);
    }

    public override void ExecuteEntry(int id)
    {
        var path = FindStoredEntryPathById(id);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (SupportsFileExecution && FileExists(path))
        {
            OpenFilePath(path);
            return;
        }

        if (DirectoryExists(path))
        {
            OpenDirectoryPath(path);
            return;
        }

        ShowPathNotFound(path);
    }

    protected virtual bool SupportsFileExecution => false;

    protected virtual bool TryOpenFirstNotInserted(string[] paths)
        => EntryDiscoveryHelper.TryOpenFirstNotInserted(paths, FindFirstNotInserted, OpenDiscoveredPath);

    protected virtual string FindFirstNotInserted(string[] paths)
    {
        var registered = Store.GetRegisteredPaths(Kind);
        return EntryDiscoveryHelper.FindFirstNotInserted(paths, path => registered.Any(stored => CatalogStore.PathsEqual(stored, path)));
    }


    protected abstract string FindStoredEntryPathById(int id);
    protected abstract void ShowDataDialog(string path);
    protected virtual void OpenDiscoveredPath(string path) => ShowDataDialog(path);

    protected virtual void OpenFilePath(string path) => EntryExecutionHelper.OpenFileInMpc(path, Settings.Default.MediaPlayerPath, FileExists, StartProcess);
    protected virtual void OpenDirectoryPath(string path) => EntryExecutionHelper.OpenDirectoryInTotalCommander(path, Settings.Default.TotalCommanderPath, StartProcess);
    protected virtual void ShowPathNotFound(string path) => ShowMessage(path, Resources.PathNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information);
    protected virtual string[] GetFiles(string path) => Directory.GetFiles(path);
    protected virtual string[] GetDirectories(string path) => Directory.GetDirectories(path);
    protected virtual bool FileExists(string path) => File.Exists(path);
    protected virtual bool DirectoryExists(string path) => Directory.Exists(path);
    protected virtual void DeleteFile(string path) => File.Delete(path);
    protected virtual void StartProcess(string fileName, string arguments) => Process.Start(fileName, arguments);
    protected virtual void StartProcess(ProcessStartInfo startInfo) => Process.Start(startInfo);
    protected virtual void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) => MessageBox.Show(text, caption, buttons, icon);
}
