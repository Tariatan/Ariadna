using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;
internal static class WpfThread
{
    private static readonly TaskCompletionSource<Dispatcher> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    static WpfThread()
    {
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            // Load styles without production startup, which reads personal configuration and opens a window.
            var app = new App(startCatalog: false);
            app.InitializeComponent();
            ready.SetResult(Dispatcher.CurrentDispatcher);
            Dispatcher.Run();
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    internal static async Task RunAsync(Func<Task> action)
    {
        var dispatcher = await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await (await dispatcher.InvokeAsync(async () =>
        {
            AssertHeadless();
            try
            {
                await action();
            }
            finally
            {
                AssertHeadless();
            }
        })).WaitAsync(TimeSpan.FromSeconds(90));
    }

    private static void AssertHeadless()
    {
        Assert.IsEmpty(PresentationSource.CurrentSources.OfType<HwndSource>(), "Tests must not create native presentation sources.");
        foreach (Window window in Application.Current.Windows)
        {
            Assert.AreEqual(IntPtr.Zero, new WindowInteropHelper(window).Handle, "Tests must not create window handles.");
            Assert.IsFalse(window.IsVisible, "Tests must not show application windows.");
        }
    }
}
