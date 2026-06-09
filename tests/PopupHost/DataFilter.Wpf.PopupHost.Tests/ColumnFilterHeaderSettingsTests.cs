using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.Wpf.Behaviors;
using DataFilter.Wpf.Controls;

namespace DataFilter.Wpf.PopupHost.Tests;

public sealed class ColumnFilterHeaderSettingsTests
{
    [Fact]
    public void Grid_disable_turns_off_column_filtering()
        => RunSta(() =>
        {
            var grid = new FilterableDataGrid { AreColumnFiltersEnabled = false };
            var header = new DataGridColumnHeader();
            var column = new DataGridTextColumn();
            grid.Columns.Add(column);

            var enabled = ColumnFilterHeaderSettings.IsColumnFilteringEnabled(column, header, grid);

            Assert.False(enabled);
        });

    [Fact]
    public void Per_column_disable_overrides_enabled_grid()
        => RunSta(() =>
        {
            var grid = new FilterableDataGrid { AreColumnFiltersEnabled = true };
            var header = new DataGridColumnHeader();
            var column = new DataGridTextColumn();
            FilterableColumnHeaderBehavior.SetIsFilterable(column, false);
            grid.Columns.Add(column);

            var enabled = ColumnFilterHeaderSettings.IsColumnFilteringEnabled(column, header, grid);

            Assert.False(enabled);
        });

    [Fact]
    public void Grid_trigger_mode_is_default_for_columns()
        => RunSta(() =>
        {
            var grid = new FilterableDataGrid { ColumnFilterTriggerMode = ColumnFilterTriggerMode.HeaderRightClick };
            var column = new DataGridTextColumn();

            var mode = ColumnFilterHeaderSettings.GetEffectiveTriggerMode(column, grid);

            Assert.Equal(ColumnFilterTriggerMode.HeaderRightClick, mode);
        });

    [Fact]
    public void Per_column_trigger_mode_overrides_grid_default()
        => RunSta(() =>
        {
            var grid = new FilterableDataGrid { ColumnFilterTriggerMode = ColumnFilterTriggerMode.FilterButton };
            var column = new DataGridTextColumn();
            FilterableColumnHeaderBehavior.SetColumnFilterTriggerMode(column, ColumnFilterTriggerMode.HeaderLeftClick);

            var mode = ColumnFilterHeaderSettings.GetEffectiveTriggerMode(column, grid);

            Assert.Equal(ColumnFilterTriggerMode.HeaderLeftClick, mode);
        });

    [Fact]
    public void Attached_grid_properties_are_used_for_plain_datagrid()
        => RunSta(() =>
        {
            var grid = new DataGrid();
            FilterableGridAttach.SetAreColumnFiltersEnabled(grid, false);
            FilterableGridAttach.SetColumnFilterTriggerMode(grid, ColumnFilterTriggerMode.HeaderLeftClick);
            var header = new DataGridColumnHeader();
            var column = new DataGridTextColumn();

            Assert.False(ColumnFilterHeaderSettings.IsColumnFilteringEnabled(column, header, grid));
            Assert.Equal(
                ColumnFilterTriggerMode.HeaderLeftClick,
                ColumnFilterHeaderSettings.GetEffectiveTriggerMode(column, grid));
        });

    private static void RunSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (captured != null)
            throw captured;
    }
}
