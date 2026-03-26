using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.UwpXaml.Demo.Services;
using DataFilter.UwpXaml.ViewModels;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class AsyncFilterScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel;

    public void Regenerate(int count)
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = new MockEmployeeApiService(count)
        };
        GridViewModel.RefreshDataAsync();
    }
}
