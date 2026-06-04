using System.Drawing;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIInputHelpers
{
    public static void Focus(Window window)
    {
        try
        {
            window.Focus();
        }
        catch
        {
            // ignored
        }
    }

    public static void Activate(Window window) => Focus(window);

    public static void InvokeOrClick(AutomationElement element)
    {
        try
        {
            element.AsButton().Invoke();
            return;
        }
        catch
        {
            // ignored
        }

        try
        {
            element.Patterns.Invoke.Pattern.Invoke();
            return;
        }
        catch
        {
            // ignored
        }

        try
        {
            element.AsCheckBox().Toggle();
            return;
        }
        catch
        {
            // ignored
        }

        try
        {
            element.Patterns.Toggle.Pattern.Toggle();
            return;
        }
        catch
        {
            // ignored
        }

        var r = element.BoundingRectangle;
        if (r.Width <= 0 || r.Height <= 0)
            throw new InvalidOperationException("Element has no bounds for click fallback.");

        var pt = new Point((int)(r.Left + r.Width / 2), (int)(r.Top + r.Height / 2));
        global::FlaUI.Core.Input.Mouse.Click(pt);
    }

    public static void SetCheckBoxState(CheckBox checkbox, bool desiredChecked)
    {
        if (checkbox.IsChecked == desiredChecked)
            return;

        try
        {
            checkbox.Toggle();
            return;
        }
        catch
        {
            // ignored
        }

        InvokeOrClick(checkbox);
    }

    public static bool IsPlausiblePopupBounds(double left, double top, double width, double height) =>
        width > 0 && width < 1600 && height > 0 && height < 1600 && left > -1000 && top > -1000;

    public static AutomationElement? FindByAutomationId(AutomationElement root, string automationId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var el = root.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (el != null)
                return el;
            Thread.Sleep(100);
        }

        return null;
    }

    public static void SelectTabItem(Tab tab, string tabHeader)
    {
        var item = tab.TabItems.FirstOrDefault(t =>
        {
            var name = t.Properties.Name.ValueOrDefault ?? string.Empty;
            return string.Equals(name, tabHeader, StringComparison.OrdinalIgnoreCase)
                || name.Contains(tabHeader, StringComparison.OrdinalIgnoreCase);
        });

        if (item == null)
            throw new InvalidOperationException($"Tab '{tabHeader}' not found.");

        item.Select();
    }
}
