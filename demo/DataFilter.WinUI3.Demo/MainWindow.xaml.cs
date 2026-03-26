using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataFilter.WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public DataFilter.WinUI3.Demo.ViewModels.MainViewModel ViewModel { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Local": ContentFrame.Navigate(typeof(DataFilter.WinUI3.Demo.Pages.LocalFilterPage), ViewModel.LocalFilterScenario); break;
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
