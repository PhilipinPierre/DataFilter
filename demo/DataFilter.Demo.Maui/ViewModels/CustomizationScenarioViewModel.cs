using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Maui.ViewModels;

namespace DataFilter.Maui.Demo.ViewModels;

public partial class CustomizationScenarioViewModel : ObservableObject
{
    [ObservableProperty]
    private FilterableDataGridViewModel<Employee> _gridViewModel;

    [ObservableProperty]
    private bool _isDarkTheme;

    public void Regenerate(int count)
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>
        {
            LocalDataSource = EmployeeDataGenerator.Employees
        };
        GridViewModel.RefreshDataAsync();
    }
}
