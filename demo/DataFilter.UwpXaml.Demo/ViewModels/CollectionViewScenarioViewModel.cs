using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.UwpXaml.ViewModels;

namespace DataFilter.UwpXaml.Demo.ViewModels;

public partial class CollectionViewScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();
    
    public CollectionViewScenarioViewModel()
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        _ = GridViewModel.RefreshDataAsync();
    }
    
    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    public void Regenerate(int count)
    {
        var newEmployees = new ObservableCollection<Employee>(EmployeeDataGenerator.Employees);
        Employees.Clear();
        foreach (var emp in newEmployees) Employees.Add(emp);
        
        GridViewModel.LocalDataSource = Employees;
        _ = GridViewModel.RefreshDataAsync();
    }
}
