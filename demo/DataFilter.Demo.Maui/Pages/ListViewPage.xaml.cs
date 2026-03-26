using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class ListViewPage : ContentPage
{
    public ListViewScenarioViewModel ViewModel { get; }

    public ListViewPage(ListViewScenarioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }
}
