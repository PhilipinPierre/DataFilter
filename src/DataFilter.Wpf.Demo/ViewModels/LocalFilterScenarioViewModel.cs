using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Wpf.Demo.Models;
using DataFilter.Wpf.Demo.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public partial class LocalFilterScenarioViewModel : ObservableObject, IDemoItem
{
    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;

    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    public LocalFilterScenarioViewModel()
    {
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        Employees = EmployeeDataGenerator.Employees;
        if (GridViewModel == null)
        {
            GridViewModel = new FilterableDataGridViewModel<Employee>();
        }
        GridViewModel.LocalDataSource = Employees;
        GridViewModel.RefreshDataAsync();
    }
}
