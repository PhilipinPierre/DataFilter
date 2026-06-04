using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Controls;

namespace DataFilter.WinForms.Services;

/// <summary>
/// Opens the column filter popup from the active-filters bar (WinForms).
/// </summary>
public sealed class FilterBarPopupService
{
    private Form? _form;
    private FilterPopupControl? _popup;
    private ColumnFilterViewModel? _columnVm;
    private IFilterableDataGridViewModel? _grid;
    private FilterBarEditRequest? _request;

    public async Task ShowAsync(
        IFilterableDataGridViewModel grid,
        FilterBarEditRequest request,
        Control placementTarget)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (placementTarget == null) throw new ArgumentNullException(nameof(placementTarget));
        if (string.IsNullOrWhiteSpace(request.PropertyName))
            throw new InvalidOperationException("Filter bar edit requires a column PropertyName.");

        Close();

        _grid = grid;
        _request = request;
        string propertyName = request.PropertyName;

        _columnVm = new ColumnFilterViewModel(
            search => grid.GetDistinctValuesAsync(propertyName, search),
            state => grid.ApplyBarCriterionAsync(request.NodeId, propertyName, state),
            () => _ = grid.RemoveBarNodeAsync(request.NodeId),
            isDesc => grid.ApplySort(propertyName, isDesc),
            isDesc => grid.AddSubSort(propertyName, isDesc),
            grid.GetPropertyType(propertyName));

        _columnVm.SetBarEditContext(new FilterBarEditContext
        {
            NodeId = request.NodeId,
            IsNew = request.IsNew,
            RemoveNodeOnClear = true,
            ApplyToPipelineAsync = state => grid.ApplyBarCriterionAsync(request.NodeId, propertyName, state),
            RemoveFromPipelineAsync = () => grid.RemoveBarNodeAsync(request.NodeId)
        });

        if (grid.PipelineSession.TryGetNode(request.NodeId, out FilterPipelineNode? node) && node is CriterionPipelineNode c)
        {
            ExcelFilterState? columnState = request.IsNew ? null : grid.GetColumnFilterState(propertyName);
            await _columnVm.LoadFromCriterionAsync(c, columnState);
        }
        else
            await _columnVm.SearchCommand.ExecuteAsync(string.Empty);

        var distinct = await grid.GetDistinctValuesAsync(propertyName, string.Empty);
        _popup = new FilterPopupControl();
        await _popup.BindAsync(_columnVm, distinct);
        _popup.RequestClose += OnPopupRequestClose;

        Point screen = placementTarget.PointToScreen(Point.Empty);
        _form = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            Width = 280,
            Height = 420,
            Location = screen,
            ShowInTaskbar = false
        };
        _popup.Dock = DockStyle.Fill;
        _form.Controls.Add(_popup);
        _form.Deactivate += (_, _) => _ = DismissAsync();
        _form.Show();
    }

    public void Close()
    {
        if (_form == null)
            return;

        _form.Close();
        _form.Dispose();
        _form = null;
        _popup = null;
        _columnVm?.SetBarEditContext(null);
        _columnVm = null;
    }

    private void OnPopupRequestClose() => Close();

    private async Task DismissAsync()
    {
        if (_request?.IsNew == true && _grid != null)
            await _grid.RemoveBarNodeAsync(_request.NodeId);
        Close();
    }
}
