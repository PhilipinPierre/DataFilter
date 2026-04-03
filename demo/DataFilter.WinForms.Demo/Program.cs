using Microsoft.Extensions.DependencyInjection;
using DataFilter.Demo.Shared;
using DataFilter.WinForms.Demo.ViewModels;
using DataFilter.WinForms.Demo.Views;

namespace DataFilter.WinForms.Demo;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        var mainForm = serviceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // Shared Services
        services.AddDataFilterDemoServices();

        // ViewModels
        services.AddTransient<LocalFilterScenarioViewModel>();
        services.AddTransient<AsyncFilterScenarioViewModel>();
        services.AddTransient<HybridFilterScenarioViewModel>();
        services.AddTransient<CustomizationScenarioViewModel>();
        services.AddTransient<ListViewScenarioViewModel>();
        services.AddTransient<CollectionViewScenarioViewModel>();

        // Views
        services.AddTransient<LocalFilterView>();
        services.AddTransient<AsyncFilterView>();
        services.AddTransient<HybridFilterView>();
        services.AddTransient<CustomizationView>();
        services.AddTransient<ListViewFilterView>();
        services.AddTransient<CollectionViewFilterView>();

        // Main Form
        services.AddSingleton<MainForm>();
    }
}
