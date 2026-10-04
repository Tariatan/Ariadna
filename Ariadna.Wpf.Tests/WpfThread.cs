using System.Windows.Threading;

namespace Ariadna.Wpf.Tests;
internal static class WpfThread
{
    private static readonly TaskCompletionSource<Dispatcher> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    static WpfThread()
    {
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            var app = new App();
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
        await (await dispatcher.InvokeAsync(action)).WaitAsync(TimeSpan.FromSeconds(90));
    }
}
