#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Ariadna.Properties;
using Ariadna.Storage;
using Microsoft.Extensions.Logging;

namespace Ariadna.AuxiliaryPopups;

// Coordinates dialog commands and persistence without owning collection controls.
internal sealed class EntryEditorSession : Component
{
    private readonly Form form;
    private readonly Button saveButton;
    private readonly CatalogKind kind;
    private readonly ILogger logger;
    private readonly Func<bool> save;
    private readonly Func<bool> dismissPicker;
    private bool shiftPressed;
    private bool saving;

    internal EntryEditorSession(IContainer components, Form form, Button saveButton, CatalogKind kind,
        ILogger logger, Func<bool> save, Func<bool> dismissPicker)
    {
        components.Add(this);
        this.form = form;
        this.saveButton = saveButton;
        this.kind = kind;
        this.logger = logger;
        this.save = save;
        this.dismissPicker = dismissPicker;
        saveButton.Click += OnSaveClick;
        form.KeyDown += OnKeyDown;
        form.KeyUp += OnKeyUp;
        form.Deactivate += OnDeactivate;
    }

    internal CatalogDetails? Loaded { get; private set; }
    internal int StoredDbEntryId { get; private set; } = -1;
    internal Utilities.EFormCloseReason FormCloseReason { get; private set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal string FilePath { get; set; } = string.Empty;
    internal CatalogStore Store => CatalogServices.CreateStore();
    internal string PosterRoot => CatalogServices.GetPosterRoot(kind);

    internal void Load(string path)
    {
        FilePath = path;
        StoredDbEntryId = Store.FindId(kind, path);
        Loaded = StoredDbEntryId == -1 ? null : Store.GetDetails(kind, StoredDbEntryId);
        UpdateCaption();
    }

    internal CatalogEntry CreateEntry(string title, string originalTitle, int year, string path, bool wanted)
    {
        return new CatalogEntry
        {
            Id = StoredDbEntryId,
            Title = title.Trim(),
            OriginalTitle = originalTitle.Trim(),
            Year = year,
            Path = path.Trim(),
            CreationDate = Loaded != null ? Loaded.Entry.CreationDate : DateOnly.FromDateTime(File.GetLastWriteTimeUtc(path)),
            Wanted = PreserveFlag(Loaded?.Entry.Wanted, wanted),
        };
    }

    internal bool? PreserveFlag(bool? previous, bool current)
        => Loaded != null && previous.GetValueOrDefault() == current ? previous : current;

    internal string? PreserveDescription(string text)
        => Loaded != null && text == Utilities.DecorateDescription(Loaded.Entry.Description ?? string.Empty)
            ? Loaded.Entry.Description : text;

    internal bool Save(CatalogDetails details, IReadOnlyDictionary<string, byte[]> images)
    {
        StoredDbEntryId = Store.Save(kind, details, new CatalogAssets(PosterRoot, images));
        details.Entry.Id = StoredDbEntryId;
        Loaded = details;
        return true;
    }

    private void UpdateCaption()
    {
        form.Text = StoredDbEntryId == -1 ? Resources.InsertEntry : Resources.UpdateEntry;
        saveButton.Text = shiftPressed ? Resources.Ignore : StoredDbEntryId == -1 ? Resources.Insert : Resources.Update;
    }

    private void OnSaveClick(object? sender, EventArgs e) => Confirm();

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        shiftPressed = e.Shift;
        UpdateCaption();
        if (e.KeyCode == Keys.Escape)
        {
            if (!dismissPicker())
            {
                form.Close();
            }
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Enter && !IsEditingText(form.ActiveControl))
        {
            Confirm();
            e.SuppressKeyPress = true;
        }
    }

    private static bool IsEditingText(Control? control)
    {
        while (control is ContainerControl container && container.ActiveControl != null)
        {
            control = container.ActiveControl;
        }
        return control is TextBox { Multiline: true } || control is ListView { LabelEdit: true, Focused: true };
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        shiftPressed = e.Shift;
        UpdateCaption();
    }

    private void OnDeactivate(object? sender, EventArgs e)
    {
        shiftPressed = false;
        UpdateCaption();
    }

    private void Confirm()
    {
        if (saving)
        {
            return;
        }
        saving = true;
        saveButton.Enabled = false;
        form.UseWaitCursor = true;
        try
        {
            if (shiftPressed)
            {
                Store.Ignore(FilePath);
                form.Close();
            }
            else if (save())
            {
                FormCloseReason = Utilities.EFormCloseReason.SUCCESS;
                form.Close();
            }
        }
        catch (Exception exception)
        {
            logger.LogError("Failed to save '{Collection}' entry '{Id}', error type '{ErrorType}'", kind, StoredDbEntryId, exception.GetType().Name);
            MessageBox.Show(form, exception.Message, Resources.FailedToSaveEntry, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            saving = false;
            if (!form.IsDisposed)
            {
                saveButton.Enabled = true;
                form.UseWaitCursor = false;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            saveButton.Click -= OnSaveClick;
            form.KeyDown -= OnKeyDown;
            form.KeyUp -= OnKeyUp;
            form.Deactivate -= OnDeactivate;
        }
        base.Dispose(disposing);
    }
}
