using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.Demo.Services;
using DataFilter.WinForms.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class HybridFilterScenarioViewModel
{
    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public HybridFilterScenarioViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = new MockEmployeeApiService()
        };
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        
        if (GridViewModel.AsyncDataProvider is MockEmployeeApiService mockService)
        {
            mockService.Regenerate(count);
        }
        
        GridViewModel.RefreshDataAsync();
    }
}
