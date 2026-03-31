using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Demo.ViewModels;

namespace DataFilter.UwpXaml.Demo.Pages;

public sealed partial class LocalFilterPage : Page
{
    public LocalFilterScenarioViewModel ViewModel { get; private set; } = null!;
#pragma warning restore CS8618

    public LocalFilterPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is LocalFilterScenarioViewModel vm)
        {
            ViewModel = vm;
            // Forcer la mise à jour des bindings après navigation
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Bindings.Update();
            });
        }
        base.OnNavigatedTo(e);
    }
}
