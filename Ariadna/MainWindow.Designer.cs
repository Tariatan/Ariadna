namespace Ariadna;

partial class MainWindow
{
    private CatalogTabControl catalogTabs;

    private void InitializeComponent()
    {
        catalogTabs = new CatalogTabControl();
        moviesPage = new System.Windows.Forms.TabPage();
        gamesPage = new System.Windows.Forms.TabPage();
        libraryPage = new System.Windows.Forms.TabPage();
        documentariesPage = new System.Windows.Forms.TabPage();
        catalogTabs.SuspendLayout();
        SuspendLayout();
        // 
        // catalogTabs
        // 
        catalogTabs.Controls.Add(moviesPage);
        catalogTabs.Controls.Add(gamesPage);
        catalogTabs.Controls.Add(libraryPage);
        catalogTabs.Controls.Add(documentariesPage);
        catalogTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        catalogTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
        catalogTabs.ItemSize = new System.Drawing.Size(200, 36);
        catalogTabs.Location = new System.Drawing.Point(0, 0);
        catalogTabs.Name = "catalogTabs";
        catalogTabs.Padding = new System.Drawing.Point(12, 4);
        catalogTabs.SelectedIndex = 0;
        catalogTabs.Size = new System.Drawing.Size(1676, 900);
        catalogTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
        catalogTabs.TabIndex = 0;
        catalogTabs.Selecting += OnCatalogSelecting;
        catalogTabs.SelectedIndexChanged += OnCatalogSelected;
        // 
        // moviesPage
        // 
        moviesPage.Location = new System.Drawing.Point(4, 40);
        moviesPage.Name = "moviesPage";
        moviesPage.Size = new System.Drawing.Size(1668, 856);
        moviesPage.TabIndex = 0;
        moviesPage.Text = "Movies";
        moviesPage.Visible = false;
        // 
        // gamesPage
        // 
        gamesPage.Location = new System.Drawing.Point(4, 40);
        gamesPage.Name = "gamesPage";
        gamesPage.Size = new System.Drawing.Size(1668, 856);
        gamesPage.TabIndex = 1;
        gamesPage.Text = "Games";
        gamesPage.Visible = false;
        // 
        // libraryPage
        // 
        libraryPage.Location = new System.Drawing.Point(4, 40);
        libraryPage.Name = "libraryPage";
        libraryPage.Size = new System.Drawing.Size(1668, 856);
        libraryPage.TabIndex = 2;
        libraryPage.Text = "Library";
        libraryPage.Visible = false;
        // 
        // documentariesPage
        // 
        documentariesPage.Location = new System.Drawing.Point(4, 40);
        documentariesPage.Name = "documentariesPage";
        documentariesPage.Size = new System.Drawing.Size(1668, 856);
        documentariesPage.TabIndex = 3;
        documentariesPage.Text = "Documentaries";
        documentariesPage.Visible = false;
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        ClientSize = new System.Drawing.Size(1676, 900);
        Controls.Add(catalogTabs);
        Font = new System.Drawing.Font("Segoe UI", 10F);
        KeyPreview = true;
        MinimumSize = new System.Drawing.Size(1000, 650);
        Name = "MainWindow";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Ariadna";
        catalogTabs.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.TabPage moviesPage;
    private System.Windows.Forms.TabPage gamesPage;
    private System.Windows.Forms.TabPage libraryPage;
    private System.Windows.Forms.TabPage documentariesPage;
}