using DataFilter.Localization;
using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Behaviors;
using System.Drawing;
using System.Windows.Forms;

namespace DataFilter.WinForms.Attach;

public static class DataGridViewFilterHeaderInteractions
{
    public sealed class Settings
    {
        public bool AreColumnFiltersEnabled { get; init; } = true;
        public ColumnFilterTriggerMode ColumnFilterTriggerMode { get; init; } = ColumnFilterTriggerMode.FilterButton;
        public bool AppendHeaderGlyph { get; init; } = true;
        public int HeaderButtonHitWidth { get; init; } = 20;
    }

    internal static bool IsColumnFilteringEnabled(DataGridViewColumn column, Settings settings) =>
        ColumnFilterHeaderOptions.IsFilteringEnabled(
            settings.AreColumnFiltersEnabled,
            DataGridViewColumnFilterAttach.GetOptions(column).IsFilterable);

    internal static ColumnFilterTriggerMode GetEffectiveTriggerMode(DataGridViewColumn column, Settings settings) =>
        ColumnFilterHeaderOptions.ResolveTriggerMode(
            settings.ColumnFilterTriggerMode,
            DataGridViewColumnFilterAttach.GetOptions(column).TriggerMode);

    public static void ApplyNativeSortPolicy(DataGridViewColumn column, Settings settings)
    {
        if (!IsColumnFilteringEnabled(column, settings))
            return;

        column.SortMode = ColumnFilterHeaderOptions.SuppressesNativeColumnSort(GetEffectiveTriggerMode(column, settings))
            ? DataGridViewColumnSortMode.NotSortable
            : DataGridViewColumnSortMode.Automatic;
    }

