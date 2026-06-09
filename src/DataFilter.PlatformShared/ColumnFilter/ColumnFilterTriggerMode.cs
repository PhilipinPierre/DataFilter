namespace DataFilter.PlatformShared.ColumnFilter;

/// <summary>
/// Defines how the column filter popup is opened from a column header.
/// </summary>
public enum ColumnFilterTriggerMode
{
    /// <summary>
    /// Inherit the trigger mode from the grid (or host). Used for per-column overrides only.
    /// </summary>
    Inherit = 0,

    /// <summary>
    /// Show a dedicated filter button in the column header (default at grid level).
    /// Filter state is shown on the button, not the header border.
    /// </summary>
    FilterButton = 1,

    /// <summary>
    /// No filter button; open the popup on right-click on the column header.
    /// </summary>
    HeaderRightClick = 2,

    /// <summary>
    /// No filter button; open the popup on left-click on the column header.
    /// Native column sorting is disabled for this column.
    /// </summary>
    HeaderLeftClick = 3,

    /// <summary>
    /// No filter button; open the popup on double-click on the column header.
    /// </summary>
    HeaderDoubleClick = 4,

    /// <summary>
    /// No filter button; open the popup on middle-click (mouse wheel button) on the column header.
    /// </summary>
    HeaderMiddleClick = 5,

    /// <summary>
    /// No header trigger. Filtering is available via the filter bar, pipeline, or programmatic APIs only.
    /// </summary>
    None = 6,

    /// <summary>
    /// Right-click opens a context menu with a Filter command instead of opening the popup immediately.
    /// </summary>
    ContextMenuFilter = 7,

    /// <summary>
    /// Open the popup after a long press on the column header (touch / pen friendly).
    /// </summary>
    HeaderLongPress = 8,

    /// <summary>
    /// Open the popup when the header has focus and the user presses Alt+Down.
    /// </summary>
    KeyboardShortcut = 9,

    /// <summary>
    /// Show the filter button only while the pointer hovers the column header.
    /// </summary>
    HoverRevealButton = 10,

    /// <summary>
    /// Open the popup on Shift+left-click on the column header.
    /// </summary>
    ShiftClick = 11,

    /// <summary>
    /// Open the popup on Ctrl+left-click on the column header.
    /// </summary>
    CtrlClick = 12,
}
