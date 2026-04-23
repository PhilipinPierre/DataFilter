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

            var desktop = automation.GetDesktop();
            var beforePopups = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
                .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                .ToHashSet();

            // Click in header area (y ~ top + 10) at far right (x ~ right - 10).
            var clickPoint = new Point((int)(r.Right - 10), (int)(r.Top + 10));
            global::FlaUI.Core.Input.Mouse.Click(clickPoint);

            // Popup host is a ContextMenuStrip (often UIA menu/popup window).
            var popup = WaitForPopup(desktop, window, beforePopups, TimeSpan.FromSeconds(5));
            AssertWithinWorkingArea(popup);

            // Close via Esc (best-effort; should dismiss context menu)
            try { window.Focus(); } catch { }
            global::FlaUI.Core.Input.Keyboard.Press(global::FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);

            WaitUntil(() =>
            {
                var current = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
                    .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                    .ToHashSet();
                return !current.Except(beforePopups).Any();
            }, TimeSpan.FromSeconds(5));
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    private static AutomationElement WaitForPopup(AutomationElement desktop, Window mainWindow, HashSet<nint> beforeHandles, TimeSpan timeout)
    {
        AutomationElement? popup = null;
        WaitUntil(() =>
        {
            popup = TryGetNewPopup(desktop, mainWindow, beforeHandles)
                    ?? TryGetAnyPopup(desktop, mainWindow);
            return popup != null;
        }, timeout);
        return popup!;
    }

    private static AutomationElement? TryGetNewPopup(AutomationElement desktop, Window mainWindow, HashSet<nint> beforeHandles)
    {
        var mainHandle = mainWindow.Properties.NativeWindowHandle.ValueOrDefault;
        var popups = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
            .ToList();

        return popups.FirstOrDefault(p =>
        {
            var h = p.Properties.NativeWindowHandle.ValueOrDefault;
            if (h == mainHandle)
                return false;
            if (beforeHandles.Contains(h))
                return false;

            var r = p.BoundingRectangle;
            // Heuristic: popup menu is a small surface.
            return r.Width > 0 && r.Width < 1600 && r.Height > 0 && r.Height < 1600;
        });
    }

    private static AutomationElement? TryGetAnyPopup(AutomationElement desktop, Window mainWindow)
    {
        var mainHandle = mainWindow.Properties.NativeWindowHandle.ValueOrDefault;
        var popups = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
            .ToList();

        var candidates = popups
            .Where(p =>
            {
                var h = p.Properties.NativeWindowHandle.ValueOrDefault;
                if (h == mainHandle)
                    return false;

                var r = p.BoundingRectangle;
                return r.Width > 0 && r.Width < 1200 && r.Height > 0 && r.Height < 1200;
            })
            .OrderBy(p => p.BoundingRectangle.Width * p.BoundingRectangle.Height)
            .ToList();

        return candidates.FirstOrDefault();
    }

    private static void AssertWithinWorkingArea(AutomationElement popup)
    {
        var r = popup.BoundingRectangle;
        const int tolerancePx = 16;
        var screen = System.Windows.Forms.Screen.FromRectangle(System.Drawing.Rectangle.FromLTRB(
            (int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom));
        var wa = screen.WorkingArea;

        Assert.True(r.Left >= wa.Left - tolerancePx, $"Expected popup within working area (left). popup={r}, workArea={wa}");
        Assert.True(r.Top >= wa.Top - tolerancePx, $"Expected popup within working area (top). popup={r}, workArea={wa}");
        Assert.True(r.Right <= wa.Right + tolerancePx, $"Expected popup within working area (right). popup={r}, workArea={wa}");
        Assert.True(r.Bottom <= wa.Bottom + tolerancePx, $"Expected popup within working area (bottom). popup={r}, workArea={wa}");
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

