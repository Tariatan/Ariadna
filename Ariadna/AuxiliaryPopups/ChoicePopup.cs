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
        Index = m_ResultList.FocusedItem!.Index;
    }

    private void OnDoubleClick(object sender, EventArgs e)
    {
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
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