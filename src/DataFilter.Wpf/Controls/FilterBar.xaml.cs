using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.Wpf.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GridVm = DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Horizontal active-filters bar bound to <see cref="FilterBarViewModel"/>.
/// </summary>
public partial class FilterBar : UserControl
{
    private readonly FilterBarPopupService _popupService = new();

    public static readonly DependencyProperty FilterBarViewModelProperty =
        DependencyProperty.Register(nameof(FilterBarViewModel), typeof(FilterBarViewModel), typeof(FilterBar),
            new PropertyMetadata(null, OnFilterBarViewModelChanged));

    public static readonly DependencyProperty GridViewModelProperty =
        DependencyProperty.Register(nameof(GridViewModel), typeof(GridVm), typeof(FilterBar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowFilterBarProperty =
        DependencyProperty.Register(nameof(ShowFilterBar), typeof(bool), typeof(FilterBar),
            new PropertyMetadata(false, OnShowFilterBarChanged));

    public FilterBarViewModel? FilterBarViewModel
    {
        get => (FilterBarViewModel?)GetValue(FilterBarViewModelProperty);
        set => SetValue(FilterBarViewModelProperty, value);
    }

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

    public FilterBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private static void OnFilterBarViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterBar bar)
            bar.DataContext = e.NewValue;
    }

    private static void OnShowFilterBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterBar bar && e.NewValue is bool visible)
            bar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (FilterBarViewModel != null)
            FilterBarViewModel.EditRequested += OnEditRequested;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (FilterBarViewModel != null)
            FilterBarViewModel.EditRequested -= OnEditRequested;
    }

    private async void OnEditRequested(object? sender, FilterBarEditRequest request)
    {
        if (GridViewModel == null)
            return;

        FrameworkElement anchor = request.Anchor as FrameworkElement ?? this;
        await _popupService.ShowAsync(GridViewModel, request, anchor);
    }

    private void Chip_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not string nodeId || FilterBarViewModel == null)
            return;

        e.Handled = true;
        if (FilterBarViewModel.ToggleEnabledCommand.CanExecute(nodeId))
            FilterBarViewModel.ToggleEnabledCommand.Execute(nodeId);
    }
}
