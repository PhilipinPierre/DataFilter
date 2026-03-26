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
}
