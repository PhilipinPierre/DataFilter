using System.Windows;
using DataFilter.PlatformShared.ColumnFilter;

namespace DataFilter.Wpf.Behaviors;

/// <summary>
/// Resolves effective column-filter header settings from grid and column attached properties.
/// </summary>
public static class ColumnFilterHeaderSettings
{
    public static bool IsColumnFilteringEnabled(
        DependencyObject? column,
        DependencyObject header,
        DependencyObject? gridHost)
    {
        if (column != null && HasLocalBool(column, FilterableColumnHeaderBehavior.IsFilterableProperty, out var columnDisabled) && !columnDisabled)
            return false;

        if (HasLocalBool(header, FilterableColumnHeaderBehavior.IsFilterableProperty, out var headerDisabled) && !headerDisabled)
            return false;

        var gridEnabled = gridHost == null || GetAreColumnFiltersEnabled(gridHost);
        bool? columnFilterable = null;
        if (column != null && HasLocalBool(column, FilterableColumnHeaderBehavior.IsFilterableProperty, out var columnValue))
            columnFilterable = columnValue;

        return ColumnFilterHeaderOptions.IsFilteringEnabled(gridEnabled, columnFilterable);
    }

    public static ColumnFilterTriggerMode GetEffectiveTriggerMode(DependencyObject? column, DependencyObject? gridHost)
    {
        var gridMode = gridHost == null
            ? ColumnFilterTriggerMode.FilterButton
            : GetColumnFilterTriggerMode(gridHost);

        var columnMode = ColumnFilterTriggerMode.Inherit;
        if (column != null && HasLocalEnum(column, FilterableColumnHeaderBehavior.ColumnFilterTriggerModeProperty, out var localMode))
            columnMode = localMode;

        return ColumnFilterHeaderOptions.ResolveTriggerMode(gridMode, columnMode);
    }

    internal static bool GetAreColumnFiltersEnabled(DependencyObject gridHost) =>
        gridHost switch
        {
            Controls.FilterableDataGrid dg => dg.AreColumnFiltersEnabled,
            Controls.FilterableGridView gv => gv.AreColumnFiltersEnabled,
            _ => FilterableGridAttach.GetAreColumnFiltersEnabled(gridHost),
        };

    internal static ColumnFilterTriggerMode GetColumnFilterTriggerMode(DependencyObject gridHost) =>
        gridHost switch
        {
            Controls.FilterableDataGrid dg => dg.ColumnFilterTriggerMode,
            Controls.FilterableGridView gv => gv.ColumnFilterTriggerMode,
            _ => FilterableGridAttach.GetColumnFilterTriggerMode(gridHost),
        };

    private static bool HasLocalBool(DependencyObject target, DependencyProperty property, out bool value)
    {
        var local = target.ReadLocalValue(property);
        if (local is bool b && local != DependencyProperty.UnsetValue)
        {
            value = b;
            return true;
        }

        value = default;
        return false;
    }

    private static bool HasLocalEnum(DependencyObject target, DependencyProperty property, out ColumnFilterTriggerMode value)
    {
        var local = target.ReadLocalValue(property);
        if (local is ColumnFilterTriggerMode mode && local != DependencyProperty.UnsetValue)
        {
            value = mode;
            return true;
        }

        value = default;
        return false;
    }
}
