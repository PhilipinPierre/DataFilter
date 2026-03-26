using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinUI3.ViewModels;

namespace DataFilter.WinUI3.Demo.ViewModels;

public partial class CollectionViewScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel;
    
    [ObservableProperty]
    private ObservableCollection<Employee> _employees;

    public void Regenerate(int count)
    {
        Employees = new ObservableCollection<Employee>(EmployeeDataGenerator.Employees);
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = Employees
        };
        GridViewModel.RefreshDataAsync();
    }
}
