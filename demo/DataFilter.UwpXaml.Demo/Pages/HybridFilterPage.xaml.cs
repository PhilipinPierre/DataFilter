using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Demo.ViewModels;

namespace DataFilter.UwpXaml.Demo.Pages;

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
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Bindings.Update();
            });
        }
        base.OnNavigatedTo(e);
    }
}
