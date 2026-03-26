using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DataFilter.WinUI3.Demo.ViewModels;

namespace DataFilter.WinUI3.Demo.Pages;

public sealed partial class HybridFilterPage : Page
{
    public HybridFilterScenarioViewModel ViewModel { get; private set; }

    public HybridFilterPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is HybridFilterScenarioViewModel vm)
        {
            ViewModel = vm;
        }
        base.OnNavigatedTo(e);
    }
}
