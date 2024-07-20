using Ariadna.Themes;

namespace Ariadna.AuxiliaryPopups
{
    partial class FloatingPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_PanelListView = new System.Windows.Forms.ListView();
            this.m_PanelImageView = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // m_PanelListView
            // 
            this.m_PanelListView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.m_PanelListView.BackColor = Theme.FloatingPanelBackColor;
            this.m_PanelListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_PanelListView.ForeColor = Theme.FloatingPanelForeColor;
            this.m_PanelListView.HideSelection = false;
            this.m_PanelListView.LargeImageList = this.m_PanelImageView;
            this.m_PanelListView.Location = new System.Drawing.Point(0, 0);
            this.m_PanelListView.MultiSelect = false;
            this.m_PanelListView.Name = "m_PanelListView";
            this.m_PanelListView.Size = new System.Drawing.Size(904, 160);
            this.m_PanelListView.TabIndex = 0;
            this.m_PanelListView.UseCompatibleStateImageBehavior = false;
            this.m_PanelListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.OnListItemChecked);
            this.m_PanelListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnListEntryDoubleClicked);
            // 
            // m_PanelImageView
            // 
            this.m_PanelImageView.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.m_PanelImageView.ImageSize = new System.Drawing.Size(64, 96);
            this.m_PanelImageView.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // FloatingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = Theme.FloatingPanelBackColor;
            this.ClientSize = new System.Drawing.Size(904, 160);
            this.ControlBox = false;
            this.Controls.Add(this.m_PanelListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FloatingPanel";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FloatingPanel";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView m_PanelListView;
        private System.Windows.Forms.ImageList m_PanelImageView;
    }
}