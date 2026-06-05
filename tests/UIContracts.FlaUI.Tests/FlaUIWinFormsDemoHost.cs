using System.Diagnostics;
using System.Drawing;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIWinFormsDemoHost
{
    private static readonly DemoExeSpec WinFormsDemo = new(
        "demo/DataFilter.WinForms.Demo/DataFilter.WinForms.Demo.csproj",
        "DataFilter.WinForms.Demo.exe",
        "net8.0-windows");

    public static string BuildAndGetExePath() => DemoExePathResolver.ResolveOrBuild(WinFormsDemo);

    public static void NavigateToAttachTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.WinForms.AttachTab);

    public static bool OpenDepartmentFilterPopup(Window window, UIA3Automation automation)
    {
        FlaUIInputHelpers.Activate(window);
        var grid = window.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.Table).Or(cf.ByControlType(ControlType.DataGrid)));
        if (grid == null)
            throw new InvalidOperationException("DataGridView not found.");

        var columnIndex = FindColumnIndex(grid, "Department");
        var headerRect = grid.BoundingRectangle;
        var colRect = GetColumnHeaderRect(grid, columnIndex, headerRect);

        // Match DataGridViewFilterAdapter painted glyph: Right-18, Top+4, 14x14.
        var filterButton = new Rectangle(
            (int)(colRect.Right - 18),
            (int)(colRect.Top + 4),
            14,
            14);
        var clickPoint = new Point(
            filterButton.Left + filterButton.Width / 2,
            filterButton.Top + filterButton.Height / 2);

        if (!FlaUIInputHelpers.TryMouseClick(clickPoint))
            return false;

        return FlaUIWinFormsPopupHelpers.WaitForFilterPopup(
            automation.GetDesktop(), window, knownHandles: null, TimeSpan.FromSeconds(15), out _);
    }

    public static void ApplyListFilterItInPopup(AutomationElement desktop, Window main)
    {
        var popup = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
            .FirstOrDefault(p => p.Properties.NativeWindowHandle.ValueOrDefault != main.Properties.NativeWindowHandle.ValueOrDefault)
            ?? throw new InvalidOperationException("Filter popup not found.");

        var selectAll = popup.FindFirstDescendant(cf => cf.ByName("df-select-all"))
            ?? popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName("Select All")));
        if (selectAll?.AsCheckBox() is { } sa && sa.IsChecked != false)
            FlaUIInputHelpers.SetCheckBoxState(sa, desiredChecked: false);

        var it = popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName("IT")))?.AsCheckBox();
        if (it != null && it.IsChecked != true)
            FlaUIInputHelpers.SetCheckBoxState(it, desiredChecked: true);

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
        try
        {
            if (!app.HasExited)
                app.Kill();
        }
        catch
        {
            // ignored
        }

        try
        {
            if (app.ProcessId > 0)
                Process.GetProcessById(app.ProcessId).Kill(entireProcessTree: true);
        }
        catch
        {
            // ignored
        }
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
        FlaUIInputHelpers.Activate(window);
        var tab = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab))?.AsTab()
            ?? throw new InvalidOperationException("Tab control not found.");
        FlaUIInputHelpers.SelectTabItem(tab, tabHeader);
        Thread.Sleep(200);
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
