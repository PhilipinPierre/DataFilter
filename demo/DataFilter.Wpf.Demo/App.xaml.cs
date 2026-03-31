using DataFilter.Wpf.Demo.ViewModels;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DataFilter.Demo.Shared;
using System.Windows.Markup;

namespace DataFilter.Wpf.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        _serviceProvider = ConfigureServices();

        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Shared Services
        services.AddDataFilterDemoServices();

        // WPF ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LocalFilterScenarioViewModel>();
        services.AddTransient<AsyncFilterScenarioViewModel>();
        services.AddTransient<HybridFilterScenarioViewModel>();
        services.AddTransient<CustomizationScenarioViewModel>();
        services.AddTransient<ListViewScenarioViewModel>();
        services.AddTransient<CollectionViewScenarioViewModel>();

        // WPF Views
        services.AddSingleton<Views.MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR");

        // Load Default Themes
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/DataFilter.Wpf;component/Themes/Generic.xaml", UriKind.Absolute) });
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/DataFilter.Wpf;component/Themes/FilterLightTheme.xaml", UriKind.Absolute) });

        var mainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }


    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Type type = e.Exception.GetType();

        AppUnhandledException(e.Exception, false);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            AppUnhandledException(exception, false);
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        string exception = e.Exception.ToString();

        if (e.Exception is XamlParseException xamlParseException)
            AppUnhandledException(xamlParseException, true);
        else if (e.Exception.Source != "WpfBindingErrors")
            AppUnhandledException(e.Exception, true);
    }

    public static void AppUnhandledException(Exception ex, bool shouldShowNotification)
    {
        string error = ex.ToString();
        if (shouldShowNotification)
        {
            Task.Run(() => MessageBox.Show(error, ex.Message));
        }
    }
}
