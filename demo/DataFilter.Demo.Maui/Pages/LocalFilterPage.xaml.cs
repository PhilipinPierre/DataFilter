using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class LocalFilterPage : ContentPage
{
    public LocalFilterScenarioViewModel ViewModel { get; }

    public LocalFilterPage(LocalFilterScenarioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }
}
