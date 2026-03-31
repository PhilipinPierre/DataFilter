using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Behaviors;
using DataFilter.UwpXaml.Controls;
using DataFilter.UwpXaml.Demo.ViewModels;

namespace DataFilter.UwpXaml.Demo.Pages;

public sealed partial class ListViewPage : Page
{
    public ListViewScenarioViewModel ViewModel { get; private set; }

    public ListViewPage()
    {
        this.InitializeComponent();
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string propertyName)
        {
            var popup = FilterHeaderBehavior.CreatePopup(ViewModel.GridViewModel, propertyName);
            var flyout = new Flyout { Content = popup };
            flyout.ShowAt(button);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ListViewScenarioViewModel vm)
        {
            if (ViewModel != null) ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ViewModel = vm;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateItemsSource();
            
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Bindings.Update();
            });
        }
        base.OnNavigatedTo(e);
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ListViewScenarioViewModel.GridViewModel))
        {
            UpdateItemsSource();
        }
    }

    private void UpdateItemsSource()
    {
        if (ViewModel?.GridViewModel == null) return;

        DataListView.ItemsSource = ViewModel.GridViewModel.FilteredItems;
    }
}
