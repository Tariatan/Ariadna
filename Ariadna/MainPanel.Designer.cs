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
            this.m_ImageListView = new Manina.Windows.Forms.ImageListView();
            this.m_ToolStrip = new System.Windows.Forms.ToolStrip();
            this.m_ToolStrip_AddBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_NameLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_EntryName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStrip_ClearTitleBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_WishlistLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_WishlistBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_RecentLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_RecentBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_NewLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_NewBtn = new System.Windows.Forms.ToolStripButton();
            this.m_ToolStrip_VRSprtr = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_VRLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_VRBtn = new System.Windows.Forms.ToolStripButton();
            this.m_ToolStrip_nonVRSprtr = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_nonVRLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_nonVRBtn = new System.Windows.Forms.ToolStripButton();
            this.m_ToolStrip_SeriesSprtr = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_SeriesLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_SeriesBtn = new System.Windows.Forms.ToolStripButton();
            this.m_ToolStrip_MoviesSprtr = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_MoviesLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_MoviesBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_DirectorLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_DirectorName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStrip_ClearDirectorBtn = new System.Windows.Forms.ToolStripButton();
            this.m_ToolStrip_DirectorSprt = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_ActorLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_ActorName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStrip_ClearActorBtn = new System.Windows.Forms.ToolStripButton();
            this.m_ToolStrip_ActorSprt = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_GenreNameLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_GenreName = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_ClearGenreBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.m_ToolStrip_EntriesCountLbl = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_EntriesCount = new System.Windows.Forms.ToolStripLabel();
            this.m_QuickListFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.m_ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_ImageListView
            // 
            this.m_ImageListView.AllowItemReorder = false;
            this.m_ImageListView.DefaultImage = global::Ariadna.Properties.Resources.empty_icon;
            this.m_ImageListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ImageListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_ImageListView.Location = new System.Drawing.Point(0, 25);
            this.m_ImageListView.MultiSelect = false;
            this.m_ImageListView.Name = "m_ImageListView";
            this.m_ImageListView.PersistentCacheDirectory = "";
            this.m_ImageListView.PersistentCacheSize = ((long)(100));
            this.m_ImageListView.Size = new System.Drawing.Size(1583, 632);
            this.m_ImageListView.TabIndex = 1;
            this.m_ImageListView.ThumbnailSize = new System.Drawing.Size(214, 321);
            this.m_ImageListView.UseWIC = true;
            this.m_ImageListView.SelectionChanged += new System.EventHandler(this.ListView_ItemSelectionChanged);
            this.m_ImageListView.Click += new System.EventHandler(this.OnListViewClick);
            this.m_ImageListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnListViewKeyDown);
            this.m_ImageListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
            this.m_ImageListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClicked);
            // 
            // m_ToolStrip
            // 
            this.m_ToolStrip.CanOverflow = false;
            this.m_ToolStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 2, 5);
            this.m_ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.m_ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStrip_AddBtn,
            this.toolStripSeparator1,
            this.m_ToolStrip_NameLbl,
            this.m_ToolStrip_EntryName,
            this.m_ToolStrip_ClearTitleBtn,
            this.toolStripSeparator3,
            this.m_ToolStrip_WishlistLbl,
            this.m_ToolStrip_WishlistBtn,
            this.toolStripSeparator5,
            this.m_ToolStrip_RecentLbl,
            this.m_ToolStrip_RecentBtn,
            this.toolStripSeparator7,
            this.m_ToolStrip_NewLbl,
            this.m_ToolStrip_NewBtn,
            this.m_ToolStrip_VRSprtr,
            this.m_ToolStrip_VRLbl,
            this.m_ToolStrip_VRBtn,
            this.m_ToolStrip_nonVRSprtr,
            this.m_ToolStrip_nonVRLbl,
            this.m_ToolStrip_nonVRBtn,
            this.m_ToolStrip_SeriesSprtr,
            this.m_ToolStrip_SeriesLbl,
            this.m_ToolStrip_SeriesBtn,
            this.m_ToolStrip_MoviesSprtr,
            this.m_ToolStrip_MoviesLbl,
            this.m_ToolStrip_MoviesBtn,
            this.toolStripSeparator4,
            this.m_ToolStrip_DirectorLbl,
            this.m_ToolStrip_DirectorName,
            this.m_ToolStrip_ClearDirectorBtn,
            this.m_ToolStrip_DirectorSprt,
            this.m_ToolStrip_ActorLbl,
            this.m_ToolStrip_ActorName,
            this.m_ToolStrip_ClearActorBtn,
            this.m_ToolStrip_ActorSprt,
            this.m_ToolStrip_GenreNameLbl,
            this.m_ToolStrip_GenreName,
            this.m_ToolStrip_ClearGenreBtn,
            this.toolStripSeparator8,
            this.m_ToolStrip_EntriesCountLbl,
            this.m_ToolStrip_EntriesCount});
            this.m_ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.m_ToolStrip.Name = "m_ToolStrip";
            this.m_ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.m_ToolStrip.Size = new System.Drawing.Size(1583, 25);
            this.m_ToolStrip.Stretch = true;
            this.m_ToolStrip.TabIndex = 2;
            // 
            // m_ToolStrip_AddBtn
            // 
            this.m_ToolStrip_AddBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_AddBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_AddBtn.Image")));
            this.m_ToolStrip_AddBtn.Name = "m_ToolStrip_AddBtn";
            this.m_ToolStrip_AddBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_AddBtn.Text = "Добавить";
            this.m_ToolStrip_AddBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStrip_AddBtn_MouseUp);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_NameLbl
            // 
            this.m_ToolStrip_NameLbl.Name = "m_ToolStrip_NameLbl";
            this.m_ToolStrip_NameLbl.Size = new System.Drawing.Size(59, 22);
            this.m_ToolStrip_NameLbl.Text = "Название";
            // 
            // m_ToolStrip_EntryName
            // 
            this.m_ToolStrip_EntryName.AutoSize = false;
            this.m_ToolStrip_EntryName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStrip_EntryName.MaxLength = 50;
            this.m_ToolStrip_EntryName.Name = "m_ToolStrip_EntryName";
            this.m_ToolStrip_EntryName.Size = new System.Drawing.Size(150, 23);
            this.m_ToolStrip_EntryName.Enter += new System.EventHandler(this.OnEntryNameTextEntered);
            this.m_ToolStrip_EntryName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnEntryNameConfirmed);
            this.m_ToolStrip_EntryName.TextChanged += new System.EventHandler(this.OnEntryNameTextChanged);
            // 
            // m_ToolStrip_ClearTitleBtn
            // 
            this.m_ToolStrip_ClearTitleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_ClearTitleBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_ClearTitleBtn.Image")));
            this.m_ToolStrip_ClearTitleBtn.Name = "m_ToolStrip_ClearTitleBtn";
            this.m_ToolStrip_ClearTitleBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_ClearTitleBtn.ToolTipText = "Очистить";
            this.m_ToolStrip_ClearTitleBtn.Click += new System.EventHandler(this.ToolStrip_ClearTitleBtn_Clicked);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_WishlistLbl
            // 
            this.m_ToolStrip_WishlistLbl.Name = "m_ToolStrip_WishlistLbl";
            this.m_ToolStrip_WishlistLbl.Size = new System.Drawing.Size(52, 22);
            this.m_ToolStrip_WishlistLbl.Text = "Хотелки";
            // 
            // m_ToolStrip_WishlistBtn
            // 
            this.m_ToolStrip_WishlistBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_WishlistBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_WishlistBtn.Image")));
            this.m_ToolStrip_WishlistBtn.Name = "m_ToolStrip_WishlistBtn";
            this.m_ToolStrip_WishlistBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_WishlistBtn.ToolTipText = "Хотелки";
            this.m_ToolStrip_WishlistBtn.Click += new System.EventHandler(this.ToolStrip_CheckboxedFilter_Clicked);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_RecentLbl
            // 
            this.m_ToolStrip_RecentLbl.Name = "m_ToolStrip_RecentLbl";
            this.m_ToolStrip_RecentLbl.Size = new System.Drawing.Size(60, 22);
            this.m_ToolStrip_RecentLbl.Text = "Недавние";
            // 
            // m_ToolStrip_RecentBtn
            // 
            this.m_ToolStrip_RecentBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_RecentBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_RecentBtn.Image")));
            this.m_ToolStrip_RecentBtn.Name = "m_ToolStrip_RecentBtn";
            this.m_ToolStrip_RecentBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_RecentBtn.Text = "Недавние";
            this.m_ToolStrip_RecentBtn.Click += new System.EventHandler(this.ToolStrip_CheckboxedFilter_Clicked);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_NewLbl
            // 
            this.m_ToolStrip_NewLbl.Name = "m_ToolStrip_NewLbl";
            this.m_ToolStrip_NewLbl.Size = new System.Drawing.Size(56, 22);
            this.m_ToolStrip_NewLbl.Text = "Новинки";
            // 
            // m_ToolStrip_NewBtn
            // 
            this.m_ToolStrip_NewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_NewBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_NewBtn.Image")));
            this.m_ToolStrip_NewBtn.Name = "m_ToolStrip_NewBtn";
            this.m_ToolStrip_NewBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_NewBtn.Text = "Новинки";
            this.m_ToolStrip_NewBtn.Click += new System.EventHandler(this.ToolStrip_CheckboxedFilter_Clicked);
            // 
            // m_ToolStrip_VRSprtr
            // 
            this.m_ToolStrip_VRSprtr.Name = "m_ToolStrip_VRSprtr";
            this.m_ToolStrip_VRSprtr.Size = new System.Drawing.Size(6, 25);
            this.m_ToolStrip_VRSprtr.Visible = false;
            // 
            // m_ToolStrip_VRLbl
            // 
            this.m_ToolStrip_VRLbl.Name = "m_ToolStrip_VRLbl";
            this.m_ToolStrip_VRLbl.Size = new System.Drawing.Size(21, 22);
            this.m_ToolStrip_VRLbl.Text = "VR";
            this.m_ToolStrip_VRLbl.Visible = false;
            // 
            // m_ToolStrip_VRBtn
            // 
            this.m_ToolStrip_VRBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_VRBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_VRBtn.Image")));
            this.m_ToolStrip_VRBtn.Name = "m_ToolStrip_VRBtn";
            this.m_ToolStrip_VRBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_VRBtn.Text = "VR";
            this.m_ToolStrip_VRBtn.Visible = false;
            this.m_ToolStrip_VRBtn.Click += new System.EventHandler(this.ToolStrip_CheckboxedFilter_Clicked);
            // 
            // m_ToolStrip_nonVRSprtr
            // 
            this.m_ToolStrip_nonVRSprtr.Name = "m_ToolStrip_nonVRSprtr";
            this.m_ToolStrip_nonVRSprtr.Size = new System.Drawing.Size(6, 25);
            this.m_ToolStrip_nonVRSprtr.Visible = false;
            // 
            // m_ToolStrip_nonVRLbl
            // 
            this.m_ToolStrip_nonVRLbl.Name = "m_ToolStrip_nonVRLbl";
            this.m_ToolStrip_nonVRLbl.Size = new System.Drawing.Size(47, 22);
            this.m_ToolStrip_nonVRLbl.Text = "non-VR";
            this.m_ToolStrip_nonVRLbl.Visible = false;
            // 
            // m_ToolStrip_nonVRBtn
            // 
            this.m_ToolStrip_nonVRBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_nonVRBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_nonVRBtn.Image")));
            this.m_ToolStrip_nonVRBtn.Name = "m_ToolStrip_nonVRBtn";
            this.m_ToolStrip_nonVRBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_nonVRBtn.Text = "nonVR";
            this.m_ToolStrip_nonVRBtn.Visible = false;
            this.m_ToolStrip_nonVRBtn.Click += new System.EventHandler(this.ToolStrip_CheckboxedFilter_Clicked);
            // 
            // m_ToolStrip_SeriesSprtr
            // 
            this.m_ToolStrip_SeriesSprtr.Name = "m_ToolStrip_SeriesSprtr";
            this.m_ToolStrip_SeriesSprtr.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_SeriesLbl
            // 
            this.m_ToolStrip_SeriesLbl.Name = "m_ToolStrip_SeriesLbl";
            this.m_ToolStrip_SeriesLbl.Size = new System.Drawing.Size(57, 22);
            this.m_ToolStrip_SeriesLbl.Text = "Сериалы";
            // 
            // m_ToolStrip_SeriesBtn
            // 
            this.m_ToolStrip_SeriesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_SeriesBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_SeriesBtn.Image")));
            this.m_ToolStrip_SeriesBtn.Name = "m_ToolStrip_SeriesBtn";
            this.m_ToolStrip_SeriesBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_SeriesBtn.Text = "Series";
            this.m_ToolStrip_SeriesBtn.Click += new System.EventHandler(this.ToolStrip_ToolStrip_SeriesBtn_Clicked);
            // 
            // m_ToolStrip_MoviesSprtr
            // 
            this.m_ToolStrip_MoviesSprtr.Name = "m_ToolStrip_MoviesSprtr";
            this.m_ToolStrip_MoviesSprtr.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_MoviesLbl
            // 
            this.m_ToolStrip_MoviesLbl.Name = "m_ToolStrip_MoviesLbl";
            this.m_ToolStrip_MoviesLbl.Size = new System.Drawing.Size(54, 22);
            this.m_ToolStrip_MoviesLbl.Text = "Фильмы";
            // 
            // m_ToolStrip_MoviesBtn
            // 
            this.m_ToolStrip_MoviesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_MoviesBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_MoviesBtn.Image")));
            this.m_ToolStrip_MoviesBtn.Name = "m_ToolStrip_MoviesBtn";
            this.m_ToolStrip_MoviesBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_MoviesBtn.Text = "Movies";
            this.m_ToolStrip_MoviesBtn.Click += new System.EventHandler(this.ToolStrip_ToolStrip_MoviesBtn_Clicked);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_DirectorLbl
            // 
            this.m_ToolStrip_DirectorLbl.Name = "m_ToolStrip_DirectorLbl";
            this.m_ToolStrip_DirectorLbl.Size = new System.Drawing.Size(55, 22);
            this.m_ToolStrip_DirectorLbl.Text = "Режисер";
            // 
            // m_ToolStrip_DirectorName
            // 
            this.m_ToolStrip_DirectorName.AutoSize = false;
            this.m_ToolStrip_DirectorName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStrip_DirectorName.MaxLength = 20;
            this.m_ToolStrip_DirectorName.Name = "m_ToolStrip_DirectorName";
            this.m_ToolStrip_DirectorName.Size = new System.Drawing.Size(100, 25);
            this.m_ToolStrip_DirectorName.Enter += new System.EventHandler(this.OnDirectorNameTextChanged);
            this.m_ToolStrip_DirectorName.TextChanged += new System.EventHandler(this.OnDirectorNameTextChanged);
            // 
            // m_ToolStrip_ClearDirectorBtn
            // 
            this.m_ToolStrip_ClearDirectorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_ClearDirectorBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_ClearDirectorBtn.Image")));
            this.m_ToolStrip_ClearDirectorBtn.Name = "m_ToolStrip_ClearDirectorBtn";
            this.m_ToolStrip_ClearDirectorBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_ClearDirectorBtn.ToolTipText = "Очистить";
            this.m_ToolStrip_ClearDirectorBtn.Click += new System.EventHandler(this.ToolStrip_ClearDirectorBtn_Clicked);
            // 
            // m_ToolStrip_DirectorSprt
            // 
            this.m_ToolStrip_DirectorSprt.Name = "m_ToolStrip_DirectorSprt";
            this.m_ToolStrip_DirectorSprt.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_ActorLbl
            // 
            this.m_ToolStrip_ActorLbl.Name = "m_ToolStrip_ActorLbl";
            this.m_ToolStrip_ActorLbl.Size = new System.Drawing.Size(39, 22);
            this.m_ToolStrip_ActorLbl.Text = "Актёр";
            // 
            // m_ToolStrip_ActorName
            // 
            this.m_ToolStrip_ActorName.AutoSize = false;
            this.m_ToolStrip_ActorName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStrip_ActorName.MaxLength = 20;
            this.m_ToolStrip_ActorName.Name = "m_ToolStrip_ActorName";
            this.m_ToolStrip_ActorName.Size = new System.Drawing.Size(100, 25);
            this.m_ToolStrip_ActorName.Enter += new System.EventHandler(this.OnActorNameTextChanged);
            this.m_ToolStrip_ActorName.TextChanged += new System.EventHandler(this.OnActorNameTextChanged);
            // 
            // m_ToolStrip_ClearActorBtn
            // 
            this.m_ToolStrip_ClearActorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_ClearActorBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_ClearActorBtn.Image")));
            this.m_ToolStrip_ClearActorBtn.Name = "m_ToolStrip_ClearActorBtn";
            this.m_ToolStrip_ClearActorBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_ClearActorBtn.ToolTipText = "Очистить";
            this.m_ToolStrip_ClearActorBtn.Click += new System.EventHandler(this.ToolStrip_ClearActorBtn_Clicked);
            // 
            // m_ToolStrip_ActorSprt
            // 
            this.m_ToolStrip_ActorSprt.Name = "m_ToolStrip_ActorSprt";
            this.m_ToolStrip_ActorSprt.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_GenreNameLbl
            // 
            this.m_ToolStrip_GenreNameLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_ToolStrip_GenreNameLbl.Name = "m_ToolStrip_GenreNameLbl";
            this.m_ToolStrip_GenreNameLbl.Size = new System.Drawing.Size(39, 22);
            this.m_ToolStrip_GenreNameLbl.Text = "Жанр";
            // 
            // m_ToolStrip_GenreName
            // 
            this.m_ToolStrip_GenreName.Name = "m_ToolStrip_GenreName";
            this.m_ToolStrip_GenreName.Size = new System.Drawing.Size(22, 22);
            this.m_ToolStrip_GenreName.Text = ". . .";
            this.m_ToolStrip_GenreName.Click += new System.EventHandler(this.ToolStrip_Genre_Clicked);
            // 
            // m_ToolStrip_ClearGenreBtn
            // 
            this.m_ToolStrip_ClearGenreBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_ClearGenreBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_ClearGenreBtn.Image")));
            this.m_ToolStrip_ClearGenreBtn.Name = "m_ToolStrip_ClearGenreBtn";
            this.m_ToolStrip_ClearGenreBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_ClearGenreBtn.ToolTipText = "Очистить";
            this.m_ToolStrip_ClearGenreBtn.Click += new System.EventHandler(this.ToolStrip_ClearGenreBtn_Clicked);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // m_ToolStrip_EntriesCountLbl
            // 
            this.m_ToolStrip_EntriesCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_ToolStrip_EntriesCountLbl.Name = "m_ToolStrip_EntriesCountLbl";
            this.m_ToolStrip_EntriesCountLbl.Size = new System.Drawing.Size(94, 22);
            this.m_ToolStrip_EntriesCountLbl.Text = "Всего записей:";
            // 
            // m_ToolStrip_EntriesCount
            // 
            this.m_ToolStrip_EntriesCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_ToolStrip_EntriesCount.Name = "m_ToolStrip_EntriesCount";
            this.m_ToolStrip_EntriesCount.Size = new System.Drawing.Size(14, 22);
            this.m_ToolStrip_EntriesCount.Text = "0";
            // 
            // m_QuickListFlow
            // 
            this.m_QuickListFlow.Dock = System.Windows.Forms.DockStyle.Right;
            this.m_QuickListFlow.Location = new System.Drawing.Point(1583, 0);
            this.m_QuickListFlow.Name = "m_QuickListFlow";
            this.m_QuickListFlow.Size = new System.Drawing.Size(93, 657);
            this.m_QuickListFlow.TabIndex = 3;
            this.m_QuickListFlow.MouseCaptureChanged += new System.EventHandler(this.OnMouseCaptureChanged);
            // 
            // MainPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(1676, 657);
            this.Controls.Add(this.m_ImageListView);
            this.Controls.Add(this.m_ToolStrip);
            this.Controls.Add(this.m_QuickListFlow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "MainPanel";
            this.Text = "Ariadna";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainPanel_Load);
            this.Click += new System.EventHandler(this.OnFormClicked);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainPanel_KeyUp);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainPanel_KeyPress);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFormClicked);
            this.Move += new System.EventHandler(this.OnPanelMoved);
            this.Resize += new System.EventHandler(this.OnPanelResized);
            this.m_ToolStrip.ResumeLayout(false);
            this.m_ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Manina.Windows.Forms.ImageListView m_ImageListView;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_AddBtn;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_RecentBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_RecentLbl;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_NewLbl;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_NewBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_NameLbl;
        private System.Windows.Forms.ToolStripTextBox m_ToolStrip_EntryName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_WishlistLbl;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_WishlistBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.FlowLayoutPanel m_QuickListFlow;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_ClearTitleBtn;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_GenreNameLbl;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_ClearGenreBtn;
        private System.Windows.Forms.ToolStripLabel m_ToolStrip_GenreName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
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

