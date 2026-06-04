using DataFilter.Localization;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Controls;

/// <summary>
/// MAUI active-filters bar.
/// </summary>
public sealed class FilterBarView : ScrollView
{
    private readonly HorizontalStackLayout _layout = new() { Spacing = 6, Padding = new Thickness(4) };
    private FilterBarViewModel? _viewModel;
    public FilterBarView()
    {
        IsVisible = false;
        AutomationId = "df-filter-bar";
        Content = _layout;
        Orientation = ScrollOrientation.Horizontal;
    }

    public static readonly BindableProperty GridViewModelProperty =
        BindableProperty.Create(nameof(GridViewModel), typeof(IFilterableDataGridViewModel), typeof(FilterBarView),
            propertyChanged: OnGridViewModelChanged);

    public static readonly BindableProperty ShowFilterBarProperty =
        BindableProperty.Create(nameof(ShowFilterBar), typeof(bool), typeof(FilterBarView), false,
            propertyChanged: (bindable, _, newValue) =>
            {
                if (bindable is FilterBarView view && newValue is bool visible)
                    view.IsVisible = visible;
            });

    public IFilterableDataGridViewModel? GridViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(GridViewModelProperty);
        set => SetValue(GridViewModelProperty, value);
    }

    public bool ShowFilterBar
    {
        get => (bool)GetValue(ShowFilterBarProperty);
        set => SetValue(ShowFilterBarProperty, value);
    }

    public event EventHandler<FilterBarEditRequest>? EditRequested;

    private static void OnGridViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not FilterBarView view)
            return;

        if (oldValue is IFilterableDataGridViewModel oldGrid)
            oldGrid.PipelineSession.PipelineChanged -= view.OnPipelineChanged;

        if (view._viewModel != null)
        {
            view._viewModel.EditRequested -= view.OnEditRequested;
            view._viewModel.DisplayChanged -= view.OnDisplayChanged;
        }

        if (newValue is IFilterableDataGridViewModel newGrid)
        {
            newGrid.PipelineSession.PipelineChanged += view.OnPipelineChanged;
            view._viewModel = newGrid.FilterBar;
            if (view._viewModel != null)
            {
                view._viewModel.EditRequested += view.OnEditRequested;
                view._viewModel.DisplayChanged += view.OnDisplayChanged;
            }
        }

        view.RebuildUi();
    }

    private void OnPipelineChanged(object? sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(RebuildUi);

    private void OnDisplayChanged(object? sender, EventArgs e) => MainThread.BeginInvokeOnMainThread(RebuildUi);

    private void OnEditRequested(object? sender, FilterBarEditRequest e) => EditRequested?.Invoke(this, e);

    public void RebuildUi()
    {
        _layout.Clear();
        if (_viewModel == null)
            return;

        var orGroupBtn = new Button
        {
            Text = "OR+",
            AutomationId = "df-filter-bar-add-or-group"
        };
#if WINDOWS
        ToolTipProperties.SetText(orGroupBtn, LocalizationManager.Instance["FilterBar_AddOrGroup"]);
#endif
        orGroupBtn.Clicked += (_, _) => _viewModel?.AddOrGroupCommand.Execute(null);
        _layout.Add(orGroupBtn);

        foreach (FilterBarDisplayItem segment in _viewModel.Segments)
        {
            if (segment is FilterBarOrSeparatorItem orSep)
            {
                var orLabel = new Label { Text = orSep.Text, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
                WireOrGapDrop(orLabel, orSep.OrInsertIndex);
                _layout.Add(orLabel);
            }
            else if (segment is FilterBarAndClusterItem cluster)
            {
                var wrap = new HorizontalStackLayout { Spacing = 4 };
                if (cluster.CanAddAnd && !string.IsNullOrEmpty(cluster.AddAndAnchorNodeId))
                    wrap.Add(CreateAddButton(cluster.AddAndAnchorNodeId));
                foreach (FilterBarChipItem chip in cluster.Chips)
                    wrap.Add(CreateChip(chip));
                var border = new Border { Content = wrap, Stroke = Colors.LightGray, Padding = new Thickness(6) };
                WireClusterDrop(border, cluster.AddAndAnchorNodeId);
                _layout.Add(border);
            }
        }
    }

    private View CreateChip(FilterBarChipItem chip)
    {
        var row = new HorizontalStackLayout { Spacing = 2 };
        var drag = new DragGestureRecognizer();
        drag.DragStarting += (_, e) =>
        {
            e.Data.Properties[FilterBarDragFormats.CriterionNodeId] = chip.NodeId;
        };
        row.GestureRecognizers.Add(drag);
        var label = new Button
        {
            Text = chip.DisplayText,
            AutomationId = $"df-filter-bar-chip-{chip.NodeId}",
            Opacity = chip.IsEnabled ? 1 : 0.45
        };
        label.Clicked += (_, _) => EditRequested?.Invoke(this, new FilterBarEditRequest { NodeId = chip.NodeId, PropertyName = chip.PropertyName });
        var toggleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        toggleTap.Tapped += (_, _) => _viewModel?.ToggleEnabledCommand.Execute(chip.NodeId);
        label.GestureRecognizers.Add(toggleTap);
        var remove = new Button { Text = "×" };
        remove.Clicked += (_, _) => _viewModel?.RemoveCommand.Execute(chip.NodeId);
        row.Add(label);
        if (chip.CanAddAnd)
            row.Add(CreateAddButton(chip.NodeId));
        row.Add(remove);
        return row;
    }

    private Button CreateAddButton(string nodeId)
    {
        var btn = new Button { Text = "+", AutomationId = $"df-filter-bar-add-{nodeId}" };
        btn.Clicked += (_, _) => _viewModel?.AddAndCommand.Execute(nodeId);
        return btn;
    }

    private void WireClusterDrop(View target, string anchorNodeId)
    {
        var drop = new DropGestureRecognizer();
        drop.DragOver += (_, e) => e.AcceptedOperation = DataPackageOperation.Copy;
        drop.Drop += (_, e) =>
        {
            if (_viewModel == null
                || !e.Data.Properties.TryGetValue(FilterBarDragFormats.CriterionNodeId, out object? value)
                || value is not string nodeId
                || string.IsNullOrEmpty(anchorNodeId))
            {
                return;
            }

            if (_viewModel.MoveToClusterCommand.CanExecute((nodeId, anchorNodeId)))
                _viewModel.MoveToClusterCommand.Execute((nodeId, anchorNodeId));
        };
        target.GestureRecognizers.Add(drop);
    }

    private void WireOrGapDrop(View target, int orInsertIndex)
    {
        var drop = new DropGestureRecognizer();
        drop.DragOver += (_, e) => e.AcceptedOperation = DataPackageOperation.Copy;
        drop.Drop += (_, e) =>
        {
            if (_viewModel == null
                || !e.Data.Properties.TryGetValue(FilterBarDragFormats.CriterionNodeId, out object? value)
                || value is not string nodeId)
            {
                return;
            }

            if (_viewModel.MoveToOrGapCommand.CanExecute((nodeId, orInsertIndex)))
                _viewModel.MoveToOrGapCommand.Execute((nodeId, orInsertIndex));
        };
        target.GestureRecognizers.Add(drop);
    }
}
