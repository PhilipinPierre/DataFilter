using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using DataFilter.UwpXaml.Demo.ViewModels;
using DataFilter.UwpXaml.Demo.Pages;

namespace DataFilter.UwpXaml.Demo
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            LocalFilterFrame.Navigate(typeof(LocalFilterPage), ViewModel.LocalFilterScenario);
            AsyncFilterFrame.Navigate(typeof(AsyncFilterPage), ViewModel.AsyncFilterScenario);
            HybridFilterFrame.Navigate(typeof(HybridFilterPage), ViewModel.HybridFilterScenario);
            CustomizationFrame.Navigate(typeof(CustomizationPage), ViewModel.CustomizationScenario);
            ListViewFrame.Navigate(typeof(ListViewPage), ViewModel.ListViewScenario);
            CollectionViewFrame.Navigate(typeof(CollectionViewPage), ViewModel.CollectionViewScenario);
        }
    }
}
