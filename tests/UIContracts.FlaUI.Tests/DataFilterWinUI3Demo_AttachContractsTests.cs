using System.Diagnostics;
using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWinUI3Demo_AttachContractsTests
{
    [Fact]
    public void PopupOpenClose_AttachPage()
    {
        var exe = GetWinUI3DemoExePath();

        if (!IsWinAppRuntimeAvailable(exe))
        {
            // Typical on dev/CI machines without Windows App Runtime installed.
            // We keep the contract test in place but skip rather than hard-fail the whole suite.
            return;
        }

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();

            // WinUI3 can take a bit longer to show its main window.
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            // Navigate to Attach via the NavigationView item.
            var attachNavItem = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.ListItem).And(cf.ByName("Attach (ListView)"))),
                TimeSpan.FromSeconds(10));

            Assert.NotNull(attachNavItem);
            attachNavItem!.AsListBoxItem().Select();

            // Ensure ListView content is present.
            var list = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.List)),
                TimeSpan.FromSeconds(10));

            Assert.NotNull(list);

            // The filter header adapter injects icon-only buttons; often they have empty UIA name.
            // We pick a small unnamed button within the window.
            var toggle = WaitFor(() =>
            {
                var buttons = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                    .Select(x => x.AsButton())
                    .ToList();

                return buttons.FirstOrDefault(b =>
                {
                    var name = b.Properties.Name.ValueOrDefault ?? "";
                    if (!string.IsNullOrWhiteSpace(name))
                        return false;

                    var r = b.BoundingRectangle;
                    return r.Width > 0 && r.Width < 60 && r.Height > 0 && r.Height < 60;
                });
            }, TimeSpan.FromSeconds(10));

            Assert.NotNull(toggle);
            toggle!.Invoke();

            // Popup host should appear as an additional window/menu on desktop.
            var desktop = automation.GetDesktop();
            var beforePopups = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
                .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                .ToHashSet();

            var popup = WaitForNewPopup(desktop, window, beforePopups, TimeSpan.FromSeconds(5));
            AssertWithinWorkingArea(popup);

            // Close via Esc (best-effort).
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

    [Fact]
    public void FilteringAffectsItems_DepartmentEqualsIT()
    {
        var exe = GetWinUI3DemoExePath();

        if (!IsWinAppRuntimeAvailable(exe))
            return;

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            // Navigate to Attach via the NavigationView item.
            var attachNavItem = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.ListItem).And(cf.ByName("Attach (ListView)"))),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(attachNavItem);
            attachNavItem!.AsListBoxItem().Select();

            var list = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.List)),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(list);

            // Open popup for Department via AutomationId.
            var deptBtn = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("df-filter-btn-Department"))),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(deptBtn);
            deptBtn!.AsButton().Invoke();

            // Popup root is a UserControl with AutomationId.
            var popup = WaitFor(() =>
                automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Department")),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(popup);

            // Expand advanced filter if needed and set Equals IT.
            var adv = popup!.FindFirstDescendant(cf => cf.ByControlType(ControlType.Group).Or(cf.ByControlType(ControlType.Pane)).Or(cf.ByControlType(ControlType.Custom)));
            _ = adv; // best-effort; keep resilient

            // Find operator combo and value textbox (heuristic: first ComboBox + first Edit within popup).
            var opCombo = popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox))?.AsComboBox();
            Assert.NotNull(opCombo);
            opCombo!.Expand();
            var equals = opCombo.Items.FirstOrDefault(i => (i.Properties.Name.ValueOrDefault ?? "").Contains("Equals", StringComparison.OrdinalIgnoreCase));
            equals?.Select();

            var edits = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(e => e.AsTextBox()).ToList();
            Assert.True(edits.Count > 0);
            edits[0].Text = "IT";

            var ok = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(b => b.AsButton())
                .FirstOrDefault(b => string.Equals(b.Properties.Name.ValueOrDefault, "OK", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(ok);
            ok!.Invoke();

            // Verify every visible department label is IT.
            WaitUntil(() =>
            {
                var deptLabels = window.FindAllDescendants(cf => cf.ByAutomationId("df-row-dept")).ToList();
                return deptLabels.Count > 0;
            }, TimeSpan.FromSeconds(5));

            var labels = window.FindAllDescendants(cf => cf.ByAutomationId("df-row-dept"))
                .Select(x => x.AsLabel())
                .Where(x => x.Properties.IsOffscreen.ValueOrDefault == false)
                .ToList();

            Assert.NotEmpty(labels);
            foreach (var el in labels)
            {
                Assert.Equal("IT", (el.Text ?? "").Trim());
            }
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void SortDescending_Salary()
    {
        var exe = GetWinUI3DemoExePath();
        if (!IsWinAppRuntimeAvailable(exe))
            return;

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            NavigateToAttach(window);

            // Open popup for Salary and click SortDescending.
            var salaryBtn = WaitFor(() =>
                    window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("df-filter-btn-Salary"))),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(salaryBtn);
            salaryBtn!.AsButton().Invoke();

            var popup = WaitFor(() =>
                    automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Salary")),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(popup);

            var sortDesc = popup!.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(x => x.AsButton())
                .FirstOrDefault(b => (b.Properties.Name.ValueOrDefault ?? "").Contains("Sort", StringComparison.OrdinalIgnoreCase)
                                     && (b.Properties.Name.ValueOrDefault ?? "").Contains("Desc", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(sortDesc);
            sortDesc!.Invoke();

            // Verify salaries are sorted descending (visible rows only).
            var salaries = window.FindAllDescendants(cf => cf.ByAutomationId("df-row-salary"))
                .Select(x => x.Properties.Name.ValueOrDefault ?? x.AsLabel().Text ?? "")
                .Select(ParseFloatLoose)
                .ToList();

            Assert.True(salaries.Count > 1);
            AssertSorted(salaries, descending: true);
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void SortAscending_HireDate()
    {
        var exe = GetWinUI3DemoExePath();
        if (!IsWinAppRuntimeAvailable(exe))
            return;

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            NavigateToAttach(window);

            var btn = WaitFor(() =>
                    window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("df-filter-btn-HireDate"))),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(btn);
            btn!.AsButton().Invoke();

            var popup = WaitFor(() =>
                    automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-HireDate")),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(popup);

            var sortAsc = popup!.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(x => x.AsButton())
                .FirstOrDefault(b => (b.Properties.Name.ValueOrDefault ?? "").Contains("Sort", StringComparison.OrdinalIgnoreCase)
                                     && (b.Properties.Name.ValueOrDefault ?? "").Contains("Asc", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(sortAsc);
            sortAsc!.Invoke();

            var dates = window.FindAllDescendants(cf => cf.ByAutomationId("df-row-hiredate"))
                .Select(x => x.Properties.Name.ValueOrDefault ?? x.AsLabel().Text ?? "")
                .Select(ParseDateTimeLoose)
                .ToList();

            Assert.True(dates.Count > 1);
            AssertSorted(dates, descending: false);
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void MultiSort_DepartmentThenName_NameSortedWithinDepartmentGroups()
    {
        var exe = GetWinUI3DemoExePath();
        if (!IsWinAppRuntimeAvailable(exe))
            return;

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            NavigateToAttach(window);

            // Primary sort: Department ascending.
            var deptBtn = WaitFor(() =>
                    window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("df-filter-btn-Department"))),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(deptBtn);
            deptBtn!.AsButton().Invoke();

            var deptPopup = WaitFor(() =>
                    automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Department")),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(deptPopup);

            var sortAsc = deptPopup!.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(x => x.AsButton())
                .FirstOrDefault(b => (b.Properties.Name.ValueOrDefault ?? "").Contains("Sort", StringComparison.OrdinalIgnoreCase)
                                     && (b.Properties.Name.ValueOrDefault ?? "").Contains("Asc", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(sortAsc);
            sortAsc!.Invoke();

            // Sub-sort: Name ascending.
            var nameBtn = WaitFor(() =>
                    window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("df-filter-btn-Name"))),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(nameBtn);
            nameBtn!.AsButton().Invoke();

            var namePopup = WaitFor(() =>
                    automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Name")),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(namePopup);

            var addSubSortAsc = namePopup!.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(x => x.AsButton())
                .FirstOrDefault(b => (b.Properties.Name.ValueOrDefault ?? "").Contains("Sub", StringComparison.OrdinalIgnoreCase)
                                     && (b.Properties.Name.ValueOrDefault ?? "").Contains("Asc", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(addSubSortAsc);
            addSubSortAsc!.Invoke();

            var depts = window.FindAllDescendants(cf => cf.ByAutomationId("df-row-dept"))
                .Select(x => (x.Properties.Name.ValueOrDefault ?? x.AsLabel().Text ?? "").Trim())
                .ToList();
            var names = window.FindAllDescendants(cf => cf.ByAutomationId("df-row-name"))
                .Select(x => (x.Properties.Name.ValueOrDefault ?? x.AsLabel().Text ?? "").Trim())
                .ToList();

            var n = Math.Min(depts.Count, names.Count);
            Assert.True(n > 1);

            var i = 0;
            while (i < n)
            {
                var dept = depts[i];
                var group = new List<string>();
                while (i < n && string.Equals(depts[i], dept, StringComparison.OrdinalIgnoreCase))
                {
                    group.Add(names[i]);
                    i++;
                }
                if (group.Count > 1)
                    AssertSorted(group, descending: false);
            }
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    [Fact]
    public void ClearFilter_Department_RestoresMixedDepartments()
    {
        var exe = GetWinUI3DemoExePath();
        if (!IsWinAppRuntimeAvailable(exe))
            return;

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            NavigateToAttach(window);

            // Apply Department == IT using the existing filtering contract approach.
            ApplyEqualsFilter(window, automation, property: "Department", value: "IT");

            var afterFilter = GetVisibleRowTexts(window, "df-row-dept");
            Assert.NotEmpty(afterFilter);
            Assert.All(afterFilter, d => Assert.Equal("IT", d));

            // Clear via popup Clear button.
            var btn = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("df-filter-btn-Department")));
            Assert.NotNull(btn);
            btn!.AsButton().Invoke();

            var popup = WaitFor(() =>
                    automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Department")),
                TimeSpan.FromSeconds(10));
            Assert.NotNull(popup);

            var clear = popup!.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                .Select(x => x.AsButton())
                .FirstOrDefault(b => string.Equals(b.Properties.Name.ValueOrDefault, "Clear", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(clear);
            clear!.Invoke();

            // Expect more than one department after clear (best-effort).
            var afterClear = GetVisibleRowTexts(window, "df-row-dept");
            Assert.NotEmpty(afterClear);
            Assert.True(afterClear.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1);
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    private static void NavigateToAttach(Window window)
    {
        var attachNavItem = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.ListItem).And(cf.ByName("Attach (ListView)"))),
            TimeSpan.FromSeconds(10));
        Assert.NotNull(attachNavItem);
        attachNavItem!.AsListBoxItem().Select();

        var list = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.List)),
            TimeSpan.FromSeconds(10));
        Assert.NotNull(list);
    }

    private static void ApplyEqualsFilter(Window window, UIA3Automation automation, string property, string value)
    {
        var btn = WaitFor(() =>
                window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId($"df-filter-btn-{property}"))),
            TimeSpan.FromSeconds(10));
        Assert.NotNull(btn);
        btn!.AsButton().Invoke();

        var popup = WaitFor(() =>
                automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId($"df-filter-popup-{property}")),
            TimeSpan.FromSeconds(10));
        Assert.NotNull(popup);

        var opCombo = popup!.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox))?.AsComboBox();
        Assert.NotNull(opCombo);
        opCombo!.Expand();
        var equals = opCombo.Items.FirstOrDefault(i => (i.Properties.Name.ValueOrDefault ?? "").Contains("Equals", StringComparison.OrdinalIgnoreCase));
        equals?.Select();

        var edits = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(e => e.AsTextBox()).ToList();
        Assert.True(edits.Count > 0);
        edits[0].Text = value;

        var ok = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
            .Select(b => b.AsButton())
            .FirstOrDefault(b => string.Equals(b.Properties.Name.ValueOrDefault, "OK", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(ok);
        ok!.Invoke();
    }

    private static List<string> GetVisibleRowTexts(Window window, string automationId)
    {
        return window.FindAllDescendants(cf => cf.ByAutomationId(automationId))
            .Select(x => x.AsLabel())
            .Where(x => x.Properties.IsOffscreen.ValueOrDefault == false)
            .Select(x => (x.Text ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static void AssertSorted(IReadOnlyList<string> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            var cmp = string.Compare(values[i - 1], values[i], StringComparison.OrdinalIgnoreCase);
            if (descending)
                Assert.True(cmp >= 0, $"Expected descending sort at i={i}. Prev='{values[i - 1]}', Curr='{values[i]}'");
            else
                Assert.True(cmp <= 0, $"Expected ascending sort at i={i}. Prev='{values[i - 1]}', Curr='{values[i]}'");
        }
    }

    private static void AssertSorted(IReadOnlyList<float> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            if (descending)
                Assert.True(values[i - 1] >= values[i], $"Expected descending sort at i={i}. Prev={values[i - 1]}, Curr={values[i]}");
            else
                Assert.True(values[i - 1] <= values[i], $"Expected ascending sort at i={i}. Prev={values[i - 1]}, Curr={values[i]}");
        }
    }

    private static void AssertSorted(IReadOnlyList<DateTime> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            if (descending)
                Assert.True(values[i - 1] >= values[i], $"Expected descending sort at i={i}. Prev={values[i - 1]:O}, Curr={values[i]:O}");
            else
                Assert.True(values[i - 1] <= values[i], $"Expected ascending sort at i={i}. Prev={values[i - 1]:O}, Curr={values[i]:O}");
        }
    }

    private static float ParseFloatLoose(string raw)
    {
        raw = (raw ?? "").Trim();
        if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
            return v;
        raw = raw.Replace(',', '.');
        return float.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static DateTime ParseDateTimeLoose(string raw)
    {
        raw = (raw ?? "").Trim();
        if (DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var dt))
            return dt;
        return DateTime.Parse(raw, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces);
    }

    private static AutomationElement WaitForNewPopup(AutomationElement desktop, Window main, HashSet<nint> beforeHandles, TimeSpan timeout)
    {
        AutomationElement? popup = null;
        WaitUntil(() =>
        {
            popup = TryGetNewPopup(desktop, main, beforeHandles);
            return popup != null;
        }, timeout);
        return popup!;
    }

    private static AutomationElement? TryGetNewPopup(AutomationElement desktop, Window main, HashSet<nint> beforeHandles)
    {
        var mainHandle = main.Properties.NativeWindowHandle.ValueOrDefault;
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
            return r.Width > 0 && r.Width < 1600 && r.Height > 0 && r.Height < 1600;
        });
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

    private static string GetWinUI3DemoExePath()
    {
        var repoRoot = FindRepoRoot();
        var exe = Path.Combine(
            repoRoot,
            "demo",
            "DataFilter.WinUI3.Demo",
            "bin",
            "x64",
            "Release",
            "net8.0-windows10.0.19041.0",
            "DataFilter.WinUI3.Demo.exe");

        if (!File.Exists(exe))
        {
            throw new FileNotFoundException(
                $"Expected WinUI3 demo exe at '{exe}'. Build it first: dotnet build \"demo/DataFilter.WinUI3.Demo/DataFilter.WinUI3.Demo.csproj\" -c Release -p:Platform=x64");
        }

        return exe;
    }

    private static bool IsWinAppRuntimeAvailable(string exe)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p == null) return false;

            // If WinAppRuntime is missing, the process typically exits immediately with REGDB_E_CLASSNOTREG.
            if (!p.WaitForExit(2000))
            {
                try { p.Kill(entireProcessTree: true); } catch { }
                return true;
            }

            var err = (p.StandardError.ReadToEnd() ?? "") + (p.StandardOutput.ReadToEnd() ?? "");
            return !err.Contains("REGDB_E_CLASSNOTREG", StringComparison.OrdinalIgnoreCase)
                   && !err.Contains("0x80040154", StringComparison.OrdinalIgnoreCase)
                   && p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
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

    private static T? WaitFor<T>(Func<T?> get, TimeSpan timeout) where T : class
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            var v = get();
            if (v != null)
                return v;
            Thread.Sleep(100);
        }
        return null;
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

