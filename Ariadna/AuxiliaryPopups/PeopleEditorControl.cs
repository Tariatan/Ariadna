#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Storage;

namespace Ariadna.AuxiliaryPopups;

public sealed class PeopleEditorControl : UserControl
{
    private readonly ListView list = new()
    {
        Dock = DockStyle.Fill,
        LabelEdit = true,
        MultiSelect = false,
        HideSelection = false,
    };
    private readonly ImageList photos = new()
    {
        ImageSize = new Size(Settings.Default.PortraitWidth, Settings.Default.PortraitHeight),
        ColorDepth = ColorDepth.Depth32Bit,
    };
    private readonly Button paste = new()
    {
        Text = "↓",
        Dock = DockStyle.Top,
        Height = 22,
    };
    private PersonRole role;

    public PeopleEditorControl()
    {
        list.LargeImageList = photos;
        Controls.Add(list);
        Controls.Add(paste);
        paste.Click += (_, _) => PastePeople();
        list.KeyUp += OnListKeyUp;
        list.AfterLabelEdit += OnRenamed;
        list.MouseDoubleClick += OnPhotoDoubleClick;
    }

    public event EventHandler? PeopleAdded;

    internal void Configure(PersonRole role) => this.role = role;

    public PersonPhoto[] GetPeople() => list.Items.Cast<ListViewItem>()
        .Select(item => new PersonPhoto(item.Text, ((PersonPhoto)item.Tag!).Photo)).ToArray();

    public void LoadPeople(IEnumerable<PersonPhoto> people)
    {
        list.Items.Clear();
        photos.Images.Clear();
        foreach (var person in people)
        {
            AddItem(person);
        }
    }

    public void AddPerson(PersonPhoto person)
    {
        if (string.IsNullOrWhiteSpace(person.Name) || person.Name == "↓" || list.Items.Cast<ListViewItem>().Any(item => item.Text.Equals(person.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        AddItem(person);
    }

    private void AddItem(PersonPhoto person)
    {
        var key = Guid.NewGuid().ToString("N");
        // Force immediate copying; ImageList otherwise retains the original until first use.
        _ = photos.Handle;
        using var photo = DecodePhoto(person.Photo);
        photos.Images.Add(key, photo);
        list.Items.Add(new ListViewItem(person.Name, key) { Tag = person });
    }

    private static Bitmap DecodePhoto(byte[]? photo) => photo?.ToBitmap() ?? new Bitmap(Resources.No_Preview_Image_small);

    private PersonPhoto? FindPerson(string name) => CatalogServices.CreateStore().FindPerson(name, actor: role == PersonRole.Actor, author: role == PersonRole.Author);

    private void PastePeople()
    {
        foreach (var text in Clipboard.GetText().Split(','))
        {
            var name = text.Trim().Capitalize();
            AddPerson(FindPerson(name) ?? new PersonPhoto(name, null));
        }
        PeopleAdded?.Invoke(this, EventArgs.Empty);
    }

    internal string[] GetMissingPhotoNames() => GetPeople().Where(person => person.Photo == null || !Utilities.IsValidPreview(person.Photo)).Select(person => person.Name).ToArray();

    internal void ApplyDownloadedPhoto(string name, byte[] photo)
    {
        var item = list.Items.Cast<ListViewItem>().FirstOrDefault(item => item.Text == name);
        if (item == null || ((PersonPhoto)item.Tag!).Photo is { } existing && Utilities.IsValidPreview(existing))
        {
            return;
        }
        ReplacePhoto(item, new PersonPhoto(name, photo));
    }

    private void ReplacePhoto(ListViewItem item, PersonPhoto person)
    {
        using var image = DecodePhoto(person.Photo);
        _ = photos.Handle;
        photos.Images[photos.Images.IndexOfKey(item.ImageKey)] = image;
        item.Tag = person;
        list.Refresh();
    }

    private void OnListKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            PastePeople();
        }
        else if (e.KeyCode == Keys.Delete && list.FocusedItem != null)
        {
            var key = list.FocusedItem.ImageKey;
            list.Items.Remove(list.FocusedItem);
            photos.Images.RemoveByKey(key);
        }
        else if (e.KeyCode == Keys.F2 && list.SelectedItems.Count > 0)
        {
            list.SelectedItems[0].BeginEdit();
        }
    }

    private void OnRenamed(object? sender, LabelEditEventArgs e)
    {
        if (e.Label == null)
        {
            return;
        }
        var name = e.Label.Trim().Capitalize();
        var item = list.Items[e.Item];
        e.CancelEdit = true;
        if (string.IsNullOrWhiteSpace(name) || list.Items.Cast<ListViewItem>().Any(other => other != item && other.Text.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        var person = FindPerson(name) ?? new PersonPhoto(name, ((PersonPhoto)item.Tag!).Photo);
        item.Text = name;
        ReplacePhoto(item, person);
    }

    private void OnPhotoDoubleClick(object? sender, MouseEventArgs e)
    {
        var item = list.GetItemAt(e.X, e.Y);
        if (item == null || !Utilities.GetBitmapFromDisk(out var image, $"Photo ({photos.ImageSize.Width}x{photos.ImageSize.Height}) (*.*)|*.*", photos.ImageSize.Width, photos.ImageSize.Height))
        {
            return;
        }
        using (image)
        {
            ReplacePhoto(item, new PersonPhoto(item.Text, image?.ToBytes()));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            list.LargeImageList = null;
            photos.Dispose();
        }
        base.Dispose(disposing);
    }
}
