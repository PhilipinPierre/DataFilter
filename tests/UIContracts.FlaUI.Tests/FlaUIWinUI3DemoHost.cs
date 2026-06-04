using System.Diagnostics;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIWinUI3DemoHost
{
    public static string BuildAndGetExePath()
    {
        var root = FindRepoRoot();
        var exe = Path.Combine(
            root,
            "demo",
            "DataFilter.WinUI3.Demo",
            "bin",
            "x64",
            "Release",
            "net8.0-windows10.0.19041.0",
            "DataFilter.WinUI3.Demo.exe");

        if (!File.Exists(exe))
            throw new FileNotFoundException($"WinUI3 demo not built. Expected: {exe}");

        return exe;
    }

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

    public static void NavigateToAttach(Window window)
    {
        var attachNavItem = window.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.ListItem).And(cf.ByName(UIContracts.Common.DemoViewCatalog.WinUi3.AttachNav)));
        attachNavItem?.AsListBoxItem().Select();
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
