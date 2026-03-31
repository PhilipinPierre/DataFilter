using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.UwpXaml.ViewModels;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class HybridFilterScenarioViewModel : ObservableObject
{
    private readonly IMockEmployeeApiService _mockService;

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public HybridFilterScenarioViewModel(IMockEmployeeApiService mockService)
    {
        _mockService = mockService;
        GridViewModel.AsyncDataProvider = _mockService;
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        _ = GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
