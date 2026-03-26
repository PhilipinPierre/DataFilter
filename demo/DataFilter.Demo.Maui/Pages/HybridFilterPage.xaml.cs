using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class HybridFilterPage : ContentPage
{
    public HybridFilterScenarioViewModel ViewModel { get; }

    public HybridFilterPage(HybridFilterScenarioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }
}
