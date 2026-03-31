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
