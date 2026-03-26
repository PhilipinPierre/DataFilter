using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.UwpXaml.Demo.Services;
using DataFilter.UwpXaml.ViewModels;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class HybridFilterScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel;

    public void Regenerate(int count)
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = EmployeeDataGenerator.Employees,
            AsyncDataProvider = new MockEmployeeApiService(count) // Only used for distinct values in Hybrid
        };
        
        GridViewModel.RefreshDataAsync();
    }
}
