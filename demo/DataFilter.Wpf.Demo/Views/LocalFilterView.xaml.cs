using System.Windows.Controls;
using DataFilter.Wpf.Controls;
using FilterableGrid = DataFilter.Wpf.Controls.FilterableDataGrid;

namespace DataFilter.Wpf.Demo.Views;

public partial class LocalFilterView : UserControl
{
    public LocalFilterView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not ViewModels.LocalFilterScenarioViewModel vm)
            return;

        var grid = new FilterableGrid
        {
            ViewModel = vm.GridViewModel,
            ItemsSource = vm.GridViewModel.FilteredItems,
            AutoGenerateColumns = true,
            FilterContext = vm.GridViewModel.Context
        };
        System.Windows.Controls.VirtualizingPanel.SetIsVirtualizing(grid, true);
        GridChrome.SetGridContent(grid);
    }
}
