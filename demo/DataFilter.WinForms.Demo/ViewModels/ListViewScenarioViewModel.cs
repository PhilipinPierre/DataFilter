using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class ListViewScenarioViewModel : IDemoHeaderSettingsHost
{
    public DemoHeaderSettings HeaderSettings { get; }

    public FilterableDataGridViewModel<Employee> GridViewModel { get; }

    public ListViewScenarioViewModel(DemoHeaderSettings headerSettings)
    {
        HeaderSettings = headerSettings;
        GridViewModel = new FilterableDataGridViewModel<Employee>();
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
