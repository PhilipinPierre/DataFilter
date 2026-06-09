using System.Runtime.CompilerServices;
using DataFilter.PlatformShared.ColumnFilter;
using System.Windows.Forms;

namespace DataFilter.WinForms.Attach;

/// <summary>
/// Per-column filter header settings for <see cref="DataGridView"/> columns.
/// </summary>
public static class DataGridViewColumnFilterAttach
{
    private static readonly ConditionalWeakTable<DataGridViewColumn, ColumnOptions> Options = new();

    public static bool GetIsFilterable(DataGridViewColumn column) => GetOptions(column).IsFilterable;

    public static void SetIsFilterable(DataGridViewColumn column, bool value) => GetOptions(column).IsFilterable = value;

    public static ColumnFilterTriggerMode GetColumnFilterTriggerMode(DataGridViewColumn column) =>
        GetOptions(column).TriggerMode;

    public static void SetColumnFilterTriggerMode(DataGridViewColumn column, ColumnFilterTriggerMode value) =>
        GetOptions(column).TriggerMode = value;

    internal static ColumnOptions GetOptions(DataGridViewColumn column) =>
        Options.GetValue(column, _ => new ColumnOptions());

    internal sealed class ColumnOptions
    {
        public bool IsFilterable { get; set; } = true;
        public ColumnFilterTriggerMode TriggerMode { get; set; } = ColumnFilterTriggerMode.Inherit;
    }
}
