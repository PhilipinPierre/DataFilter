using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.Demo.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class AsyncFilterScenarioViewModel : ObservableObject, IDemoItem
{
    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;

    public AsyncFilterScenarioViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = new MockEmployeeApiService()
        };
        // Initial load
        GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        if (GridViewModel.AsyncDataProvider is MockEmployeeApiService mockService)
        {
            mockService.Regenerate(count);
            GridViewModel.RefreshDataAsync();
        }
    }
}
