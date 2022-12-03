namespace Ariadna
{
    partial class MovieData
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
            System.Windows.Forms.Label lblPath;
            System.Windows.Forms.Label lblCast;
            System.Windows.Forms.Label lblLength;
            System.Windows.Forms.Label lblDescription;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MovieData));
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.txtTitleOriginal = new System.Windows.Forms.TextBox();
            this.txtYear = new System.Windows.Forms.TextBox();
            this.btnInsert = new System.Windows.Forms.Button();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.checkToSee = new System.Windows.Forms.CheckBox();
            this.m_DirectorsPhotos = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.m_DirectorsList = new System.Windows.Forms.ListView();
            this.label5 = new System.Windows.Forms.Label();
            this.m_CastList = new System.Windows.Forms.ListView();
            this.m_CastPhotos = new System.Windows.Forms.ImageList(this.components);
            this.m_GenresList = new System.Windows.Forms.ListView();
            this.m_GenresImages = new System.Windows.Forms.ImageList(this.components);
            this.m_AddGenreBtn = new System.Windows.Forms.Label();
            this.picPoster = new System.Windows.Forms.PictureBox();
            this.m_GenrePaste = new System.Windows.Forms.Label();
            this.m_DescriptionPaste = new System.Windows.Forms.Label();
            this.m_DirectorPaste = new System.Windows.Forms.Label();
            this.m_CastPaste = new System.Windows.Forms.Label();
            lblPath = new System.Windows.Forms.Label();
            lblCast = new System.Windows.Forms.Label();
            lblLength = new System.Windows.Forms.Label();
            lblDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picPoster)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPath
            // 
            lblPath.AutoSize = true;
            lblPath.ForeColor = System.Drawing.Color.White;
            lblPath.Location = new System.Drawing.Point(10, 613);
            lblPath.Name = "lblPath";
            lblPath.Size = new System.Drawing.Size(31, 13);
            lblPath.TabIndex = 2;
            lblPath.Text = "Путь";
            // 
            // lblCast
            // 
            lblCast.AutoSize = true;
            lblCast.ForeColor = System.Drawing.Color.White;
            lblCast.Location = new System.Drawing.Point(556, 353);
            lblCast.Name = "lblCast";
            lblCast.Size = new System.Drawing.Size(45, 13);
            lblCast.TabIndex = 10;
            lblCast.Text = "Актеры";
            // 
            // lblLength
            // 
            lblLength.AutoSize = true;
            lblLength.ForeColor = System.Drawing.Color.White;
            lblLength.Location = new System.Drawing.Point(420, 613);
            lblLength.Name = "lblLength";
            lblLength.Size = new System.Drawing.Size(111, 13);
            lblLength.TabIndex = 13;
            lblLength.Text = "Продолжительность";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.ForeColor = System.Drawing.Color.White;
            lblDescription.Location = new System.Drawing.Point(419, 185);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(57, 13);
            lblDescription.TabIndex = 20;
            lblDescription.Text = "Описание";
            // 
            // txtTitle
            // 
            this.txtTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtTitle.ForeColor = System.Drawing.Color.White;
            this.txtTitle.Location = new System.Drawing.Point(416, 22);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(603, 29);
            this.txtTitle.TabIndex = 1;
            // 
            // txtPath
            // 
            this.txtPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.txtPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPath.Enabled = false;
            this.txtPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtPath.ForeColor = System.Drawing.Color.LightGray;
            this.txtPath.Location = new System.Drawing.Point(7, 627);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(400, 22);
            this.txtPath.TabIndex = 3;
            // 
            // txtTitleOriginal
            // 
            this.txtTitleOriginal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.txtTitleOriginal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTitleOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtTitleOriginal.ForeColor = System.Drawing.Color.White;
            this.txtTitleOriginal.Location = new System.Drawing.Point(416, 65);
            this.txtTitleOriginal.Name = "txtTitleOriginal";
            this.txtTitleOriginal.Size = new System.Drawing.Size(603, 29);
            this.txtTitleOriginal.TabIndex = 2;
            // 
            // txtYear
            // 
            this.txtYear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.txtYear.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtYear.ForeColor = System.Drawing.Color.White;
            this.txtYear.Location = new System.Drawing.Point(416, 108);
            this.txtYear.Name = "txtYear";
            this.txtYear.Size = new System.Drawing.Size(77, 26);
            this.txtYear.TabIndex = 3;
            this.txtYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnInsert
            // 
            this.btnInsert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(0)))), ((int)(((byte)(75)))));
            this.btnInsert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInsert.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnInsert.ForeColor = System.Drawing.Color.White;
            this.btnInsert.Location = new System.Drawing.Point(925, 619);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(94, 29);
            this.btnInsert.TabIndex = 9;
            this.btnInsert.Text = "Вставить";
            this.btnInsert.UseVisualStyleBackColor = false;
            this.btnInsert.Click += new System.EventHandler(this.BtnInsert_Click);
            // 
            // txtLength
            // 
            this.txtLength.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.txtLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLength.Enabled = false;
            this.txtLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtLength.ForeColor = System.Drawing.Color.LightGray;
            this.txtLength.Location = new System.Drawing.Point(416, 627);
            this.txtLength.Name = "txtLength";
            this.txtLength.Size = new System.Drawing.Size(118, 22);
            this.txtLength.TabIndex = 19;
            this.txtLength.Text = "00:00:00";
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtDescription.ForeColor = System.Drawing.Color.White;
            this.txtDescription.Location = new System.Drawing.Point(416, 197);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(603, 154);
            this.txtDescription.TabIndex = 5;
            this.txtDescription.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnDescriptionKeyUp);
            // 
            // checkToSee
            // 
            this.checkToSee.AutoSize = true;
            this.checkToSee.ForeColor = System.Drawing.Color.Gold;
            this.checkToSee.Location = new System.Drawing.Point(843, 631);
            this.checkToSee.Name = "checkToSee";
            this.checkToSee.Size = new System.Drawing.Size(76, 17);
            this.checkToSee.TabIndex = 8;
            this.checkToSee.Text = "В хотелки";
            this.checkToSee.UseVisualStyleBackColor = true;
            // 
            // m_DirectorsPhotos
            // 
            this.m_DirectorsPhotos.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.m_DirectorsPhotos.ImageSize = new System.Drawing.Size(64, 96);
            this.m_DirectorsPhotos.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(504, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Жанр";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(419, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Название";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(419, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "Оригинальное название";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(419, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 32;
            this.label4.Text = "Год выпуска";
            // 
            // m_DirectorsList
            // 
            this.m_DirectorsList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.m_DirectorsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_DirectorsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_DirectorsList.ForeColor = System.Drawing.Color.White;
            this.m_DirectorsList.HideSelection = false;
            this.m_DirectorsList.LabelEdit = true;
            this.m_DirectorsList.LargeImageList = this.m_DirectorsPhotos;
            this.m_DirectorsList.Location = new System.Drawing.Point(416, 365);
            this.m_DirectorsList.MultiSelect = false;
            this.m_DirectorsList.Name = "m_DirectorsList";
            this.m_DirectorsList.Size = new System.Drawing.Size(130, 246);
            this.m_DirectorsList.TabIndex = 6;
            this.m_DirectorsList.UseCompatibleStateImageBehavior = false;
            this.m_DirectorsList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnDirectorsKeyUp);
            this.m_DirectorsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnDirectorsDoubleClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(420, 353);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 33;
            this.label5.Text = "Режисер";
            // 
            // m_CastList
            // 
            this.m_CastList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.m_CastList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_CastList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_CastList.ForeColor = System.Drawing.Color.White;
            this.m_CastList.HideSelection = false;
            this.m_CastList.LabelEdit = true;
            this.m_CastList.LargeImageList = this.m_CastPhotos;
            this.m_CastList.Location = new System.Drawing.Point(553, 365);
            this.m_CastList.MultiSelect = false;
            this.m_CastList.Name = "m_CastList";
            this.m_CastList.Size = new System.Drawing.Size(466, 246);
            this.m_CastList.TabIndex = 7;
            this.m_CastList.UseCompatibleStateImageBehavior = false;
            this.m_CastList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnCastKeyUp);
            this.m_CastList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnCastDoubleClicked);
            // 
            // m_CastPhotos
            // 
            this.m_CastPhotos.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.m_CastPhotos.ImageSize = new System.Drawing.Size(64, 96);
            this.m_CastPhotos.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_GenresList
            // 
            this.m_GenresList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.m_GenresList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_GenresList.ForeColor = System.Drawing.Color.White;
            this.m_GenresList.HideSelection = false;
            this.m_GenresList.LargeImageList = this.m_GenresImages;
            this.m_GenresList.Location = new System.Drawing.Point(499, 108);
            this.m_GenresList.MultiSelect = false;
            this.m_GenresList.Name = "m_GenresList";
            this.m_GenresList.Size = new System.Drawing.Size(520, 83);
            this.m_GenresList.TabIndex = 4;
            this.m_GenresList.UseCompatibleStateImageBehavior = false;
            this.m_GenresList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnGenresKeyUp);
            // 
            // m_GenresImages
            // 
            this.m_GenresImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.m_GenresImages.ImageSize = new System.Drawing.Size(60, 60);
            this.m_GenresImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_AddGenreBtn
            // 
            this.m_AddGenreBtn.AutoSize = true;
            this.m_AddGenreBtn.ForeColor = System.Drawing.Color.White;
            this.m_AddGenreBtn.Location = new System.Drawing.Point(998, 174);
            this.m_AddGenreBtn.Name = "m_AddGenreBtn";
            this.m_AddGenreBtn.Size = new System.Drawing.Size(16, 13);
            this.m_AddGenreBtn.TabIndex = 34;
            this.m_AddGenreBtn.Text = "...";
            this.m_AddGenreBtn.Click += new System.EventHandler(this.OnAddGenreClicked);
            // 
            // picPoster
            // 
            this.picPoster.BackColor = System.Drawing.SystemColors.ControlDark;
            this.picPoster.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picPoster.ErrorImage = ((System.Drawing.Image)(resources.GetObject("picPoster.ErrorImage")));
            this.picPoster.Image = global::Ariadna.Properties.Resources.No_Preview_Image;
            this.picPoster.InitialImage = ((System.Drawing.Image)(resources.GetObject("picPoster.InitialImage")));
            this.picPoster.Location = new System.Drawing.Point(7, 10);
            this.picPoster.Name = "picPoster";
            this.picPoster.Size = new System.Drawing.Size(400, 600);
            this.picPoster.TabIndex = 14;
            this.picPoster.TabStop = false;
            this.picPoster.DoubleClick += new System.EventHandler(this.PicPoster_DoubleClick);
            // 
            // m_GenrePaste
            // 
            this.m_GenrePaste.AutoSize = true;
            this.m_GenrePaste.ForeColor = System.Drawing.Color.White;
            this.m_GenrePaste.Location = new System.Drawing.Point(540, 96);
            this.m_GenrePaste.Name = "m_GenrePaste";
            this.m_GenrePaste.Size = new System.Drawing.Size(13, 13);
            this.m_GenrePaste.TabIndex = 35;
            this.m_GenrePaste.Text = "↓";
            this.m_GenrePaste.Click += new System.EventHandler(this.OnGenrePasteClick);
            // 
            // m_DescriptionPaste
            // 
            this.m_DescriptionPaste.AutoSize = true;
            this.m_DescriptionPaste.ForeColor = System.Drawing.Color.White;
            this.m_DescriptionPaste.Location = new System.Drawing.Point(476, 185);
            this.m_DescriptionPaste.Name = "m_DescriptionPaste";
            this.m_DescriptionPaste.Size = new System.Drawing.Size(13, 13);
            this.m_DescriptionPaste.TabIndex = 36;
            this.m_DescriptionPaste.Text = "↓";
            this.m_DescriptionPaste.Click += new System.EventHandler(this.OnDescriptionPasteClick);
            // 
            // m_DirectorPaste
            // 
            this.m_DirectorPaste.AutoSize = true;
            this.m_DirectorPaste.ForeColor = System.Drawing.Color.White;
            this.m_DirectorPaste.Location = new System.Drawing.Point(472, 353);
            this.m_DirectorPaste.Name = "m_DirectorPaste";
            this.m_DirectorPaste.Size = new System.Drawing.Size(13, 13);
            this.m_DirectorPaste.TabIndex = 37;
            this.m_DirectorPaste.Text = "↓";
            this.m_DirectorPaste.Click += new System.EventHandler(this.OnDirectorPasteClick);
            // 
            // m_CastPaste
            // 
            this.m_CastPaste.AutoSize = true;
            this.m_CastPaste.ForeColor = System.Drawing.Color.White;
            this.m_CastPaste.Location = new System.Drawing.Point(601, 353);
            this.m_CastPaste.Name = "m_CastPaste";
            this.m_CastPaste.Size = new System.Drawing.Size(13, 13);
            this.m_CastPaste.TabIndex = 38;
            this.m_CastPaste.Text = "↓";
            this.m_CastPaste.Click += new System.EventHandler(this.OnCastPasteClick);
            // 
            // MovieData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1027, 656);
            this.Controls.Add(this.m_CastPaste);
            this.Controls.Add(this.m_DirectorPaste);
            this.Controls.Add(this.m_DescriptionPaste);
            this.Controls.Add(this.m_GenrePaste);
            this.Controls.Add(this.m_AddGenreBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_GenresList);
            this.Controls.Add(lblDescription);
            this.Controls.Add(lblLength);
            this.Controls.Add(lblPath);
            this.Controls.Add(lblCast);
            this.Controls.Add(this.m_CastList);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.m_DirectorsList);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtYear);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.txtTitleOriginal);
            this.Controls.Add(this.checkToSee);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtLength);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.picPoster);
            this.Controls.Add(this.txtPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "MovieData";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавление записи";
            this.Load += new System.EventHandler(this.AddMovie_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MovieData_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MovieData_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.picPoster)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.TextBox txtTitleOriginal;
        private System.Windows.Forms.TextBox txtYear;
        private System.Windows.Forms.PictureBox picPoster;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.CheckBox checkToSee;
        private System.Windows.Forms.ImageList m_DirectorsPhotos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView m_DirectorsList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListView m_CastList;
        private System.Windows.Forms.ImageList m_CastPhotos;
        private System.Windows.Forms.ListView m_GenresList;
        private System.Windows.Forms.ImageList m_GenresImages;
        private System.Windows.Forms.Label m_AddGenreBtn;
        private System.Windows.Forms.Label m_GenrePaste;
        private System.Windows.Forms.Label m_DescriptionPaste;
        private System.Windows.Forms.Label m_DirectorPaste;
        private System.Windows.Forms.Label m_CastPaste;
    }
}