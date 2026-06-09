using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using DataFilter.Demo.Shared.Services;
using DataFilter.Wpf.Controls;
using FilterableGrid = DataFilter.Wpf.Controls.FilterableDataGrid;

namespace DataFilter.Wpf.Demo.Views;

public partial class LocalFilterView : UserControl
{
    private FilterableGrid? _grid;

    public LocalFilterView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += (_, _) => WireHeaderSettings();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not ViewModels.LocalFilterScenarioViewModel vm)
            return;

        var gridVm = vm.GridViewModel;
        _grid = new FilterableGrid { AutoGenerateColumns = true };
        VirtualizingPanel.SetIsVirtualizing(_grid, true);

        _grid.SetBinding(FilterableGrid.ItemsSourceProperty, new Binding(nameof(gridVm.FilteredItems)) { Source = gridVm });
        _grid.SetBinding(FilterableGrid.ViewModelProperty, new Binding { Source = gridVm });
        _grid.SetBinding(FilterableGrid.FilterContextProperty, new Binding(nameof(gridVm.Context)) { Source = gridVm });

        GridChrome.SetGridContent(_grid);
        WireHeaderSettings();
    }

    private void WireHeaderSettings()
    {
        if (_grid == null || DataContext is not IDemoHeaderSettingsHost host)
            return;

        var settings = host.HeaderSettings;
        _grid.AreColumnFiltersEnabled = settings.AreColumnFiltersEnabled;
        _grid.ColumnFilterTriggerMode = settings.ColumnFilterTriggerMode;

        settings.PropertyChanged -= OnHeaderSettingsChanged;
        settings.PropertyChanged += OnHeaderSettingsChanged;
    }

    private void OnHeaderSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_grid == null || sender is not DemoHeaderSettings settings)
            return;

        if (e.PropertyName is nameof(DemoHeaderSettings.AreColumnFiltersEnabled)
            or nameof(DemoHeaderSettings.ColumnFilterTriggerMode))
        {
            _grid.AreColumnFiltersEnabled = settings.AreColumnFiltersEnabled;
            _grid.ColumnFilterTriggerMode = settings.ColumnFilterTriggerMode;
        }
    }
}
