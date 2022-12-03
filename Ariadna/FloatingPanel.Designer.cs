namespace Ariadna
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
            this.mPanelListView = new System.Windows.Forms.ListView();
            this.mPanelImageView = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // mPanelListView
            // 
            this.mPanelListView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.mPanelListView.BackColor = System.Drawing.Color.DarkMagenta;
            this.mPanelListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanelListView.ForeColor = System.Drawing.Color.White;
            this.mPanelListView.HideSelection = false;
            this.mPanelListView.LargeImageList = this.mPanelImageView;
            this.mPanelListView.Location = new System.Drawing.Point(0, 0);
            this.mPanelListView.MultiSelect = false;
            this.mPanelListView.Name = "mPanelListView";
            this.mPanelListView.Size = new System.Drawing.Size(904, 160);
            this.mPanelListView.TabIndex = 0;
            this.mPanelListView.UseCompatibleStateImageBehavior = false;
            this.mPanelListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.OnListItemChecked);
            this.mPanelListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnListEntryDoubleClicked);
            // 
            // mPanelImageView
            // 
            this.mPanelImageView.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.mPanelImageView.ImageSize = new System.Drawing.Size(64, 96);
            this.mPanelImageView.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // FloatingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkMagenta;
            this.ClientSize = new System.Drawing.Size(904, 160);
            this.ControlBox = false;
            this.Controls.Add(this.mPanelListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FloatingPanel";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FloatingPanel";
            this.Activated += new System.EventHandler(this.OnFormActivated);
            this.Deactivate += new System.EventHandler(this.OnFormDeactivated);
            this.Enter += new System.EventHandler(this.OnFormEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.Leave += new System.EventHandler(this.OnFormLeave);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView mPanelListView;
        private System.Windows.Forms.ImageList mPanelImageView;
    }
}