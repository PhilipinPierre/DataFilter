using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinUI3.ViewModels;

namespace DataFilter.WinUI3.Demo.ViewModels;

public partial class ListViewScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel;

    public void Regenerate(int count)
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = EmployeeDataGenerator.Employees
        };
        GridViewModel.RefreshDataAsync();
    }
}
