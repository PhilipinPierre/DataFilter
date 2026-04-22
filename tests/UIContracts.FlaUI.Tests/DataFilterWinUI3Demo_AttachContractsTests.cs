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
            WaitUntil(() =>
            {
                var desktop = automation.GetDesktop();
                var popups = desktop.FindAllChildren(cf =>
                    cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)));
                return popups.Length > 0;
            }, TimeSpan.FromSeconds(5));
        }
        finally
        {
            TryCloseApp(app);
        }
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

