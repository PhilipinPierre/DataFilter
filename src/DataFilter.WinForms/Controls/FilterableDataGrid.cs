using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Behaviors;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataFilter.WinForms.Controls;

public class FilterableDataGrid : DataGridView
{
    private readonly ContextMenuStrip _popupHost = new();

    public FilterableDataGrid()
    {
        EnableHeadersVisualStyles = false;
        CellPainting += OnCellPainting;
        CellMouseClick += OnCellMouseClick;
        DataBindingComplete += OnDataBindingComplete;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFilterableDataGridViewModel? ViewModel { get; set; }
    public DataFilter.Core.Abstractions.IFilterContext? FilterContext => ViewModel?.Context;

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex != -1 || e.ColumnIndex < 0) return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.All);
        var rect = new Rectangle(e.CellBounds.Right - 18, e.CellBounds.Top + 4, 14, 14);
        ControlPaint.DrawButton(e.Graphics, rect, ButtonState.Flat);
        using var font = new Font("Segoe UI", 8f, FontStyle.Regular);
        TextRenderer.DrawText(e.Graphics, "▼", font, rect, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        e.Handled = true;
    }

    private void OnDataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewColumn column in Columns)
        {
            if (!column.HeaderText.EndsWith(" \u25BE", StringComparison.Ordinal))
            {
                column.HeaderText += " \u25BE";
            }
        }
    }

    private async void OnCellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex != -1 || e.ColumnIndex < 0 || ViewModel == null) return;
        var headerRect = GetCellDisplayRectangle(e.ColumnIndex, -1, true);
        if (e.X < headerRect.Width - 20) return;

        var column = Columns[e.ColumnIndex];
        var propertyName = !string.IsNullOrWhiteSpace(column.DataPropertyName) ? column.DataPropertyName : column.Name;
        if (string.IsNullOrWhiteSpace(propertyName)) return;

        var popup = await FilterHeaderBehavior.CreatePopupAsync(ViewModel, propertyName);
        
        // Pass theme to popup
        bool isDark = BackgroundColor.R < 128;
        popup.ApplyTheme(isDark);

        popup.Width = 320;
        popup.Height = 420;
        popup.RequestClose += () =>
        {
            if (_popupHost.Visible) _popupHost.Close();
        };

        _popupHost.Items.Clear();
        var host = new ToolStripControlHost(popup) { AutoSize = false, Width = popup.Width, Height = popup.Height };
        _popupHost.Items.Add(host);
        _popupHost.Show(this, new Point(headerRect.Right - popup.Width, headerRect.Bottom));
    }
}
