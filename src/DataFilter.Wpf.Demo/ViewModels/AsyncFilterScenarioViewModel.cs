using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.Demo.Models;
using DataFilter.Wpf.Demo.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class AsyncFilterScenarioViewModel : ObservableObject
{
    public IFilterableDataGridViewModel<Employee> GridViewModel { get; }

    public AsyncFilterScenarioViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            AsyncDataProvider = new MockEmployeeApiService()
        };
        // Initial load
        GridViewModel.RefreshData();
    }
}
