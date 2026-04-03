using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class ListViewScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public ListViewScenarioViewModel()
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        _ = GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = EmployeeDataGenerator.Employees
        };
        GridViewModel.RefreshDataAsync();
    }
}
