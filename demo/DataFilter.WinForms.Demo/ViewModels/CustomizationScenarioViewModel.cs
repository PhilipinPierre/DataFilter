using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.WinForms.ViewModels;

namespace DataFilter.WinForms.Demo.ViewModels;

public class CustomizationScenarioViewModel
{
    public FilterableDataGridViewModel<Employee> GridViewModel { get; }
    
    // In WinForms, we can just trigger an event or property change that the View listens to.
    private bool _isDarkTheme;
    public bool IsDarkTheme 
    { 
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme != value)
            {
                _isDarkTheme = value;
                IsDarkThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public event EventHandler? IsDarkThemeChanged;

    public CustomizationScenarioViewModel()
    {
        GridViewModel = new FilterableDataGridViewModel<Employee>();
        Regenerate(1000);
    }

    public void Regenerate(int count)
    {
        GridViewModel.LocalDataSource = EmployeeDataGenerator.Employees;
        GridViewModel.RefreshDataAsync();
    }
}
