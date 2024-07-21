
namespace Ariadna.AuxiliaryPopups
{
    partial class DetailsForm
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
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailsForm));
            m_TxtTitle = new System.Windows.Forms.TextBox();
            m_TxtPath = new System.Windows.Forms.TextBox();
            m_TxtTitleOrig = new System.Windows.Forms.TextBox();
            m_TxtYear = new System.Windows.Forms.TextBox();
            m_BtnInsert = new System.Windows.Forms.Button();
            m_TxtLength = new System.Windows.Forms.TextBox();
            m_TxtDescription = new System.Windows.Forms.TextBox();
            m_WantToSee = new System.Windows.Forms.CheckBox();
            m_DirectorsPhotos = new System.Windows.Forms.ImageList(components);
            m_LblGenre = new System.Windows.Forms.Label();
            m_LblTitle = new System.Windows.Forms.Label();
            m_LblTitleOrig = new System.Windows.Forms.Label();
            m_LblYear = new System.Windows.Forms.Label();
            m_DirectorsList = new System.Windows.Forms.ListView();
            m_LblDirector = new System.Windows.Forms.Label();
            m_CastList = new System.Windows.Forms.ListView();
            m_CastPhotos = new System.Windows.Forms.ImageList(components);
            m_GenresList = new System.Windows.Forms.ListView();
            m_GenresImages = new System.Windows.Forms.ImageList(components);
            m_AddGenreBtn = new System.Windows.Forms.Label();
            m_GenrePaste = new System.Windows.Forms.Label();
            m_DescriptionPaste = new System.Windows.Forms.Label();
            m_DirectorPaste = new System.Windows.Forms.Label();
            m_CastPaste = new System.Windows.Forms.Label();
            m_LblPath = new System.Windows.Forms.Label();
            m_LblCast = new System.Windows.Forms.Label();
            m_LblDescr = new System.Windows.Forms.Label();
            m_Preview4 = new System.Windows.Forms.PictureBox();
            m_Preview3 = new System.Windows.Forms.PictureBox();
            m_Preview2 = new System.Windows.Forms.PictureBox();
            m_Preview1 = new System.Windows.Forms.PictureBox();
            m_PreviewFull = new System.Windows.Forms.PictureBox();
            m_PicPoster = new System.Windows.Forms.PictureBox();
            m_VR = new System.Windows.Forms.CheckBox();
            m_LblVersion = new System.Windows.Forms.Label();
            m_TxtVersion = new System.Windows.Forms.TextBox();
            m_LblVolume = new System.Windows.Forms.Label();
            m_TxtVolume = new System.Windows.Forms.TextBox();
            m_TxtDimension = new System.Windows.Forms.TextBox();
            m_TxtBitrate = new System.Windows.Forms.TextBox();
            m_LangImages = new System.Windows.Forms.ImageList(components);
            m_PicFlag1 = new System.Windows.Forms.PictureBox();
            m_PicFlag2 = new System.Windows.Forms.PictureBox();
            m_PicFlag3 = new System.Windows.Forms.PictureBox();
            m_PicFlag4 = new System.Windows.Forms.PictureBox();
            m_LblDuration = new System.Windows.Forms.Label();
            m_LblDimensions = new System.Windows.Forms.Label();
            m_LblBitrate = new System.Windows.Forms.Label();
            m_LblAudioStreams = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)m_Preview4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_Preview3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_Preview2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_Preview1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_PreviewFull).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_PicPoster).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag4).BeginInit();
            SuspendLayout();
            // 
            // m_TxtTitle
            // 
            m_TxtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtTitle.Location = new System.Drawing.Point(416, 22);
            m_TxtTitle.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtTitle.Name = "m_TxtTitle";
            m_TxtTitle.Size = new System.Drawing.Size(603, 29);
            m_TxtTitle.TabIndex = 1;
            m_TxtTitle.Enter += HideFloatingPanel;
            // 
            // m_TxtPath
            // 
            m_TxtPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtPath.Location = new System.Drawing.Point(7, 627);
            m_TxtPath.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtPath.Name = "m_TxtPath";
            m_TxtPath.Size = new System.Drawing.Size(323, 22);
            m_TxtPath.TabIndex = 3;
            m_TxtPath.TextChanged += OnFilePathChanged;
            // 
            // m_TxtTitleOrig
            // 
            m_TxtTitleOrig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtTitleOrig.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtTitleOrig.Location = new System.Drawing.Point(416, 65);
            m_TxtTitleOrig.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtTitleOrig.Name = "m_TxtTitleOrig";
            m_TxtTitleOrig.Size = new System.Drawing.Size(603, 29);
            m_TxtTitleOrig.TabIndex = 2;
            m_TxtTitleOrig.Enter += HideFloatingPanel;
            // 
            // m_TxtYear
            // 
            m_TxtYear.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtYear.Location = new System.Drawing.Point(416, 108);
            m_TxtYear.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtYear.Name = "m_TxtYear";
            m_TxtYear.Size = new System.Drawing.Size(77, 26);
            m_TxtYear.TabIndex = 3;
            m_TxtYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_BtnInsert
            // 
            m_BtnInsert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            m_BtnInsert.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_BtnInsert.Location = new System.Drawing.Point(925, 619);
            m_BtnInsert.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_BtnInsert.Name = "m_BtnInsert";
            m_BtnInsert.Size = new System.Drawing.Size(94, 29);
            m_BtnInsert.TabIndex = 9;
            m_BtnInsert.Text = "Insert";
            m_BtnInsert.UseVisualStyleBackColor = false;
            m_BtnInsert.Click += OnBtnInsert_Click;
            // 
            // m_TxtLength
            // 
            m_TxtLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtLength.Enabled = false;
            m_TxtLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtLength.Location = new System.Drawing.Point(416, 627);
            m_TxtLength.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtLength.Name = "m_TxtLength";
            m_TxtLength.Size = new System.Drawing.Size(60, 22);
            m_TxtLength.TabIndex = 19;
            m_TxtLength.Text = "00:00:00";
            m_TxtLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_TxtDescription
            // 
            m_TxtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtDescription.Location = new System.Drawing.Point(416, 197);
            m_TxtDescription.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtDescription.Multiline = true;
            m_TxtDescription.Name = "m_TxtDescription";
            m_TxtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            m_TxtDescription.Size = new System.Drawing.Size(603, 154);
            m_TxtDescription.TabIndex = 5;
            m_TxtDescription.Enter += HideFloatingPanel;
            m_TxtDescription.KeyUp += OnDescriptionKeyUp;
            // 
            // m_WantToSee
            // 
            m_WantToSee.AutoSize = true;
            m_WantToSee.BackColor = System.Drawing.Color.Transparent;
            m_WantToSee.Location = new System.Drawing.Point(843, 626);
            m_WantToSee.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_WantToSee.Name = "m_WantToSee";
            m_WantToSee.Size = new System.Drawing.Size(80, 19);
            m_WantToSee.TabIndex = 8;
            m_WantToSee.Text = "To wishlist";
            m_WantToSee.UseVisualStyleBackColor = false;
            // 
            // m_DirectorsPhotos
            // 
            m_DirectorsPhotos.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            m_DirectorsPhotos.ImageSize = new System.Drawing.Size(64, 96);
            m_DirectorsPhotos.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_LblGenre
            // 
            m_LblGenre.AutoSize = true;
            m_LblGenre.BackColor = System.Drawing.Color.Transparent;
            m_LblGenre.Location = new System.Drawing.Point(504, 96);
            m_LblGenre.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblGenre.Name = "m_LblGenre";
            m_LblGenre.Size = new System.Drawing.Size(38, 15);
            m_LblGenre.TabIndex = 29;
            m_LblGenre.Text = "Genre";
            // 
            // m_LblTitle
            // 
            m_LblTitle.AutoSize = true;
            m_LblTitle.BackColor = System.Drawing.Color.Transparent;
            m_LblTitle.Location = new System.Drawing.Point(419, 10);
            m_LblTitle.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblTitle.Name = "m_LblTitle";
            m_LblTitle.Size = new System.Drawing.Size(29, 15);
            m_LblTitle.TabIndex = 30;
            m_LblTitle.Text = "Title";
            // 
            // m_LblTitleOrig
            // 
            m_LblTitleOrig.AutoSize = true;
            m_LblTitleOrig.BackColor = System.Drawing.Color.Transparent;
            m_LblTitleOrig.Location = new System.Drawing.Point(419, 53);
            m_LblTitleOrig.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblTitleOrig.Name = "m_LblTitleOrig";
            m_LblTitleOrig.Size = new System.Drawing.Size(72, 15);
            m_LblTitleOrig.TabIndex = 31;
            m_LblTitleOrig.Text = "Original title";
            // 
            // m_LblYear
            // 
            m_LblYear.AutoSize = true;
            m_LblYear.BackColor = System.Drawing.Color.Transparent;
            m_LblYear.Location = new System.Drawing.Point(419, 96);
            m_LblYear.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblYear.Name = "m_LblYear";
            m_LblYear.Size = new System.Drawing.Size(29, 15);
            m_LblYear.TabIndex = 32;
            m_LblYear.Text = "Year";
            // 
            // m_DirectorsList
            // 
            m_DirectorsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_DirectorsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_DirectorsList.LabelEdit = true;
            m_DirectorsList.LargeImageList = m_DirectorsPhotos;
            m_DirectorsList.Location = new System.Drawing.Point(416, 365);
            m_DirectorsList.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_DirectorsList.MultiSelect = false;
            m_DirectorsList.Name = "m_DirectorsList";
            m_DirectorsList.Size = new System.Drawing.Size(130, 246);
            m_DirectorsList.TabIndex = 6;
            m_DirectorsList.UseCompatibleStateImageBehavior = false;
            m_DirectorsList.AfterLabelEdit += OnDirectorRenamed;
            m_DirectorsList.Enter += HideFloatingPanel;
            m_DirectorsList.KeyUp += OnDirectorsKeyUp;
            m_DirectorsList.MouseDoubleClick += OnDirectorsDoubleClicked;
            // 
            // m_LblDirector
            // 
            m_LblDirector.AutoSize = true;
            m_LblDirector.BackColor = System.Drawing.Color.Transparent;
            m_LblDirector.Location = new System.Drawing.Point(420, 353);
            m_LblDirector.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblDirector.Name = "m_LblDirector";
            m_LblDirector.Size = new System.Drawing.Size(49, 15);
            m_LblDirector.TabIndex = 33;
            m_LblDirector.Text = "Director";
            // 
            // m_CastList
            // 
            m_CastList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_CastList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_CastList.LabelEdit = true;
            m_CastList.LargeImageList = m_CastPhotos;
            m_CastList.Location = new System.Drawing.Point(553, 365);
            m_CastList.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_CastList.MultiSelect = false;
            m_CastList.Name = "m_CastList";
            m_CastList.Size = new System.Drawing.Size(466, 246);
            m_CastList.TabIndex = 7;
            m_CastList.UseCompatibleStateImageBehavior = false;
            m_CastList.AfterLabelEdit += OnActorRenamed;
            m_CastList.Enter += HideFloatingPanel;
            m_CastList.KeyUp += OnCastKeyUp;
            m_CastList.MouseDoubleClick += OnCastDoubleClicked;
            // 
            // m_CastPhotos
            // 
            m_CastPhotos.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            m_CastPhotos.ImageSize = new System.Drawing.Size(64, 96);
            m_CastPhotos.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_GenresList
            // 
            m_GenresList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_GenresList.LargeImageList = m_GenresImages;
            m_GenresList.Location = new System.Drawing.Point(499, 108);
            m_GenresList.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_GenresList.MultiSelect = false;
            m_GenresList.Name = "m_GenresList";
            m_GenresList.Scrollable = false;
            m_GenresList.Size = new System.Drawing.Size(520, 83);
            m_GenresList.TabIndex = 4;
            m_GenresList.UseCompatibleStateImageBehavior = false;
            m_GenresList.Enter += HideFloatingPanel;
            m_GenresList.KeyUp += OnGenresKeyUp;
            // 
            // m_GenresImages
            // 
            m_GenresImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            m_GenresImages.ImageSize = new System.Drawing.Size(60, 60);
            m_GenresImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_AddGenreBtn
            // 
            m_AddGenreBtn.AutoSize = true;
            m_AddGenreBtn.Location = new System.Drawing.Point(998, 174);
            m_AddGenreBtn.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_AddGenreBtn.Name = "m_AddGenreBtn";
            m_AddGenreBtn.Size = new System.Drawing.Size(16, 15);
            m_AddGenreBtn.TabIndex = 34;
            m_AddGenreBtn.Text = "...";
            m_AddGenreBtn.Click += OnAddGenreClicked;
            // 
            // m_GenrePaste
            // 
            m_GenrePaste.AutoSize = true;
            m_GenrePaste.CausesValidation = false;
            m_GenrePaste.Location = new System.Drawing.Point(546, 96);
            m_GenrePaste.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_GenrePaste.Name = "m_GenrePaste";
            m_GenrePaste.Size = new System.Drawing.Size(13, 15);
            m_GenrePaste.TabIndex = 35;
            m_GenrePaste.Text = "↓";
            m_GenrePaste.UseMnemonic = false;
            m_GenrePaste.Click += OnGenrePasteClick;
            // 
            // m_DescriptionPaste
            // 
            m_DescriptionPaste.AutoSize = true;
            m_DescriptionPaste.CausesValidation = false;
            m_DescriptionPaste.Location = new System.Drawing.Point(490, 185);
            m_DescriptionPaste.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_DescriptionPaste.Name = "m_DescriptionPaste";
            m_DescriptionPaste.Size = new System.Drawing.Size(13, 15);
            m_DescriptionPaste.TabIndex = 36;
            m_DescriptionPaste.Text = "↓";
            m_DescriptionPaste.UseMnemonic = false;
            m_DescriptionPaste.Click += OnDescriptionPasteClick;
            // 
            // m_DirectorPaste
            // 
            m_DirectorPaste.AutoSize = true;
            m_DirectorPaste.CausesValidation = false;
            m_DirectorPaste.Location = new System.Drawing.Point(474, 353);
            m_DirectorPaste.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_DirectorPaste.Name = "m_DirectorPaste";
            m_DirectorPaste.Size = new System.Drawing.Size(13, 15);
            m_DirectorPaste.TabIndex = 37;
            m_DirectorPaste.Text = "↓";
            m_DirectorPaste.UseMnemonic = false;
            m_DirectorPaste.Click += OnDirectorPasteClick;
            // 
            // m_CastPaste
            // 
            m_CastPaste.AutoSize = true;
            m_CastPaste.CausesValidation = false;
            m_CastPaste.Location = new System.Drawing.Point(593, 353);
            m_CastPaste.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_CastPaste.Name = "m_CastPaste";
            m_CastPaste.Size = new System.Drawing.Size(13, 15);
            m_CastPaste.TabIndex = 38;
            m_CastPaste.Text = "↓";
            m_CastPaste.UseMnemonic = false;
            m_CastPaste.Click += OnCastPasteClick;
            // 
            // m_LblPath
            // 
            m_LblPath.AutoSize = true;
            m_LblPath.BackColor = System.Drawing.Color.Transparent;
            m_LblPath.Location = new System.Drawing.Point(10, 613);
            m_LblPath.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblPath.Name = "m_LblPath";
            m_LblPath.Size = new System.Drawing.Size(31, 15);
            m_LblPath.TabIndex = 2;
            m_LblPath.Text = "Path";
            // 
            // m_LblCast
            // 
            m_LblCast.AutoSize = true;
            m_LblCast.BackColor = System.Drawing.Color.Transparent;
            m_LblCast.Location = new System.Drawing.Point(556, 353);
            m_LblCast.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblCast.Name = "m_LblCast";
            m_LblCast.Size = new System.Drawing.Size(30, 15);
            m_LblCast.TabIndex = 10;
            m_LblCast.Text = "Cast";
            // 
            // m_LblDescr
            // 
            m_LblDescr.AutoSize = true;
            m_LblDescr.BackColor = System.Drawing.Color.Transparent;
            m_LblDescr.Location = new System.Drawing.Point(419, 185);
            m_LblDescr.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblDescr.Name = "m_LblDescr";
            m_LblDescr.Size = new System.Drawing.Size(67, 15);
            m_LblDescr.TabIndex = 20;
            m_LblDescr.Text = "Description";
            // 
            // m_Preview4
            // 
            m_Preview4.BackColor = System.Drawing.SystemColors.ControlDark;
            m_Preview4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_Preview4.Cursor = System.Windows.Forms.Cursors.Hand;
            m_Preview4.ErrorImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview4.Image = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview4.InitialImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview4.Location = new System.Drawing.Point(882, 534);
            m_Preview4.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_Preview4.Name = "m_Preview4";
            m_Preview4.Size = new System.Drawing.Size(137, 77);
            m_Preview4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            m_Preview4.TabIndex = 43;
            m_Preview4.TabStop = false;
            m_Preview4.Visible = false;
            m_Preview4.Click += OnClick;
            m_Preview4.DoubleClick += OnPreview_DoubleClick;
            // 
            // m_Preview3
            // 
            m_Preview3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_Preview3.Cursor = System.Windows.Forms.Cursors.Hand;
            m_Preview3.ErrorImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview3.Image = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview3.InitialImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview3.Location = new System.Drawing.Point(726, 534);
            m_Preview3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_Preview3.Name = "m_Preview3";
            m_Preview3.Size = new System.Drawing.Size(137, 77);
            m_Preview3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            m_Preview3.TabIndex = 42;
            m_Preview3.TabStop = false;
            m_Preview3.Visible = false;
            m_Preview3.Click += OnClick;
            m_Preview3.DoubleClick += OnPreview_DoubleClick;
            // 
            // m_Preview2
            // 
            m_Preview2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_Preview2.Cursor = System.Windows.Forms.Cursors.Hand;
            m_Preview2.ErrorImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview2.Image = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview2.InitialImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview2.Location = new System.Drawing.Point(571, 534);
            m_Preview2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_Preview2.Name = "m_Preview2";
            m_Preview2.Size = new System.Drawing.Size(137, 77);
            m_Preview2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            m_Preview2.TabIndex = 41;
            m_Preview2.TabStop = false;
            m_Preview2.Visible = false;
            m_Preview2.Click += OnClick;
            m_Preview2.DoubleClick += OnPreview_DoubleClick;
            // 
            // m_Preview1
            // 
            m_Preview1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_Preview1.Cursor = System.Windows.Forms.Cursors.Hand;
            m_Preview1.ErrorImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview1.Image = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview1.InitialImage = Properties.Resources.No_Preview_Image_Wide_small;
            m_Preview1.Location = new System.Drawing.Point(416, 534);
            m_Preview1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_Preview1.Name = "m_Preview1";
            m_Preview1.Size = new System.Drawing.Size(137, 77);
            m_Preview1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            m_Preview1.TabIndex = 40;
            m_Preview1.TabStop = false;
            m_Preview1.Visible = false;
            m_Preview1.Click += OnClick;
            m_Preview1.DoubleClick += OnPreview_DoubleClick;
            // 
            // m_PreviewFull
            // 
            m_PreviewFull.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_PreviewFull.Enabled = false;
            m_PreviewFull.ErrorImage = Properties.Resources.No_Preview_Image_Wide;
            m_PreviewFull.Image = Properties.Resources.No_Preview_Image_Wide;
            m_PreviewFull.InitialImage = Properties.Resources.No_Preview_Image_Wide;
            m_PreviewFull.Location = new System.Drawing.Point(416, 194);
            m_PreviewFull.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_PreviewFull.Name = "m_PreviewFull";
            m_PreviewFull.Size = new System.Drawing.Size(603, 339);
            m_PreviewFull.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            m_PreviewFull.TabIndex = 39;
            m_PreviewFull.TabStop = false;
            m_PreviewFull.Visible = false;
            // 
            // m_PicPoster
            // 
            m_PicPoster.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            m_PicPoster.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            m_PicPoster.ErrorImage = (System.Drawing.Image)resources.GetObject("m_PicPoster.ErrorImage");
            m_PicPoster.Image = Properties.Resources.No_Preview_Image;
            m_PicPoster.InitialImage = (System.Drawing.Image)resources.GetObject("m_PicPoster.InitialImage");
            m_PicPoster.Location = new System.Drawing.Point(7, 10);
            m_PicPoster.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_PicPoster.Name = "m_PicPoster";
            m_PicPoster.Size = new System.Drawing.Size(400, 600);
            m_PicPoster.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            m_PicPoster.TabIndex = 14;
            m_PicPoster.TabStop = false;
            m_PicPoster.DoubleClick += OnPicPoster_DoubleClick;
            // 
            // m_VR
            // 
            m_VR.AutoSize = true;
            m_VR.BackColor = System.Drawing.Color.Transparent;
            m_VR.Location = new System.Drawing.Point(793, 626);
            m_VR.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_VR.Name = "m_VR";
            m_VR.Size = new System.Drawing.Size(40, 19);
            m_VR.TabIndex = 44;
            m_VR.Text = "VR";
            m_VR.UseVisualStyleBackColor = false;
            m_VR.Visible = false;
            // 
            // m_LblVersion
            // 
            m_LblVersion.AutoSize = true;
            m_LblVersion.BackColor = System.Drawing.Color.Transparent;
            m_LblVersion.Location = new System.Drawing.Point(419, 136);
            m_LblVersion.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblVersion.Name = "m_LblVersion";
            m_LblVersion.Size = new System.Drawing.Size(45, 15);
            m_LblVersion.TabIndex = 45;
            m_LblVersion.Text = "Version";
            m_LblVersion.Visible = false;
            // 
            // m_TxtVersion
            // 
            m_TxtVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtVersion.Location = new System.Drawing.Point(417, 148);
            m_TxtVersion.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtVersion.Name = "m_TxtVersion";
            m_TxtVersion.Size = new System.Drawing.Size(77, 22);
            m_TxtVersion.TabIndex = 46;
            m_TxtVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            m_TxtVersion.Visible = false;
            // 
            // m_LblVolume
            // 
            m_LblVolume.AutoSize = true;
            m_LblVolume.Location = new System.Drawing.Point(336, 613);
            m_LblVolume.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblVolume.Name = "m_LblVolume";
            m_LblVolume.Size = new System.Drawing.Size(27, 15);
            m_LblVolume.TabIndex = 47;
            m_LblVolume.Text = "Size";
            // 
            // m_TxtVolume
            // 
            m_TxtVolume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtVolume.Enabled = false;
            m_TxtVolume.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtVolume.Location = new System.Drawing.Point(336, 627);
            m_TxtVolume.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtVolume.Name = "m_TxtVolume";
            m_TxtVolume.Size = new System.Drawing.Size(71, 22);
            m_TxtVolume.TabIndex = 48;
            m_TxtVolume.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_TxtDimension
            // 
            m_TxtDimension.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtDimension.Enabled = false;
            m_TxtDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtDimension.Location = new System.Drawing.Point(484, 627);
            m_TxtDimension.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtDimension.Name = "m_TxtDimension";
            m_TxtDimension.Size = new System.Drawing.Size(73, 22);
            m_TxtDimension.TabIndex = 49;
            m_TxtDimension.Text = "---x---";
            m_TxtDimension.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_TxtBitrate
            // 
            m_TxtBitrate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            m_TxtBitrate.Enabled = false;
            m_TxtBitrate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
            m_TxtBitrate.Location = new System.Drawing.Point(563, 627);
            m_TxtBitrate.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_TxtBitrate.Name = "m_TxtBitrate";
            m_TxtBitrate.Size = new System.Drawing.Size(60, 22);
            m_TxtBitrate.TabIndex = 50;
            m_TxtBitrate.Text = " Mbps";
            m_TxtBitrate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_LangImages
            // 
            m_LangImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            m_LangImages.ImageSize = new System.Drawing.Size(20, 11);
            m_LangImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_PicFlag1
            // 
            m_PicFlag1.InitialImage = null;
            m_PicFlag1.Location = new System.Drawing.Point(642, 633);
            m_PicFlag1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_PicFlag1.Name = "m_PicFlag1";
            m_PicFlag1.Size = new System.Drawing.Size(20, 11);
            m_PicFlag1.TabIndex = 51;
            m_PicFlag1.TabStop = false;
            // 
            // m_PicFlag2
            // 
            m_PicFlag2.InitialImage = null;
            m_PicFlag2.Location = new System.Drawing.Point(668, 633);
            m_PicFlag2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_PicFlag2.Name = "m_PicFlag2";
            m_PicFlag2.Size = new System.Drawing.Size(20, 11);
            m_PicFlag2.TabIndex = 52;
            m_PicFlag2.TabStop = false;
            // 
            // m_PicFlag3
            // 
            m_PicFlag3.InitialImage = null;
            m_PicFlag3.Location = new System.Drawing.Point(694, 633);
            m_PicFlag3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_PicFlag3.Name = "m_PicFlag3";
            m_PicFlag3.Size = new System.Drawing.Size(20, 11);
            m_PicFlag3.TabIndex = 53;
            m_PicFlag3.TabStop = false;
            // 
            // m_PicFlag4
            // 
            m_PicFlag4.InitialImage = null;
            m_PicFlag4.Location = new System.Drawing.Point(720, 633);
            m_PicFlag4.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            m_PicFlag4.Name = "m_PicFlag4";
            m_PicFlag4.Size = new System.Drawing.Size(20, 11);
            m_PicFlag4.TabIndex = 54;
            m_PicFlag4.TabStop = false;
            // 
            // m_LblDuration
            // 
            m_LblDuration.AutoSize = true;
            m_LblDuration.Location = new System.Drawing.Point(417, 613);
            m_LblDuration.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblDuration.Name = "m_LblDuration";
            m_LblDuration.Size = new System.Drawing.Size(53, 15);
            m_LblDuration.TabIndex = 55;
            m_LblDuration.Text = "Duration";
            // 
            // m_LblDimensions
            // 
            m_LblDimensions.AutoSize = true;
            m_LblDimensions.Location = new System.Drawing.Point(487, 613);
            m_LblDimensions.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblDimensions.Name = "m_LblDimensions";
            m_LblDimensions.Size = new System.Drawing.Size(69, 15);
            m_LblDimensions.TabIndex = 56;
            m_LblDimensions.Text = "Dimensions";
            // 
            // m_LblBitrate
            // 
            m_LblBitrate.AutoSize = true;
            m_LblBitrate.Location = new System.Drawing.Point(566, 613);
            m_LblBitrate.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblBitrate.Name = "m_LblBitrate";
            m_LblBitrate.Size = new System.Drawing.Size(41, 15);
            m_LblBitrate.TabIndex = 57;
            m_LblBitrate.Text = "Bitrate";
            // 
            // m_LblAudioStreams
            // 
            m_LblAudioStreams.AutoSize = true;
            m_LblAudioStreams.Location = new System.Drawing.Point(639, 613);
            m_LblAudioStreams.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            m_LblAudioStreams.Name = "m_LblAudioStreams";
            m_LblAudioStreams.Size = new System.Drawing.Size(83, 15);
            m_LblAudioStreams.TabIndex = 58;
            m_LblAudioStreams.Text = "Audio streams";
            // 
            // DetailsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1025, 657);
            Controls.Add(m_LblAudioStreams);
            Controls.Add(m_LblBitrate);
            Controls.Add(m_LblDimensions);
            Controls.Add(m_LblDuration);
            Controls.Add(m_PicFlag4);
            Controls.Add(m_PicFlag3);
            Controls.Add(m_PicFlag2);
            Controls.Add(m_PicFlag1);
            Controls.Add(m_TxtBitrate);
            Controls.Add(m_TxtDimension);
            Controls.Add(m_TxtVolume);
            Controls.Add(m_LblVolume);
            Controls.Add(m_LblVersion);
            Controls.Add(m_TxtVersion);
            Controls.Add(m_VR);
            Controls.Add(m_CastPaste);
            Controls.Add(m_DirectorPaste);
            Controls.Add(m_DescriptionPaste);
            Controls.Add(m_GenrePaste);
            Controls.Add(m_AddGenreBtn);
            Controls.Add(m_LblGenre);
            Controls.Add(m_GenresList);
            Controls.Add(m_LblDescr);
            Controls.Add(m_LblPath);
            Controls.Add(m_LblCast);
            Controls.Add(m_CastList);
            Controls.Add(m_LblDirector);
            Controls.Add(m_DirectorsList);
            Controls.Add(m_LblYear);
            Controls.Add(m_TxtYear);
            Controls.Add(m_LblTitleOrig);
            Controls.Add(m_LblTitle);
            Controls.Add(m_TxtTitle);
            Controls.Add(m_TxtTitleOrig);
            Controls.Add(m_WantToSee);
            Controls.Add(m_TxtDescription);
            Controls.Add(m_TxtLength);
            Controls.Add(m_BtnInsert);
            Controls.Add(m_PicPoster);
            Controls.Add(m_TxtPath);
            Controls.Add(m_Preview1);
            Controls.Add(m_PreviewFull);
            Controls.Add(m_Preview2);
            Controls.Add(m_Preview3);
            Controls.Add(m_Preview4);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DetailsForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Insert entry";
            Load += OnLoad;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            ((System.ComponentModel.ISupportInitialize)m_Preview4).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_Preview3).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_Preview2).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_Preview1).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_PreviewFull).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_PicPoster).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag1).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag2).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag3).EndInit();
            ((System.ComponentModel.ISupportInitialize)m_PicFlag4).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        protected System.Windows.Forms.TextBox m_TxtTitle;
        protected System.Windows.Forms.TextBox m_TxtPath;
        protected System.Windows.Forms.TextBox m_TxtTitleOrig;
        protected System.Windows.Forms.TextBox m_TxtYear;
        protected System.Windows.Forms.PictureBox m_PicPoster;
        protected System.Windows.Forms.Button m_BtnInsert;
        protected System.Windows.Forms.TextBox m_TxtLength;
        protected System.Windows.Forms.TextBox m_TxtDescription;
        protected System.Windows.Forms.CheckBox m_WantToSee;
        protected System.Windows.Forms.ImageList m_DirectorsPhotos;
        protected System.Windows.Forms.Label m_LblGenre;
        protected System.Windows.Forms.Label m_LblTitle;
        protected System.Windows.Forms.Label m_LblTitleOrig;
        protected System.Windows.Forms.Label m_LblYear;
        protected System.Windows.Forms.ListView m_DirectorsList;
        protected System.Windows.Forms.Label m_LblDirector;
        protected System.Windows.Forms.ListView m_CastList;
        protected System.Windows.Forms.ImageList m_CastPhotos;
        protected System.Windows.Forms.ListView m_GenresList;
        protected System.Windows.Forms.ImageList m_GenresImages;
        protected System.Windows.Forms.Label m_AddGenreBtn;
        protected System.Windows.Forms.Label m_GenrePaste;
        protected System.Windows.Forms.Label m_DescriptionPaste;
        protected System.Windows.Forms.Label m_DirectorPaste;
        protected System.Windows.Forms.Label m_CastPaste;
        protected System.Windows.Forms.Label m_LblPath;
        protected System.Windows.Forms.PictureBox m_PreviewFull;
        protected System.Windows.Forms.PictureBox m_Preview1;
        protected System.Windows.Forms.PictureBox m_Preview2;
        protected System.Windows.Forms.PictureBox m_Preview3;
        protected System.Windows.Forms.PictureBox m_Preview4;
        protected System.Windows.Forms.Label m_LblCast;
        protected System.Windows.Forms.Label m_LblDescr;
        protected System.Windows.Forms.CheckBox m_VR;
        protected System.Windows.Forms.Label m_LblVersion;
        protected System.Windows.Forms.TextBox m_TxtVersion;
        protected System.Windows.Forms.Label m_LblVolume;
        protected System.Windows.Forms.TextBox m_TxtVolume;
        protected System.Windows.Forms.TextBox m_TxtDimension;
        protected System.Windows.Forms.TextBox m_TxtBitrate;
        protected System.Windows.Forms.ImageList m_LangImages;
        protected System.Windows.Forms.PictureBox m_PicFlag1;
        protected System.Windows.Forms.PictureBox m_PicFlag2;
        protected System.Windows.Forms.PictureBox m_PicFlag3;
        protected System.Windows.Forms.PictureBox m_PicFlag4;
        protected System.Windows.Forms.Label m_LblDuration;
        protected System.Windows.Forms.Label m_LblDimensions;
        protected System.Windows.Forms.Label m_LblBitrate;
        protected System.Windows.Forms.Label m_LblAudioStreams;
    }
}