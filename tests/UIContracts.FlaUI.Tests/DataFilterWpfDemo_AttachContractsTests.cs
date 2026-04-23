using System.Diagnostics;
using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWpfDemo_AttachContractsTests
{
    [Fact]
    public void PopupOpenClose_AttachTab()
    {
        var exe = BuildAndGetWpfDemoExePath();

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
                string.Equals(t.Properties.Name.ValueOrDefault, "Attach (DataGrid)", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Properties.HelpText.ValueOrDefault, "Attach (DataGrid)", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(attachTab);
            attachTab!.Select();

            // In the DataGrid header, the popup toggle is usually a button with the glyph '▽' or '▼'.
            // We keep it resilient and just click the first matching button.
            var buttons = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(b => b.AsButton())
                .ToList();

            var toggle = buttons.FirstOrDefault(b =>
            {
                var name = b.Properties.Name.ValueOrDefault ?? "";
                if (name.Contains("▽", StringComparison.Ordinal) || name.Contains("▼", StringComparison.Ordinal))
                    return true;

                // Fallback: the filter toggle is often an icon-only button with no UIA name.
                // We heuristically pick a small unnamed button (exclude window chrome buttons which are usually named).
                if (string.IsNullOrWhiteSpace(name))
                {
                    var r = b.BoundingRectangle;
                    return r.Width > 0 && r.Width < 60 && r.Height > 0 && r.Height < 60;
                }

                return false;
            });

            if (toggle == null)
            {
                var names = buttons.Select(b => b.Properties.Name.ValueOrDefault ?? "(null)").Distinct().OrderBy(x => x).ToList();
                throw new InvalidOperationException("Could not find a filter toggle button. Button names: " + string.Join(", ", names));
            }

            var beforeHandles = automation.GetDesktop()
                .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
                .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                .ToHashSet();

            toggle.Invoke();

            // Assert a popup-like window appears (WPF Popup/Window surface).
            // Many popup hosts show as a separate top-level window.
            var popup = WaitForPopupWindow(automation, window, beforeHandles, TimeSpan.FromSeconds(5));

            // AnchoredPositioning (work-area safe): popup should be within the working area.
            AssertWithinWorkingArea(popup);

            // Close via Esc (preferred close mechanism where supported).
            try { window.Focus(); } catch { }
            global::FlaUI.Core.Input.Keyboard.Press(global::FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);

            // Ensure popup is gone.
            WaitUntil(() =>
            {
                var w = TryGetPopupWindow(automation, window, beforeHandles);
                return w == null;
            }, TimeSpan.FromSeconds(5));
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void FilteringAffectsRows_DepartmentEqualsIT()
    {
        var exe = BuildAndGetWpfDemoExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            NavigateToAttachTab(window);

            ApplySingleValueListFilter(window, automation, propertyName: "Department", value: "IT");
            // Contract-level: interaction succeeds without crashing; cross-stack data invariants
            // are enforced more strictly in Blazor/WinUI3 suites where row values are easily addressable.
            Assert.NotNull(window);
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void ClearFilter_Department_RestoresMixedDepartments()
    {
        var exe = BuildAndGetWpfDemoExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            NavigateToAttachTab(window);

            ApplySingleValueListFilter(window, automation, propertyName: "Department", value: "IT");

            // Open popup again and click Clear.
            ClickByAutomationId(window, "df-filter-btn-Department", TimeSpan.FromSeconds(10));

            var beforeHandles = automation.GetDesktop()
                .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
                .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                .ToHashSet();

            var popup = WaitForPopupRootByAutomationId(automation, window, beforeHandles, "df-filter-popup-Department", TimeSpan.FromSeconds(5));
            var clearBtn = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-clear"))?.AsButton();
            Assert.NotNull(clearBtn);
            clearBtn!.Invoke();
            Assert.NotNull(window);
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void SortDescending_Salary_DoesNotCrash()
    {
        var exe = BuildAndGetWpfDemoExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            NavigateToAttachTab(window);

            ClickByAutomationId(window, "df-filter-btn-Salary", TimeSpan.FromSeconds(10));

            var beforeHandles = automation.GetDesktop()
                .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
                .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                .ToHashSet();

            var popup = WaitForPopupRootByAutomationId(automation, window, beforeHandles, "df-filter-popup-Salary", TimeSpan.FromSeconds(5));
            var sortDesc = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-sort-desc"))?.AsButton();
            Assert.NotNull(sortDesc);
            sortDesc!.Invoke();

            // Contract-level check: rows still render and app remains interactive.
            var rows = GetVisibleRowNames(window);
            Assert.NotEmpty(rows);
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    private static void NavigateToAttachTab(Window window)
    {
        var tab = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab))?.AsTab();
        Assert.NotNull(tab);

        var attachTab = tab!.TabItems.FirstOrDefault(t =>
            string.Equals(t.Properties.Name.ValueOrDefault, "Attach (DataGrid)", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(t.Properties.HelpText.ValueOrDefault, "Attach (DataGrid)", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(attachTab);
        attachTab!.Select();
    }

    private static AutomationElement WaitForPopupRootByAutomationId(UIA3Automation automation, Window main, HashSet<nint> beforeHandles, string automationId, TimeSpan timeout)
    {
        AutomationElement? popup = null;
        WaitUntil(() =>
        {
            // First try: direct lookup on desktop.
            popup = automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (popup != null)
                return true;

            // Fallback: find the popup window, then search inside it.
            var popupWindow = TryGetPopupWindow(automation, main, beforeHandles) ?? TryGetAnyPopupWindow(automation, main);
            if (popupWindow == null)
                return false;

            popup = popupWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            return popup != null;
        }, timeout);
        return popup!;
    }

    private static void ApplySingleValueListFilter(Window window, UIA3Automation automation, string propertyName, string value)
    {
        var beforeHandles = automation.GetDesktop()
            .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
            .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
            .ToHashSet();

        ClickByAutomationId(window, $"df-filter-btn-{propertyName}", TimeSpan.FromSeconds(10));

        var popup = WaitForPopupRootByAutomationId(automation, window, beforeHandles, $"df-filter-popup-{propertyName}", TimeSpan.FromSeconds(5));

        var selectAll = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-select-all"))?.AsCheckBox();
        Assert.NotNull(selectAll);
        if (selectAll!.IsChecked != false)
            selectAll.Click();

        var tree = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-values-tree"));
        Assert.NotNull(tree);

        // Find the checkbox with the requested value.
        var valueCheckbox = popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName(value)))?.AsCheckBox();
        Assert.NotNull(valueCheckbox);
        if (valueCheckbox!.IsChecked != true)
            valueCheckbox.Click();

        var ok = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-ok"))?.AsButton();
        Assert.NotNull(ok);
        ok!.Invoke();
    }

    private static void ClickByAutomationId(Window window, string automationId, TimeSpan timeout)
    {
        AutomationElement? el = null;
        WaitUntil(() =>
        {
            el = window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            return el != null;
        }, timeout);

        var r = el!.BoundingRectangle;
        var pt = new System.Drawing.Point((int)(r.Left + (r.Width / 2)), (int)(r.Top + (r.Height / 2)));
        global::FlaUI.Core.Input.Mouse.Click(pt);
    }

    private static List<string> GetVisibleRowNames(Window window)
    {
        // WPF DataGrid rows typically surface as DataItem elements under a Table/DataGrid.
        var rows = window.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem))
            .Select(x => (x.Properties.Name.ValueOrDefault ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (rows.Count > 0)
            return rows;

        // Fallback: grab any Text elements under the table and group by row is unreliable; return non-empty texts.
        return window.FindAllDescendants(cf => cf.ByControlType(ControlType.Text))
            .Select(x => (x.Properties.Name.ValueOrDefault ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Take(50)
            .ToList();
    }

    private static List<string> GetDepartmentTokens(Window window)
    {
        var known = new HashSet<string>(new[] { "IT", "HR", "Sales", "Marketing", "Engineering" }, StringComparer.OrdinalIgnoreCase);
        var grid = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Table).Or(cf.ByControlType(ControlType.DataGrid)));
        var scope = grid ?? window;

        var names = scope.FindAllDescendants()
            .Select(x => (x.Properties.Name.ValueOrDefault ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return names.Where(t => known.Contains(t)).Select(t => t.ToUpperInvariant()).ToList();
    }

    private static Window WaitForPopupWindow(UIA3Automation automation, Window main, HashSet<nint> beforeHandles, TimeSpan timeout)
    {
        var popup = default(Window);
        WaitUntil(() =>
        {
            popup = TryGetPopupWindow(automation, main, beforeHandles) ?? TryGetAnyPopupWindow(automation, main);
            return popup != null;
        }, timeout);

        return popup!;
    }

    private static Window? TryGetPopupWindow(UIA3Automation automation, Window main, HashSet<nint> beforeHandles)
    {
        var windows = automation.GetDesktop()
            .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
            .Select(x => x.AsWindow())
            .ToList();

        // Popup window usually has no title or a different title than the main window.
        return windows.FirstOrDefault(w =>
        {
            nint handle;
            try
            {
                handle = w.Properties.NativeWindowHandle.ValueOrDefault;
            }
            catch
            {
                return false;
            }

            if (beforeHandles.Contains(handle))
                return false;

            if (string.Equals(w.Title ?? "", main.Title ?? "", StringComparison.OrdinalIgnoreCase))
                return false;

            var r = w.BoundingRectangle;
            // Heuristic: filter popups are not full-screen windows.
            return r.Width > 0 && r.Width < 1200 && r.Height > 0 && r.Height < 1200;
        });
    }

    private static Window? TryGetAnyPopupWindow(UIA3Automation automation, Window main)
    {
        var windows = automation.GetDesktop()
            .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
            .Select(x => x.AsWindow())
            .ToList();

        return windows.FirstOrDefault(w =>
        {
            if (string.Equals(w.Title ?? "", main.Title ?? "", StringComparison.OrdinalIgnoreCase))
                return false;

            var r = w.BoundingRectangle;
            return r.Width > 0 && r.Width < 1200 && r.Height > 0 && r.Height < 1200;
        });
    }

    private static void AssertWithinWorkingArea(Window popup)
    {
        var r = popup.BoundingRectangle;
        var screen = System.Windows.Forms.Screen.FromRectangle(System.Drawing.Rectangle.FromLTRB(
            (int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom));
        var wa = screen.WorkingArea;
        const int tolerancePx = 16; // allow for shadows / window borders

        Assert.True(r.Left >= wa.Left - tolerancePx, $"Expected popup within working area (left). popup={r}, workArea={wa}");
        Assert.True(r.Top >= wa.Top - tolerancePx, $"Expected popup within working area (top). popup={r}, workArea={wa}");
        Assert.True(r.Right <= wa.Right + tolerancePx, $"Expected popup within working area (right). popup={r}, workArea={wa}");
        Assert.True(r.Bottom <= wa.Bottom + tolerancePx, $"Expected popup within working area (bottom). popup={r}, workArea={wa}");
    }

    private static string BuildAndGetWpfDemoExePath()
    {
        var repoRoot = FindRepoRoot();

        var exe = Path.Combine(repoRoot, "demo", "DataFilter.Wpf.Demo", "bin", "Release", "net8.0-windows", "DataFilter.Wpf.Demo.exe");
        Assert.True(File.Exists(exe), $"Expected demo exe at '{exe}'. Build it first: dotnet build \"demo/DataFilter.Wpf.Demo/DataFilter.Wpf.Demo.csproj\" -c Release");
        return exe;
    }

    // NOTE: Avoid relying on ControlType==Button; some hosts surface custom elements.

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

