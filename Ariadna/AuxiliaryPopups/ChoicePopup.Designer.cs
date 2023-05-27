namespace Ariadna
{
    partial class ChoicePopup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChoicePopup));
            this.mResultList = new System.Windows.Forms.ListView();
            this.colTitleRu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTitleOrig = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colYear = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mStatusStrip = new System.Windows.Forms.StatusStrip();
            this.mToolStripPath = new System.Windows.Forms.ToolStripStatusLabel();
            this.mStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mResultList
            // 
            this.mResultList.BackColor = System.Drawing.Color.DarkMagenta;
            this.mResultList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTitleRu,
            this.colTitleOrig,
            this.colYear});
            this.mResultList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mResultList.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.mResultList.ForeColor = System.Drawing.Color.White;
            this.mResultList.HideSelection = false;
            this.mResultList.Location = new System.Drawing.Point(0, 0);
            this.mResultList.Name = "mResultList";
            this.mResultList.Size = new System.Drawing.Size(582, 257);
            this.mResultList.TabIndex = 0;
            this.mResultList.UseCompatibleStateImageBehavior = false;
            this.mResultList.View = System.Windows.Forms.View.Details;
            this.mResultList.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            this.mResultList.DoubleClick += new System.EventHandler(this.OnDoubleClick);
            // 
            // colTitleRu
            // 
            this.colTitleRu.Text = "Русское название";
            this.colTitleRu.Width = 250;
            // 
            // colTitleOrig
            // 
            this.colTitleOrig.Text = "Оригинальное название";
            this.colTitleOrig.Width = 250;
            // 
            // colYear
            // 
            this.colYear.Text = "Год";
            this.colYear.Width = 80;
            // 
            // mStatusStrip
            // 
            this.mStatusStrip.AutoSize = false;
            this.mStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mToolStripPath});
            this.mStatusStrip.Location = new System.Drawing.Point(0, 235);
            this.mStatusStrip.Name = "mStatusStrip";
            this.mStatusStrip.Size = new System.Drawing.Size(582, 22);
            this.mStatusStrip.SizingGrip = false;
            this.mStatusStrip.Stretch = false;
            this.mStatusStrip.TabIndex = 1;
            // 
            // mToolStripPath
            // 
            this.mToolStripPath.ForeColor = System.Drawing.Color.White;
            this.mToolStripPath.Name = "mToolStripPath";
            this.mToolStripPath.Size = new System.Drawing.Size(0, 17);
            // 
            // ChoicePopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkMagenta;
            this.ClientSize = new System.Drawing.Size(582, 257);
            this.ControlBox = false;
            this.Controls.Add(this.mStatusStrip);
            this.Controls.Add(this.mResultList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChoicePopup";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Доступные записи";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.mStatusStrip.ResumeLayout(false);
            this.mStatusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView mResultList;
        private System.Windows.Forms.ColumnHeader colTitleRu;
        private System.Windows.Forms.ColumnHeader colTitleOrig;
        private System.Windows.Forms.ColumnHeader colYear;
        private System.Windows.Forms.StatusStrip mStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel mToolStripPath;
    }
}