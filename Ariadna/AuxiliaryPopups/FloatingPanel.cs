using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Ariadna.Themes;

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

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EPanelContentType PanelContentType { get; set; }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Utilities.EFormCloseReason FormCloseReason { get; set; }
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<string> EntryNames { get; set; }

    public event EventHandler ItemSelected;
    public event EventHandler SelectionConfirmed;

    public FloatingPanel()
    {
        InitializeComponent();
        EntryNames = [];
    }
    internal void ApplyTheme(Theme theme)
    {
        BackColor = theme.FloatingPanelBackColor;
        m_PanelListView.BackColor = theme.FloatingPanelBackColor;
        m_PanelListView.ForeColor = theme.FloatingPanelForeColor;
    }

    public void UpdateListView(ImmutableSortedDictionary<string, Bitmap> values, EPanelContentType contentType, bool checkBox = false, bool multiSelect = false, int imageW = 64, int imageH = 96)
    {
        ResetState(contentType);
        ConfigureListView(checkBox, multiSelect, imageW, imageH);

        // A created native list copies added images before temporary bitmaps are disposed.
        _ = m_PanelImageView.Handle;

        using var empty = new Bitmap(Properties.Resources.No_Preview_Image_small);
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
            EntryNames.Clear();
            FormCloseReason = Utilities.EFormCloseReason.NONE;
            Hide();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            ConfirmFocusedItem();
        }
    }
    private void OnListEntryDoubleClicked(object sender, MouseEventArgs e)
    {
        ConfirmFocusedItem();
    }

    private void ConfirmFocusedItem()
    {
        if (m_PanelListView.FocusedItem == null)
        {
            return;
        }
        EntryNames.Add(GetFocusedEntryName());
        FormCloseReason = Utilities.EFormCloseReason.SUCCESS;
        Hide();
        SelectionConfirmed?.Invoke(this, EventArgs.Empty);
    }

    private void OnListItemChecked(object sender, ItemCheckedEventArgs e)
    {
        if (!IsPanelListFocused())
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

        ItemSelected?.Invoke(this, e);
    }

    protected virtual bool IsPanelListFocused()
    {
        return m_PanelListView.Focused;
    }

    private void ResetState(EPanelContentType contentType)
    {
        EntryNames.Clear();
        FormCloseReason = Utilities.EFormCloseReason.NONE;
        PanelContentType = contentType;
    }

    private void ConfigureListView(bool checkBox, bool multiSelect, int imageW, int imageH)
    {
        m_PanelImageView.Images.Clear();
        m_PanelImageView.ImageSize = new Size(imageW, imageH);
        m_PanelListView.Items.Clear();
        m_PanelListView.CheckBoxes = checkBox;
        m_PanelListView.MultiSelect = multiSelect;
    }

    private string GetFocusedEntryName()
    {
        return m_PanelListView.FocusedItem!.Text;
    }
}
