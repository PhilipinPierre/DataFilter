using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class AsyncFilterScenarioViewModel : ObservableObject, IDemoHeaderSettingsHost
{
    private readonly IMockEmployeeApiService _apiService;

    public DemoHeaderSettings HeaderSettings { get; }

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public AsyncFilterScenarioViewModel(IMockEmployeeApiService apiService, DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        _apiService = apiService;
        GridViewModel.AsyncDataProvider = _apiService;
        GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _apiService.Regenerate(count);
        GridViewModel.RefreshDataAsync();
    }
}
