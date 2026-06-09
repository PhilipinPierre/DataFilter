using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class AsyncFilterScenarioViewModel : IDemoHeaderSettingsHost
{
    private readonly IMockEmployeeApiService _mockService;

    public DemoHeaderSettings HeaderSettings { get; }

    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public AsyncFilterScenarioViewModel(IMockEmployeeApiService mockService, DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        _mockService = mockService;
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = _mockService
        };
        GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        GridViewModel.RefreshDataAsync();
    }
}
