using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class CustomizationPage : ContentPage
{
    public CustomizationScenarioViewModel ViewModel { get; }

    public CustomizationPage(CustomizationScenarioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }

    private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
