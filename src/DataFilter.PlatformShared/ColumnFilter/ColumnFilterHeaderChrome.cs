namespace DataFilter.PlatformShared.ColumnFilter;

/// <summary>
/// Cross-framework header chrome constants (border state, long-press timing, keyboard shortcut).
/// </summary>
public static class ColumnFilterHeaderChrome
{
    public const int LongPressDurationMs = 500;

    /// <summary>Blazor/CSS class for the inner filtered-column indicator (non-button modes, active filter only).</summary>
    public const string BlazorFilteredIndicatorClass = "df-column-header-filter-active";

    /// <summary>
    /// Default keyboard chord for <see cref="ColumnFilterTriggerMode.KeyboardShortcut"/> (header must have focus).
    /// </summary>
    public const string KeyboardShortcutDisplay = "Alt+Down";
}
