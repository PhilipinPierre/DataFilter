using System.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class CollectionViewScenarioViewModel
{
    public FilterableDataGridViewModel<Employee> GridViewModel { get; }
    public BindingSource BindingSource { get; }

    public CollectionViewScenarioViewModel()
    {
        BindingSource = new BindingSource();
        GridViewModel = new FilterableDataGridViewModel<Employee>();
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        var employees = new BindingList<Employee>(EmployeeDataGenerator.Employees);
        BindingSource.DataSource = employees;
        
        GridViewModel.LocalDataSource = employees;
        GridViewModel.RefreshDataAsync();
    }
}
