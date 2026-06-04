using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Maui.Controls;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Services;

/// <summary>
/// Shows the column filter popup from the active-filters bar (MAUI overlay host).
/// </summary>
public sealed class FilterBarPopupService
{
    private BoxView? _overlay;
    private Frame? _frame;
    private ContentView? _container;
    private FilterPopupView? _popup;
    private ColumnFilterViewModel? _columnVm;
    private IFilterableDataGridViewModel? _grid;
    private FilterBarEditRequest? _request;

    public void AttachOverlay(BoxView overlay, Frame frame, ContentView container)
    {
        _overlay = overlay;
        _frame = frame;
        _container = container;
    }

    public async Task ShowAsync(
        IFilterableDataGridViewModel grid,
        FilterBarEditRequest request,
        VisualElement placementTarget)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (_overlay == null || _frame == null || _container == null)
            throw new InvalidOperationException("Call AttachOverlay before ShowAsync.");
        if (string.IsNullOrWhiteSpace(request.PropertyName))
            throw new InvalidOperationException("Filter bar edit requires a column PropertyName.");

        await HideAsync().ConfigureAwait(false);

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
        else if (_columnVm.SearchCommand.CanExecute(string.Empty))
            await _columnVm.SearchCommand.ExecuteAsync(string.Empty);

        _popup = new FilterPopupView();
        _popup.Bind(_columnVm);
        _popup.CloseRequested += OnPopupCloseRequested;
        _popup.CancelRequested += OnPopupCancelRequested;
        _container.Content = _popup;

        _overlay.IsVisible = true;
        _frame.IsVisible = true;
        _frame.Margin = new Thickness(12, 8, 0, 0);
        _frame.HorizontalOptions = LayoutOptions.Start;
        _frame.VerticalOptions = LayoutOptions.Start;
    }

    public Task HideAsync()
    {
        if (_popup != null)
        {
            _popup.CloseRequested -= OnPopupCloseRequested;
            _popup.CancelRequested -= OnPopupCancelRequested;
        }

        if (_overlay != null)
            _overlay.IsVisible = false;
        if (_frame != null)
            _frame.IsVisible = false;
        if (_container != null)
            _container.Content = null;

        _columnVm?.SetBarEditContext(null);
        _popup = null;
        _columnVm = null;
        _request = null;
        return Task.CompletedTask;
    }

    private void OnPopupCloseRequested(object? sender, EventArgs e) => _ = HideAsync();

    private async void OnPopupCancelRequested(object? sender, EventArgs e) => await CancelAsync();

    /// <summary>Dismisses the popup; removes a new bar draft when applicable.</summary>
    public async Task CancelAsync()
    {
        if (_request?.IsNew == true && _grid != null)
            await _grid.RemoveBarNodeAsync(_request.NodeId);
        await HideAsync().ConfigureAwait(false);
    }
}
