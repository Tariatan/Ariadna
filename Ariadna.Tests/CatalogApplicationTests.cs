using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Ariadna.Tests.AuxiliaryPopups;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
[DoNotParallelize]
public sealed class CatalogApplicationTests
{
    [TestMethod]
    public void OnRun_MainWindowLoading_KeepsSplashUntilWindowIsDisplayed()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var testee = new CatalogApplication(NullLogger.Instance);
            using var splash = new Form();
            using var window = new Form();
            using var completionTimer = new System.Windows.Forms.Timer { Interval = 10 };
            var timeout = Stopwatch.StartNew();
            var splashVisibleDuringLoad = false;
            var splashVisibleWhenShown = false;
            var paintedBeforeSplashClosed = false;
            var dismissedWhileWindowVisible = false;
            splash.Show();
            testee.SplashScreen = splash;
            SetMainForm(testee, window);
            window.Load += (_, _) => splashVisibleDuringLoad = splash.Visible && !splash.IsDisposed;
            window.Shown += (_, _) => splashVisibleWhenShown = splash.Visible && !splash.IsDisposed;
            window.Paint += (_, _) => paintedBeforeSplashClosed |= !splash.IsDisposed;
            completionTimer.Tick += (_, _) =>
            {
                if (splash.IsDisposed || timeout.Elapsed > TimeSpan.FromSeconds(5))
                {
                    dismissedWhileWindowVisible = splash.IsDisposed && window.Visible;
                    completionTimer.Stop();
                    window.Close();
                }
            };

            // Act
            completionTimer.Start();
            RunMainWindow(testee);

            // Assert
            Assert.IsTrue(splashVisibleDuringLoad);
            Assert.IsTrue(splashVisibleWhenShown);
            Assert.IsTrue(paintedBeforeSplashClosed);
            Assert.IsTrue(dismissedWhileWindowVisible);
            Assert.IsTrue(splash.IsDisposed);
        });
    }

    [TestMethod]
    public void OnRun_MainWindowClosesDuringLoad_DisposesSplash()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var testee = new CatalogApplication(NullLogger.Instance);
            using var splash = new Form();
            using var window = new Form();
            splash.Show();
            testee.SplashScreen = splash;
            SetMainForm(testee, window);
            window.Load += (_, _) => window.Close();

            // Act
            RunMainWindow(testee);

            // Assert
            Assert.IsTrue(splash.IsDisposed);
        });
    }

    [TestMethod]
    public void OnRun_NoSplashConfigured_DisplaysAndClosesMainWindow()
    {
        UiTest.Run(() =>
        {
            // Arrange
            var testee = new CatalogApplication(NullLogger.Instance);
            using var window = new Form();
            using var completionTimer = new System.Windows.Forms.Timer { Interval = 10 };
            var displayed = false;
            SetMainForm(testee, window);
            completionTimer.Tick += (_, _) =>
            {
                displayed = window.Visible;
                completionTimer.Stop();
                window.Close();
            };

            // Act
            completionTimer.Start();
            RunMainWindow(testee);

            // Assert
            Assert.IsTrue(displayed);
            Assert.IsNull(testee.SplashScreen);
        });
    }

    private static void SetMainForm(CatalogApplication application, Form window)
        => typeof(WindowsFormsApplicationBase).GetProperty("MainForm", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(application, window);

    private static void RunMainWindow(CatalogApplication application)
        => typeof(CatalogApplication).GetMethod("OnRun", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(application, null);
}