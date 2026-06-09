using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.Wpf.Controls;
using GridVm = DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel;
using ColumnFilterVm = DataFilter.Wpf.ViewModels.ColumnFilterViewModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using System.Windows.Input;

namespace DataFilter.Wpf.Services;

/// <summary>
/// Opens the column filter popup from the active-filters bar.
/// </summary>
public sealed class FilterBarPopupService
{
    private Popup? _popup;
    private ColumnFilterVm? _columnVm;
    private FilterPopup? _filterControl;
    private GridVm? _grid;
    private FilterBarEditRequest? _request;

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

        if (string.IsNullOrWhiteSpace(request.PropertyName))
            throw new InvalidOperationException("Filter bar edit requires a column PropertyName.");

        _grid = grid;
        _request = request;
        string propertyName = request.PropertyName;

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
        {
            ExcelFilterState? columnState = request.IsNew ? null : grid.GetColumnFilterState(propertyName);
            await _columnVm.LoadFromCriterionAsync(c, columnState);
        }
        else
            await _columnVm.SearchCommand.ExecuteAsync(string.Empty);

        if (_popup == null)
            CreatePopupShell(placementTarget);
        else
            _popup.PlacementTarget = placementTarget;

        var filterControl = new FilterPopup { DataContext = _columnVm };
        AutomationProperties.SetAutomationId(filterControl, $"df-filter-popup-bar-{propertyName}");
        _popup!.Child = filterControl;
        AttachHandlers(filterControl);

        _popup.IsOpen = true;
    }

    public void Close()
    {
        if (_popup != null)
            _popup.IsOpen = false;
    }

    private void CreatePopupShell(FrameworkElement placementTarget)
    {
        _popup = new Popup
        {
            StaysOpen = true,
            AllowsTransparency = true,
            PlacementTarget = placementTarget,
            Placement = PlacementMode.Bottom,
            PopupAnimation = PopupAnimation.Fade
        };
        _popup.Opened += OnPopupOpened;
        _popup.Closed += OnPopupClosed;
    }

    private void AttachHandlers(FilterPopup filterControl)
    {
        DetachHandlers();
        _filterControl = filterControl;
        filterControl.CancelRequested += OnCancelRequested;
        _columnVm!.OnApply += OnApplyOrClear;
        _columnVm.OnClear += OnApplyOrClear;
    }

    private void DetachHandlers()
    {
        if (_filterControl != null)
            _filterControl.CancelRequested -= OnCancelRequested;

        if (_columnVm != null)
        {
            _columnVm.OnApply -= OnApplyOrClear;
            _columnVm.OnClear -= OnApplyOrClear;
        }

        _filterControl = null;
    }

    private void OnCancelRequested(object? sender, EventArgs e) => _ = DismissAsync();

    private void OnApplyOrClear(object? sender, EventArgs e) => Close();

    private async Task DismissAsync()
    {
        if (_request?.IsNew == true && _grid != null)
            await _grid.RemoveBarNodeAsync(_request.NodeId);
        Close();
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        Window? window = Window.GetWindow(_popup?.PlacementTarget);
        if (window != null)
            window.PreviewMouseDown += OnWindowMouseDown;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        Window? window = Window.GetWindow(_popup?.PlacementTarget);
        if (window != null)
            window.PreviewMouseDown -= OnWindowMouseDown;
        DetachHandlers();
        _columnVm?.SetBarEditContext(null);
    }

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_popup is not { IsOpen: true, Child: UIElement child })
            return;

        double w = child.RenderSize.Width;
        double h = child.RenderSize.Height;
        if (child is FrameworkElement fe)
        {
            if (w <= 0) w = fe.ActualWidth;
            if (h <= 0) h = fe.ActualHeight;
        }

        if (w <= 0 || h <= 0)
            return;

        if (!new Rect(0, 0, w, h).Contains(e.GetPosition(child)))
            _ = DismissAsync();
    }
}
