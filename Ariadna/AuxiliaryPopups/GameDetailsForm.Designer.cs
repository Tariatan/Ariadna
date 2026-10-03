#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Ariadna.Properties;

namespace Ariadna.AuxiliaryPopups;

partial class GameDetailsForm
{
    private IContainer components = null!;
    private TextBox titleText = null!;
    private TextBox originalTitleText = null!;
    private TextBox yearText = null!;
    private TextBox pathText = null!;
    private Button saveButton = null!;
    private CheckBox wanted = null!;
    private ImageEditorControl poster = null!;
    private GenreSelectionControl genres = null!;
    private FileSizeControl fileSize = null!;
    private TextBox versionText = null!;
    private CheckBox vr = null!;
    private GamePreviewsControl previews = null!;
    private Label titleLabel = null!;
    private Label originalTitleLabel = null!;
    private Label yearLabel = null!;
    private Label genreLabel = null!;
    private Label pathLabel = null!;
    private Label versionLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();
        titleText = new TextBox();
        originalTitleText = new TextBox();
        yearText = new TextBox();
        pathText = new TextBox();
        saveButton = new Button();
        wanted = new CheckBox();
        poster = new ImageEditorControl();
        genres = new GenreSelectionControl();
        fileSize = new FileSizeControl();
        versionText = new TextBox();
        vr = new CheckBox();
        previews = new GamePreviewsControl();
        titleLabel = new Label();
        originalTitleLabel = new Label();
        yearLabel = new Label();
        genreLabel = new Label();
        pathLabel = new Label();
        versionLabel = new Label();
        SuspendLayout();
        titleText.Name = "titleText";
        titleText.Location = new Point(416, 22);
        titleText.Size = new Size(603, 29);
        titleText.TabIndex = 0;
        titleText.BorderStyle = BorderStyle.FixedSingle;
        originalTitleText.Name = "originalTitleText";
        originalTitleText.Location = new Point(416, 65);
        originalTitleText.Size = new Size(603, 29);
        originalTitleText.TabIndex = 1;
        originalTitleText.BorderStyle = BorderStyle.FixedSingle;
        yearText.Name = "yearText";
        yearText.Location = new Point(416, 110);
        yearText.Size = new Size(77, 26);
        yearText.TabIndex = 2;
        yearText.BorderStyle = BorderStyle.FixedSingle;
        pathText.Name = "pathText";
        pathText.Location = new Point(7, 627);
        pathText.Size = new Size(323, 22);
        pathText.TabIndex = 10;
        pathText.BorderStyle = BorderStyle.FixedSingle;
        titleText.Font = new Font("Microsoft Sans Serif", 14.25F);
        originalTitleText.Font = titleText.Font;
        yearText.Font = new Font("Microsoft Sans Serif", 12F);
        pathText.TextChanged += OnPathChanged;
        poster.Name = "poster";
        poster.Location = new Point(7, 7);
        poster.Size = new Size(400, 600);
        poster.ImageWidth = Settings.Default.PosterWidth;
        poster.ImageHeight = Settings.Default.PosterHeight;
        poster.TabIndex = 11;
        genres.Name = "genres";
        genres.Location = new Point(500, 110);
        genres.Size = new Size(519, 81);
        genres.TabIndex = 3;
        fileSize.Name = "fileSize";
        fileSize.Location = new Point(337, 627);
        fileSize.Size = new Size(70, 22);
        wanted.Name = "wanted";
        wanted.Text = "To wishlist";
        wanted.Location = new Point(827, 621);
        wanted.Size = new Size(96, 29);
        wanted.TabIndex = 8;
        saveButton.Name = "saveButton";
        saveButton.Location = new Point(925, 619);
        saveButton.Size = new Size(94, 29);
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.Font = new Font("Microsoft Sans Serif", 12F);
        saveButton.TabIndex = 9;
        versionText.Name = "versionText";
        versionText.Location = new Point(416, 215);
        versionText.Size = new Size(160, 23);
        versionText.TabIndex = 4;
        vr.Name = "vr";
        vr.Text = "VR";
        vr.Location = new Point(590, 215);
        vr.Size = new Size(60, 23);
        vr.TabIndex = 5;
        previews.Name = "previews";
        previews.Location = new Point(416, 249);
        previews.Size = new Size(603, 360);
        previews.TabIndex = 6;
        titleLabel.Text = "Title";
        titleLabel.Location = new Point(416, 5);
        titleLabel.AutoSize = true;
        originalTitleLabel.Text = "Original title";
        originalTitleLabel.Location = new Point(416, 48);
        originalTitleLabel.AutoSize = true;
        yearLabel.Text = "Year";
        yearLabel.Location = new Point(416, 93);
        yearLabel.AutoSize = true;
        genreLabel.Text = "Genre";
        genreLabel.Location = new Point(500, 93);
        genreLabel.AutoSize = true;
        pathLabel.Text = "Path";
        pathLabel.Location = new Point(7, 608);
        pathLabel.AutoSize = true;
        versionLabel.Text = "Version";
        versionLabel.Location = new Point(416, 197);
        versionLabel.AutoSize = true;
        Controls.Add(titleText);
        Controls.Add(originalTitleText);
        Controls.Add(yearText);
        Controls.Add(pathText);
        Controls.Add(saveButton);
        Controls.Add(wanted);
        Controls.Add(poster);
        Controls.Add(genres);
        Controls.Add(fileSize);
        Controls.Add(versionText);
        Controls.Add(vr);
        Controls.Add(previews);
        Controls.Add(titleLabel);
        Controls.Add(originalTitleLabel);
        Controls.Add(yearLabel);
        Controls.Add(genreLabel);
        Controls.Add(pathLabel);
        Controls.Add(versionLabel);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1025, 657);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        Icon = Resources.AriadnaGames;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        KeyPreview = true;
        Name = "GameDetailsForm";
        Load += OnLoad;
        FormClosed += OnFormClosed;
        ResumeLayout(false);
        PerformLayout();
    }
}
