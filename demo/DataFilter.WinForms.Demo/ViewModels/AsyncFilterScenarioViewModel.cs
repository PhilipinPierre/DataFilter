using DataFilter.Demo.Shared.Models;
using DataFilter.WinForms.Demo.Services;
using DataFilter.WinForms.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class AsyncFilterScenarioViewModel
{
    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public AsyncFilterScenarioViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = new MockEmployeeApiService()
        };
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
