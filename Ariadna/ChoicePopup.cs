﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ariadna
{
    public partial class ChoicePopup : Form
    {
        public int index { get; set; }
        public ChoicePopup(string path, List<Utilities.MovieChoiceDto> results)
        {
            InitializeComponent();

            foreach(var result in results)
            {
                var itm = new ListViewItem(new string[] { result.titleRu, result.titleOrig, result.year.ToString() });
                mResultList.Items.Add(itm);
            }
            mToolStripPath.Text = path;
        }
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            index = mResultList.FocusedItem.Index;
        }
        private void OnDoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
