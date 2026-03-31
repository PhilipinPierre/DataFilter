using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Maui.Demo.Services;
using DataFilter.Maui.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class AsyncFilterScenarioViewModel : ObservableObject
{
    private readonly IMockEmployeeApiService _apiService;

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public AsyncFilterScenarioViewModel(IMockEmployeeApiService apiService)
    {
        _apiService = apiService;
    }

    public void Regenerate(int count)
    {
        GridViewModel.AsyncDataProvider = _apiService;
        GridViewModel.RefreshDataAsync();
    }
}
