using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class AsyncFilterScenarioViewModel : ObservableObject
{
    private readonly IMockEmployeeApiService _mockService;

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public AsyncFilterScenarioViewModel(IMockEmployeeApiService mockService)
    {
        _mockService = mockService;
        GridViewModel.AsyncDataProvider = _mockService;
        _ = GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        _mockService.Regenerate(count);
        GridViewModel.RefreshDataAsync();
    }
}
