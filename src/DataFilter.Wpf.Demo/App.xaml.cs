using DataFilter.Wpf.Demo.ViewModels;
using System.Windows;
using System.Windows.Markup;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataFilter.Wpf.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {

        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

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
