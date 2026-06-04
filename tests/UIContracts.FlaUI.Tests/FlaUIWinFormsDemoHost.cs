using System.Diagnostics;
using System.Drawing;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIWinFormsDemoHost
{
    public static string BuildAndGetExePath()
    {
        var root = FindRepoRoot();
        var exe = Path.Combine(root, "demo", "DataFilter.WinForms.Demo", "bin", "Release", "net8.0-windows", "DataFilter.WinForms.Demo.exe");
        if (!File.Exists(exe))
            throw new FileNotFoundException($"WinForms demo not built. Expected: {exe}");
        return exe;
    }

    public static void NavigateToAttachTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.WinForms.AttachTab);

    public static void OpenDepartmentFilterPopup(Window window, UIA3Automation automation)
    {
        var grid = window.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.Table).Or(cf.ByControlType(ControlType.DataGrid)));
        if (grid == null)
            throw new InvalidOperationException("DataGridView not found.");

        var columnIndex = FindColumnIndex(grid, "Department");
        var headerRect = grid.BoundingRectangle;
        var colRect = GetColumnHeaderRect(grid, columnIndex, headerRect);
        var clickPoint = new Point((int)(colRect.Right - 10), (int)(colRect.Top + 10));
        global::FlaUI.Core.Input.Mouse.Click(clickPoint);
        Thread.Sleep(400);
        _ = automation;
    }

    public static void ApplyListFilterItInPopup(AutomationElement desktop, Window main)
    {
        var popup = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
            .FirstOrDefault(p => p.Properties.NativeWindowHandle.ValueOrDefault != main.Properties.NativeWindowHandle.ValueOrDefault)
            ?? throw new InvalidOperationException("Filter popup not found.");

        var selectAll = popup.FindFirstDescendant(cf => cf.ByName("df-select-all"))
            ?? popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName("Select All")));
        if (selectAll?.AsCheckBox() is { } sa && sa.IsChecked != false)
            sa.Click();

        var it = popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName("IT")))?.AsCheckBox();
        if (it != null && it.IsChecked != true)
            it.Click();

        var ok = popup.FindFirstDescendant(cf => cf.ByName("df-ok"))?.AsButton()
            ?? popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(b => b.AsButton())
                .FirstOrDefault(b => string.Equals(b.Name, "OK", StringComparison.OrdinalIgnoreCase));
        ok?.Invoke();
    }

    public static List<string> ReadDataGridColumn(Window window, string propertyName)
    {
        var grid = window.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.Table).Or(cf.ByControlType(ControlType.DataGrid)));
        if (grid == null)
            return [];

        var colIndex = FindColumnIndex(grid, propertyName);
        var values = new List<string>();
        foreach (var row in grid.FindAllChildren(cf => cf.ByControlType(ControlType.DataItem)))
        {
            var cells = row.FindAllChildren(cf => cf.ByControlType(ControlType.ListItem)
                .Or(cf.ByControlType(ControlType.Custom))
                .Or(cf.ByControlType(ControlType.Text)));
            if (colIndex < cells.Length)
                values.Add((cells[colIndex].Name ?? "").Trim());
        }

        return values.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
    }

    public static void TryCloseApp(global::FlaUI.Core.Application app)
    {
        try { app.Close(); } catch { }
        try { if (!app.HasExited) app.Kill(); } catch { }
    }

    private static int FindColumnIndex(AutomationElement grid, string propertyName)
    {
        var headers = grid.FindAllChildren(cf => cf.ByControlType(ControlType.Header));
        for (var i = 0; i < headers.Length; i++)
        {
            var name = headers[i].Name ?? "";
            if (name.Contains(propertyName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return 2;
    }

    private static Rectangle GetColumnHeaderRect(AutomationElement grid, int columnIndex, Rectangle gridRect)
    {
        var headers = grid.FindAllChildren(cf => cf.ByControlType(ControlType.Header));
        if (columnIndex < headers.Length)
            return headers[columnIndex].BoundingRectangle;
        return new Rectangle((int)gridRect.Left, (int)gridRect.Top, (int)gridRect.Width / 6, 30);
    }

    private static void SelectTab(Window window, string tabHeader)
    {
        var tab = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab))?.AsTab();
        var item = tab?.TabItems.FirstOrDefault(t =>
            string.Equals(t.Name, tabHeader, StringComparison.OrdinalIgnoreCase));
        item?.Select();
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "DataFilter.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repo root not found.");
    }
}
