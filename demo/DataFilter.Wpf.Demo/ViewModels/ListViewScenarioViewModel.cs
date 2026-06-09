using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class ListViewScenarioViewModel : ObservableObject, IDemoItem, IDemoHeaderSettingsHost
{
    public DemoHeaderSettings HeaderSettings { get; }

    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;
    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    public ListViewScenarioViewModel(DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
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
