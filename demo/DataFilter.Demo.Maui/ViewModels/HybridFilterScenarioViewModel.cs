using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class HybridFilterScenarioViewModel : ObservableObject
{
    private readonly IMockEmployeeApiService _apiService;

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public HybridFilterScenarioViewModel(IMockEmployeeApiService apiService)
    {
        _apiService = apiService;
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.AsyncDataProvider = _apiService;
        GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _apiService.Regenerate(count);
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
