using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DataFilter.WinUI3.Demo.ViewModels;

namespace DataFilter.WinUI3.Demo.Pages;

public sealed partial class AsyncFilterPage : Page
{
    public AsyncFilterScenarioViewModel ViewModel { get; private set; }

    public AsyncFilterPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is AsyncFilterScenarioViewModel vm)
        {
            ViewModel = vm;
        }
        base.OnNavigatedTo(e);
    }
}
