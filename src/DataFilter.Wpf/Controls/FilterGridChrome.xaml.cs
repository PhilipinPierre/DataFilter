using System.Windows;
using System.Windows.Controls;
using DataFilter.PlatformShared.ViewModels;
using GridVm = DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Hosts an optional <see cref="FilterBar"/> above a grid (or other content).
/// </summary>
public partial class FilterGridChrome : UserControl
{
    public static readonly DependencyProperty GridViewModelProperty =
        DependencyProperty.Register(nameof(GridViewModel), typeof(GridVm), typeof(FilterGridChrome), new PropertyMetadata(null));

    public static readonly DependencyProperty ShowFilterBarProperty =
        DependencyProperty.Register(nameof(ShowFilterBar), typeof(bool), typeof(FilterGridChrome), new PropertyMetadata(false));

    public GridVm? GridViewModel
    {
        get => (GridVm?)GetValue(GridViewModelProperty);
        set => SetValue(GridViewModelProperty, value);
    }

    public bool ShowFilterBar
    {
        get => (bool)GetValue(ShowFilterBarProperty);
        set => SetValue(ShowFilterBarProperty, value);
    }

    public FilterGridChrome()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the grid (or other content) displayed below the filter bar.
    /// </summary>
    public void SetGridContent(UIElement content) => GridHost.Content = content;
}
