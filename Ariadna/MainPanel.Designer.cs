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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainPanel));
            this.listView = new System.Windows.Forms.ListView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.m_ToolStrip_AddBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStripMovieName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStripClearTitleBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_WishlistBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_RecentBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStrip_NewBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStripDirectorName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStripClearDirectorBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStripActorName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStripClearActorBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
            this.m_ToolStripGenreName = new System.Windows.Forms.ToolStripTextBox();
            this.m_ToolStripClearGenreBtn = new System.Windows.Forms.ToolStripButton();
            this.m_QuickListFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.toolStrip.SuspendLayout();
            this.m_QuickListFlow.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.BackColor = System.Drawing.Color.Purple;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listView.ForeColor = System.Drawing.Color.White;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.LargeImageList = this.imageList;
            this.listView.Location = new System.Drawing.Point(0, 25);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.OwnerDraw = true;
            this.listView.Size = new System.Drawing.Size(1272, 632);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.VirtualMode = true;
            this.listView.CacheVirtualItems += new System.Windows.Forms.CacheVirtualItemsEventHandler(this.ListView_CacheVirtualItems);
            this.listView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListView_DrawItem);
            this.listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListView_ItemSelectionChanged);
            this.listView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ListView_RetrieveVirtualItem);
            this.listView.SearchForVirtualItem += new System.Windows.Forms.SearchForVirtualItemEventHandler(this.ListView_SearchForVirtualItem);
            this.listView.Click += new System.EventHandler(this.OnListViewClick);
            this.listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageList.ImageSize = new System.Drawing.Size(171, 256);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.Color.Purple;
            this.toolStrip.CanOverflow = false;
            this.toolStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 2, 5);
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStrip_AddBtn,
            this.toolStripSeparator1,
            this.toolStripLabel3,
            this.m_ToolStripMovieName,
            this.m_ToolStripClearTitleBtn,
            this.toolStripSeparator3,
            this.toolStripLabel4,
            this.m_ToolStrip_WishlistBtn,
            this.toolStripSeparator5,
            this.toolStripLabel1,
            this.m_ToolStrip_RecentBtn,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.m_ToolStrip_NewBtn,
            this.toolStripSeparator4,
            this.toolStripLabel5,
            this.m_ToolStripDirectorName,
            this.m_ToolStripClearDirectorBtn,
            this.toolStripSeparator6,
            this.toolStripLabel6,
            this.m_ToolStripActorName,
            this.m_ToolStripClearActorBtn,
            this.toolStripSeparator7,
            this.toolStripLabel7,
            this.m_ToolStripGenreName,
            this.m_ToolStripClearGenreBtn});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip.Size = new System.Drawing.Size(1272, 25);
            this.toolStrip.Stretch = true;
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip";
            // 
            // m_ToolStrip_AddBtn
            // 
            this.m_ToolStrip_AddBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_AddBtn.ForeColor = System.Drawing.Color.White;
            this.m_ToolStrip_AddBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_AddBtn.Image")));
            this.m_ToolStrip_AddBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStrip_AddBtn.Name = "m_ToolStrip_AddBtn";
            this.m_ToolStrip_AddBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_AddBtn.Text = "Добавить";
            this.m_ToolStrip_AddBtn.Click += new System.EventHandler(this.ToolStripAddBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(59, 22);
            this.toolStripLabel3.Text = "Название";
            // 
            // m_ToolStripMovieName
            // 
            this.m_ToolStripMovieName.AutoSize = false;
            this.m_ToolStripMovieName.BackColor = System.Drawing.Color.DarkMagenta;
            this.m_ToolStripMovieName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStripMovieName.ForeColor = System.Drawing.Color.White;
            this.m_ToolStripMovieName.MaxLength = 50;
            this.m_ToolStripMovieName.Name = "m_ToolStripMovieName";
            this.m_ToolStripMovieName.Size = new System.Drawing.Size(150, 23);
            this.m_ToolStripMovieName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnMovieNameConfirmed);
            // 
            // m_ToolStripClearTitleBtn
            // 
            this.m_ToolStripClearTitleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStripClearTitleBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripClearTitleBtn.Image")));
            this.m_ToolStripClearTitleBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStripClearTitleBtn.Name = "m_ToolStripClearTitleBtn";
            this.m_ToolStripClearTitleBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStripClearTitleBtn.Text = "toolStripButton1";
            this.m_ToolStripClearTitleBtn.ToolTipText = "Очистить";
            this.m_ToolStripClearTitleBtn.Click += new System.EventHandler(this.OnToolStripClearTitleBtnClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(52, 22);
            this.toolStripLabel4.Text = "Хотелки";
            // 
            // m_ToolStrip_WishlistBtn
            // 
            this.m_ToolStrip_WishlistBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_WishlistBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_WishlistBtn.Image")));
            this.m_ToolStrip_WishlistBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStrip_WishlistBtn.Name = "m_ToolStrip_WishlistBtn";
            this.m_ToolStrip_WishlistBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_WishlistBtn.Text = "toolStripButton1";
            this.m_ToolStrip_WishlistBtn.ToolTipText = "Хотелки";
            this.m_ToolStrip_WishlistBtn.Click += new System.EventHandler(this.ToolStrip_WishlistBtn_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(60, 22);
            this.toolStripLabel1.Text = "Недавние";
            // 
            // m_ToolStrip_RecentBtn
            // 
            this.m_ToolStrip_RecentBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_RecentBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_RecentBtn.Image")));
            this.m_ToolStrip_RecentBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStrip_RecentBtn.Name = "m_ToolStrip_RecentBtn";
            this.m_ToolStrip_RecentBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_RecentBtn.Text = "Недавние";
            this.m_ToolStrip_RecentBtn.Click += new System.EventHandler(this.ToolStripRecentBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(56, 22);
            this.toolStripLabel2.Text = "Новинки";
            // 
            // m_ToolStrip_NewBtn
            // 
            this.m_ToolStrip_NewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStrip_NewBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStrip_NewBtn.Image")));
            this.m_ToolStrip_NewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStrip_NewBtn.Name = "m_ToolStrip_NewBtn";
            this.m_ToolStrip_NewBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStrip_NewBtn.Text = "Новинки";
            this.m_ToolStrip_NewBtn.Click += new System.EventHandler(this.ToolStrip_NewBtn_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(55, 22);
            this.toolStripLabel5.Text = "Режисер";
            // 
            // m_ToolStripDirectorName
            // 
            this.m_ToolStripDirectorName.AutoSize = false;
            this.m_ToolStripDirectorName.BackColor = System.Drawing.Color.DarkMagenta;
            this.m_ToolStripDirectorName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStripDirectorName.ForeColor = System.Drawing.Color.White;
            this.m_ToolStripDirectorName.MaxLength = 20;
            this.m_ToolStripDirectorName.Name = "m_ToolStripDirectorName";
            this.m_ToolStripDirectorName.Size = new System.Drawing.Size(100, 25);
            this.m_ToolStripDirectorName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnToolStripDirectorConfirmed);
            this.m_ToolStripDirectorName.TextChanged += new System.EventHandler(this.OnDirectorNameTextChanged);
            // 
            // m_ToolStripClearDirectorBtn
            // 
            this.m_ToolStripClearDirectorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStripClearDirectorBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripClearDirectorBtn.Image")));
            this.m_ToolStripClearDirectorBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStripClearDirectorBtn.Name = "m_ToolStripClearDirectorBtn";
            this.m_ToolStripClearDirectorBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStripClearDirectorBtn.Text = "toolStripButton1";
            this.m_ToolStripClearDirectorBtn.ToolTipText = "Очистить";
            this.m_ToolStripClearDirectorBtn.Click += new System.EventHandler(this.OnToolStripClearDirectorBtnClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel6.Name = "toolStripLabel6";
            this.toolStripLabel6.Size = new System.Drawing.Size(39, 22);
            this.toolStripLabel6.Text = "Актёр";
            // 
            // m_ToolStripActorName
            // 
            this.m_ToolStripActorName.AutoSize = false;
            this.m_ToolStripActorName.BackColor = System.Drawing.Color.DarkMagenta;
            this.m_ToolStripActorName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStripActorName.ForeColor = System.Drawing.Color.White;
            this.m_ToolStripActorName.MaxLength = 20;
            this.m_ToolStripActorName.Name = "m_ToolStripActorName";
            this.m_ToolStripActorName.Size = new System.Drawing.Size(100, 25);
            this.m_ToolStripActorName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnToolStripCastConfirmed);
            this.m_ToolStripActorName.TextChanged += new System.EventHandler(this.OnActorNameTextChanged);
            // 
            // m_ToolStripClearActorBtn
            // 
            this.m_ToolStripClearActorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStripClearActorBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripClearActorBtn.Image")));
            this.m_ToolStripClearActorBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStripClearActorBtn.Name = "m_ToolStripClearActorBtn";
            this.m_ToolStripClearActorBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStripClearActorBtn.Text = "toolStripButton1";
            this.m_ToolStripClearActorBtn.ToolTipText = "Очистить";
            this.m_ToolStripClearActorBtn.Click += new System.EventHandler(this.OnToolStripClearActorBtnClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel7
            // 
            this.toolStripLabel7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStripLabel7.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel7.Name = "toolStripLabel7";
            this.toolStripLabel7.Size = new System.Drawing.Size(39, 22);
            this.toolStripLabel7.Text = "Жанр";
            // 
            // m_ToolStripGenreName
            // 
            this.m_ToolStripGenreName.BackColor = System.Drawing.Color.DarkMagenta;
            this.m_ToolStripGenreName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_ToolStripGenreName.ForeColor = System.Drawing.Color.White;
            this.m_ToolStripGenreName.MaxLength = 100;
            this.m_ToolStripGenreName.Name = "m_ToolStripGenreName";
            this.m_ToolStripGenreName.Size = new System.Drawing.Size(200, 25);
            this.m_ToolStripGenreName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnToolStripGenreConfirmed);
            this.m_ToolStripGenreName.Click += new System.EventHandler(this.OnToolStripGenreClicked);
            // 
            // m_ToolStripClearGenreBtn
            // 
            this.m_ToolStripClearGenreBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_ToolStripClearGenreBtn.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripClearGenreBtn.Image")));
            this.m_ToolStripClearGenreBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_ToolStripClearGenreBtn.Name = "m_ToolStripClearGenreBtn";
            this.m_ToolStripClearGenreBtn.Size = new System.Drawing.Size(23, 22);
            this.m_ToolStripClearGenreBtn.Text = "toolStripButton1";
            this.m_ToolStripClearGenreBtn.ToolTipText = "Очистить";
            this.m_ToolStripClearGenreBtn.Click += new System.EventHandler(this.OnToolStripClearGenreBtnClick);
            // 
            // m_QuickListFlow
            // 
            this.m_QuickListFlow.BackColor = System.Drawing.Color.Purple;
            this.m_QuickListFlow.Controls.Add(this.button1);
            this.m_QuickListFlow.Controls.Add(this.button2);
            this.m_QuickListFlow.Dock = System.Windows.Forms.DockStyle.Right;
            this.m_QuickListFlow.Location = new System.Drawing.Point(1272, 0);
            this.m_QuickListFlow.Name = "m_QuickListFlow";
            this.m_QuickListFlow.Size = new System.Drawing.Size(93, 657);
            this.m_QuickListFlow.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.DarkMagenta;
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(40, 40);
            this.button1.TabIndex = 0;
            this.button1.Text = "A";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.DarkMagenta;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(49, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(40, 40);
            this.button2.TabIndex = 1;
            this.button2.Text = "A";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // MainPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Purple;
            this.ClientSize = new System.Drawing.Size(1365, 657);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.m_QuickListFlow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "MainPanel";
            this.Text = "Ariadna";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainPanel_Load);
            this.Click += new System.EventHandler(this.OnFormClicked);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainPanel_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainPanel_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFormClicked);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.m_QuickListFlow.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_AddBtn;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_RecentBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_NewBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripTextBox m_ToolStripMovieName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripButton m_ToolStrip_WishlistBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.FlowLayoutPanel m_QuickListFlow;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripTextBox m_ToolStripDirectorName;
        private System.Windows.Forms.ToolStripButton m_ToolStripClearDirectorBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripLabel toolStripLabel6;
        private System.Windows.Forms.ToolStripTextBox m_ToolStripActorName;
        private System.Windows.Forms.ToolStripButton m_ToolStripClearActorBtn;
        private System.Windows.Forms.ToolStripButton m_ToolStripClearTitleBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripLabel toolStripLabel7;
        private System.Windows.Forms.ToolStripTextBox m_ToolStripGenreName;
        private System.Windows.Forms.ToolStripButton m_ToolStripClearGenreBtn;
    }
}

