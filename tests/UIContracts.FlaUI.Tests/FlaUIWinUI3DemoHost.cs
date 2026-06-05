using System.Diagnostics;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIWinUI3DemoHost
{
    private static readonly DemoExeSpec WinUI3Demo = new(
        "demo/DataFilter.WinUI3.Demo/DataFilter.WinUI3.Demo.csproj",
        "DataFilter.WinUI3.Demo.exe",
        "net8.0-windows10.0.19041.0",
        Platform: "x64");

    public static string BuildAndGetExePath() => DemoExePathResolver.ResolveOrBuild(WinUI3Demo);

    public static bool IsWinAppRuntimeAvailable(string exe)
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
            if (p == null)
                return false;

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

    public static void NavigateToAttach(Window window) =>
        NavigateToNavItem(window, UIContracts.Common.DemoViewCatalog.WinUi3.AttachNav);

    public static void NavigateToAsync(Window window) =>
        NavigateToNavItem(window, UIContracts.Common.DemoViewCatalog.WinUi3.AsyncNav);

    public static void NavigateToHybrid(Window window) =>
        NavigateToNavItem(window, UIContracts.Common.DemoViewCatalog.WinUi3.HybridNav);

    public static void NavigateToListView(Window window) =>
        NavigateToNavItem(window, UIContracts.Common.DemoViewCatalog.WinUi3.ListViewNav);

    public static void NavigateToCollectionView(Window window) =>
        NavigateToNavItem(window, UIContracts.Common.DemoViewCatalog.WinUi3.CollectionViewNav);

    public static void ApplyEqualsFilter(Window window, UIA3Automation automation, string property, string value)
    {
        var btn = window.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.Button).And(cf.ByAutomationId($"df-filter-btn-{property}")));
        btn?.AsButton().Invoke();

        var popup = automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId($"df-filter-popup-{property}"));
        if (popup == null)
            return;

        var opCombo = popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox))?.AsComboBox();
        opCombo?.Expand();
        opCombo?.Items.FirstOrDefault(i => (i.Properties.Name.ValueOrDefault ?? "").Contains("Equals", StringComparison.OrdinalIgnoreCase))?.Select();

        var edits = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(e => e.AsTextBox()).ToList();
        if (edits.Count > 0)
            edits[0].Text = value;

        var ok = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.Button).Or(cf.ByAutomationId("df-ok")))
            .Select(b => b.AsButton())
            .FirstOrDefault(b => string.Equals(b.Properties.Name.ValueOrDefault, "OK", StringComparison.OrdinalIgnoreCase));
        ok?.Invoke();
    }

    public static List<string> GetVisibleRowTexts(Window window, string automationId) =>
        window.FindAllDescendants(cf => cf.ByAutomationId(automationId))
            .Where(x => x.Properties.IsOffscreen.ValueOrDefault == false)
            .Select(x => (x.AsLabel().Text ?? x.Properties.Name.ValueOrDefault ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

    private static void NavigateToNavItem(Window window, string navText)
    {
        var item = window.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.ListItem).And(cf.ByName(navText)));
        item?.AsListBoxItem().Select();
        Thread.Sleep(500);
    }

    public static void TryCloseApp(global::FlaUI.Core.Application app)
    {
        try { app.Close(); } catch { }
        try
        {
            if (app.ProcessId > 0)
                Process.GetProcessById(app.ProcessId).Kill(entireProcessTree: true);
        }
        catch { }
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

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
