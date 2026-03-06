using DataFilter.Core.Abstractions;
using DataFilter.Wpf.Behaviors;
using DataFilter.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// A DataGrid that supports Excel-like column filtering.
/// </summary>
public class FilterableDataGrid : DataGrid
{
    public FilterableDataGrid()
    {
        var style = new Style(typeof(DataGridColumnHeader));
        style.Setters.Add(new Setter(FilterableColumnHeaderBehavior.IsFilterableProperty, true));

        this.ColumnHeaderStyle = style;
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(IFilterableDataGridViewModel), typeof(FilterableDataGrid), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the filtering orchestrator.
    /// </summary>
    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty AsyncDataProviderProperty =
        DependencyProperty.Register(nameof(AsyncDataProvider), typeof(object), typeof(FilterableDataGrid), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the async data provider. Resolves to IAsyncDataProvider&lt;T&gt;.
    /// </summary>
    public object AsyncDataProvider
    {
        get { return GetValue(AsyncDataProviderProperty); }
        set { SetValue(AsyncDataProviderProperty, value); }
    }

    public static readonly DependencyProperty EnableAsyncFetchProperty =
        DependencyProperty.Register(nameof(EnableAsyncFetch), typeof(bool), typeof(FilterableDataGrid), new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets a value indicating whether async data fetching is enabled.
    /// </summary>
    public bool EnableAsyncFetch
    {
        get { return (bool)GetValue(EnableAsyncFetchProperty); }
        set { SetValue(EnableAsyncFetchProperty, value); }
    }

    public static readonly DependencyProperty FilterContextProperty =
        DependencyProperty.Register(nameof(FilterContext), typeof(IFilterContext), typeof(FilterableDataGrid), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the current filter context used by the grid.
    /// </summary>
    public IFilterContext FilterContext
    {
        get { return (IFilterContext)GetValue(FilterContextProperty); }
        set { SetValue(FilterContextProperty, value); }
    }
}