    internal static void PaintFilterHeader(
        DataGridViewCellPaintingEventArgs e,
        DataGridViewColumn column,
        Settings settings,
        IFilterableDataGridViewModel? viewModel,
        int? hoveredColumnIndex)
    {
        if (!IsColumnFilteringEnabled(column, settings))
            return;

        ApplyNativeSortPolicy(column, settings);

        var mode = GetEffectiveTriggerMode(column, settings);
        e.Paint(e.CellBounds, DataGridViewPaintParts.All);

        if (ColumnFilterHeaderOptions.ShowsFilterStateOnHeaderBorder(mode))
        {
            var propertyName = ResolvePropertyName(column);
            bool active = ColumnFilterHeaderOptions.IsColumnFilterActive(viewModel, propertyName);
            if (active)
            {
                using var pen = new Pen(Color.DodgerBlue, 2f);
                var borderRect = Rectangle.Inflate(e.CellBounds, -2, -2);
                e.Graphics!.DrawRectangle(pen, borderRect);
            }
        }

        if (ColumnFilterHeaderOptions.UsesAlwaysVisibleFilterButton(mode)
            || (ColumnFilterHeaderOptions.UsesHoverRevealFilterButton(mode) && hoveredColumnIndex == e.ColumnIndex))
        {
            var rect = new Rectangle(e.CellBounds.Right - 18, e.CellBounds.Top + 4, 14, 14);
            ControlPaint.DrawButton(e.Graphics, rect, ButtonState.Flat);
            using var font = new Font("Segoe UI", 8f, FontStyle.Regular);
            TextRenderer.DrawText(e.Graphics, "▼", font, rect, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        e.Handled = true;
    }

    internal static void AppendHeaderGlyphs(DataGridView grid, Settings settings)
    {
        if (!settings.AppendHeaderGlyph)
            return;

        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (!IsColumnFilteringEnabled(column, settings))
                continue;

            var mode = GetEffectiveTriggerMode(column, settings);
            if (!ColumnFilterHeaderOptions.UsesAlwaysVisibleFilterButton(mode))
                continue;

            if (!column.HeaderText.EndsWith(" \u25BE", StringComparison.Ordinal))
                column.HeaderText += " \u25BE";
        }
    }

    public static bool ShouldOpenFilterPopup(
        DataGridViewCellMouseEventArgs e,
        Rectangle headerRect,
        DataGridViewColumn column,
        Settings settings,
        int? hoveredColumnIndex)
    {
        if (!IsColumnFilteringEnabled(column, settings))
            return false;

        var mode = GetEffectiveTriggerMode(column, settings);
        bool shift = (Control.ModifierKeys & Keys.Shift) != 0;
        bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;

        return mode switch
        {
            ColumnFilterTriggerMode.FilterButton =>
                e.Button == MouseButtons.Left && e.Clicks == 1 && !shift && !ctrl
                && e.X >= headerRect.Width - settings.HeaderButtonHitWidth,
            ColumnFilterTriggerMode.HoverRevealButton =>
                e.Button == MouseButtons.Left && e.Clicks == 1 && !shift && !ctrl
                && hoveredColumnIndex == e.ColumnIndex
                && e.X >= headerRect.Width - settings.HeaderButtonHitWidth,
            ColumnFilterTriggerMode.HeaderLeftClick =>
                e.Button == MouseButtons.Left && e.Clicks == 1 && !shift && !ctrl,
            ColumnFilterTriggerMode.HeaderRightClick =>
                e.Button == MouseButtons.Right && e.Clicks == 1,
            ColumnFilterTriggerMode.HeaderDoubleClick =>
                e.Button == MouseButtons.Left && e.Clicks >= 2,
            ColumnFilterTriggerMode.HeaderMiddleClick =>
                e.Button == MouseButtons.Middle,
            ColumnFilterTriggerMode.ShiftClick =>
                e.Button == MouseButtons.Left && e.Clicks == 1 && shift,
            ColumnFilterTriggerMode.CtrlClick =>
                e.Button == MouseButtons.Left && e.Clicks == 1 && ctrl,
            ColumnFilterTriggerMode.ContextMenuFilter or ColumnFilterTriggerMode.HeaderLongPress or ColumnFilterTriggerMode.KeyboardShortcut or ColumnFilterTriggerMode.None => false,
            _ => false,
        };
    }

    public static bool ShouldShowContextMenuFilter(DataGridViewCellMouseEventArgs e, DataGridViewColumn column, Settings settings) =>
        IsColumnFilteringEnabled(column, settings)
        && GetEffectiveTriggerMode(column, settings) == ColumnFilterTriggerMode.ContextMenuFilter
        && e.Button == MouseButtons.Right
        && e.Clicks == 1;

    public static ContextMenuStrip CreateFilterContextMenu(Action openFilter)
    {
        var menu = new ContextMenuStrip();
        var item = new ToolStripMenuItem(LocalizationManager.Instance["OpenColumnFilter"]);
        item.Click += (_, _) => openFilter();
        menu.Items.Add(item);
        return menu;
    }

    public static bool TryHandleKeyboardShortcut(KeyEventArgs e, ColumnFilterTriggerMode mode)
    {
        if (mode != ColumnFilterTriggerMode.KeyboardShortcut)
            return false;

        return e.Alt && e.KeyCode == Keys.Down;
    }

    internal static async Task ShowFilterPopupAsync(
        DataGridView grid,
        ContextMenuStrip popupHost,
        IFilterableDataGridViewModel viewModel,
        DataGridViewColumn column,
        Rectangle headerRect)
    {
        var propertyName = ResolvePropertyName(column);
        if (string.IsNullOrWhiteSpace(propertyName))
            return;

        var popup = await FilterHeaderBehavior.CreatePopupAsync(viewModel, propertyName);

        bool isDark = grid.BackgroundColor.R < 128;
        popup.ApplyTheme(isDark);

        popup.Width = 320;
        popup.Height = 420;
        popup.RequestClose += () =>
        {
            if (popupHost.Visible)
                popupHost.Close();
        };

        popupHost.Items.Clear();
        var host = new ToolStripControlHost(popup) { AutoSize = false, Width = popup.Width, Height = popup.Height };
        popupHost.Items.Add(host);

        bool isRtl = grid.RightToLeft == RightToLeft.Yes;
        var desiredScreen = grid.PointToScreen(new Point(isRtl ? headerRect.Left : headerRect.Right, headerRect.Bottom));
        var work = Screen.FromControl(grid).WorkingArea;
        const int margin = 8;

        int maxPopupWidth = Math.Max(200, work.Width - (margin * 2));
        int maxPopupHeight = Math.Max(200, work.Height - (margin * 2));
        popup.Width = Math.Min(popup.Width, maxPopupWidth);
        popup.Height = Math.Min(popup.Height, maxPopupHeight);
        host.Width = popup.Width;
        host.Height = popup.Height;

        int left = isRtl ? desiredScreen.X - popup.Width : desiredScreen.X;
        int top = desiredScreen.Y;

        int minX = work.Left + margin;
        int maxX = Math.Max(minX, work.Right - popup.Width - margin);
        int minY = work.Top + margin;
        int maxY = Math.Max(minY, work.Bottom - popup.Height - margin);

        left = Math.Min(Math.Max(left, minX), maxX);
        top = Math.Min(Math.Max(top, minY), maxY);

        var showPointClient = grid.PointToClient(new Point(left, top));
        popupHost.Show(grid, showPointClient);
    }

    private static string? ResolvePropertyName(DataGridViewColumn column) =>
        !string.IsNullOrWhiteSpace(column.DataPropertyName) ? column.DataPropertyName : column.Name;
}
