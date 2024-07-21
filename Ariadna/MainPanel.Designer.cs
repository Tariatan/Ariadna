using Ariadna.Themes;

namespace Ariadna
{
    partial class MainPanel
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
                m_ListViewRenderer.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainPanel));
            m_ImageListView = new Manina.Windows.Forms.ImageListView();
            m_ToolStrip = new System.Windows.Forms.ToolStrip();
            m_ToolStrip_AddBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_NameLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_EntryName = new System.Windows.Forms.ToolStripTextBox();
            m_ToolStrip_ClearTitleBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_WishlistLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_WishlistBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_RecentLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_RecentBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_NewLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_NewBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStrip_VRSprtr = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_VRLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_VRBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStrip_nonVRSprtr = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_nonVRLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_nonVRBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStrip_SeriesSprtr = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_SeriesLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_SeriesBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStrip_MoviesSprtr = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_MoviesLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_MoviesBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_DirectorLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_DirectorName = new System.Windows.Forms.ToolStripTextBox();
            m_ToolStrip_ClearDirectorBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStrip_DirectorSprt = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_ActorLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_ActorName = new System.Windows.Forms.ToolStripTextBox();
            m_ToolStrip_ClearActorBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStrip_ActorSprt = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_GenreNameLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_GenreName = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_ClearGenreBtn = new System.Windows.Forms.ToolStripButton();
            m_ToolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            m_ToolStrip_EntriesCountLbl = new System.Windows.Forms.ToolStripLabel();
            m_ToolStrip_EntriesCount = new System.Windows.Forms.ToolStripLabel();
            m_QuickListFlow = new System.Windows.Forms.FlowLayoutPanel();
            m_ToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // m_ImageListView
            // 
            m_ImageListView.AllowItemReorder = false;
            m_ImageListView.DefaultImage = Properties.Resources.empty_icon;
            m_ImageListView.Dock = System.Windows.Forms.DockStyle.Fill;
            m_ImageListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            m_ImageListView.Location = new System.Drawing.Point(0, 25);
            m_ImageListView.MultiSelect = false;
            m_ImageListView.Name = "m_ImageListView";
            m_ImageListView.PersistentCacheDirectory = "";
            m_ImageListView.PersistentCacheSize = 100L;
            m_ImageListView.Size = new System.Drawing.Size(1583, 632);
            m_ImageListView.TabIndex = 1;
            m_ImageListView.ThumbnailSize = new System.Drawing.Size(214, 321);
            m_ImageListView.UseWIC = true;
            m_ImageListView.SelectionChanged += ListView_ItemSelectionChanged;
            m_ImageListView.Click += OnListViewClick;
            m_ImageListView.KeyDown += OnListViewKeyDown;
            m_ImageListView.MouseDoubleClick += ListView_MouseDoubleClick;
            m_ImageListView.MouseUp += ListView_MouseClicked;
            // 
            // m_ToolStrip
            // 
            m_ToolStrip.CanOverflow = false;
            m_ToolStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 2, 5);
            m_ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            m_ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { m_ToolStrip_AddBtn, m_ToolStripSeparator1, m_ToolStrip_NameLbl, m_ToolStrip_EntryName, m_ToolStrip_ClearTitleBtn, m_ToolStripSeparator3, m_ToolStrip_WishlistLbl, m_ToolStrip_WishlistBtn, m_ToolStripSeparator5, m_ToolStrip_RecentLbl, m_ToolStrip_RecentBtn, m_ToolStripSeparator7, m_ToolStrip_NewLbl, m_ToolStrip_NewBtn, m_ToolStrip_VRSprtr, m_ToolStrip_VRLbl, m_ToolStrip_VRBtn, m_ToolStrip_nonVRSprtr, m_ToolStrip_nonVRLbl, m_ToolStrip_nonVRBtn, m_ToolStrip_SeriesSprtr, m_ToolStrip_SeriesLbl, m_ToolStrip_SeriesBtn, m_ToolStrip_MoviesSprtr, m_ToolStrip_MoviesLbl, m_ToolStrip_MoviesBtn, m_ToolStripSeparator4, m_ToolStrip_DirectorLbl, m_ToolStrip_DirectorName, m_ToolStrip_ClearDirectorBtn, m_ToolStrip_DirectorSprt, m_ToolStrip_ActorLbl, m_ToolStrip_ActorName, m_ToolStrip_ClearActorBtn, m_ToolStrip_ActorSprt, m_ToolStrip_GenreNameLbl, m_ToolStrip_GenreName, m_ToolStrip_ClearGenreBtn, m_ToolStripSeparator8, m_ToolStrip_EntriesCountLbl, m_ToolStrip_EntriesCount });
            m_ToolStrip.Location = new System.Drawing.Point(0, 0);
            m_ToolStrip.Name = "m_ToolStrip";
            m_ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            m_ToolStrip.Size = new System.Drawing.Size(1583, 25);
            m_ToolStrip.Stretch = true;
            m_ToolStrip.TabIndex = 2;
            // 
            // m_ToolStrip_AddBtn
            // 
            m_ToolStrip_AddBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_AddBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_AddBtn.Image");
            m_ToolStrip_AddBtn.Name = "m_ToolStrip_AddBtn";
            m_ToolStrip_AddBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_AddBtn.Text = "Add new...";
            m_ToolStrip_AddBtn.MouseUp += ToolStrip_AddBtn_MouseUp;
            // 
            // m_ToolStripSeparator1
            // 
            m_ToolStripSeparator1.Name = "m_ToolStripSeparator1";
            m_ToolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_NameLbl
            // 
            m_ToolStrip_NameLbl.Name = "m_ToolStrip_NameLbl";
            m_ToolStrip_NameLbl.Size = new System.Drawing.Size(29, 22);
            m_ToolStrip_NameLbl.Text = "Title";
            // 
            // m_ToolStrip_EntryName
            // 
            m_ToolStrip_EntryName.AutoSize = false;
            m_ToolStrip_EntryName.MaxLength = 50;
            m_ToolStrip_EntryName.Name = "m_ToolStrip_EntryName";
            m_ToolStrip_EntryName.Size = new System.Drawing.Size(150, 23);
            m_ToolStrip_EntryName.Enter += OnEntryNameTextEntered;
            m_ToolStrip_EntryName.KeyDown += OnEntryNameConfirmed;
            m_ToolStrip_EntryName.TextChanged += OnEntryNameTextChanged;
            // 
            // m_ToolStrip_ClearTitleBtn
            // 
            m_ToolStrip_ClearTitleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_ClearTitleBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_ClearTitleBtn.Image");
            m_ToolStrip_ClearTitleBtn.Name = "m_ToolStrip_ClearTitleBtn";
            m_ToolStrip_ClearTitleBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_ClearTitleBtn.ToolTipText = "Очистить";
            m_ToolStrip_ClearTitleBtn.Click += ToolStrip_ClearTitleBtn_Clicked;
            // 
            // m_ToolStripSeparator3
            // 
            m_ToolStripSeparator3.Name = "m_ToolStripSeparator3";
            m_ToolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_WishlistLbl
            // 
            m_ToolStrip_WishlistLbl.Name = "m_ToolStrip_WishlistLbl";
            m_ToolStrip_WishlistLbl.Size = new System.Drawing.Size(48, 22);
            m_ToolStrip_WishlistLbl.Text = "Wishlist";
            // 
            // m_ToolStrip_WishlistBtn
            // 
            m_ToolStrip_WishlistBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_WishlistBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_WishlistBtn.Image");
            m_ToolStrip_WishlistBtn.Name = "m_ToolStrip_WishlistBtn";
            m_ToolStrip_WishlistBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_WishlistBtn.ToolTipText = "Хотелки";
            m_ToolStrip_WishlistBtn.Click += ToolStrip_CheckboxedFilter_Clicked;
            // 
            // m_ToolStripSeparator5
            // 
            m_ToolStripSeparator5.Name = "m_ToolStripSeparator5";
            m_ToolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_RecentLbl
            // 
            m_ToolStrip_RecentLbl.Name = "m_ToolStrip_RecentLbl";
            m_ToolStrip_RecentLbl.Size = new System.Drawing.Size(43, 22);
            m_ToolStrip_RecentLbl.Text = "Recent";
            // 
            // m_ToolStrip_RecentBtn
            // 
            m_ToolStrip_RecentBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_RecentBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_RecentBtn.Image");
            m_ToolStrip_RecentBtn.Name = "m_ToolStrip_RecentBtn";
            m_ToolStrip_RecentBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_RecentBtn.Text = "Недавние";
            m_ToolStrip_RecentBtn.Click += ToolStrip_CheckboxedFilter_Clicked;
            // 
            // m_ToolStripSeparator7
            // 
            m_ToolStripSeparator7.Name = "m_ToolStripSeparator7";
            m_ToolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_NewLbl
            // 
            m_ToolStrip_NewLbl.Name = "m_ToolStrip_NewLbl";
            m_ToolStrip_NewLbl.Size = new System.Drawing.Size(31, 22);
            m_ToolStrip_NewLbl.Text = "New";
            // 
            // m_ToolStrip_NewBtn
            // 
            m_ToolStrip_NewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_NewBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_NewBtn.Image");
            m_ToolStrip_NewBtn.Name = "m_ToolStrip_NewBtn";
            m_ToolStrip_NewBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_NewBtn.Text = "Новинки";
            m_ToolStrip_NewBtn.Click += ToolStrip_CheckboxedFilter_Clicked;
            // 
            // m_ToolStrip_VRSprtr
            // 
            m_ToolStrip_VRSprtr.Name = "m_ToolStrip_VRSprtr";
            m_ToolStrip_VRSprtr.Size = new System.Drawing.Size(6, 25);
            m_ToolStrip_VRSprtr.Visible = false;
            // 
            // m_ToolStrip_VRLbl
            // 
            m_ToolStrip_VRLbl.Name = "m_ToolStrip_VRLbl";
            m_ToolStrip_VRLbl.Size = new System.Drawing.Size(21, 22);
            m_ToolStrip_VRLbl.Text = "VR";
            m_ToolStrip_VRLbl.Visible = false;
            // 
            // m_ToolStrip_VRBtn
            // 
            m_ToolStrip_VRBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_VRBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_VRBtn.Image");
            m_ToolStrip_VRBtn.Name = "m_ToolStrip_VRBtn";
            m_ToolStrip_VRBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_VRBtn.Text = "VR";
            m_ToolStrip_VRBtn.Visible = false;
            m_ToolStrip_VRBtn.Click += ToolStrip_ToolStrip_VRBtn_Clicked;
            // 
            // m_ToolStrip_nonVRSprtr
            // 
            m_ToolStrip_nonVRSprtr.Name = "m_ToolStrip_nonVRSprtr";
            m_ToolStrip_nonVRSprtr.Size = new System.Drawing.Size(6, 25);
            m_ToolStrip_nonVRSprtr.Visible = false;
            // 
            // m_ToolStrip_nonVRLbl
            // 
            m_ToolStrip_nonVRLbl.Name = "m_ToolStrip_nonVRLbl";
            m_ToolStrip_nonVRLbl.Size = new System.Drawing.Size(47, 22);
            m_ToolStrip_nonVRLbl.Text = "non-VR";
            m_ToolStrip_nonVRLbl.Visible = false;
            // 
            // m_ToolStrip_nonVRBtn
            // 
            m_ToolStrip_nonVRBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_nonVRBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_nonVRBtn.Image");
            m_ToolStrip_nonVRBtn.Name = "m_ToolStrip_nonVRBtn";
            m_ToolStrip_nonVRBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_nonVRBtn.Text = "nonVR";
            m_ToolStrip_nonVRBtn.Visible = false;
            m_ToolStrip_nonVRBtn.Click += ToolStrip_ToolStrip_NonVRBtn_Clicked;
            // 
            // m_ToolStrip_SeriesSprtr
            // 
            m_ToolStrip_SeriesSprtr.Name = "m_ToolStrip_SeriesSprtr";
            m_ToolStrip_SeriesSprtr.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_SeriesLbl
            // 
            m_ToolStrip_SeriesLbl.Name = "m_ToolStrip_SeriesLbl";
            m_ToolStrip_SeriesLbl.Size = new System.Drawing.Size(37, 22);
            m_ToolStrip_SeriesLbl.Text = "Series";
            // 
            // m_ToolStrip_SeriesBtn
            // 
            m_ToolStrip_SeriesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_SeriesBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_SeriesBtn.Image");
            m_ToolStrip_SeriesBtn.Name = "m_ToolStrip_SeriesBtn";
            m_ToolStrip_SeriesBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_SeriesBtn.Text = "Series";
            m_ToolStrip_SeriesBtn.Click += ToolStrip_ToolStrip_SeriesBtn_Clicked;
            // 
            // m_ToolStrip_MoviesSprtr
            // 
            m_ToolStrip_MoviesSprtr.Name = "m_ToolStrip_MoviesSprtr";
            m_ToolStrip_MoviesSprtr.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_MoviesLbl
            // 
            m_ToolStrip_MoviesLbl.Name = "m_ToolStrip_MoviesLbl";
            m_ToolStrip_MoviesLbl.Size = new System.Drawing.Size(45, 22);
            m_ToolStrip_MoviesLbl.Text = "Movies";
            // 
            // m_ToolStrip_MoviesBtn
            // 
            m_ToolStrip_MoviesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_MoviesBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_MoviesBtn.Image");
            m_ToolStrip_MoviesBtn.Name = "m_ToolStrip_MoviesBtn";
            m_ToolStrip_MoviesBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_MoviesBtn.Text = "Movies";
            m_ToolStrip_MoviesBtn.Click += ToolStrip_ToolStrip_MoviesBtn_Clicked;
            // 
            // m_ToolStripSeparator4
            // 
            m_ToolStripSeparator4.Name = "m_ToolStripSeparator4";
            m_ToolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_DirectorLbl
            // 
            m_ToolStrip_DirectorLbl.Name = "m_ToolStrip_DirectorLbl";
            m_ToolStrip_DirectorLbl.Size = new System.Drawing.Size(49, 22);
            m_ToolStrip_DirectorLbl.Text = "Director";
            // 
            // m_ToolStrip_DirectorName
            // 
            m_ToolStrip_DirectorName.AutoSize = false;
            m_ToolStrip_DirectorName.MaxLength = 20;
            m_ToolStrip_DirectorName.Name = "m_ToolStrip_DirectorName";
            m_ToolStrip_DirectorName.Size = new System.Drawing.Size(100, 25);
            m_ToolStrip_DirectorName.Enter += OnDirectorNameTextChanged;
            m_ToolStrip_DirectorName.TextChanged += OnDirectorNameTextChanged;
            // 
            // m_ToolStrip_ClearDirectorBtn
            // 
            m_ToolStrip_ClearDirectorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_ClearDirectorBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_ClearDirectorBtn.Image");
            m_ToolStrip_ClearDirectorBtn.Name = "m_ToolStrip_ClearDirectorBtn";
            m_ToolStrip_ClearDirectorBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_ClearDirectorBtn.ToolTipText = "Очистить";
            m_ToolStrip_ClearDirectorBtn.Click += ToolStrip_ClearDirectorBtn_Clicked;
            // 
            // m_ToolStrip_DirectorSprt
            // 
            m_ToolStrip_DirectorSprt.Name = "m_ToolStrip_DirectorSprt";
            m_ToolStrip_DirectorSprt.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_ActorLbl
            // 
            m_ToolStrip_ActorLbl.Name = "m_ToolStrip_ActorLbl";
            m_ToolStrip_ActorLbl.Size = new System.Drawing.Size(36, 22);
            m_ToolStrip_ActorLbl.Text = "Actor";
            // 
            // m_ToolStrip_ActorName
            // 
            m_ToolStrip_ActorName.AutoSize = false;
            m_ToolStrip_ActorName.MaxLength = 20;
            m_ToolStrip_ActorName.Name = "m_ToolStrip_ActorName";
            m_ToolStrip_ActorName.Size = new System.Drawing.Size(100, 25);
            m_ToolStrip_ActorName.Enter += OnActorNameTextChanged;
            m_ToolStrip_ActorName.TextChanged += OnActorNameTextChanged;
            // 
            // m_ToolStrip_ClearActorBtn
            // 
            m_ToolStrip_ClearActorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_ClearActorBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_ClearActorBtn.Image");
            m_ToolStrip_ClearActorBtn.Name = "m_ToolStrip_ClearActorBtn";
            m_ToolStrip_ClearActorBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_ClearActorBtn.ToolTipText = "Очистить";
            m_ToolStrip_ClearActorBtn.Click += ToolStrip_ClearActorBtn_Clicked;
            // 
            // m_ToolStrip_ActorSprt
            // 
            m_ToolStrip_ActorSprt.Name = "m_ToolStrip_ActorSprt";
            m_ToolStrip_ActorSprt.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_GenreNameLbl
            // 
            m_ToolStrip_GenreNameLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            m_ToolStrip_GenreNameLbl.Name = "m_ToolStrip_GenreNameLbl";
            m_ToolStrip_GenreNameLbl.Size = new System.Drawing.Size(42, 22);
            m_ToolStrip_GenreNameLbl.Text = "Genre";
            // 
            // m_ToolStrip_GenreName
            // 
            m_ToolStrip_GenreName.Name = "m_ToolStrip_GenreName";
            m_ToolStrip_GenreName.Size = new System.Drawing.Size(22, 22);
            m_ToolStrip_GenreName.Text = ". . .";
            m_ToolStrip_GenreName.Click += ToolStrip_Genre_Clicked;
            // 
            // m_ToolStrip_ClearGenreBtn
            // 
            m_ToolStrip_ClearGenreBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            m_ToolStrip_ClearGenreBtn.Image = (System.Drawing.Image)resources.GetObject("m_ToolStrip_ClearGenreBtn.Image");
            m_ToolStrip_ClearGenreBtn.Name = "m_ToolStrip_ClearGenreBtn";
            m_ToolStrip_ClearGenreBtn.Size = new System.Drawing.Size(23, 22);
            m_ToolStrip_ClearGenreBtn.ToolTipText = "Очистить";
            m_ToolStrip_ClearGenreBtn.Click += ToolStrip_ClearGenreBtn_Clicked;
            // 
            // m_ToolStripSeparator8
            // 
            m_ToolStripSeparator8.Name = "m_ToolStripSeparator8";
            m_ToolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_EntriesCountLbl
            // 
            m_ToolStrip_EntriesCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            m_ToolStrip_EntriesCountLbl.Name = "m_ToolStrip_EntriesCountLbl";
            m_ToolStrip_EntriesCountLbl.Size = new System.Drawing.Size(86, 22);
            m_ToolStrip_EntriesCountLbl.Text = "Found entries:";
            // 
            // m_ToolStrip_EntriesCount
            // 
            m_ToolStrip_EntriesCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            m_ToolStrip_EntriesCount.Name = "m_ToolStrip_EntriesCount";
            m_ToolStrip_EntriesCount.Size = new System.Drawing.Size(14, 22);
            m_ToolStrip_EntriesCount.Text = "0";
            // 
            // m_QuickListFlow
            // 
            m_QuickListFlow.Dock = System.Windows.Forms.DockStyle.Right;
            m_QuickListFlow.Location = new System.Drawing.Point(1583, 0);
            m_QuickListFlow.Name = "m_QuickListFlow";
            m_QuickListFlow.Size = new System.Drawing.Size(93, 657);
            m_QuickListFlow.TabIndex = 3;
            m_QuickListFlow.MouseCaptureChanged += OnMouseCaptureChanged;
            // 
            // MainPanel
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            ClientSize = new System.Drawing.Size(1676, 657);
            Controls.Add(m_ImageListView);
            Controls.Add(m_ToolStrip);
            Controls.Add(m_QuickListFlow);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "MainPanel";
            Text = "Ariadna";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Load += MainPanel_Load;
            Click += OnFormClicked;
            KeyDown += MainPanel_KeyUp;
            KeyPress += MainPanel_KeyPress;
            MouseClick += OnFormClicked;
            Move += OnPanelMoved;
            Resize += OnPanelResized;
            m_ToolStrip.ResumeLayout(false);
            m_ToolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Manina.Windows.Forms.ImageListView m_ImageListView;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_AddBtn;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_RecentBtn;
        private System.Windows.Forms.ToolStripSeparator m_ToolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_RecentLbl;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_NewLbl;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_NewBtn;
        private System.Windows.Forms.ToolStripSeparator m_ToolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_NameLbl;
        private System.Windows.Forms.ToolStripTextBox m_ToolStrip_EntryName;
        private System.Windows.Forms.ToolStripSeparator m_ToolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_WishlistLbl;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_WishlistBtn;
        private System.Windows.Forms.ToolStripSeparator m_ToolStripSeparator5;
        private System.Windows.Forms.FlowLayoutPanel m_QuickListFlow;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_ClearTitleBtn;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_GenreNameLbl;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_ClearGenreBtn;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_GenreName;
        private System.Windows.Forms.ToolStripSeparator m_ToolStripSeparator8;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_EntriesCountLbl;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_EntriesCount;
        public System.Windows.Forms.ToolStripLabel m_ToolStrip_DirectorLbl;
        public System.Windows.Forms.ToolStripTextBox m_ToolStrip_DirectorName;
        public System.Windows.Forms.ToolStripButton m_ToolStrip_ClearDirectorBtn;
        public System.Windows.Forms.ToolStripLabel m_ToolStrip_ActorLbl;
        public System.Windows.Forms.ToolStripTextBox m_ToolStrip_ActorName;
        public System.Windows.Forms.ToolStripButton m_ToolStrip_ClearActorBtn;
        public System.Windows.Forms.ToolStripSeparator m_ToolStrip_DirectorSprt;
        public System.Windows.Forms.ToolStripSeparator m_ToolStrip_ActorSprt;
        public System.Windows.Forms.ToolStrip m_ToolStrip;
        public System.Windows.Forms.ToolStripLabel m_ToolStrip_VRLbl;
        public System.Windows.Forms.ToolStripButton m_ToolStrip_VRBtn;
        public System.Windows.Forms.ToolStripLabel m_ToolStrip_nonVRLbl;
        public System.Windows.Forms.ToolStripButton m_ToolStrip_nonVRBtn;
        private System.Windows.Forms.ToolStripSeparator m_ToolStripSeparator7;
        public System.Windows.Forms.ToolStripSeparator m_ToolStrip_VRSprtr;
        public System.Windows.Forms.ToolStripSeparator m_ToolStrip_nonVRSprtr;
        public System.Windows.Forms.ToolStripSeparator m_ToolStrip_SeriesSprtr;
        public System.Windows.Forms.ToolStripLabel m_ToolStrip_SeriesLbl;
        public System.Windows.Forms.ToolStripButton m_ToolStrip_SeriesBtn;
        public System.Windows.Forms.ToolStripSeparator m_ToolStrip_MoviesSprtr;
        public System.Windows.Forms.ToolStripLabel m_ToolStrip_MoviesLbl;
        public System.Windows.Forms.ToolStripButton m_ToolStrip_MoviesBtn;
    }
}

