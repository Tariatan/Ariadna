#nullable enable
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Ariadna.AuxiliaryPopups;

// Paint the local content before revealing the window; asynchronous inspection stays independent.
internal sealed class DetailFormPresentation : Component
{
    private readonly Form form;
    private readonly double opacity;
    private bool stopped;

    internal DetailFormPresentation(IContainer components, Form form)
    {
        this.form = form;
        opacity = form.Opacity;
        components.Add(this);
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }
        form.Opacity = 0;
        form.Shown += OnShown;
        form.FormClosed += OnFormClosed;
    }

    private void OnShown(object? sender, EventArgs e)
    {
        form.Shown -= OnShown;
        form.BeginInvoke((Action)Reveal);
    }

    private void Reveal()
    {
        if (stopped || form.IsDisposed || form.Disposing || !form.Visible)
        {
            return;
        }
        try
        {
            // Refresh also paints the child windows while the form is still transparent.
            form.Refresh();
        }
        finally
        {
            if (!stopped && !form.IsDisposed && !form.Disposing)
            {
                form.Opacity = opacity;
            }
        }
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e) => stopped = true;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            stopped = true;
            form.Shown -= OnShown;
            form.FormClosed -= OnFormClosed;
        }
        base.Dispose(disposing);
    }
}
