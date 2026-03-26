using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using DataFilter.WinUI3.Demo.ViewModels;

namespace DataFilter.WinUI3.Demo.Pages;

public sealed partial class CustomizationPage : Page
{
    public CustomizationScenarioViewModel ViewModel { get; private set; }

    public CustomizationPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is CustomizationScenarioViewModel vm)
        {
            ViewModel = vm;
        }
        base.OnNavigatedTo(e);
    }

    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch && toggleSwitch.XamlRoot?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = toggleSwitch.IsOn ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}
