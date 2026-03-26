using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class AsyncFilterPage : ContentPage
{
    public AsyncFilterScenarioViewModel ViewModel { get; }

    public AsyncFilterPage(AsyncFilterScenarioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }
}
