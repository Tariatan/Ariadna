using Ariadna.Storage;

namespace Ariadna;

partial class MainWindow
{
    private CatalogTabControl catalogTabs;

    private void InitializeComponent()
    {
        catalogTabs = new CatalogTabControl();
        SuspendLayout();
        catalogTabs.Dock = System.Windows.Forms.DockStyle.Fill;
        catalogTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
        catalogTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
        catalogTabs.ItemSize = new System.Drawing.Size(200, 36);
        catalogTabs.Padding = new System.Drawing.Point(12, 4);
        catalogTabs.Name = "catalogTabs";
        catalogTabs.TabIndex = 0;
        catalogTabs.TabPages.Add(new System.Windows.Forms.TabPage("Movies") { Tag = CatalogKind.Movie });
        catalogTabs.TabPages.Add(new System.Windows.Forms.TabPage("Games") { Tag = CatalogKind.Game });
        catalogTabs.TabPages.Add(new System.Windows.Forms.TabPage("Library") { Tag = CatalogKind.Library });
        catalogTabs.TabPages.Add(new System.Windows.Forms.TabPage("Documentaries") { Tag = CatalogKind.Documentary });
        catalogTabs.Selecting += OnCatalogSelecting;
        catalogTabs.SelectedIndexChanged += OnCatalogSelected;
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        ClientSize = new System.Drawing.Size(1676, 900);
        MinimumSize = new System.Drawing.Size(1000, 650);
        Font = new System.Drawing.Font("Segoe UI", 10F);
        Controls.Add(catalogTabs);
        Icon = (System.Drawing.Icon)new System.ComponentModel.ComponentResourceManager(typeof(MainPanel)).GetObject("$this.Icon");
        KeyPreview = true;
        Name = "MainWindow";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Ariadna";
        ResumeLayout(false);
    }
}
