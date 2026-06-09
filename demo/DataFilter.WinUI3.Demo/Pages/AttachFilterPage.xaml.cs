using System.ComponentModel;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinUI3.Attach;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DataFilter.WinUI3.Demo.Pages;

public sealed partial class AttachFilterPage : Page
{
    private ListViewFilterHeaderAdapter? _adapter;

    public ViewModels.LocalFilterScenarioViewModel ViewModel { get; private set; } = null!;

    public AttachFilterPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not ViewModels.LocalFilterScenarioViewModel vm)
            return;

        ViewModel = vm;
        ViewModel.HeaderSettings.PropertyChanged -= OnHeaderSettingsChanged;
        ViewModel.HeaderSettings.PropertyChanged += OnHeaderSettingsChanged;

        _adapter?.Dispose();
        _adapter = ListViewFilterHeaderAdapter.Attach(
            EmployeesList,
            ViewModel.GridViewModel,
            new ListViewFilterHeaderAdapter.Column("Id", "Id", 80),
            new ListViewFilterHeaderAdapter.Column("Name", "Name", 150),
            new ListViewFilterHeaderAdapter.Column("Department", "Department", 150),
            new ListViewFilterHeaderAdapter.Column("Country", "Country", 150));

        ApplyHeaderSettings();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.HeaderSettings.PropertyChanged -= OnHeaderSettingsChanged;
        base.OnNavigatedFrom(e);
    }

    private void OnHeaderSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DemoHeaderSettings.AreColumnFiltersEnabled)
            or nameof(DemoHeaderSettings.ColumnFilterTriggerMode))
        {
            ApplyHeaderSettings();
        }
    }

    private void ApplyHeaderSettings()
    {
        if (_adapter == null)
            return;

        _adapter.ApplyHeaderSettings(
            ViewModel.HeaderSettings.AreColumnFiltersEnabled,
            ViewModel.HeaderSettings.ColumnFilterTriggerMode);
    }
}
