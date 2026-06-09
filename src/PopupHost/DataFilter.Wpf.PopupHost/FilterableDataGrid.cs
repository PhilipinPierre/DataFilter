using DataFilter.Core.Abstractions;
using DataFilter.Wpf.Behaviors;
using DataFilter.Wpf.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using DataFilter.PlatformShared.ColumnFilter;

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
        CanUserReorderColumns = false;
        Loaded += OnFilterableDataGridLoaded;
        Columns.CollectionChanged += (_, _) =>
        {
            if (!IsLoaded)
                return;

            EnsureFilterableColumnHeaderStyle();
            PreserveColumnDisplayOrder();
        };
    }

    private void OnFilterableDataGridLoaded(object sender, RoutedEventArgs e)
    {
        EnsureFilterableColumnHeaderStyle();
        PreserveColumnDisplayOrder();
        DataGridScrollViewerFix.Apply(this);
        DataGridHeaderRowBackgroundFix.Apply(this);

        // Auto-generated columns may appear after the first Loaded pass (code-behind / late bindings).
        Dispatcher.BeginInvoke(() =>
        {
            EnsureFilterableColumnHeaderStyle();
            PreserveColumnDisplayOrder();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Keeps columns in XAML declaration order (WPF default allows drag-reorder via header).
    /// </summary>
    public void PreserveColumnDisplayOrder()
    {
        for (var i = 0; i < Columns.Count; i++)
            Columns[i].DisplayIndex = i;
    }

    private static void OnColumnHeaderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FilterableDataGrid grid || grid._applyingFilterableHeaderStyle)
            return;

        if (e.NewValue == DependencyProperty.UnsetValue)
            return;

        grid.EnsureFilterableColumnHeaderStyle();
        DataGridHeaderRowBackgroundFix.Apply(grid);
    }

    /// <summary>
    /// Merges filterable header behavior into the existing column header style
    /// (e.g. Material Design) instead of replacing it.
    /// </summary>
    public void EnsureFilterableColumnHeaderStyle()
    {
        if (!TryGetColumnHeaderStyle(out var columnHeaderStyle))
            return;

        var baseStyle = ResolveUserColumnHeaderBaseStyle(columnHeaderStyle);
        if (baseStyle != null && baseStyle.TargetType != typeof(DataGridColumnHeader))
            baseStyle = null;

        if (columnHeaderStyle is Style current
            && HasFilterableSetter(current)
            && (baseStyle == null ? current.BasedOn == null : ReferenceEquals(current.BasedOn, baseStyle)))
        {
            return;
        }

        if (baseStyle != null && HasFilterableSetter(baseStyle) && ReferenceEquals(columnHeaderStyle, baseStyle))
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

    private bool TryGetColumnHeaderStyle(out Style? style)
    {
        var value = ReadLocalValue(ColumnHeaderStyleProperty);
        if (value == DependencyProperty.UnsetValue)
        {
            // No user style (demo / default grid): still apply the filterable header wrapper.
            style = null;
            return true;
        }

        style = value as Style;
        return true;
    }

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

    public static readonly DependencyProperty AreColumnFiltersEnabledProperty =
        DependencyProperty.Register(
            nameof(AreColumnFiltersEnabled),
            typeof(bool),
            typeof(FilterableDataGrid),
            new PropertyMetadata(true, ColumnFilterHeaderRefresh.OnGridHeaderSettingsChanged));

    /// <summary>
    /// Gets or sets whether column filter UI is enabled for this grid. When <c>false</c>, filter buttons
    /// and header triggers are hidden on every column. Per-column disable uses
    /// <see cref="FilterableColumnHeaderBehavior.IsFilterableProperty"/> on the column.
    /// </summary>
    public bool AreColumnFiltersEnabled
    {
        get => (bool)GetValue(AreColumnFiltersEnabledProperty);
        set => SetValue(AreColumnFiltersEnabledProperty, value);
    }

    public static readonly DependencyProperty ColumnFilterTriggerModeProperty =
        DependencyProperty.Register(
            nameof(ColumnFilterTriggerMode),
            typeof(ColumnFilterTriggerMode),
            typeof(FilterableDataGrid),
            new PropertyMetadata(ColumnFilterTriggerMode.FilterButton, ColumnFilterHeaderRefresh.OnGridHeaderSettingsChanged));

    /// <summary>
    /// Gets or sets the default way column filter popups are opened from headers.
    /// Per-column overrides use <see cref="FilterableColumnHeaderBehavior.ColumnFilterTriggerModeProperty"/>.
    /// </summary>
    public ColumnFilterTriggerMode ColumnFilterTriggerMode
    {
        get => (ColumnFilterTriggerMode)GetValue(ColumnFilterTriggerModeProperty);
        set => SetValue(ColumnFilterTriggerModeProperty, value);
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
