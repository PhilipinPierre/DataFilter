using DataFilter.PlatformShared.ColumnFilter;

namespace DataFilter.PlatformShared.Tests;

public sealed class ColumnFilterHeaderOptionsTests
{
    [Fact]
    public void Grid_disable_turns_off_filtering()
    {
        Assert.False(ColumnFilterHeaderOptions.IsFilteringEnabled(gridAreColumnFiltersEnabled: false));
    }

    [Fact]
    public void Per_column_disable_overrides_enabled_grid()
    {
        Assert.False(ColumnFilterHeaderOptions.IsFilteringEnabled(true, columnIsFilterable: false));
    }

    [Fact]
    public void Column_trigger_mode_overrides_grid_default()
    {
        var mode = ColumnFilterHeaderOptions.ResolveTriggerMode(
            ColumnFilterTriggerMode.FilterButton,
            ColumnFilterTriggerMode.HeaderLeftClick);

        Assert.Equal(ColumnFilterTriggerMode.HeaderLeftClick, mode);
    }

    [Fact]
    public void Inherit_uses_grid_trigger_mode()
    {
        var mode = ColumnFilterHeaderOptions.ResolveTriggerMode(
            ColumnFilterTriggerMode.HeaderRightClick,
            ColumnFilterTriggerMode.Inherit);

        Assert.Equal(ColumnFilterTriggerMode.HeaderRightClick, mode);
    }

    [Fact]
    public void HeaderLeftClick_suppresses_native_sort()
    {
        Assert.True(ColumnFilterHeaderOptions.SuppressesNativeColumnSort(ColumnFilterTriggerMode.HeaderLeftClick));
        Assert.False(ColumnFilterHeaderOptions.SuppressesNativeColumnSort(ColumnFilterTriggerMode.HeaderDoubleClick));
    }

    [Fact]
    public void None_has_no_header_trigger()
    {
        Assert.False(ColumnFilterHeaderOptions.HasHeaderFilterTrigger(ColumnFilterTriggerMode.None));
        Assert.True(ColumnFilterHeaderOptions.HasHeaderFilterTrigger(ColumnFilterTriggerMode.FilterButton));
    }

    [Fact]
    public void Border_chrome_applies_when_not_filter_button()
    {
        Assert.False(ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(ColumnFilterTriggerMode.FilterButton));
        Assert.True(ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(ColumnFilterTriggerMode.HeaderLeftClick));
        Assert.True(ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(ColumnFilterTriggerMode.HoverRevealButton));
        Assert.True(ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(ColumnFilterTriggerMode.ContextMenuFilter));
    }

    [Fact]
    public void Inner_indicator_only_when_filtered_and_not_filter_button()
    {
        Assert.False(ColumnFilterHeaderOptions.ShowsFilteredColumnInnerIndicator(ColumnFilterTriggerMode.FilterButton, true));
        Assert.False(ColumnFilterHeaderOptions.ShowsFilteredColumnInnerIndicator(ColumnFilterTriggerMode.HeaderLeftClick, false));
        Assert.True(ColumnFilterHeaderOptions.ShowsFilteredColumnInnerIndicator(ColumnFilterTriggerMode.HeaderLeftClick, true));
    }
}
