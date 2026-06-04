using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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
                _viewModel.EditRequested -= OnEditRequested;
            _viewModel = value;
            if (_viewModel != null)
                _viewModel.EditRequested += OnEditRequested;
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

        foreach (FilterBarDisplayItem segment in _viewModel.Segments)
        {
            if (segment is FilterBarOrSeparatorItem orSep)
            {
                _layout.Children.Add(new TextBlock { Text = orSep.Text, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(8, 0, 4, 0) });
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
                    CornerRadius = new CornerRadius(6)
                };
                if (cluster.CanAddAnd && !string.IsNullOrEmpty(cluster.AddAndAnchorNodeId))
                    wrap.Children.Add(CreateAddButton(cluster.AddAndAnchorNodeId));
                foreach (FilterBarChipItem chip in cluster.Chips)
                    wrap.Children.Add(CreateChip(chip));
                _layout.Children.Add(wrap);
            }
        }
    }

    private StackPanel CreateChip(FilterBarChipItem chip)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        var label = new Button { Content = chip.DisplayText, Padding = new Thickness(6, 2, 6, 2) };
        AutomationProperties.SetName(label, $"df-filter-bar-chip-{chip.NodeId}");
        label.Click += (_, _) => EditRequested?.Invoke(this, new FilterBarEditRequest { NodeId = chip.NodeId, PropertyName = chip.PropertyName });
        label.RightTapped += (_, _) => _viewModel?.ToggleEnabledCommand.Execute(chip.NodeId);
        row.Children.Add(label);
        if (chip.CanAddAnd)
            row.Children.Add(CreateAddButton(chip.NodeId));
        var remove = new Button { Content = "×", Padding = new Thickness(4, 0, 4, 0) };
        remove.Click += (_, _) => _viewModel?.RemoveCommand.Execute(chip.NodeId);
        row.Children.Add(remove);
        return row;
    }

    private Button CreateAddButton(string nodeId)
    {
        var btn = new Button { Content = "+", Padding = new Thickness(4, 0, 4, 0) };
        AutomationProperties.SetName(btn, $"df-filter-bar-add-{nodeId}");
        btn.Click += (_, _) => _viewModel?.AddAndCommand.Execute(nodeId);
        return btn;
    }

    private void OnEditRequested(object? sender, FilterBarEditRequest e) => EditRequested?.Invoke(this, e);
}
