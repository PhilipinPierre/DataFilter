using System.Diagnostics;
using System.Drawing;
using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWinFormsDemo_AttachContractsTests
{
    [Fact]
    public void PopupOpenClose_AttachTab()
    {
        var exe = BuildAndGetWinFormsDemoExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            // Navigate to the attach tab.
            var tab = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab))?.AsTab();
            Assert.NotNull(tab);

            var attachTab = tab!.TabItems.FirstOrDefault(t =>
                string.Equals(t.Properties.Name.ValueOrDefault, "Attach (DataGridView)", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Properties.HelpText.ValueOrDefault, "Attach (DataGridView)", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(attachTab);
            attachTab!.Select();

            // WinForms header toggle is custom-painted (not a real UIA button). We click by coordinates near the top-right of the grid.
            var grid = window.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Table).Or(cf.ByControlType(ControlType.DataGrid)));
            Assert.NotNull(grid);

            var r = grid!.BoundingRectangle;
            Assert.True(r.Width > 0 && r.Height > 0);

            // Click in header area (y ~ top + 10) at far right (x ~ right - 10).
            var clickPoint = new Point((int)(r.Right - 10), (int)(r.Top + 10));
            global::FlaUI.Core.Input.Mouse.Click(clickPoint);

            // Popup host is a ContextMenuStrip (often UIA menu/popup window).
            WaitUntil(() =>
            {
                var desktop = automation.GetDesktop();
                var popupWindows = desktop.FindAllChildren(cf =>
                    cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)));
                return popupWindows.Length > 0;
            }, TimeSpan.FromSeconds(5));
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    private static string BuildAndGetWinFormsDemoExePath()
    {
        var repoRoot = FindRepoRoot();
        var exe = Path.Combine(repoRoot, "demo", "DataFilter.WinForms.Demo", "bin", "Release", "net8.0-windows", "DataFilter.WinForms.Demo.exe");
        Assert.True(File.Exists(exe), $"Expected demo exe at '{exe}'. Build it first: dotnet build \"demo/DataFilter.WinForms.Demo/DataFilter.WinForms.Demo.csproj\" -c Release");
        return exe;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "DataFilter.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find repo root (DataFilter.slnx).");
    }

    private static void WaitUntil(Func<bool> predicate, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            if (predicate())
                return;
            Thread.Sleep(100);
        }
        throw new TimeoutException("Condition not met within timeout.");
    }

    private static void TryCloseApp(Application app)
    {
        try { app.Close(); } catch { }
        try
        {
            if (!app.HasExited)
                app.Kill();
        }
        catch { }
    }
}

