#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.SplashScreen;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.ApplicationServices;

namespace Ariadna;

internal sealed class CatalogApplication : WindowsFormsApplicationBase
{
    private readonly ILogger logger;
    private readonly TaskCompletionSource splashDisplayed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CatalogKind initialCatalog = CatalogKind.Movie;

    internal CatalogApplication(ILogger logger) : base(AuthenticationMode.Windows)
    {
        this.logger = logger;
        IsSingleInstance = true;
        EnableVisualStyles = true;
        // Preserve Windows scaling for the existing pixel-based catalog layouts.
        HighDpiMode = System.Windows.Forms.HighDpiMode.DpiUnaware;
        ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        MinimumSplashScreenDisplayTime = 0;
    }

    protected override bool OnStartup(StartupEventArgs eventArgs)
    {
        if (SplashScreen != null)
        {
            splashDisplayed.Task.GetAwaiter().GetResult();
        }
        initialCatalog = CatalogMode.Parse(eventArgs.CommandLine.FirstOrDefault()) ?? CatalogKind.Movie;
        try
        {
            CatalogServices.CreateDatabase().RecoverAssets();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to open the Ariadna catalog");
            HideSplashScreen();
            MessageBox.Show(exception.Message + "\n\nRestore or import your catalog and check the AriadnaCatalog setting.",
                "Ariadna catalog", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        if (base.OnStartup(eventArgs))
        {
            return true;
        }
        HideSplashScreen();
        return false;
    }

    protected override void OnCreateSplashScreen()
    {
        var splash = new SplashForm();
        splash.Shown += (_, _) =>
        {
            splash.Refresh();
            splashDisplayed.SetResult();
        };
        SplashScreen = splash;
    }
    protected override void OnCreateMainForm() => MainForm = new MainWindow(logger, initialCatalog);

    protected override void OnRun()
    {
        try
        {
            if (MainForm == null)
            {
                // Supplying the form before OnRun prevents the model from hiding the splash during Load.
                OnCreateMainForm();
            }
            var window = MainForm ?? throw new NoStartupFormException();
            window.Shown += OnMainFormShown;
            base.OnRun();
        }
        finally
        {
            if (MainForm != null)
            {
                MainForm.Shown -= OnMainFormShown;
            }
            HideSplashScreen();
        }
    }

    private void OnMainFormShown(object? sender, EventArgs e)
    {
        var window = (Form)sender!;
        window.Shown -= OnMainFormShown;
        window.BeginInvoke(new Action(() =>
        {
            if (!window.IsDisposed)
            {
                window.Refresh();
            }
            HideSplashScreen();
        }));
    }

    protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
    {
        ((MainWindow)MainForm).ActivateExisting(CatalogMode.Parse(eventArgs.CommandLine.FirstOrDefault()));
        eventArgs.BringToForeground = true;
        base.OnStartupNextInstance(eventArgs);
    }
}