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

        private void OnFilterId(object sender, EventArgs e) => ShowPopup("Id");
        private void OnFilterName(object sender, EventArgs e) => ShowPopup("Name");
        private void OnFilterDept(object sender, EventArgs e) => ShowPopup("Department");
        private void OnFilterCountry(object sender, EventArgs e) => ShowPopup("Country");

        private void ShowPopup(string propertyName)
        {
            var popup = DataFilter.Maui.Behaviors.FilterHeaderBehavior.CreatePopup(ViewModel.GridViewModel, propertyName);
            PopupContainer.Content = popup;
            PopupOverlay.IsVisible = true;
            PopupFrame.IsVisible = true;
        }

        private void OnClosePopup(object sender, EventArgs e)
        {
            PopupOverlay.IsVisible = false;
            PopupFrame.IsVisible = false;
        }
    }
