using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Data;

namespace Ariadna.AuxiliaryPopups;

public partial class ChoicePopup : Form
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Index { get; set; }
    public ChoicePopup(string path, List<MovieChoiceDto> results)
    {
        InitializeComponent();
        AddResults(results);
        m_ToolStripPath.Text = path;
        Index = -1;
    }

    private void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        Index = m_ResultList.SelectedItems.Count > 0 ? m_ResultList.SelectedItems[0].Index : -1;
    }

    private void OnDoubleClick(object sender, EventArgs e)
    {
        if (Index < 0)
        {
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Index = -1;
            DialogResult = DialogResult.Cancel;
            Close();
        }
        else if (e.KeyCode == Keys.Enter && Index >= 0)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK)
        {
            Index = -1;
        }
        base.OnFormClosing(e);
    }

    private void AddResults(IEnumerable<MovieChoiceDto> results)
    {
        foreach (var item in results.Select(CreateResultListViewItem))
        {
            m_ResultList.Items.Add(item);
        }
    }

    private static ListViewItem CreateResultListViewItem(MovieChoiceDto result)
    {
        return new ListViewItem([result.Title, result.TitleOrig, result.Year.ToString()]);
    }
}
