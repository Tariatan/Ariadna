namespace Ariadna
{
    partial class SplashForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.m_StatusInfoLbl = new System.Windows.Forms.Label();
            this.m_SplashAnimation = new System.Windows.Forms.PictureBox();
            this.m_Label = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_SplashAnimation)).BeginInit();
            this.SuspendLayout();
            // 
            // m_StatusInfoLbl
            // 
            this.m_StatusInfoLbl.BackColor = System.Drawing.Color.Black;
            this.m_StatusInfoLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_StatusInfoLbl.ForeColor = System.Drawing.Color.DarkMagenta;
            this.m_StatusInfoLbl.Location = new System.Drawing.Point(12, 373);
            this.m_StatusInfoLbl.Name = "m_StatusInfoLbl";
            this.m_StatusInfoLbl.Size = new System.Drawing.Size(136, 23);
            this.m_StatusInfoLbl.TabIndex = 0;
            this.m_StatusInfoLbl.Text = "Loading database";
            // 
            // m_SplashAnimation
            // 
            this.m_SplashAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SplashAnimation.ErrorImage = null;
            this.m_SplashAnimation.Image = ((System.Drawing.Image)(resources.GetObject("m_SplashAnimation.Image")));
            this.m_SplashAnimation.InitialImage = null;
            this.m_SplashAnimation.Location = new System.Drawing.Point(0, 0);
            this.m_SplashAnimation.Name = "m_SplashAnimation";
            this.m_SplashAnimation.Size = new System.Drawing.Size(405, 405);
            this.m_SplashAnimation.TabIndex = 1;
            this.m_SplashAnimation.TabStop = false;
            // 
            // m_Label
            // 
            this.m_Label.BackColor = System.Drawing.Color.Black;
            this.m_Label.Font = new System.Drawing.Font("Bauhaus 93", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Label.ForeColor = System.Drawing.Color.DarkMagenta;
            this.m_Label.Location = new System.Drawing.Point(40, 40);
            this.m_Label.Name = "m_Label";
            this.m_Label.Size = new System.Drawing.Size(176, 41);
            this.m_Label.TabIndex = 2;
            this.m_Label.Text = "Ariadna";
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkMagenta;
            this.ClientSize = new System.Drawing.Size(405, 405);
            this.Controls.Add(this.m_Label);
            this.Controls.Add(this.m_StatusInfoLbl);
            this.Controls.Add(this.m_SplashAnimation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SplashForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashForm";
            ((System.ComponentModel.ISupportInitialize)(this.m_SplashAnimation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_StatusInfoLbl;
        private System.Windows.Forms.PictureBox m_SplashAnimation;
        private System.Windows.Forms.Label m_Label;
    }
}