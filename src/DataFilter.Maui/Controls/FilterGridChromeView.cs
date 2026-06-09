using DataFilter.Maui.Services;
using DataFilter.Maui.Theming;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.Theming;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Controls;

/// <summary>
/// Hosts an optional <see cref="FilterBarView"/> above grid content with a built-in popup overlay.
/// </summary>
public sealed class FilterGridChromeView : Grid
{
    private readonly FilterBarView _filterBar = new();
    private readonly ContentView _gridHost = new();
    private readonly BoxView _overlay = new() { IsVisible = false };
    private readonly Frame _popupFrame = new()
    {
        IsVisible = false,
        HasShadow = true,
        CornerRadius = 8,
        Padding = 0
    };
    private readonly ContentView _popupContainer = new();
    private readonly FilterBarPopupService _popupService = new();

    public FilterGridChromeView()
    {
        ApplyOverlayTheme();
        FilterTheme.CurrentChanged += (_, _) => ApplyOverlayTheme();

        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

        _filterBar.SetValue(Grid.RowProperty, 0);
        _gridHost.SetValue(Grid.RowProperty, 1);
        Children.Add(_filterBar);
        Children.Add(_gridHost);

        _popupFrame.Content = _popupContainer;
        Children.Add(_overlay);
        Children.Add(_popupFrame);
        _overlay.SetValue(Grid.RowSpanProperty, 2);
        _popupFrame.SetValue(Grid.RowSpanProperty, 2);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _ = _popupService.CancelAsync();
        _overlay.GestureRecognizers.Add(tap);

        _popupService.AttachOverlay(_overlay, _popupFrame, _popupContainer);
        _filterBar.EditRequested += OnFilterBarEditRequested;
    }

    public static readonly BindableProperty GridViewModelProperty =
        BindableProperty.Create(nameof(GridViewModel), typeof(IFilterableDataGridViewModel), typeof(FilterGridChromeView),
            propertyChanged: (b, _, v) =>
            {
                if (b is FilterGridChromeView chrome)
                {
                    chrome._filterBar.GridViewModel = v as IFilterableDataGridViewModel;
                    chrome._filterBar.ShowFilterBar = chrome.ShowFilterBar;
                }
            });

    public static readonly BindableProperty ShowFilterBarProperty =
        BindableProperty.Create(nameof(ShowFilterBar), typeof(bool), typeof(FilterGridChromeView), false,
            propertyChanged: (b, _, v) =>
            {
                if (b is FilterGridChromeView chrome && v is bool visible)
                    chrome._filterBar.ShowFilterBar = visible;
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

    public void SetGridContent(View content) => _gridHost.Content = content;

    private async void OnFilterBarEditRequested(object? sender, FilterBarEditRequest request)
    {
        if (_filterBar.GridViewModel == null)
            return;

        await _popupService.ShowAsync(_filterBar.GridViewModel, request, _filterBar);
    }

    private void ApplyOverlayTheme() =>
        _overlay.BackgroundColor = FilterThemeApplier.ToMauiColor(FilterTheme.Current.OverlayBackground);
}
