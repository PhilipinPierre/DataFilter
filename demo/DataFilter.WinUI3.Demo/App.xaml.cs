using Microsoft.Extensions.DependencyInjection;
using DataFilter.Demo.Shared;
using DataFilter.WinUI3.Demo.ViewModels;
using Microsoft.UI.Xaml;

namespace DataFilter.WinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            ServiceProvider = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Shared Services
            services.AddDataFilterDemoServices();

            // WinUI 3 ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LocalFilterScenarioViewModel>();
            services.AddTransient<AsyncFilterScenarioViewModel>();
            services.AddTransient<HybridFilterScenarioViewModel>();
            services.AddTransient<CustomizationScenarioViewModel>();
            services.AddTransient<ListViewScenarioViewModel>();
            services.AddTransient<CollectionViewScenarioViewModel>();

            // WinUI 3 Views
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = ServiceProvider.GetRequiredService<MainWindow>();
            _window.Activate();
        }
    }
}
