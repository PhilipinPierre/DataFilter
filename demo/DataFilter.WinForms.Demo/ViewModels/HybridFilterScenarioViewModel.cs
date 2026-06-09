using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class HybridFilterScenarioViewModel : IDemoHeaderSettingsHost
{
    private readonly IMockEmployeeApiService _mockService;

    public DemoHeaderSettings HeaderSettings { get; }

    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public HybridFilterScenarioViewModel(IMockEmployeeApiService mockService, DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        _mockService = mockService;
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = _mockService
        };
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
