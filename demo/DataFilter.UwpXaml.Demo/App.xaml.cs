using Microsoft.Extensions.DependencyInjection;
using DataFilter.Demo.Shared;
using DataFilter.UwpXaml.Demo.ViewModels;
using Windows.UI.Xaml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DataFilter.UwpXaml.Demo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
    /// </summary>
    public sealed partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            ServiceProvider = ConfigureServices();
            Suspending += OnSuspending;
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Shared Services
            services.AddDataFilterDemoServices();

            // UWP ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LocalFilterScenarioViewModel>();
            services.AddSingleton<AsyncFilterScenarioViewModel>();
            services.AddSingleton<HybridFilterScenarioViewModel>();
            services.AddSingleton<CustomizationScenarioViewModel>();
            services.AddSingleton<ListViewScenarioViewModel>();
            services.AddSingleton<CollectionViewScenarioViewModel>();

            return services.BuildServiceProvider();
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (Window.Current.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails.
        /// </summary>
        /// <param name="sender">The Frame which failed navigation.</param>
        /// <param name="e">Details about the navigation failure.</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page '{e.SourcePageType.FullName}'.");
        }

        /// <summary>
        /// Invoked when application execution is being suspended. Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
