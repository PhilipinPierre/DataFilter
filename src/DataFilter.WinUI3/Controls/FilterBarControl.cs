using DataFilter.Localization;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;

namespace DataFilter.WinUI3.Controls;

/// <summary>
/// WinUI 3 active-filters bar.
/// </summary>
public sealed class FilterBarControl : UserControl
{
    private readonly StackPanel _layout = new() { Orientation = Orientation.Horizontal, Spacing = 6 };
    private FilterBarViewModel? _viewModel;
    private IFilterableDataGridViewModel? _gridViewModel;

    public FilterBarControl()
    {
        Visibility = Visibility.Collapsed;
        AutomationProperties.SetName(this, "df-filter-bar");
        Content = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _layout
        };
    }

    public FilterBarViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != null)
            {
                _viewModel.EditRequested -= OnEditRequested;
                _viewModel.DisplayChanged -= OnDisplayChanged;
            }
            _viewModel = value;
            if (_viewModel != null)
            {
                _viewModel.EditRequested += OnEditRequested;
                _viewModel.DisplayChanged += OnDisplayChanged;
            }

            RebuildUi();
        }
    }

    public IFilterableDataGridViewModel? GridViewModel
    {
        get => _gridViewModel;
        set
        {
            if (_gridViewModel?.PipelineSession != null)
                _gridViewModel.PipelineSession.PipelineChanged -= OnPipelineChanged;
            _gridViewModel = value;
            if (_gridViewModel?.PipelineSession != null)
                _gridViewModel.PipelineSession.PipelineChanged += OnPipelineChanged;
            ViewModel = value?.FilterBar;
        }
    }

    public bool ShowFilterBar
    {
        get => Visibility == Visibility.Visible;
        set => Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public event EventHandler<FilterBarEditRequest>? EditRequested;

    private void OnPipelineChanged(object? sender, EventArgs e) => RebuildUi();

    private void RebuildUi()
    {
        _layout.Children.Clear();
        if (_viewModel == null)
            return;

        var orGroupBtn = new Button
        {
            Content = "OR+",
            Padding = new Thickness(8, 2, 8, 2)
        };
        ToolTipService.SetToolTip(orGroupBtn, LocalizationManager.Instance["FilterBar_AddOrGroup"]);
        AutomationProperties.SetName(orGroupBtn, "df-filter-bar-add-or-group");
        orGroupBtn.Click += (_, _) => _viewModel.AddOrGroupCommand.Execute(null);
        _layout.Children.Add(orGroupBtn);

        foreach (FilterBarDisplayItem segment in _viewModel.Segments)
        {
            if (segment is FilterBarOrSeparatorItem orSep)
            {
                var orDrop = new Border
                {
                    Padding = new Thickness(8, 4, 4, 4),
                    Child = new TextBlock { Text = orSep.Text, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold }
                };
                WireOrGapDrop(orDrop, orSep.OrInsertIndex);
                _layout.Children.Add(orDrop);
            }
            else if (segment is FilterBarAndClusterItem cluster)
            {
                var wrap = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4,
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(6),
                    CornerRadius = new CornerRadius(6),
                    AllowDrop = true
                };
                WireClusterDrop(wrap, cluster.AddAndAnchorNodeId);
                foreach (FilterBarChipItem chip in cluster.Chips)
                    wrap.Children.Add(CreateChip(chip));
                _layout.Children.Add(wrap);
            }
        }
    }

    private StackPanel CreateChip(FilterBarChipItem chip)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, CanDrag = true };
        row.DragStarting += (_, e) =>
        {
            e.Data.Properties[FilterBarDragFormats.CriterionNodeId] = chip.NodeId;
            e.Data.RequestedOperation = DataPackageOperation.Move;
        };
        var label = new Button { Content = chip.DisplayText, Padding = new Thickness(6, 2, 6, 2) };
        AutomationProperties.SetName(label, $"df-filter-bar-chip-{chip.NodeId}");
        label.Click += (_, _) => EditRequested?.Invoke(this, new FilterBarEditRequest { NodeId = chip.NodeId, PropertyName = chip.PropertyName });
        label.RightTapped += (_, _) => _viewModel?.ToggleEnabledCommand.Execute(chip.NodeId);
        row.Children.Add(label);
        var remove = new Button { Content = "×", Padding = new Thickness(4, 0, 4, 0) };
        remove.Click += (_, _) => _viewModel?.RemoveCommand.Execute(chip.NodeId);
        row.Children.Add(remove);
        return row;
    }

    private void OnEditRequested(object? sender, FilterBarEditRequest e) => EditRequested?.Invoke(this, e);

    private void OnDisplayChanged(object? sender, EventArgs e) => RebuildUi();

    private void WireClusterDrop(UIElement target, string anchorNodeId)
    {
        target.AllowDrop = true;
        target.DragOver += (_, e) =>
        {
            if (e.DataView.Properties.ContainsKey(FilterBarDragFormats.CriterionNodeId))
                e.AcceptedOperation = DataPackageOperation.Move;
        };
        target.Drop += (_, e) =>
        {
            if (_viewModel == null)
                return;

            string? nodeId = e.DataView.Properties[FilterBarDragFormats.CriterionNodeId] as string;
            if (string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(anchorNodeId))
                return;

            if (_viewModel.MoveToClusterCommand.CanExecute((nodeId, anchorNodeId)))
                _viewModel.MoveToClusterCommand.Execute((nodeId, anchorNodeId));
        };
    }

    private void WireOrGapDrop(UIElement target, int orInsertIndex)
    {
        target.AllowDrop = true;
        target.DragOver += (_, e) =>
        {
            if (e.DataView.Properties.ContainsKey(FilterBarDragFormats.CriterionNodeId))
                e.AcceptedOperation = DataPackageOperation.Move;
        };
        target.Drop += (_, e) =>
        {
            if (_viewModel == null)
                return;

            string? nodeId = e.DataView.Properties[FilterBarDragFormats.CriterionNodeId] as string;
            if (string.IsNullOrEmpty(nodeId))
                return;

            if (_viewModel.MoveToOrGapCommand.CanExecute((nodeId, orInsertIndex)))
                _viewModel.MoveToOrGapCommand.Execute((nodeId, orInsertIndex));
        };
    }
}
