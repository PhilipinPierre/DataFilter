using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Demo.ViewModels;

public sealed partial class CustomizationScenarioViewModel : ObservableObject, IDemoItem
{
    [ObservableProperty]
    private IFilterableDataGridViewModel<Employee> _gridViewModel;


    [ObservableProperty]
    private IEnumerable<Employee> _employees;

    [ObservableProperty]
    private bool _isDarkTheme;

    public CustomizationScenarioViewModel()
    {
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

    partial void OnIsDarkThemeChanged(bool value)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        // Determine which theme to load
        string themeName = value ? "FilterDarkTheme" : "FilterLightTheme";
        var uri = new System.Uri($"pack://application:,,,/DataFilter.Wpf;component/Themes/{themeName}.xaml");

        // Find and replace existing theme
        for (int i = 0; i < app.Resources.MergedDictionaries.Count; i++)
        {
            var dict = app.Resources.MergedDictionaries[i];
            if (dict.Source != null)
            {
                string sourceStr = dict.Source.OriginalString;
                if (sourceStr.Contains("FilterLightTheme.xaml") || sourceStr.Contains("FilterDarkTheme.xaml"))
                {
                    app.Resources.MergedDictionaries[i] = new System.Windows.ResourceDictionary { Source = uri };
                    return;
                }
            }
        }

        // Fallback: Add if not found
        app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = uri });
    }
}
