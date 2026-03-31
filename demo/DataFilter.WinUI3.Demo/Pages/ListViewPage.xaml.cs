using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DataFilter.WinUI3.Behaviors;
using DataFilter.WinUI3.Controls;
using DataFilter.WinUI3.Demo.ViewModels;

namespace DataFilter.WinUI3.Demo.Pages;

public sealed partial class ListViewPage : Page
{
    public ListViewScenarioViewModel ViewModel { get; private set; }

    public ListViewPage()
    {
        this.InitializeComponent();
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string propertyName)
        {
            var popup = FilterHeaderBehavior.CreatePopup(ViewModel.GridViewModel, propertyName);
            var flyout = new Flyout { Content = popup };
            flyout.ShowAt(button);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ListViewScenarioViewModel vm)
        {
            ViewModel = vm;
        }
        base.OnNavigatedTo(e);
    }
}
