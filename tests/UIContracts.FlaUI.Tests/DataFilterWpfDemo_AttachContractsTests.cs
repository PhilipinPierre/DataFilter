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

            toggle.Invoke();

            // Assert a popup-like window appears (WPF Popup/Window surface).
            // Many popup hosts show as a separate top-level window. We look for any non-main window.
            WaitUntil(() =>
            {
                var windows = automation.GetDesktop()
                    .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
                    .Select(x => x.AsWindow())
                    .ToList();
                return windows.Any(w => !string.Equals(w.Title, window.Title, StringComparison.OrdinalIgnoreCase));
            }, TimeSpan.FromSeconds(5));
        }
        finally
        {
            TryCloseApp(app);
        }
    }

    private static string BuildAndGetWpfDemoExePath()
    {
        var repoRoot = FindRepoRoot();

        // Avoid building the app from inside UI tests (it makes test runs slow and flaky).
        // The demo exe should exist after a normal solution build.
        var exe = Path.Combine(repoRoot, "demo", "DataFilter.Wpf.Demo", "bin", "Release", "net8.0-windows", "DataFilter.Wpf.Demo.exe");
        Assert.True(File.Exists(exe), $"Expected demo exe at '{exe}'. Build it first: dotnet build \"demo/DataFilter.Wpf.Demo/DataFilter.Wpf.Demo.csproj\" -c Release");
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

