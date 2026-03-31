using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinUI3.ViewModels;

namespace DataFilter.WinUI3.Demo.ViewModels;

public partial class HybridFilterScenarioViewModel : ObservableObject
{
    private readonly IMockEmployeeApiService _mockService;

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel;

    public HybridFilterScenarioViewModel(IMockEmployeeApiService mockService)
    {
        _mockService = mockService;
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = EmployeeDataGenerator.Employees,
            AsyncDataProvider = _mockService
        };
        GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
