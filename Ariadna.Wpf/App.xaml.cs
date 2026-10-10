using System.IO;
using System.Windows;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.Wpf;
public partial class App : Application
{
    private readonly bool startCatalog;
    private SingleInstance? instance;
    private ILoggerFactory? logging;

    public App() : this(true) { }

    internal App(bool startCatalog) => this.startCatalog = startCatalog;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (!startCatalog)
        {
            return;
        }
        try
        {
            logging = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logging.CreateLogger("Ariadna.Wpf");
            var configuration = new CatalogConfiguration(Path.Combine(AppContext.BaseDirectory, "Ariadna.Wpf.dll.config"));
            instance = new SingleInstance(configuration.DatabasePath, logger);
            var argument = e.Args.FirstOrDefault();
            if (!instance.IsPrimary)
            {
                await instance.ActivateAsync(argument, CancellationToken.None);
                Shutdown();
                return;
            }

            var database = new CatalogDatabase(configuration.DatabasePath);
            await Task.Run(database.RecoverAssets);
            var placementPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ariadna", "Wpf", "window.json");
            var window = new MainWindow(new CatalogStore(database), configuration, logger, argument, new WindowPlacement(placementPath, logger));
            MainWindow = window;
            instance.StartListening(request => window.Dispatcher.InvokeAsync(() => window.ActivateCatalog(request)));
            await window.PrepareAsync(CancellationToken.None);
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
            await window.Dispatcher.InvokeAsync(() =>
            {
            }, System.Windows.Threading.DispatcherPriority.Render);
            await window.PreloadAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logging?.CreateLogger("Ariadna.Wpf").LogError("Startup failed, error type '{ErrorType}'", exception.GetType().Name);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            CatalogDialog.ShowError(null, CatalogKind.Movie, "Ariadna could not start", "Check the catalog configuration and recovery files.\n\n" + exception.Message);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        instance?.Dispose();
        logging?.Dispose();
        base.OnExit(e);
    }
}