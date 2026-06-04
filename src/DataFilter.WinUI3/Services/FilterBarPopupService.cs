using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace DataFilter.WinUI3.Services;

/// <summary>
/// Opens the column filter popup from the active-filters bar (WinUI 3).
/// </summary>
public sealed class FilterBarPopupService
{
    private Flyout? _flyout;
    private FilterPopupControl? _popup;
    private ColumnFilterViewModel? _columnVm;
    private IFilterableDataGridViewModel? _grid;
    private FilterBarEditRequest? _request;

    public async Task ShowAsync(
        IFilterableDataGridViewModel grid,
        FilterBarEditRequest request,
        FrameworkElement placementTarget)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (placementTarget == null) throw new ArgumentNullException(nameof(placementTarget));
        if (string.IsNullOrWhiteSpace(request.PropertyName))
            throw new InvalidOperationException("Filter bar edit requires a column PropertyName.");

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

        _popup = new FilterPopupControl();
        _popup.Bind(_columnVm);
        _popup.CancelRequested += OnCancelRequested;
        _columnVm.OnApply += OnApplyOrClear;
        _columnVm.OnClear += OnApplyOrClear;

        _flyout = new Flyout
        {
            Content = _popup,
            Placement = FlyoutPlacementMode.Bottom
        };
        _flyout.Closed += (_, _) => Cleanup();
        _flyout.ShowAt(placementTarget);
    }

    public void Close() => _flyout?.Hide();

    private void OnCancelRequested(object? sender, EventArgs e) => _ = DismissAsync();

    private void OnApplyOrClear(object? sender, EventArgs e) => Close();

    private async Task DismissAsync()
    {
        if (_request?.IsNew == true && _grid != null)
            await _grid.RemoveBarNodeAsync(_request.NodeId);
        Close();
    }

    private void Cleanup()
    {
        if (_popup != null)
            _popup.CancelRequested -= OnCancelRequested;
        if (_columnVm != null)
        {
            _columnVm.OnApply -= OnApplyOrClear;
            _columnVm.OnClear -= OnApplyOrClear;
        }

        _columnVm?.SetBarEditContext(null);
        _popup = null;
        _flyout = null;
    }
}
