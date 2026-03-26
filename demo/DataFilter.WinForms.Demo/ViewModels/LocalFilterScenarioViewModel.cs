using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class LocalFilterScenarioViewModel
{
    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public LocalFilterScenarioViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>();
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
