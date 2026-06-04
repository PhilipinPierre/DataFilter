using System.Windows.Controls;
using System.Windows.Data;
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

        var gridVm = vm.GridViewModel;
        var grid = new FilterableGrid { AutoGenerateColumns = true };
        VirtualizingPanel.SetIsVirtualizing(grid, true);

        // One-shot ItemsSource assignment does not refresh when FilteredItems is replaced after filtering.
        grid.SetBinding(FilterableGrid.ItemsSourceProperty, new Binding(nameof(gridVm.FilteredItems)) { Source = gridVm });
        grid.SetBinding(FilterableGrid.ViewModelProperty, new Binding { Source = gridVm });
        grid.SetBinding(FilterableGrid.FilterContextProperty, new Binding(nameof(gridVm.Context)) { Source = gridVm });

        GridChrome.SetGridContent(grid);
    }
}
