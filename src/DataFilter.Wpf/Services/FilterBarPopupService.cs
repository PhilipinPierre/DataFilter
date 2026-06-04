using DataFilter.Core.Pipeline;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.Wpf.Controls;
using GridVm = DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel;
using ColumnFilterVm = DataFilter.Wpf.ViewModels.ColumnFilterViewModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;

namespace DataFilter.Wpf.Services;

/// <summary>
/// Opens the column filter popup from the active-filters bar.
/// </summary>
public sealed class FilterBarPopupService
{
    private Popup? _popup;
    private ColumnFilterVm? _columnVm;

    /// <summary>
    /// Shows the filter popup for a bar edit request.
    /// </summary>
    public async Task ShowAsync(
        GridVm grid,
        FilterBarEditRequest request,
        FrameworkElement placementTarget)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (placementTarget == null) throw new ArgumentNullException(nameof(placementTarget));

        string propertyName = ResolvePropertyName(grid, request);

        _columnVm = new ColumnFilterVm(
            search => grid.GetDistinctValuesAsync(propertyName, search),
            state => grid.ApplyBarCriterionAsync(request.NodeId, propertyName, state),
            () => _ = grid.RemoveBarNodeAsync(request.NodeId),
            propertyType: grid.GetPropertyType(propertyName));

        _columnVm.SetBarEditContext(new FilterBarEditContext
        {
            NodeId = request.NodeId,
            IsNew = request.IsNew,
            RemoveNodeOnClear = true,
            ApplyToPipelineAsync = state => grid.ApplyBarCriterionAsync(request.NodeId, propertyName, state),
            RemoveFromPipelineAsync = () => grid.RemoveBarNodeAsync(request.NodeId)
        });

        if (grid.PipelineSession.TryGetNode(request.NodeId, out FilterPipelineNode? node) && node is CriterionPipelineNode c)
            await _columnVm.LoadFromCriterionAsync(c);
        else
            await _columnVm.SearchCommand.ExecuteAsync(string.Empty);

        if (_popup == null)
            BuildPopup(placementTarget, propertyName);
        else
        {
            _popup.PlacementTarget = placementTarget;
            _popup.Child = new FilterPopup { DataContext = _columnVm };
        }

        _popup!.IsOpen = true;
    }

    public void Close()
    {
        if (_popup != null)
            _popup.IsOpen = false;
    }

    private void BuildPopup(FrameworkElement placementTarget, string propertyName)
    {
        _popup = new Popup
        {
            StaysOpen = true,
            AllowsTransparency = true,
            PlacementTarget = placementTarget,
            Placement = PlacementMode.Bottom,
            PopupAnimation = PopupAnimation.Fade
        };

        var filterControl = new FilterPopup { DataContext = _columnVm };
        AutomationProperties.SetAutomationId(filterControl, $"df-filter-popup-bar-{propertyName}");
        _popup.Child = filterControl;

        filterControl.CancelRequested += (_, _) => _popup.IsOpen = false;
        _columnVm!.OnApply += (_, _) => _popup.IsOpen = false;
        _columnVm.OnClear += (_, _) => _popup.IsOpen = false;
    }

    private static string ResolvePropertyName(GridVm grid, FilterBarEditRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.PropertyName))
            return request.PropertyName;

        if (grid.FilterableProperties.Count > 0)
            return grid.FilterableProperties.First();

        return string.Empty;
    }
}
