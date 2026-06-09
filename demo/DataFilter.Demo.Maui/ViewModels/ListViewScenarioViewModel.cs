using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class ListViewScenarioViewModel : ObservableObject, IDemoHeaderSettingsHost
{
    public DemoHeaderSettings HeaderSettings { get; }

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public ListViewScenarioViewModel(DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        _ = GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
