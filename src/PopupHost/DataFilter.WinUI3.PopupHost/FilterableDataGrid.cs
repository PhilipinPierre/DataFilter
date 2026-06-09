using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Attach;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.WinUI3.Controls;

public sealed partial class FilterableDataGrid : ListView
{
    private static readonly ListViewFilterHeaderAdapter.Column[] DefaultColumns =
    [
        new("Id", "Id", 80),
        new("Name", "Name", 150),
        new("Department", "Department", 150),
        new("Country", "Country", 150),
    ];

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(IFilterableDataGridViewModel),
            typeof(FilterableDataGrid),
            new PropertyMetadata(null, OnHeaderSettingsChanged));

    public static readonly DependencyProperty AreColumnFiltersEnabledProperty =
        DependencyProperty.Register(
            nameof(AreColumnFiltersEnabled),
            typeof(bool),
            typeof(FilterableDataGrid),
            new PropertyMetadata(true, OnHeaderSettingsChanged));

    public static readonly DependencyProperty ColumnFilterTriggerModeProperty =
        DependencyProperty.Register(
            nameof(ColumnFilterTriggerMode),
            typeof(ColumnFilterTriggerMode),
            typeof(FilterableDataGrid),
            new PropertyMetadata(ColumnFilterTriggerMode.FilterButton, OnHeaderSettingsChanged));

    public IFilterableDataGridViewModel? ViewModel
    {
        get => (IFilterableDataGridViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public bool AreColumnFiltersEnabled
    {
        get => (bool)GetValue(AreColumnFiltersEnabledProperty);
        set => SetValue(AreColumnFiltersEnabledProperty, value);
    }

    public ColumnFilterTriggerMode ColumnFilterTriggerMode
    {
        get => (ColumnFilterTriggerMode)GetValue(ColumnFilterTriggerModeProperty);
        set => SetValue(ColumnFilterTriggerModeProperty, value);
    }

    public FilterableDataGrid()
    {
        RebuildHeader();
    }

    private static void OnHeaderSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterableDataGrid grid)
            grid.RebuildHeader();
    }

    private void RebuildHeader()
    {
        if (ViewModel == null)
        {
            Header = null;
            return;
        }

        var specs = DefaultColumns.Select(c => new ListViewFilterHeaderInteractions.ColumnSpec
        {
            Title = c.Title,
            PropertyName = c.PropertyName,
            Width = c.Width,
            IsFilterable = c.IsFilterable,
            TriggerMode = c.TriggerMode,
        }).ToList();

        Header = ListViewFilterHeaderInteractions.BuildHeader(
            ViewModel,
            specs,
            new ListViewFilterHeaderInteractions.Settings
            {
                AreColumnFiltersEnabled = AreColumnFiltersEnabled,
                ColumnFilterTriggerMode = ColumnFilterTriggerMode,
            });
    }
}
