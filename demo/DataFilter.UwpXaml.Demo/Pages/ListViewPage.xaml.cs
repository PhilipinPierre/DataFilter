using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Demo.ViewModels;

namespace DataFilter.UwpXaml.Demo.Pages;

public sealed partial class ListViewPage : Page
{
    public ListViewScenarioViewModel ViewModel { get; private set; }

    public ListViewPage()
    {
        InitializeComponent();
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
