using DataFilter.Demo.Shared.Services;
using DataFilter.Demo.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class AsyncFilterScenarioViewModel : ObservableObject, IDemoItem, IDemoHeaderSettingsHost
{
    private readonly IMockEmployeeApiService _mockService;

    public DemoHeaderSettings HeaderSettings { get; }

    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;

    public AsyncFilterScenarioViewModel(IMockEmployeeApiService mockService, DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        _mockService = mockService;
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = _mockService
        };
        // Initial load
        GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        GridViewModel.RefreshDataAsync();
    }
}
