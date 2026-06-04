using System.Windows;
using System.Windows.Controls;
using GridVm = DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Hosts an optional <see cref="FilterBar"/> above grid content.
/// </summary>
public partial class FilterGridChrome : UserControl
{
    public static readonly DependencyProperty GridViewModelProperty =
        DependencyProperty.Register(nameof(GridViewModel), typeof(GridVm), typeof(FilterGridChrome),
            new PropertyMetadata(null, OnGridViewModelChanged));

    public static readonly DependencyProperty ShowFilterBarProperty =
        DependencyProperty.Register(nameof(ShowFilterBar), typeof(bool), typeof(FilterGridChrome),
            new PropertyMetadata(false, OnShowFilterBarChanged));

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

    private static void OnGridViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FilterGridChrome chrome)
            return;

        chrome.FilterBarHost.GridViewModel = e.NewValue as GridVm;
    }

    private static void OnShowFilterBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterGridChrome chrome && e.NewValue is bool visible)
            chrome.FilterBarHost.ShowFilterBar = visible;
    }

    /// <summary>
    /// Sets the grid (or other content) displayed below the filter bar.
    /// </summary>
    public void SetGridContent(UIElement content) => GridHost.Content = content;
}
