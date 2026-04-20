using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DataFilter.UwpXaml.Demo.ViewModels;
using Windows.UI.Xaml;

namespace DataFilter.UwpXaml.Demo.Pages;

public sealed partial class CollectionViewPage : Page
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CollectionViewScenarioViewModel),
            typeof(CollectionViewPage),
            new PropertyMetadata(null));

    public CollectionViewScenarioViewModel? ViewModel
    {
        get => (CollectionViewScenarioViewModel?)GetValue(ViewModelProperty);
        private set => SetValue(ViewModelProperty, value);
    }

    public CollectionViewPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is CollectionViewScenarioViewModel vm)
        {
            ViewModel = vm;
        }
        base.OnNavigatedTo(e);
    }
}
