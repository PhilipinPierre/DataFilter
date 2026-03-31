using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Demo.ViewModels;

namespace DataFilter.UwpXaml.Demo.Pages;

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
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Bindings.Update();
            });
        }
        base.OnNavigatedTo(e);
    }
}
