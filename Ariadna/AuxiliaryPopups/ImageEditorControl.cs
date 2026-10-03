#nullable enable
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ariadna.Properties;

namespace Ariadna.AuxiliaryPopups;

public sealed class ImageEditorControl : UserControl
{
    private readonly PictureBox picture = new()
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.Zoom,
    };

    public ImageEditorControl()
    {
        Controls.Add(picture);
        picture.DoubleClick += OnChooseImage;
        picture.Click += (_, _) => Selected?.Invoke(this, EventArgs.Empty);
        SetImage(Resources.No_Preview_Image);
    }

    [DefaultValue(400)]
    public int ImageWidth { get; set; } = 400;
    [DefaultValue(600)]
    public int ImageHeight { get; set; } = 600;
    public event EventHandler? ImageChanged;
    public event EventHandler? Selected;

    public void SetImage(Image image)
    {
        var previous = picture.Image;
        picture.Image = new Bitmap(image);
        previous?.Dispose();
        ImageChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadImage(string path)
    {
        if (File.Exists(path))
        {
            using var image = new Bitmap(path);
            SetImage(image);
        }
    }

    internal Bitmap CopyImage() => new(picture.Image!);

    public byte[] GetPngBytes()
    {
        using var stream = new MemoryStream();
        picture.Image!.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private void OnChooseImage(object? sender, EventArgs e)
    {
        if (Utilities.GetBitmapFromDisk(out var image, $"Image ({ImageWidth}x{ImageHeight}) (*.*)|*.*", ImageWidth, ImageHeight))
        {
            using (image)
            {
                SetImage(image ?? Resources.No_Preview_Image);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            picture.Image?.Dispose();
            picture.Image = null;
        }
        base.Dispose(disposing);
    }
}
