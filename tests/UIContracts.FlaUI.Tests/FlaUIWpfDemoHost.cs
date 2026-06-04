using System.Diagnostics;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIWpfDemoHost
{
    public static string BuildAndGetExePath()
    {
        var root = FindRepoRoot();
        var exe = Path.Combine(root, "demo", "DataFilter.Wpf.Demo", "bin", "Release", "net8.0-windows", "DataFilter.Wpf.Demo.exe");
        if (!File.Exists(exe))
            throw new FileNotFoundException($"WPF demo not built. Expected: {exe}. Run dotnet build first.");
        return exe;
    }

    public static void NavigateToAttachTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.Wpf.AttachTab);

    public static void NavigateToLocalTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.Wpf.LocalTab);

    public static void NavigateToAsyncTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.Wpf.AsyncTab);

    public static void NavigateToHybridTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.Wpf.HybridTab);

    public static void NavigateToListViewTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.Wpf.ListViewTab);

    public static void NavigateToCollectionViewTab(Window window) =>
        SelectTab(window, UIContracts.Common.DemoViewCatalog.Wpf.CollectionViewTab);

    public static void ApplyListFilter(Window window, UIA3Automation automation, string propertyName, string value)
    {
        var beforeHandles = automation.GetDesktop()
            .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
            .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
            .ToHashSet();

        ClickByAutomationId(window, $"df-filter-btn-{propertyName}", TimeSpan.FromSeconds(15));

        var popup = WaitForPopupRoot(automation, $"df-filter-popup-{propertyName}", TimeSpan.FromSeconds(10));

        var selectAll = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-select-all"))?.AsCheckBox();
        if (selectAll != null && selectAll.IsChecked != false)
            selectAll.Click();

        var valueCheckbox = popup.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName(value)))?.AsCheckBox();
        if (valueCheckbox != null && valueCheckbox.IsChecked != true)
            valueCheckbox.Click();

        var ok = popup.FindFirstDescendant(cf => cf.ByAutomationId("df-ok"))?.AsButton();
        ok?.Invoke();

        _ = beforeHandles;
    }

    private static AutomationElement WaitForPopupRoot(UIA3Automation automation, string automationId, TimeSpan timeout)
    {
        AutomationElement? popup = null;
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            popup = automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (popup != null)
                return popup;
            Thread.Sleep(100);
        }

        throw new TimeoutException($"Popup '{automationId}' not found.");
    }

    private static void SelectTab(Window window, string tabHeader)
    {
        var tabs = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab));
        var tabItems = tabs?.FindAllChildren(cf => cf.ByControlType(ControlType.TabItem));
        if (tabItems == null || tabItems.Length == 0)
            throw new InvalidOperationException("Tab control not found.");

        foreach (var tab in tabItems)
        {
            var name = tab.Properties.Name.ValueOrDefault ?? string.Empty;
            if (string.Equals(name, tabHeader, StringComparison.OrdinalIgnoreCase) ||
                name.Contains(tabHeader, StringComparison.OrdinalIgnoreCase))
            {
                tab.Click();
                return;
            }
        }

        throw new InvalidOperationException($"Tab '{tabHeader}' not found.");
    }

    public static void ClickByAutomationId(Window window, string automationId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        AutomationElement? el = null;
        while (DateTime.UtcNow < deadline)
        {
            el = window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (el != null)
                break;
            Thread.Sleep(100);
        }

        if (el == null)
            throw new InvalidOperationException($"Could not find AutomationId '{automationId}' within {timeout}.");

        el.Click();
    }

    public static void TryCloseApp(global::FlaUI.Core.Application app)
    {
        try
        {
            app.Close();
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

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "DataFilter.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Could not locate repository root (DataFilter.slnx).");
    }
}
