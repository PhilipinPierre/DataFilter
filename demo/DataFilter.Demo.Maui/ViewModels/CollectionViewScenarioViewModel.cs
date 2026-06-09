using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class CollectionViewScenarioViewModel : ObservableObject, IDemoHeaderSettingsHost
{
    public DemoHeaderSettings HeaderSettings { get; }

    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel = new();

    public CollectionViewScenarioViewModel(DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
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
