#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ariadna.Properties;

namespace Ariadna.AuxiliaryPopups;

public sealed class GamePreviewsControl : UserControl
{
    private readonly PictureBox full = new()
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.Zoom,
    };
    private readonly ImageEditorControl[] previews = new ImageEditorControl[4];

    public GamePreviewsControl()
    {
        var thumbnails = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 85,
            ColumnCount = 4,
            RowCount = 1,
        };
        for (var index = 0; index < previews.Length; index++)
        {
            thumbnails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            var preview = new ImageEditorControl
            {
                Name = $"preview{index + 1}",
                Dock = DockStyle.Fill,
                ImageWidth = Settings.Default.PreviewWidth,
                ImageHeight = Settings.Default.PreviewHeight,
            };
            preview.Selected += (_, _) => SelectPreview(preview);
            preview.ImageChanged += (_, _) => SelectPreview(preview);
            previews[index] = preview;
            thumbnails.Controls.Add(preview, index, 0);
        }
        Controls.Add(full);
        Controls.Add(thumbnails);
        SelectPreview(previews[0]);
    }

    private void SelectPreview(ImageEditorControl preview)
    {
        var previous = full.Image;
        full.Image = preview.CopyImage();
        previous?.Dispose();
    }

    public void LoadImages(string root, int id)
    {
        for (var index = 0; index < previews.Length; index++)
        {
            previews[index].LoadImage(Path.Combine(root, $"{id}{Settings.Default.PreviewSuffix}{index + 1}"));
        }
        SelectPreview(previews[0]);
    }

    public IReadOnlyDictionary<string, byte[]> GetImages()
    {
        var images = new Dictionary<string, byte[]>();
        for (var index = 0; index < previews.Length; index++)
        {
            images[$"{Settings.Default.PreviewSuffix}{index + 1}"] = previews[index].GetPngBytes();
        }
        return images;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            full.Image?.Dispose();
            full.Image = null;
        }
        base.Dispose(disposing);
    }
}
