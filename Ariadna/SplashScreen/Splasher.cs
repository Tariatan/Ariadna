using System;
using System.Threading;
using System.Windows.Forms;

namespace Ariadna.SplashScreen;

public class Splasher
{
    private static SplashForm splashForm;
    private static Thread splashThread;

    //	internally used as a thread function - showing the form and
    //	starting the message loop for it
    private static void ShowThread()
    {
        splashForm = new SplashForm();
        Application.Run(splashForm);
    }

    //	public Method to show the SplashForm
    public static void Show()
    {
        if (splashThread != null)
        {
            return;
        }

        splashThread = new Thread(Splasher.ShowThread)
        {
            IsBackground = true
        };
        splashThread.SetApartmentState(ApartmentState.STA);
        splashThread.Start();
    }

    //	public Method to hide the SplashForm
    public static void Close()
    {
        if (splashThread == null || splashForm == null)
        {
            return;
        }

        try
        {
            splashForm.Invoke(new MethodInvoker(splashForm.Close));
        }
        catch
        {
        }
        splashThread = null;
        splashForm = null;
    }

    //	public Method to set or get the loading Status
    public static string Status
    {
        set
        {
            if (splashForm == null)
            {
                return;
            }

            splashForm.StatusInfo = value;
        }
        get
        {
            if (splashForm == null)
            {
                throw new InvalidOperationException("Splash Form not on screen");
            }
            return splashForm.StatusInfo;
        }
    }
}