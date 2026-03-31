using DataFilter.Demo.Shared.Services;
using DataFilter.Demo.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class HybridFilterScenarioViewModel : ObservableObject, IDemoItem
{
    private readonly IMockEmployeeApiService _mockService;

    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;
    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    public HybridFilterScenarioViewModel(IMockEmployeeApiService mockService)
    {
        _mockService = mockService;
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        Employees = DataFilter.Demo.Shared.Services.EmployeeDataGenerator.Employees;
        
        if (GridViewModel == null)
        {
            GridViewModel = new FilterableDataGridViewModel<Employee>
            {
                AsyncDataProvider = _mockService
            };
        }
        
        GridViewModel.LocalDataSource = Employees;
        GridViewModel.RefreshDataAsync();
    }
}
