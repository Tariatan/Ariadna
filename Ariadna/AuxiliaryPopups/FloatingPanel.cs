using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Windows.Forms;

namespace Ariadna.AuxiliaryPopups;

public partial class FloatingPanel : Form
{
    public enum EPanelContentType
    {
        DIRECTORS = 0,
        CAST,
        GENRES,
        SUBGENRES,
    }

    public EPanelContentType PanelContentType { get; set; }
    public Utilities.EFormCloseReason FormCloseReason { get; set; }
    public List<string> EntryNames { get; set; }

    public event EventHandler ItemSelected;

    public FloatingPanel()
    {
        InitializeComponent();
        EntryNames = [];
    }
    public void UpdateListView(ImmutableSortedDictionary<string, Bitmap> values, EPanelContentType contentType, bool checkBox = false, bool multiSelect = false, int imageW = 64, int imageH = 96)
    {
        EntryNames.Clear();
        FormCloseReason = Utilities.EFormCloseReason.NONE;
        PanelContentType = contentType;

        m_PanelImageView.Images.Clear();
        m_PanelImageView.ImageSize = new Size(imageW, imageH);
        m_PanelListView.Items.Clear();
        m_PanelListView.CheckBoxes = checkBox;
        m_PanelListView.MultiSelect = multiSelect;

        var empty = new Bitmap(Properties.Resources.No_Preview_Image_small);
        foreach (var value in values)
        {
            m_PanelImageView.Images.Add(value.Key, value.Value ?? empty);
            m_PanelListView.Items.Add(new ListViewItem(value.Key, m_PanelImageView.Images.IndexOfKey(value.Key)));
        }
    }
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Hide();
        }
    }
    private void OnListEntryDoubleClicked(object sender, MouseEventArgs e)
    {
        EntryNames.Add(m_PanelListView.FocusedItem!.Text);

        FormCloseReason = Utilities.EFormCloseReason.SUCCESS;
        Hide();
    }

    private void OnListItemChecked(object sender, ItemCheckedEventArgs e)
    {
        if (!m_PanelListView.Focused)
        {
            return;
        }

        if (e.Item.Checked)
        {
            EntryNames.Add(e.Item.Text);
        }
        else
        {
            EntryNames.Remove(e.Item.Text);
        }

        ItemSelected!.Invoke(this, e);
    }
}