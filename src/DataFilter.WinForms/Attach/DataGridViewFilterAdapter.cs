using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Behaviors;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataFilter.WinForms.Attach;

/// <summary>
/// Attachable adapter enabling DataFilter column filtering on an existing <see cref="DataGridView"/>.
/// </summary>
public sealed class DataGridViewFilterAdapter : IDisposable
{
    private readonly DataGridView _grid;
    private readonly ContextMenuStrip _popupHost = new();
    private bool _isDisposed;

    public DataGridViewFilterAdapter(DataGridView grid, IFilterableDataGridViewModel viewModel)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        _grid.CellPainting += OnCellPainting;
        _grid.CellMouseClick += OnCellMouseClick;
        _grid.DataBindingComplete += OnDataBindingComplete;
    }

    public static DataGridViewFilterAdapter Attach(DataGridView grid, IFilterableDataGridViewModel viewModel)
        => new(grid, viewModel);

    [Browsable(false)]
    public IFilterableDataGridViewModel ViewModel { get; }

    /// <summary>
    /// If true, appends a ▼ glyph to the header text after binding.
    /// </summary>
    public bool AppendHeaderGlyph { get; set; } = true;

    /// <summary>
    /// Width reserved on the right side of the header for the filter button hit target.
    /// </summary>
    public int HeaderButtonHitWidth { get; set; } = 20;

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (_isDisposed) return;
        if (e.RowIndex != -1 || e.ColumnIndex < 0) return;
        if (e.Graphics == null) return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.All);
        var rect = new Rectangle(e.CellBounds.Right - 18, e.CellBounds.Top + 4, 14, 14);
        ControlPaint.DrawButton(e.Graphics, rect, ButtonState.Flat);
        using var font = new Font("Segoe UI", 8f, FontStyle.Regular);
        TextRenderer.DrawText(e.Graphics, "▼", font, rect, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        e.Handled = true;
    }

    private void OnDataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (_isDisposed) return;
        if (!AppendHeaderGlyph) return;

        foreach (DataGridViewColumn column in _grid.Columns)
        {
            if (!column.HeaderText.EndsWith(" \u25BE", StringComparison.Ordinal))
            {
                column.HeaderText += " \u25BE";
            }
        }
    }

    private async void OnCellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_isDisposed) return;
        if (e.RowIndex != -1 || e.ColumnIndex < 0) return;

        var headerRect = _grid.GetCellDisplayRectangle(e.ColumnIndex, -1, true);
        if (e.X < headerRect.Width - HeaderButtonHitWidth) return;

        var column = _grid.Columns[e.ColumnIndex];
        var propertyName = !string.IsNullOrWhiteSpace(column.DataPropertyName) ? column.DataPropertyName : column.Name;
        if (string.IsNullOrWhiteSpace(propertyName)) return;

        var popup = await FilterHeaderBehavior.CreatePopupAsync(ViewModel, propertyName);

        // Best-effort theme inference (keeps parity with the existing FilterableDataGrid control).
        bool isDark = _grid.BackgroundColor.R < 128;
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
        _popupHost.Show(_grid, new Point(headerRect.Right - popup.Width, headerRect.Bottom));
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _grid.CellPainting -= OnCellPainting;
        _grid.CellMouseClick -= OnCellMouseClick;
        _grid.DataBindingComplete -= OnDataBindingComplete;

        _popupHost.Close();
        _popupHost.Dispose();
    }
}

