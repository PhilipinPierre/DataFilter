using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class CollectionViewPage : ContentPage
{
    public CollectionViewScenarioViewModel ViewModel { get; }

    public CollectionViewPage(CollectionViewScenarioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }
}
