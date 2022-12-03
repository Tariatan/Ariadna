using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Ariadna
{
    public partial class FloatingPanel : Form
    {
        public enum EPanelContentType
        {
            DIRECTORS = 0,
            CAST,
            GENRES,
        }

        public EPanelContentType PanelContentType { get; set; }
        public Utilities.EFormCloseReason FormCloseReason { get; set; }
        public List<string> EntryNames { get; set; }

        public event EventHandler ItemSelected;

        public FloatingPanel()
        {
            InitializeComponent();
            EntryNames = new List<string>();
        }
        public void UpdateListView(SortedDictionary<string, Bitmap> values, EPanelContentType contentType, bool checkBox = false, bool multiSelect = false, int imageW = 64, int imageH = 96)
        {
            EntryNames.Clear();
            FormCloseReason = Utilities.EFormCloseReason.None;
            PanelContentType = contentType;

            mPanelImageView.Images.Clear();
            mPanelImageView.ImageSize = new Size(imageW, imageH);
            mPanelListView.Items.Clear();
            mPanelListView.CheckBoxes = checkBox;
            mPanelListView.MultiSelect = multiSelect;

            var empty = new Bitmap(Properties.Resources.No_Preview_Image_small);
            foreach (var value in values)
            {
                mPanelImageView.Images.Add(value.Key, (value.Value != null) ? value.Value : empty);
                mPanelListView.Items.Add(new ListViewItem(value.Key, mPanelImageView.Images.IndexOfKey(value.Key)));
            }
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
                return;
            }
        }
        private void OnListEntryDoubleClicked(object sender, MouseEventArgs e)
        {
            EntryNames.Add(mPanelListView.FocusedItem.Text);

            FormCloseReason = Utilities.EFormCloseReason.SUCCESS;
            this.Hide();
        }

        private void OnListItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!mPanelListView.Focused)
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

            ItemSelected.Invoke(this, e);
        }

        private void OnFormActivated(object sender, EventArgs e)
        {
            int i = 0;
        }

        private void OnFormDeactivated(object sender, EventArgs e)
        {

            int i = 0;
        }

        private void OnFormEnter(object sender, EventArgs e)
        {

            int i = 0;
        }

        private void OnFormLeave(object sender, EventArgs e)
        {

            int i = 0;
        }
    }
}
