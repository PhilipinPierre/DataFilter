using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.PlatformShared.ColumnFilter;

/// <summary>
/// Resolves effective column-filter header settings from grid and column options.
/// </summary>
public static class ColumnFilterHeaderOptions
{
    public static bool IsFilteringEnabled(bool gridAreColumnFiltersEnabled, bool? columnIsFilterable = null)
    {
        if (columnIsFilterable == false)
            return false;

        return gridAreColumnFiltersEnabled;
    }

    public static ColumnFilterTriggerMode ResolveTriggerMode(
        ColumnFilterTriggerMode gridTriggerMode,
        ColumnFilterTriggerMode columnTriggerMode = ColumnFilterTriggerMode.Inherit)
    {
        return columnTriggerMode != ColumnFilterTriggerMode.Inherit
            ? columnTriggerMode
            : gridTriggerMode;
    }

    public static bool SuppressesNativeColumnSort(ColumnFilterTriggerMode triggerMode) =>
        triggerMode == ColumnFilterTriggerMode.HeaderLeftClick;

    public static bool HasHeaderFilterTrigger(ColumnFilterTriggerMode triggerMode) =>
        triggerMode is not ColumnFilterTriggerMode.None;

    /// <summary>
    /// When true, a filtered column may show an inner header indicator (not the filter button).
    /// </summary>
    public static bool ShowsFilterStateOnHeaderBorder(ColumnFilterTriggerMode triggerMode) =>
        triggerMode != ColumnFilterTriggerMode.FilterButton;

    /// <summary>
    /// When true, draw the inner filtered-column indicator on the header (non-button modes only).
    /// </summary>
    public static bool ShowsFilteredColumnInnerIndicator(ColumnFilterTriggerMode triggerMode, bool isColumnFilterActive) =>
        ShowsFilterStateOnHeaderBorder(triggerMode) && isColumnFilterActive;

    public static bool UsesAlwaysVisibleFilterButton(ColumnFilterTriggerMode triggerMode) =>
        triggerMode == ColumnFilterTriggerMode.FilterButton;

    public static bool UsesHoverRevealFilterButton(ColumnFilterTriggerMode triggerMode) =>
        triggerMode == ColumnFilterTriggerMode.HoverRevealButton;

    public static bool UsesFilterButtonChrome(ColumnFilterTriggerMode triggerMode) =>
        UsesAlwaysVisibleFilterButton(triggerMode) || UsesHoverRevealFilterButton(triggerMode);

    public static bool IsColumnFilterActive(IFilterableDataGridViewModel? viewModel, string? propertyName)
    {
        if (viewModel == null || string.IsNullOrWhiteSpace(propertyName))
            return false;

        return ExcelFilterActiveState.IsActive(viewModel.GetColumnFilterState(propertyName));
    }

    public static bool IsColumnFilterActive(ExcelFilterState? state) =>
        ExcelFilterActiveState.IsActive(state);
}
