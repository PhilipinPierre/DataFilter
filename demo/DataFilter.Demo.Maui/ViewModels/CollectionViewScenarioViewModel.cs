using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Maui.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

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
        Employees = new ObservableCollection<Employee>(EmployeeDataGenerator.Employees);
        GridViewModel.LocalDataSource = Employees;
        GridViewModel.RefreshDataAsync();
    }
}
