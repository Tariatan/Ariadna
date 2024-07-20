namespace Ariadna.AuxiliaryPopups
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
            this.m_ResultList = new System.Windows.Forms.ListView();
            this.m_colTitleRu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_colTitleOrig = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_colYear = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_StatusStrip = new System.Windows.Forms.StatusStrip();
            this.m_ToolStripPath = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_ResultList
            // 
            this.m_ResultList.BackColor = System.Drawing.Color.DarkMagenta;
            this.m_ResultList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_colTitleRu,
            this.m_colTitleOrig,
            this.m_colYear});
            this.m_ResultList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ResultList.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_ResultList.ForeColor = System.Drawing.Color.White;
            this.m_ResultList.HideSelection = false;
            this.m_ResultList.Location = new System.Drawing.Point(0, 0);
            this.m_ResultList.Name = "m_ResultList";
            this.m_ResultList.Size = new System.Drawing.Size(582, 257);
            this.m_ResultList.TabIndex = 0;
            this.m_ResultList.UseCompatibleStateImageBehavior = false;
            this.m_ResultList.View = System.Windows.Forms.View.Details;
            this.m_ResultList.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            this.m_ResultList.DoubleClick += new System.EventHandler(this.OnDoubleClick);
            // 
            // m_colTitleRu
            // 
            this.m_colTitleRu.Text = "Русское название";
            this.m_colTitleRu.Width = 250;
            // 
            // m_colTitleOrig
            // 
            this.m_colTitleOrig.Text = "Оригинальное название";
            this.m_colTitleOrig.Width = 250;
            // 
            // m_colYear
            // 
            this.m_colYear.Text = "Год";
            this.m_colYear.Width = 80;
            // 
            // m_StatusStrip
            // 
            this.m_StatusStrip.AutoSize = false;
            this.m_StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStripPath});
            this.m_StatusStrip.Location = new System.Drawing.Point(0, 235);
            this.m_StatusStrip.Name = "m_StatusStrip";
            this.m_StatusStrip.Size = new System.Drawing.Size(582, 22);
            this.m_StatusStrip.SizingGrip = false;
            this.m_StatusStrip.Stretch = false;
            this.m_StatusStrip.TabIndex = 1;
            // 
            // m_ToolStripPath
            // 
            this.m_ToolStripPath.ForeColor = System.Drawing.Color.White;
            this.m_ToolStripPath.Name = "m_ToolStripPath";
            this.m_ToolStripPath.Size = new System.Drawing.Size(0, 17);
            // 
            // ChoicePopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkMagenta;
            this.ClientSize = new System.Drawing.Size(582, 257);
            this.ControlBox = false;
            this.Controls.Add(this.m_StatusStrip);
            this.Controls.Add(this.m_ResultList);
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
            this.m_StatusStrip.ResumeLayout(false);
            this.m_StatusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView m_ResultList;
        private System.Windows.Forms.ColumnHeader m_colTitleRu;
        private System.Windows.Forms.ColumnHeader m_colTitleOrig;
        private System.Windows.Forms.ColumnHeader m_colYear;
        private System.Windows.Forms.StatusStrip m_StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel m_ToolStripPath;
    }
}