using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.Demo.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class HybridFilterScenarioViewModel : ObservableObject, IDemoItem
{

    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;
    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    public HybridFilterScenarioViewModel()
    {
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        Employees = DataFilter.Demo.Shared.Services.EmployeeDataGenerator.Employees;
        if (GridViewModel == null)
        {
            GridViewModel = new FilterableDataGridViewModel<Employee>
            {
                AsyncDataProvider = new MockEmployeeApiService()
            };
        }
        
        GridViewModel.LocalDataSource = Employees;
        
        if (GridViewModel.AsyncDataProvider is MockEmployeeApiService mockService)
        {
            mockService.Regenerate(count);
        }
        
        GridViewModel.RefreshDataAsync();
    }
}
