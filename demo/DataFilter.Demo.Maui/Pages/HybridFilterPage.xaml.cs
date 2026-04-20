using DataFilter.Maui.Demo.ViewModels;
using DataFilter.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

    public partial class HybridFilterPage : ContentPage
    {
        public HybridFilterScenarioViewModel ViewModel { get; }

        public HybridFilterPage(HybridFilterScenarioViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = viewModel;
        }

        private void OnFilterId(object sender, EventArgs e) => ShowPopup((VisualElement)sender, "Id");
        private void OnFilterName(object sender, EventArgs e) => ShowPopup((VisualElement)sender, "Name");
        private void OnFilterDept(object sender, EventArgs e) => ShowPopup((VisualElement)sender, "Department");
        private void OnFilterCountry(object sender, EventArgs e) => ShowPopup((VisualElement)sender, "Country");

        private void ShowPopup(VisualElement anchor, string propertyName)
        {
            var popup = DataFilter.Maui.Behaviors.FilterHeaderBehavior.CreatePopup(ViewModel.GridViewModel, propertyName);
            popup.CloseRequested += (_, _) => OnClosePopup(this, EventArgs.Empty);
            PopupContainer.Content = popup;
            PopupOverlay.IsVisible = true;
            PopupFrame.IsVisible = true;

            PositionPopup(anchor);
        }

        private void PositionPopup(VisualElement anchor)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var anchorPos = GetAbsolutePosition(anchor);

                var desiredX = anchorPos.X;
                var desiredY = anchorPos.Y + anchor.Height + 4;

                var popupWidth = PopupFrame.Width > 0 ? PopupFrame.Width : 280;
                var popupHeight = PopupFrame.Height > 0 ? PopupFrame.Height : 350;

                var maxX = Math.Max(0, Width - popupWidth - 8);
                var maxY = Math.Max(0, Height - popupHeight - 8);

                PopupFrame.TranslationX = Clamp(desiredX, 8, maxX);
                PopupFrame.TranslationY = Clamp(desiredY, 8, maxY);
            });
        }

        private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);

        private static Point GetAbsolutePosition(VisualElement element)
        {
            double x = element.X;
            double y = element.Y;

            var parent = element.Parent as VisualElement;
            while (parent != null)
            {
                x += parent.X;
                y += parent.Y;
                parent = parent.Parent as VisualElement;
            }

            return new Point(x, y);
        }

        private void OnClosePopup(object sender, EventArgs e)
        {
            PopupOverlay.IsVisible = false;
            PopupFrame.IsVisible = false;
            PopupContainer.Content = null;
        }
    }
