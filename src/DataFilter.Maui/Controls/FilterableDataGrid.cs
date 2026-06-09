using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Controls;

public sealed class FilterableDataGrid : ListView
{
    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(nameof(ViewModel), typeof(IFilterableDataGridViewModel), typeof(FilterableDataGrid));

    public static readonly BindableProperty AreColumnFiltersEnabledProperty =
        BindableProperty.Create(nameof(AreColumnFiltersEnabled), typeof(bool), typeof(FilterableDataGrid), true);

    public static readonly BindableProperty ColumnFilterTriggerModeProperty =
        BindableProperty.Create(
            nameof(ColumnFilterTriggerMode),
            typeof(ColumnFilterTriggerMode),
            typeof(FilterableDataGrid),
            ColumnFilterTriggerMode.FilterButton);

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
        SeparatorVisibility = SeparatorVisibility.Default;
        SelectionMode = ListViewSelectionMode.None;
    }
}
