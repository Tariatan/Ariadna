#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Extension;
using Ariadna.Properties;
using Ariadna.Themes;

namespace Ariadna.AuxiliaryPopups;

public sealed class GenreSelectionControl : UserControl
{
    private readonly ListView list = new()
    {
        Dock = DockStyle.Fill,
        MultiSelect = false,
        Sorting = SortOrder.Ascending,
        View = View.LargeIcon,
    };
    private readonly ImageList images = new()
    {
        ImageSize = new Size(Settings.Default.GenreImageWidth, Settings.Default.GenreImageHeight),
        ColorDepth = ColorDepth.Depth32Bit,
    };
    private readonly Button add = new()
    {
        Text = "...",
        Dock = DockStyle.Right,
        Width = 30,
    };
    private readonly Button paste = new()
    {
        Text = "↓",
        Dock = DockStyle.Right,
        Width = 30,
    };
    private readonly FloatingPanel picker = new();
    private IReadOnlyCollection<string> available = [];
    private Func<string, string> normalize = name => name;
    private Func<string, Bitmap> imageFor = _ => Resources.No_Image;

    internal void ApplyPickerTheme(Theme theme) => picker.ApplyTheme(theme);

    public event EventHandler? GenresChanged;

    public GenreSelectionControl()
    {
        list.LargeImageList = images;
        Controls.Add(list);
        Controls.Add(paste);
        Controls.Add(add);
        add.Click += OnAddClick;
        paste.Click += (_, _) => PasteGenres();
        list.KeyUp += OnListKeyUp;
        picker.SelectionConfirmed += OnSelectionConfirmed;
        picker.Deactivate += OnPickerDeactivated;
    }

    internal void Configure(IReadOnlyCollection<string> available, Func<string, string> normalize, Func<string, Bitmap> imageFor)
    {
        this.available = available;
        this.normalize = normalize;
        this.imageFor = imageFor;
    }

    public string[] GetGenres() => list.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray();

    public void LoadGenres(IEnumerable<string> genres)
    {
        list.BeginUpdate();
        try
        {
            list.Items.Clear();
            images.Images.Clear();
            foreach (var genre in genres)
            {
                AddItem(genre);
            }
            UpdateAddButton();
        }
        finally
        {
            list.EndUpdate();
        }
    }

    public void AddGenre(string name)
    {
        name = normalize(name.Trim().Capitalize());
        if (string.IsNullOrWhiteSpace(name) || list.Items.Cast<ListViewItem>().Any(item => item.Text.Equals(name, StringComparison.OrdinalIgnoreCase))
            || list.Items.Count >= Settings.Default.MaxGenresCount)
        {
            return;
        }
        AddItem(name);
        UpdateAddButton();
        GenresChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AddItem(string name)
    {
        var key = Guid.NewGuid().ToString("N");
        images.Images.Add(key, imageFor(name));
        list.Items.Add(new ListViewItem(name, key));
    }

    private void UpdateAddButton() => add.Enabled = list.Items.Count < Settings.Default.MaxGenresCount;

    private void PasteGenres()
    {
        foreach (var name in Clipboard.GetText().Split(','))
        {
            AddGenre(name);
        }
    }

    private void OnListKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.V)
        {
            PasteGenres();
        }
        else if (e.KeyCode == Keys.Delete && list.FocusedItem != null)
        {
            var key = list.FocusedItem.ImageKey;
            list.Items.Remove(list.FocusedItem);
            images.Images.RemoveByKey(key);
            UpdateAddButton();
            GenresChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnAddClick(object? sender, EventArgs e)
    {
        if (DismissPicker())
        {
            return;
        }
        var selected = GetGenres().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var choices = available.Where(name => !selected.Contains(name)).ToImmutableSortedDictionary(name => name, imageFor);
        picker.UpdateListView(choices, FloatingPanel.EPanelContentType.GENRES, imageW: Settings.Default.GenreImageWidth, imageH: Settings.Default.GenreImageHeight);
        picker.Bounds = new Rectangle(PointToScreen(new Point(0, Height)), new Size(Width, Settings.Default.GenreImageHeight * 7 - 10));
        picker.Show(FindForm());
    }

    private void OnSelectionConfirmed(object? sender, EventArgs e)
    {
        if (picker.EntryNames.FirstOrDefault() is { } name)
        {
            AddGenre(name);
        }
    }

    internal bool DismissPicker()
    {
        if (!picker.Visible)
        {
            return false;
        }
        picker.Hide();
        return true;
    }

    private void OnPickerDeactivated(object? sender, EventArgs e) => picker.Hide();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            picker.SelectionConfirmed -= OnSelectionConfirmed;
            picker.Deactivate -= OnPickerDeactivated;
            picker.Dispose();
            list.LargeImageList = null;
            images.Dispose();
        }
        base.Dispose(disposing);
    }
}
