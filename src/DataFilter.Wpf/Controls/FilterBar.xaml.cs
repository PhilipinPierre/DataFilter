using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.Wpf.Services;
using GridVm = DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

    private void OnLoaded(object sender, RoutedEventArgs e) => SubscribeEditRequested();

    private void OnUnloaded(object sender, RoutedEventArgs e) => UnsubscribeEditRequested();

    private void SubscribeEditRequested()
    {
        if (FilterBarViewModel == null)
            return;

        FilterBarViewModel.EditRequested -= OnEditRequested;
        FilterBarViewModel.EditRequested += OnEditRequested;
    }

    private void UnsubscribeEditRequested()
    {
        if (FilterBarViewModel == null)
            return;

        FilterBarViewModel.EditRequested -= OnEditRequested;
    }

    private async void OnEditRequested(object? sender, FilterBarEditRequest request)
    {
        if (GridViewModel == null)
            return;

        FrameworkElement anchor = request.Anchor as FrameworkElement ?? this;
        await _popupService.ShowAsync(GridViewModel, request, anchor);
    }

    private static void OnFilterBarViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FilterBar bar)
            return;

        if (e.OldValue is FilterBarViewModel oldVm)
            oldVm.EditRequested -= bar.OnEditRequested;

        bar.DataContext = e.NewValue;
        bar.SubscribeEditRequested();
    }

    private static void OnShowFilterBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterBar bar && e.NewValue is bool visible)
            bar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
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
