using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Behaviors;
using DataFilter.UwpXaml.Controls;
using DataFilter.UwpXaml.Demo.ViewModels;

namespace DataFilter.UwpXaml.Demo.Pages;

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
