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
            m_ResultList = new System.Windows.Forms.ListView();
            m_colTitleRu = new System.Windows.Forms.ColumnHeader();
            m_colTitleOrig = new System.Windows.Forms.ColumnHeader();
            m_colYear = new System.Windows.Forms.ColumnHeader();
            m_StatusStrip = new System.Windows.Forms.StatusStrip();
            m_ToolStripPath = new System.Windows.Forms.ToolStripStatusLabel();
            m_StatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // m_ResultList
            // 
            m_ResultList.BackColor = System.Drawing.Color.DarkMagenta;
            m_ResultList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { m_colTitleRu, m_colTitleOrig, m_colYear });
            m_ResultList.Dock = System.Windows.Forms.DockStyle.Fill;
            m_ResultList.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_ResultList.ForeColor = System.Drawing.Color.White;
            m_ResultList.Location = new System.Drawing.Point(0, 0);
            m_ResultList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            m_ResultList.Name = "m_ResultList";
            m_ResultList.Size = new System.Drawing.Size(679, 297);
            m_ResultList.TabIndex = 0;
            m_ResultList.UseCompatibleStateImageBehavior = false;
            m_ResultList.View = System.Windows.Forms.View.Details;
            m_ResultList.SelectedIndexChanged += OnSelectedIndexChanged;
            m_ResultList.DoubleClick += OnDoubleClick;
            // 
            // m_colTitleRu
            // 
            m_colTitleRu.Text = "Russian title";
            m_colTitleRu.Width = 250;
            // 
            // m_colTitleOrig
            // 
            m_colTitleOrig.Text = "Original title";
            m_colTitleOrig.Width = 250;
            // 
            // m_colYear
            // 
            m_colYear.Text = "Year";
            m_colYear.Width = 80;
            // 
            // m_StatusStrip
            // 
            m_StatusStrip.AutoSize = false;
            m_StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { m_ToolStripPath });
            m_StatusStrip.Location = new System.Drawing.Point(0, 272);
            m_StatusStrip.Name = "m_StatusStrip";
            m_StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            m_StatusStrip.Size = new System.Drawing.Size(679, 25);
            m_StatusStrip.SizingGrip = false;
            m_StatusStrip.Stretch = false;
            m_StatusStrip.TabIndex = 1;
            // 
            // m_ToolStripPath
            // 
            m_ToolStripPath.ForeColor = System.Drawing.Color.White;
            m_ToolStripPath.Name = "m_ToolStripPath";
            m_ToolStripPath.Size = new System.Drawing.Size(0, 20);
            // 
            // ChoicePopup
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.DarkMagenta;
            ClientSize = new System.Drawing.Size(679, 297);
            ControlBox = false;
            Controls.Add(m_StatusStrip);
            Controls.Add(m_ResultList);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChoicePopup";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Available entries";
            KeyDown += OnKeyDown;
            m_StatusStrip.ResumeLayout(false);
            m_StatusStrip.PerformLayout();
            ResumeLayout(false);
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