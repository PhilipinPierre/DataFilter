using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.WinUI3.Controls;

/// <summary>
/// Hosts an optional <see cref="FilterBarControl"/> above grid content.
/// </summary>
public sealed class FilterGridChrome : UserControl
{
    private readonly FilterBarControl _filterBar = new();
    private readonly ContentPresenter _gridHost = new();
    private readonly FilterBarPopupService _popupService = new();

    public FilterGridChrome()
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(_filterBar, 0);
        Grid.SetRow(_gridHost, 1);
        root.Children.Add(_filterBar);
        root.Children.Add(_gridHost);
        Content = root;

        _filterBar.EditRequested += OnFilterBarEditRequested;
    }

    public IFilterableDataGridViewModel? GridViewModel
    {
        get => _filterBar.GridViewModel;
        set => _filterBar.GridViewModel = value;
    }

    public bool ShowFilterBar
    {
        get => _filterBar.ShowFilterBar;
        set => _filterBar.ShowFilterBar = value;
    }

    public void SetGridContent(UIElement content) => _gridHost.Content = content;

    private async void OnFilterBarEditRequested(object? sender, FilterBarEditRequest request)
    {
        if (_filterBar.GridViewModel == null)
            return;

        await _popupService.ShowAsync(_filterBar.GridViewModel, request, _filterBar);
    }
}
