#nullable enable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ariadna.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ariadna.AuxiliaryPopups;

public sealed class VideoInfoControl : UserControl
{
    private readonly Label duration = new()
    {
        AutoSize = true,
        Text = "00:00:00",
        Name = "duration",
    };
    private readonly Label dimensions = new()
    {
        AutoSize = true,
        Name = "dimensions",
    };
    private readonly Label bitrate = new()
    {
        AutoSize = true,
        Name = "bitrate",
    };
    private readonly PictureBox[] flags = new PictureBox[4];
    private CancellationTokenSource? pending;
    private ILogger logger = NullLogger.Instance;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal IFileInspectionService Inspection { get; set; } = new FileInspectionService(NullLogger.Instance);

    public VideoInfoControl()
    {
        var row = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
        };
        row.Controls.AddRange([duration, dimensions, bitrate]);
        for (var index = 0; index < flags.Length; index++)
        {
            flags[index] = new PictureBox
            {
                Size = new Size(20, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
            };
            row.Controls.Add(flags[index]);
        }
        Controls.Add(row);
    }

    internal void Configure(ILogger logger)
    {
        this.logger = logger;
        Inspection = new FileInspectionService(logger);
    }

    internal async Task LoadPathAsync(string path)
    {
        Cancel();
        duration.Text = "00:00:00";
        dimensions.Text = string.Empty;
        bitrate.Text = string.Empty;
        foreach (var flag in flags)
        {
            flag.Image = null;
        }
        var cancellation = new CancellationTokenSource();
        pending = cancellation;
        var token = cancellation.Token;
        try
        {
            var info = await Inspection.GetVideoInfoAsync(path, token);
            if (info == null || !ReferenceEquals(pending, cancellation) || token.IsCancellationRequested || IsDisposed)
            {
                return;
            }
            duration.Text = info.Duration.ToString(@"hh\:mm\:ss");
            dimensions.Text = $"{info.Width}x{info.Height}";
            bitrate.Text = $"{info.Bitrate / 1000000} {Resources.Mbps}";
            for (var index = 0; index < Math.Min(flags.Length, info.Languages.Count); index++)
            {
                flags[index].Image = info.Languages[index] switch
                {
                    "Ukrainian" => Resources.ua_flag,
                    "Russian" => Resources.ru_flag,
                    "English" => Resources.en_flag,
                    "French" => Resources.fr_flag,
                    _ => Resources.unknown,
                };
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            logger.LogDebug("Video inspection failed, error type '{ErrorType}'", exception.GetType().Name);
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
