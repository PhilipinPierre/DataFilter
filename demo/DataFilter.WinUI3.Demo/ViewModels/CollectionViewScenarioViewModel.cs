using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinUI3.Demo.ViewModels;

public partial class CollectionViewScenarioViewModel : ObservableObject, IDemoHeaderSettingsHost
{
    public DemoHeaderSettings HeaderSettings { get; }

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();
    
    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    public CollectionViewScenarioViewModel(DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        Employees = new ObservableCollection<Employee>(EmployeeDataGenerator.Employees);
        GridViewModel.LocalDataSource = Employees;
        _ = GridViewModel.RefreshDataAsync();
    }

    public void Regenerate(int count)
    {
        var newEmployees = new ObservableCollection<Employee>(EmployeeDataGenerator.Employees);
        Employees.Clear();
        foreach (var emp in newEmployees) Employees.Add(emp);
        
        GridViewModel.LocalDataSource = Employees;
        _ = GridViewModel.RefreshDataAsync();
    }
}
