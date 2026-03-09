using DataFilter.Wpf.Demo.ViewModels;
using System.Windows;

namespace DataFilter.Wpf.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR");

        // Load Default Themes
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/DataFilter.Wpf;component/Themes/Generic.xaml", UriKind.Absolute) });
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml", UriKind.Absolute) });

        var mainWindow = new Views.MainWindow
        {
            DataContext = new MainViewModel()
        };
        mainWindow.Show();
    }
}
