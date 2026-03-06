using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.Demo.Models;
using DataFilter.Wpf.Demo.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class HybridFilterScenarioViewModel : ObservableObject
{
    public IFilterableDataGridViewModel<Employee> GridViewModel { get; }

    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    public HybridFilterScenarioViewModel()
    {
        Employees = EmployeeDataGenerator.Generate(100);

        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = Employees,
            // Mock API service for fetching distincts, but filtering runs locally over LocalDataSource
            AsyncDataProvider = new MockEmployeeApiService()
        };
        GridViewModel.RefreshData();
    }
}
