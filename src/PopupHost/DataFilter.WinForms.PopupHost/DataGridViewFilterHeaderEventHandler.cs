using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using System.ComponentModel;

namespace DataFilter.WinForms.Attach;

internal sealed class DataGridViewFilterHeaderEventHandler : IDisposable
{
    private readonly DataGridView _grid;
    private readonly Func<IFilterableDataGridViewModel?> _getViewModel;
    private readonly Func<DataGridViewFilterHeaderInteractions.Settings> _getSettings;
    private readonly ContextMenuStrip _popupHost = new();
    private int? _hoveredColumnIndex;
    private int? _focusedHeaderColumnIndex;
    private System.Windows.Forms.Timer? _longPressTimer;
    private int _longPressColumnIndex = -1;
    private ContextMenuStrip? _filterContextMenu;
    private bool _isDisposed;

    public DataGridViewFilterHeaderEventHandler(
        DataGridView grid,
        Func<IFilterableDataGridViewModel?> getViewModel,
        Func<DataGridViewFilterHeaderInteractions.Settings> getSettings)
    {
        _grid = grid;
        _getViewModel = getViewModel;
        _getSettings = getSettings;

        _grid.CellPainting += OnCellPainting;
        _grid.CellMouseClick += OnCellMouseClick;
        _grid.CellMouseDown += OnCellMouseDown;
        _grid.CellMouseUp += OnCellMouseUp;
        _grid.CellMouseMove += OnCellMouseMove;
        _grid.KeyDown += OnGridKeyDown;
        _grid.DataBindingComplete += OnDataBindingComplete;
    }

    private void OnDataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (_isDisposed)
            return;

        DataGridViewFilterHeaderInteractions.AppendHeaderGlyphs(_grid, _getSettings());
    }

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (_isDisposed || e.RowIndex != -1 || e.ColumnIndex < 0)
            return;

        DataGridViewFilterHeaderInteractions.PaintFilterHeader(
            e,
            _grid.Columns[e.ColumnIndex],
            _getSettings(),
            _getViewModel(),
            _hoveredColumnIndex);
    }

    private void OnCellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_isDisposed || e.RowIndex != -1 || e.ColumnIndex < 0)
            return;

        if (_hoveredColumnIndex != e.ColumnIndex)
        {
            var previous = _hoveredColumnIndex;
            _hoveredColumnIndex = e.ColumnIndex;
            if (previous is int p)
                _grid.InvalidateCell(p, -1);
            _grid.InvalidateCell(e.ColumnIndex, -1);
        }
    }

    private void OnCellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_isDisposed || e.RowIndex != -1 || e.ColumnIndex < 0)
            return;

        _focusedHeaderColumnIndex = e.ColumnIndex;
        var column = _grid.Columns[e.ColumnIndex];
        var mode = DataGridViewFilterHeaderInteractions.GetEffectiveTriggerMode(column, _getSettings());
        if (mode != ColumnFilterTriggerMode.HeaderLongPress)
            return;

        _longPressColumnIndex = e.ColumnIndex;
        _longPressTimer?.Stop();
        _longPressTimer?.Dispose();
        _longPressTimer = new System.Windows.Forms.Timer { Interval = ColumnFilterHeaderChrome.LongPressDurationMs };
        _longPressTimer.Tick += async (_, _) =>
        {
            _longPressTimer?.Stop();
            if (_longPressColumnIndex >= 0)
                await OpenFilterPopupAsync(_longPressColumnIndex);
        };
        _longPressTimer.Start();
    }

    private void OnCellMouseUp(object? sender, DataGridViewCellMouseEventArgs e)
    {
        _longPressTimer?.Stop();
        _longPressColumnIndex = -1;
    }

    private async void OnCellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (_isDisposed || e.RowIndex != -1 || e.ColumnIndex < 0)
            return;

        var viewModel = _getViewModel();
        if (viewModel == null)
            return;

        _focusedHeaderColumnIndex = e.ColumnIndex;
        var column = _grid.Columns[e.ColumnIndex];
        var settings = _getSettings();
        var headerRect = _grid.GetCellDisplayRectangle(e.ColumnIndex, -1, true);

        if (DataGridViewFilterHeaderInteractions.ShouldShowContextMenuFilter(e, column, settings))
        {
            var columnIndex = e.ColumnIndex;
            _filterContextMenu?.Dispose();
            _filterContextMenu = DataGridViewFilterHeaderInteractions.CreateFilterContextMenu(
                () => _ = OpenFilterPopupAsync(columnIndex));
            _filterContextMenu.Show(Cursor.Position);
            return;
        }

        if (!DataGridViewFilterHeaderInteractions.ShouldOpenFilterPopup(e, headerRect, column, settings, _hoveredColumnIndex))
            return;

        await OpenFilterPopupAsync(e.ColumnIndex);
    }

    private async void OnGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (_isDisposed || _getViewModel() == null || _focusedHeaderColumnIndex is not int columnIndex)
            return;

        var column = _grid.Columns[columnIndex];
        var mode = DataGridViewFilterHeaderInteractions.GetEffectiveTriggerMode(column, _getSettings());
        if (!DataGridViewFilterHeaderInteractions.TryHandleKeyboardShortcut(e, mode))
            return;

        e.Handled = true;
        await OpenFilterPopupAsync(columnIndex);
    }

    private async Task OpenFilterPopupAsync(int columnIndex)
    {
        var viewModel = _getViewModel();
        if (viewModel == null)
            return;

        var column = _grid.Columns[columnIndex];
        var headerRect = _grid.GetCellDisplayRectangle(columnIndex, -1, true);
        await DataGridViewFilterHeaderInteractions.ShowFilterPopupAsync(_grid, _popupHost, viewModel, column, headerRect);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        _grid.CellPainting -= OnCellPainting;
        _grid.CellMouseClick -= OnCellMouseClick;
        _grid.CellMouseDown -= OnCellMouseDown;
        _grid.CellMouseUp -= OnCellMouseUp;
        _grid.CellMouseMove -= OnCellMouseMove;
        _grid.KeyDown -= OnGridKeyDown;
        _grid.DataBindingComplete -= OnDataBindingComplete;

        _longPressTimer?.Stop();
        _longPressTimer?.Dispose();
        _filterContextMenu?.Dispose();
        _popupHost.Close();
        _popupHost.Dispose();
    }
}
