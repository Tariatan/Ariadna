#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Ariadna.Properties;

namespace Ariadna.AuxiliaryPopups;

partial class DocumentaryDetailsForm
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
    private TextBox descriptionText = null!;
    private Button descriptionPaste = null!;
    private VideoInfoControl videoInfo = null!;
    private Label titleLabel = null!;
    private Label originalTitleLabel = null!;
    private Label yearLabel = null!;
    private Label genreLabel = null!;
    private Label pathLabel = null!;
    private Label descriptionLabel = null!;

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
        descriptionText = new TextBox();
        descriptionPaste = new Button();
        videoInfo = new VideoInfoControl();
        titleLabel = new Label();
        originalTitleLabel = new Label();
        yearLabel = new Label();
        genreLabel = new Label();
        pathLabel = new Label();
        descriptionLabel = new Label();
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
        descriptionText.Name = "descriptionText";
        descriptionText.Location = new Point(416, 215);
        descriptionText.Size = new Size(603, 394);
        descriptionText.Multiline = true;
        descriptionText.ScrollBars = ScrollBars.Vertical;
        descriptionText.BorderStyle = BorderStyle.FixedSingle;
        descriptionText.TabIndex = 4;
        descriptionText.KeyUp += OnDescriptionKeyUp;
        descriptionPaste.Name = "descriptionPaste";
        descriptionPaste.Text = "↓";
        descriptionPaste.Location = new Point(500, 194);
        descriptionPaste.Size = new Size(24, 21);
        descriptionPaste.TabIndex = 5;
        descriptionPaste.Click += OnDescriptionPaste;
        videoInfo.Name = "videoInfo";
        videoInfo.Location = new Point(416, 627);
        videoInfo.Size = new Size(405, 22);
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
        descriptionLabel.Text = "Description";
        descriptionLabel.Location = new Point(416, 197);
        descriptionLabel.AutoSize = true;
        Controls.Add(titleText);
        Controls.Add(originalTitleText);
        Controls.Add(yearText);
        Controls.Add(pathText);
        Controls.Add(saveButton);
        Controls.Add(wanted);
        Controls.Add(poster);
        Controls.Add(genres);
        Controls.Add(fileSize);
        Controls.Add(descriptionText);
        Controls.Add(descriptionPaste);
        Controls.Add(videoInfo);
        Controls.Add(titleLabel);
        Controls.Add(originalTitleLabel);
        Controls.Add(yearLabel);
        Controls.Add(genreLabel);
        Controls.Add(pathLabel);
        Controls.Add(descriptionLabel);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1025, 657);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        Icon = Resources.AriadnaDocumentaries;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        KeyPreview = true;
        Name = "DocumentaryDetailsForm";
        Load += OnLoad;
        FormClosed += OnFormClosed;
        ResumeLayout(false);
        PerformLayout();
    }
}
