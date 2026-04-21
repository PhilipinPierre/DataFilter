using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using DataFilter.Localization;

namespace DataFilter.WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public DataFilter.WinUI3.Demo.ViewModels.MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = App.ServiceProvider.GetRequiredService<DataFilter.WinUI3.Demo.ViewModels.MainViewModel>();

            var options = LocalizationManager.GetAvailableCultures()
                .Select(c => new LanguageOption(c))
                .ToList();

            LanguageCombo.ItemsSource = options;
            LanguageCombo.DisplayMemberPath = nameof(LanguageOption.Label);
            LanguageCombo.SelectedValuePath = nameof(LanguageOption.Culture);
            LanguageCombo.SelectedValue = LocalizationManager.Instance.Culture;
            LanguageCombo.SelectionChanged += (_, __) =>
            {
                if (LanguageCombo.SelectedValue is CultureInfo culture)
                    LocalizationManager.Instance.SetCulture(culture);
            };
            
            this.Activated += (s, e) => {
                if (NavView.SelectedItem == null)
                    NavView.SelectedItem = NavView.MenuItems[0];
            };
        }

        private sealed class LanguageOption
        {
            public LanguageOption(CultureInfo culture)
            {
                Culture = culture;
                Label = culture == CultureInfo.InvariantCulture ? "Default" : culture.NativeName;
            }

            public CultureInfo Culture { get; }
            public string Label { get; }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Local": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.LocalFilterPage), ViewModel.LocalFilterScenario); break;
                    case "Attach": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.AttachFilterPage)); break;
                    case "Async": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.AsyncFilterPage), ViewModel.AsyncFilterScenario); break;
                    case "Hybrid": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.HybridFilterPage), ViewModel.HybridFilterScenario); break;
                    case "Customization": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.CustomizationPage), ViewModel.CustomizationScenario); break;
                    case "ListView": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.ListViewPage), ViewModel.ListViewScenario); break;
                    case "CollectionView": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.CollectionViewPage), ViewModel.CollectionViewScenario); break;
                }
            }
        }
    }
}
