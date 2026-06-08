using DataFilter.Core.Abstractions;
using DataFilter.Wpf.Behaviors;
using DataFilter.Wpf.ViewModels;
using System.Linq;
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
    private bool _applyingFilterableHeaderStyle;

    static FilterableDataGrid()
    {
        ColumnHeaderStyleProperty.OverrideMetadata(
            typeof(FilterableDataGrid),
            new FrameworkPropertyMetadata(null, OnColumnHeaderStylePropertyChanged));
    }

    public FilterableDataGrid()
    {
        Loaded += (_, _) => EnsureFilterableColumnHeaderStyle();
        EnsureFilterableColumnHeaderStyle();
    }

    private static void OnColumnHeaderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FilterableDataGrid grid || grid._applyingFilterableHeaderStyle)
            return;

        grid.EnsureFilterableColumnHeaderStyle();
    }

    /// <summary>
    /// Merges filterable header behavior into the existing column header style
    /// (e.g. Material Design) instead of replacing it.
    /// </summary>
    public void EnsureFilterableColumnHeaderStyle()
    {
        var baseStyle = ResolveUserColumnHeaderBaseStyle(ColumnHeaderStyle);
        if (baseStyle != null && baseStyle.TargetType != typeof(DataGridColumnHeader))
            baseStyle = null;

        if (ColumnHeaderStyle is Style current
            && HasFilterableSetter(current)
            && (baseStyle == null ? current.BasedOn == null : ReferenceEquals(current.BasedOn, baseStyle)))
        {
            return;
        }

        if (baseStyle != null && HasFilterableSetter(baseStyle) && ReferenceEquals(ColumnHeaderStyle, baseStyle))
            return;

        var style = new Style(typeof(DataGridColumnHeader), baseStyle);
        style.Setters.Add(new Setter(FilterableColumnHeaderBehavior.IsFilterableProperty, true));

        _applyingFilterableHeaderStyle = true;
        try
        {
            ColumnHeaderStyle = style;
        }
        finally
        {
            _applyingFilterableHeaderStyle = false;
        }
    }

    private static bool HasFilterableSetter(Style style) =>
        style.Setters.OfType<Setter>().Any(s =>
            s.Property == FilterableColumnHeaderBehavior.IsFilterableProperty && s.Value is true);

    private static Style? ResolveUserColumnHeaderBaseStyle(Style? style)
    {
        if (style == null)
            return null;

        if (!HasFilterableSetter(style))
            return style;

        // Our wrapper only adds IsFilterable=true on top of the user style.
        if (style.Setters.Count == 1)
            return style.BasedOn;

        return style;
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
