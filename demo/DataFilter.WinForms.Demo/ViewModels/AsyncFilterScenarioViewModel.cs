using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class AsyncFilterScenarioViewModel
{
    private readonly IMockEmployeeApiService _mockService;

    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public AsyncFilterScenarioViewModel(IMockEmployeeApiService mockService)
    {
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
