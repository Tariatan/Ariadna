
namespace Ariadna
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
                NO_PREVIEW_IMAGE_SMALL.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailsForm));
            this.m_TxtTitle = new System.Windows.Forms.TextBox();
            this.m_TxtPath = new System.Windows.Forms.TextBox();
            this.m_TxtTitleOrig = new System.Windows.Forms.TextBox();
            this.m_TxtYear = new System.Windows.Forms.TextBox();
            this.m_BtnInsert = new System.Windows.Forms.Button();
            this.m_TxtLength = new System.Windows.Forms.TextBox();
            this.m_TxtDescription = new System.Windows.Forms.TextBox();
            this.m_WanToSee = new System.Windows.Forms.CheckBox();
            this.m_DirectorsPhotos = new System.Windows.Forms.ImageList(this.components);
            this.m_LblGenre = new System.Windows.Forms.Label();
            this.m_LblTitle = new System.Windows.Forms.Label();
            this.m_LblTitleOrig = new System.Windows.Forms.Label();
            this.m_LblYear = new System.Windows.Forms.Label();
            this.m_DirectorsList = new System.Windows.Forms.ListView();
            this.m_LblDirector = new System.Windows.Forms.Label();
            this.m_CastList = new System.Windows.Forms.ListView();
            this.m_CastPhotos = new System.Windows.Forms.ImageList(this.components);
            this.m_GenresList = new System.Windows.Forms.ListView();
            this.m_GenresImages = new System.Windows.Forms.ImageList(this.components);
            this.m_AddGenreBtn = new System.Windows.Forms.Label();
            this.m_GenrePaste = new System.Windows.Forms.Label();
            this.m_DescriptionPaste = new System.Windows.Forms.Label();
            this.m_DirectorPaste = new System.Windows.Forms.Label();
            this.m_CastPaste = new System.Windows.Forms.Label();
            this.m_LblPath = new System.Windows.Forms.Label();
            this.m_LblCast = new System.Windows.Forms.Label();
            this.m_LblDescr = new System.Windows.Forms.Label();
            this.m_Preview4 = new System.Windows.Forms.PictureBox();
            this.m_Preview3 = new System.Windows.Forms.PictureBox();
            this.m_Preview2 = new System.Windows.Forms.PictureBox();
            this.m_Preview1 = new System.Windows.Forms.PictureBox();
            this.m_PreviewFull = new System.Windows.Forms.PictureBox();
            this.m_PicPoster = new System.Windows.Forms.PictureBox();
            this.m_VR = new System.Windows.Forms.CheckBox();
            this.m_LblVersion = new System.Windows.Forms.Label();
            this.m_TxtVersion = new System.Windows.Forms.TextBox();
            this.m_LblVolume = new System.Windows.Forms.Label();
            this.m_TxtVolume = new System.Windows.Forms.TextBox();
            this.m_TxtDimension = new System.Windows.Forms.TextBox();
            this.m_TxtBitrate = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_PreviewFull)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_PicPoster)).BeginInit();
            this.SuspendLayout();
            // 
            // m_TxtTitle
            // 
            this.m_TxtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtTitle.Location = new System.Drawing.Point(416, 22);
            this.m_TxtTitle.Name = "m_TxtTitle";
            this.m_TxtTitle.Size = new System.Drawing.Size(603, 29);
            this.m_TxtTitle.TabIndex = 1;
            this.m_TxtTitle.Enter += new System.EventHandler(this.HideFloatingPanel);
            // 
            // m_TxtPath
            // 
            this.m_TxtPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtPath.Location = new System.Drawing.Point(7, 627);
            this.m_TxtPath.Name = "m_TxtPath";
            this.m_TxtPath.Size = new System.Drawing.Size(323, 22);
            this.m_TxtPath.TabIndex = 3;
            this.m_TxtPath.TextChanged += new System.EventHandler(this.OnFilePathChanged);
            // 
            // m_TxtTitleOrig
            // 
            this.m_TxtTitleOrig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtTitleOrig.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtTitleOrig.Location = new System.Drawing.Point(416, 65);
            this.m_TxtTitleOrig.Name = "m_TxtTitleOrig";
            this.m_TxtTitleOrig.Size = new System.Drawing.Size(603, 29);
            this.m_TxtTitleOrig.TabIndex = 2;
            this.m_TxtTitleOrig.Enter += new System.EventHandler(this.HideFloatingPanel);
            // 
            // m_TxtYear
            // 
            this.m_TxtYear.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtYear.Location = new System.Drawing.Point(416, 108);
            this.m_TxtYear.Name = "m_TxtYear";
            this.m_TxtYear.Size = new System.Drawing.Size(77, 26);
            this.m_TxtYear.TabIndex = 3;
            this.m_TxtYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // m_BtnInsert
            // 
            this.m_BtnInsert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_BtnInsert.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_BtnInsert.Location = new System.Drawing.Point(925, 619);
            this.m_BtnInsert.Name = "m_BtnInsert";
            this.m_BtnInsert.Size = new System.Drawing.Size(94, 29);
            this.m_BtnInsert.TabIndex = 9;
            this.m_BtnInsert.Text = "Вставить";
            this.m_BtnInsert.UseVisualStyleBackColor = false;
            this.m_BtnInsert.Click += new System.EventHandler(this.OnBtnInsert_Click);
            // 
            // m_TxtLength
            // 
            this.m_TxtLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtLength.Enabled = false;
            this.m_TxtLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtLength.Location = new System.Drawing.Point(416, 627);
            this.m_TxtLength.Name = "m_TxtLength";
            this.m_TxtLength.Size = new System.Drawing.Size(60, 22);
            this.m_TxtLength.TabIndex = 19;
            this.m_TxtLength.Text = "00:00:00";
            // 
            // m_TxtDescription
            // 
            this.m_TxtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtDescription.Location = new System.Drawing.Point(416, 197);
            this.m_TxtDescription.Multiline = true;
            this.m_TxtDescription.Name = "m_TxtDescription";
            this.m_TxtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m_TxtDescription.Size = new System.Drawing.Size(603, 154);
            this.m_TxtDescription.TabIndex = 5;
            this.m_TxtDescription.Enter += new System.EventHandler(this.HideFloatingPanel);
            this.m_TxtDescription.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnDescriptionKeyUp);
            // 
            // m_WanToSee
            // 
            this.m_WanToSee.AutoSize = true;
            this.m_WanToSee.BackColor = System.Drawing.Color.Transparent;
            this.m_WanToSee.Location = new System.Drawing.Point(843, 631);
            this.m_WanToSee.Name = "m_WanToSee";
            this.m_WanToSee.Size = new System.Drawing.Size(76, 17);
            this.m_WanToSee.TabIndex = 8;
            this.m_WanToSee.Text = "В хотелки";
            this.m_WanToSee.UseVisualStyleBackColor = false;
            // 
            // m_DirectorsPhotos
            // 
            this.m_DirectorsPhotos.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.m_DirectorsPhotos.ImageSize = new System.Drawing.Size(64, 96);
            this.m_DirectorsPhotos.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // m_LblGenre
            // 
            this.m_LblGenre.AutoSize = true;
            this.m_LblGenre.BackColor = System.Drawing.Color.Transparent;
            this.m_LblGenre.Location = new System.Drawing.Point(504, 96);
            this.m_LblGenre.Name = "m_LblGenre";
            this.m_LblGenre.Size = new System.Drawing.Size(36, 13);
            this.m_LblGenre.TabIndex = 29;
            this.m_LblGenre.Text = "Жанр";
            // 
            // m_LblTitle
            // 
            this.m_LblTitle.AutoSize = true;
            this.m_LblTitle.BackColor = System.Drawing.Color.Transparent;
            this.m_LblTitle.Location = new System.Drawing.Point(419, 10);
            this.m_LblTitle.Name = "m_LblTitle";
            this.m_LblTitle.Size = new System.Drawing.Size(57, 13);
            this.m_LblTitle.TabIndex = 30;
            this.m_LblTitle.Text = "Название";
            // 
            // m_LblTitleOrig
            // 
            this.m_LblTitleOrig.AutoSize = true;
            this.m_LblTitleOrig.BackColor = System.Drawing.Color.Transparent;
            this.m_LblTitleOrig.Location = new System.Drawing.Point(419, 53);
            this.m_LblTitleOrig.Name = "m_LblTitleOrig";
            this.m_LblTitleOrig.Size = new System.Drawing.Size(131, 13);
            this.m_LblTitleOrig.TabIndex = 31;
            this.m_LblTitleOrig.Text = "Оригинальное название";
            // 
            // m_LblYear
            // 
            this.m_LblYear.AutoSize = true;
            this.m_LblYear.BackColor = System.Drawing.Color.Transparent;
            this.m_LblYear.Location = new System.Drawing.Point(419, 96);
            this.m_LblYear.Name = "m_LblYear";
            this.m_LblYear.Size = new System.Drawing.Size(71, 13);
            this.m_LblYear.TabIndex = 32;
            this.m_LblYear.Text = "Год выпуска";
            // 
            // m_DirectorsList
            // 
            this.m_DirectorsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_DirectorsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_DirectorsList.HideSelection = false;
            this.m_DirectorsList.LabelEdit = true;
            this.m_DirectorsList.LargeImageList = this.m_DirectorsPhotos;
            this.m_DirectorsList.Location = new System.Drawing.Point(416, 365);
            this.m_DirectorsList.MultiSelect = false;
            this.m_DirectorsList.Name = "m_DirectorsList";
            this.m_DirectorsList.Size = new System.Drawing.Size(130, 246);
            this.m_DirectorsList.TabIndex = 6;
            this.m_DirectorsList.UseCompatibleStateImageBehavior = false;
            this.m_DirectorsList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnDirectorRenamed);
            this.m_DirectorsList.Enter += new System.EventHandler(this.HideFloatingPanel);
            this.m_DirectorsList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnDirectorsKeyUp);
            this.m_DirectorsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnDirectorsDoubleClicked);
            // 
            // m_LblDirector
            // 
            this.m_LblDirector.AutoSize = true;
            this.m_LblDirector.BackColor = System.Drawing.Color.Transparent;
            this.m_LblDirector.Location = new System.Drawing.Point(420, 353);
            this.m_LblDirector.Name = "m_LblDirector";
            this.m_LblDirector.Size = new System.Drawing.Size(52, 13);
            this.m_LblDirector.TabIndex = 33;
            this.m_LblDirector.Text = "Режисер";
            // 
            // m_CastList
            // 
            this.m_CastList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_CastList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_CastList.HideSelection = false;
            this.m_CastList.LabelEdit = true;
            this.m_CastList.LargeImageList = this.m_CastPhotos;
            this.m_CastList.Location = new System.Drawing.Point(553, 365);
            this.m_CastList.MultiSelect = false;
            this.m_CastList.Name = "m_CastList";
            this.m_CastList.Size = new System.Drawing.Size(466, 246);
            this.m_CastList.TabIndex = 7;
            this.m_CastList.UseCompatibleStateImageBehavior = false;
            this.m_CastList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnActorRenamed);
            this.m_CastList.Enter += new System.EventHandler(this.HideFloatingPanel);
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
            this.m_GenresList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_GenresList.HideSelection = false;
            this.m_GenresList.LargeImageList = this.m_GenresImages;
            this.m_GenresList.Location = new System.Drawing.Point(499, 108);
            this.m_GenresList.MultiSelect = false;
            this.m_GenresList.Name = "m_GenresList";
            this.m_GenresList.Size = new System.Drawing.Size(520, 83);
            this.m_GenresList.TabIndex = 4;
            this.m_GenresList.UseCompatibleStateImageBehavior = false;
            this.m_GenresList.Enter += new System.EventHandler(this.HideFloatingPanel);
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
            this.m_AddGenreBtn.Location = new System.Drawing.Point(998, 174);
            this.m_AddGenreBtn.Name = "m_AddGenreBtn";
            this.m_AddGenreBtn.Size = new System.Drawing.Size(16, 13);
            this.m_AddGenreBtn.TabIndex = 34;
            this.m_AddGenreBtn.Text = "...";
            this.m_AddGenreBtn.Click += new System.EventHandler(this.OnAddGenreClicked);
            // 
            // m_GenrePaste
            // 
            this.m_GenrePaste.AutoSize = true;
            this.m_GenrePaste.CausesValidation = false;
            this.m_GenrePaste.Location = new System.Drawing.Point(540, 96);
            this.m_GenrePaste.Name = "m_GenrePaste";
            this.m_GenrePaste.Size = new System.Drawing.Size(13, 13);
            this.m_GenrePaste.TabIndex = 35;
            this.m_GenrePaste.Text = "↓";
            this.m_GenrePaste.UseMnemonic = false;
            this.m_GenrePaste.Click += new System.EventHandler(this.OnGenrePasteClick);
            // 
            // m_DescriptionPaste
            // 
            this.m_DescriptionPaste.AutoSize = true;
            this.m_DescriptionPaste.CausesValidation = false;
            this.m_DescriptionPaste.Location = new System.Drawing.Point(476, 185);
            this.m_DescriptionPaste.Name = "m_DescriptionPaste";
            this.m_DescriptionPaste.Size = new System.Drawing.Size(13, 13);
            this.m_DescriptionPaste.TabIndex = 36;
            this.m_DescriptionPaste.Text = "↓";
            this.m_DescriptionPaste.UseMnemonic = false;
            this.m_DescriptionPaste.Click += new System.EventHandler(this.OnDescriptionPasteClick);
            // 
            // m_DirectorPaste
            // 
            this.m_DirectorPaste.AutoSize = true;
            this.m_DirectorPaste.CausesValidation = false;
            this.m_DirectorPaste.Location = new System.Drawing.Point(472, 353);
            this.m_DirectorPaste.Name = "m_DirectorPaste";
            this.m_DirectorPaste.Size = new System.Drawing.Size(13, 13);
            this.m_DirectorPaste.TabIndex = 37;
            this.m_DirectorPaste.Text = "↓";
            this.m_DirectorPaste.UseMnemonic = false;
            this.m_DirectorPaste.Click += new System.EventHandler(this.OnDirectorPasteClick);
            // 
            // m_CastPaste
            // 
            this.m_CastPaste.AutoSize = true;
            this.m_CastPaste.CausesValidation = false;
            this.m_CastPaste.Location = new System.Drawing.Point(601, 353);
            this.m_CastPaste.Name = "m_CastPaste";
            this.m_CastPaste.Size = new System.Drawing.Size(13, 13);
            this.m_CastPaste.TabIndex = 38;
            this.m_CastPaste.Text = "↓";
            this.m_CastPaste.UseMnemonic = false;
            this.m_CastPaste.Click += new System.EventHandler(this.OnCastPasteClick);
            // 
            // m_LblPath
            // 
            this.m_LblPath.AutoSize = true;
            this.m_LblPath.BackColor = System.Drawing.Color.Transparent;
            this.m_LblPath.Location = new System.Drawing.Point(10, 613);
            this.m_LblPath.Name = "m_LblPath";
            this.m_LblPath.Size = new System.Drawing.Size(31, 13);
            this.m_LblPath.TabIndex = 2;
            this.m_LblPath.Text = "Путь";
            // 
            // m_LblCast
            // 
            this.m_LblCast.AutoSize = true;
            this.m_LblCast.BackColor = System.Drawing.Color.Transparent;
            this.m_LblCast.Location = new System.Drawing.Point(556, 353);
            this.m_LblCast.Name = "m_LblCast";
            this.m_LblCast.Size = new System.Drawing.Size(45, 13);
            this.m_LblCast.TabIndex = 10;
            this.m_LblCast.Text = "Актеры";
            // 
            // m_LblDescr
            // 
            this.m_LblDescr.AutoSize = true;
            this.m_LblDescr.BackColor = System.Drawing.Color.Transparent;
            this.m_LblDescr.Location = new System.Drawing.Point(419, 185);
            this.m_LblDescr.Name = "m_LblDescr";
            this.m_LblDescr.Size = new System.Drawing.Size(57, 13);
            this.m_LblDescr.TabIndex = 20;
            this.m_LblDescr.Text = "Описание";
            // 
            // m_Preview4
            // 
            this.m_Preview4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.m_Preview4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_Preview4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.m_Preview4.ErrorImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview4.Image = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview4.InitialImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview4.Location = new System.Drawing.Point(882, 534);
            this.m_Preview4.Name = "m_Preview4";
            this.m_Preview4.Size = new System.Drawing.Size(137, 77);
            this.m_Preview4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_Preview4.TabIndex = 43;
            this.m_Preview4.TabStop = false;
            this.m_Preview4.Visible = false;
            this.m_Preview4.Click += new System.EventHandler(this.OnClick);
            this.m_Preview4.DoubleClick += new System.EventHandler(this.OnPreview_DoubleClick);
            // 
            // m_Preview3
            // 
            this.m_Preview3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_Preview3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.m_Preview3.ErrorImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview3.Image = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview3.InitialImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview3.Location = new System.Drawing.Point(726, 534);
            this.m_Preview3.Name = "m_Preview3";
            this.m_Preview3.Size = new System.Drawing.Size(137, 77);
            this.m_Preview3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_Preview3.TabIndex = 42;
            this.m_Preview3.TabStop = false;
            this.m_Preview3.Visible = false;
            this.m_Preview3.Click += new System.EventHandler(this.OnClick);
            this.m_Preview3.DoubleClick += new System.EventHandler(this.OnPreview_DoubleClick);
            // 
            // m_Preview2
            // 
            this.m_Preview2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_Preview2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.m_Preview2.ErrorImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview2.Image = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview2.InitialImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview2.Location = new System.Drawing.Point(571, 534);
            this.m_Preview2.Name = "m_Preview2";
            this.m_Preview2.Size = new System.Drawing.Size(137, 77);
            this.m_Preview2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_Preview2.TabIndex = 41;
            this.m_Preview2.TabStop = false;
            this.m_Preview2.Visible = false;
            this.m_Preview2.Click += new System.EventHandler(this.OnClick);
            this.m_Preview2.DoubleClick += new System.EventHandler(this.OnPreview_DoubleClick);
            // 
            // m_Preview1
            // 
            this.m_Preview1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_Preview1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.m_Preview1.ErrorImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview1.Image = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview1.InitialImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide_small;
            this.m_Preview1.Location = new System.Drawing.Point(416, 534);
            this.m_Preview1.Name = "m_Preview1";
            this.m_Preview1.Size = new System.Drawing.Size(137, 77);
            this.m_Preview1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_Preview1.TabIndex = 40;
            this.m_Preview1.TabStop = false;
            this.m_Preview1.Visible = false;
            this.m_Preview1.Click += new System.EventHandler(this.OnClick);
            this.m_Preview1.DoubleClick += new System.EventHandler(this.OnPreview_DoubleClick);
            // 
            // m_PreviewFull
            // 
            this.m_PreviewFull.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_PreviewFull.Enabled = false;
            this.m_PreviewFull.ErrorImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide;
            this.m_PreviewFull.Image = global::Ariadna.Properties.Resources.No_Preview_Image_Wide;
            this.m_PreviewFull.InitialImage = global::Ariadna.Properties.Resources.No_Preview_Image_Wide;
            this.m_PreviewFull.Location = new System.Drawing.Point(416, 194);
            this.m_PreviewFull.Name = "m_PreviewFull";
            this.m_PreviewFull.Size = new System.Drawing.Size(603, 339);
            this.m_PreviewFull.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_PreviewFull.TabIndex = 39;
            this.m_PreviewFull.TabStop = false;
            this.m_PreviewFull.Visible = false;
            // 
            // m_PicPoster
            // 
            this.m_PicPoster.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_PicPoster.ErrorImage = ((System.Drawing.Image)(resources.GetObject("m_PicPoster.ErrorImage")));
            this.m_PicPoster.Image = global::Ariadna.Properties.Resources.No_Preview_Image;
            this.m_PicPoster.InitialImage = ((System.Drawing.Image)(resources.GetObject("m_PicPoster.InitialImage")));
            this.m_PicPoster.Location = new System.Drawing.Point(7, 10);
            this.m_PicPoster.Name = "m_PicPoster";
            this.m_PicPoster.Size = new System.Drawing.Size(400, 600);
            this.m_PicPoster.TabIndex = 14;
            this.m_PicPoster.TabStop = false;
            this.m_PicPoster.DoubleClick += new System.EventHandler(this.OnPicPoster_DoubleClick);
            // 
            // m_VR
            // 
            this.m_VR.AutoSize = true;
            this.m_VR.BackColor = System.Drawing.Color.Transparent;
            this.m_VR.Location = new System.Drawing.Point(793, 631);
            this.m_VR.Name = "m_VR";
            this.m_VR.Size = new System.Drawing.Size(41, 17);
            this.m_VR.TabIndex = 44;
            this.m_VR.Text = "VR";
            this.m_VR.UseVisualStyleBackColor = false;
            this.m_VR.Visible = false;
            // 
            // m_LblVersion
            // 
            this.m_LblVersion.AutoSize = true;
            this.m_LblVersion.BackColor = System.Drawing.Color.Transparent;
            this.m_LblVersion.Location = new System.Drawing.Point(419, 136);
            this.m_LblVersion.Name = "m_LblVersion";
            this.m_LblVersion.Size = new System.Drawing.Size(44, 13);
            this.m_LblVersion.TabIndex = 45;
            this.m_LblVersion.Text = "Версия";
            this.m_LblVersion.Visible = false;
            // 
            // m_TxtVersion
            // 
            this.m_TxtVersion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtVersion.Location = new System.Drawing.Point(417, 148);
            this.m_TxtVersion.Name = "m_TxtVersion";
            this.m_TxtVersion.Size = new System.Drawing.Size(77, 22);
            this.m_TxtVersion.TabIndex = 46;
            this.m_TxtVersion.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.m_TxtVersion.Visible = false;
            // 
            // m_LblVolume
            // 
            this.m_LblVolume.AutoSize = true;
            this.m_LblVolume.Location = new System.Drawing.Point(336, 613);
            this.m_LblVolume.Name = "m_LblVolume";
            this.m_LblVolume.Size = new System.Drawing.Size(46, 13);
            this.m_LblVolume.TabIndex = 47;
            this.m_LblVolume.Text = "Размер";
            // 
            // m_TxtVolume
            // 
            this.m_TxtVolume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtVolume.Enabled = false;
            this.m_TxtVolume.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtVolume.Location = new System.Drawing.Point(336, 627);
            this.m_TxtVolume.Name = "m_TxtVolume";
            this.m_TxtVolume.Size = new System.Drawing.Size(71, 22);
            this.m_TxtVolume.TabIndex = 48;
            // 
            // m_TxtDimension
            // 
            this.m_TxtDimension.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtDimension.Enabled = false;
            this.m_TxtDimension.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtDimension.Location = new System.Drawing.Point(482, 627);
            this.m_TxtDimension.Name = "m_TxtDimension";
            this.m_TxtDimension.Size = new System.Drawing.Size(60, 22);
            this.m_TxtDimension.TabIndex = 49;
            this.m_TxtDimension.Text = "---x---";
            // 
            // m_TxtBitrate
            // 
            this.m_TxtBitrate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_TxtBitrate.Enabled = false;
            this.m_TxtBitrate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_TxtBitrate.Location = new System.Drawing.Point(548, 627);
            this.m_TxtBitrate.Name = "m_TxtBitrate";
            this.m_TxtBitrate.Size = new System.Drawing.Size(60, 22);
            this.m_TxtBitrate.TabIndex = 50;
            this.m_TxtBitrate.Text = " Mbps";
            // 
            // DetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1027, 656);
            this.Controls.Add(this.m_TxtBitrate);
            this.Controls.Add(this.m_TxtDimension);
            this.Controls.Add(this.m_TxtVolume);
            this.Controls.Add(this.m_LblVolume);
            this.Controls.Add(this.m_LblVersion);
            this.Controls.Add(this.m_TxtVersion);
            this.Controls.Add(this.m_VR);
            this.Controls.Add(this.m_CastPaste);
            this.Controls.Add(this.m_DirectorPaste);
            this.Controls.Add(this.m_DescriptionPaste);
            this.Controls.Add(this.m_GenrePaste);
            this.Controls.Add(this.m_AddGenreBtn);
            this.Controls.Add(this.m_LblGenre);
            this.Controls.Add(this.m_GenresList);
            this.Controls.Add(this.m_LblDescr);
            this.Controls.Add(this.m_LblPath);
            this.Controls.Add(this.m_LblCast);
            this.Controls.Add(this.m_CastList);
            this.Controls.Add(this.m_LblDirector);
            this.Controls.Add(this.m_DirectorsList);
            this.Controls.Add(this.m_LblYear);
            this.Controls.Add(this.m_TxtYear);
            this.Controls.Add(this.m_LblTitleOrig);
            this.Controls.Add(this.m_LblTitle);
            this.Controls.Add(this.m_TxtTitle);
            this.Controls.Add(this.m_TxtTitleOrig);
            this.Controls.Add(this.m_WanToSee);
            this.Controls.Add(this.m_TxtDescription);
            this.Controls.Add(this.m_TxtLength);
            this.Controls.Add(this.m_BtnInsert);
            this.Controls.Add(this.m_PicPoster);
            this.Controls.Add(this.m_TxtPath);
            this.Controls.Add(this.m_Preview1);
            this.Controls.Add(this.m_PreviewFull);
            this.Controls.Add(this.m_Preview2);
            this.Controls.Add(this.m_Preview3);
            this.Controls.Add(this.m_Preview4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DetailsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавление записи";
            this.Load += new System.EventHandler(this.OnLoad);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Preview1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_PreviewFull)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_PicPoster)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        protected System.Windows.Forms.CheckBox m_WanToSee;
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
    }
}