#nullable enable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.AuxiliaryPopups;

public sealed class FileSizeControl : UserControl
{
    private readonly Label value = new()
    {
        Dock = DockStyle.Fill,
        Text = "0 Mb",
        TextAlign = ContentAlignment.MiddleLeft,
    };
    private CancellationTokenSource? pending;
    private ILogger logger = NullLogger.Instance;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal IFileInspectionService Inspection { get; set; } = new FileInspectionService(NullLogger.Instance);

    public FileSizeControl() => Controls.Add(value);

    internal void Configure(ILogger logger)
    {
        this.logger = logger;
        Inspection = new FileInspectionService(logger);
    }

    internal async Task LoadPathAsync(string path)
    {
        Cancel();
        var cancellation = new CancellationTokenSource();
        pending = cancellation;
        var token = cancellation.Token;
        value.ForeColor = Color.Yellow;
        try
        {
            var progress = new Progress<long>(bytes =>
            {
                if (ReferenceEquals(pending, cancellation) && !token.IsCancellationRequested && !IsDisposed)
                {
                    ShowSize(bytes);
                }
            });
            var bytes = await Inspection.GetSizeAsync(path, token, progress);
            if (ReferenceEquals(pending, cancellation) && !token.IsCancellationRequested && !IsDisposed)
            {
                ShowSize(bytes);
                value.ForeColor = ForeColor;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            logger.LogDebug("File size inspection failed, error type '{ErrorType}'", exception.GetType().Name);
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation))
            {
                pending = null;
            }
            cancellation.Dispose();
        }
    }

    private void ShowSize(long bytes)
    {
        var text = (bytes / (1024 * 1024)).ToString(CultureInfo.InvariantCulture);
        if (text.Length > 3)
        {
            text = text.Insert(text.Length - 3, " ");
        }
        value.Text = text + " Mb";
    }

    internal void Cancel()
    {
        pending?.Cancel();
        pending = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Cancel();
        }
        base.Dispose(disposing);
    }
}
