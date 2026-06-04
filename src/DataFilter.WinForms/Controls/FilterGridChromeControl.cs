using System.ComponentModel;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Services;

namespace DataFilter.WinForms.Controls;

/// <summary>
/// Stacks <see cref="FilterBarControl"/> above grid content.
/// </summary>
public sealed class FilterGridChromeControl : Panel
{
    private readonly FilterBarControl _filterBar = new();
    private readonly FilterBarPopupService _popupService = new();

    public FilterGridChromeControl()
    {
        Dock = DockStyle.Fill;
        _filterBar.Dock = DockStyle.Top;
        _filterBar.OnEditRequestedHandler = OnFilterBarEditRequested;
        Controls.Add(_filterBar);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFilterableDataGridViewModel? GridViewModel
    {
        get => _filterBar.GridViewModel;
        set
        {
            _filterBar.GridViewModel = value;
            _filterBar.ViewModel = value?.FilterBar;
        }
    }

    [DefaultValue(false)]
    public bool ShowFilterBar
    {
        get => _filterBar.ShowFilterBar;
        set => _filterBar.ShowFilterBar = value;
    }

    public FilterBarControl FilterBar => _filterBar;

    public void SetGridContent(Control grid)
    {
        grid.Dock = DockStyle.Fill;
        Controls.Add(grid);
        grid.BringToFront();
    }

    private async void OnFilterBarEditRequested(FilterBarEditRequest request)
    {
        if (_filterBar.GridViewModel == null)
            return;

        await _popupService.ShowAsync(_filterBar.GridViewModel, request, _filterBar);
    }
}
