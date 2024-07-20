using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ariadna.Data;

namespace Ariadna.AuxiliaryPopups;

public partial class ChoicePopup : Form
{
    public int Index { get; set; }
    public ChoicePopup(string path, List<MovieChoiceDto> results)
    {
        InitializeComponent();

        foreach (var itm in results.Select(result => new ListViewItem([result.Title, result.TitleOrig, result.Year.ToString()])))
        {
            m_ResultList.Items.Add(itm);
        }
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
}
