using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.Demo.Models;
using DataFilter.Wpf.Demo.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public partial class LocalFilterScenarioViewModel : ObservableObject
{
    public IFilterableDataGridViewModel<Employee> GridViewModel { get; }

    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    public LocalFilterScenarioViewModel()
    {
        Employees = EmployeeDataGenerator.Generate(100);

        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = Employees
        };
        GridViewModel.RefreshData();
    }
}
