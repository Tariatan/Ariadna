#nullable enable
using System;
using System.Linq;
using System.Windows.Forms;
using Ariadna.SplashScreen;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.ApplicationServices;

namespace Ariadna;

internal sealed class CatalogApplication : WindowsFormsApplicationBase
{
    private readonly ILogger logger;
    private CatalogKind initialCatalog = CatalogKind.Movie;

    internal CatalogApplication(ILogger logger) : base(AuthenticationMode.Windows)
    {
        this.logger = logger;
        IsSingleInstance = true;
        EnableVisualStyles = true;
        // Preserve Windows scaling for the existing pixel-based catalog layouts.
        HighDpiMode = System.Windows.Forms.HighDpiMode.DpiUnaware;
        ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        MinimumSplashScreenDisplayTime = 200;
    }

    protected override bool OnStartup(StartupEventArgs eventArgs)
    {
        initialCatalog = CatalogMode.Parse(eventArgs.CommandLine.FirstOrDefault()) ?? CatalogKind.Movie;
        try
        {
            CatalogServices.CreateDatabase().RecoverAssets();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to open the Ariadna catalog");
            MessageBox.Show(exception.Message + "\n\nRestore or import your catalog and check the AriadnaCatalog setting.",
                "Ariadna catalog", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return base.OnStartup(eventArgs);
    }

    protected override void OnCreateSplashScreen() => SplashScreen = new SplashForm();
    protected override void OnCreateMainForm() => MainForm = new MainWindow(logger, initialCatalog);

    protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
    {
        ((MainWindow)MainForm).ActivateExisting(CatalogMode.Parse(eventArgs.CommandLine.FirstOrDefault()));
        eventArgs.BringToForeground = true;
        base.OnStartupNextInstance(eventArgs);
    }
}
