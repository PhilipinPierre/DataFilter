using System.Runtime.InteropServices;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;

namespace UIContracts.FlaUI.Tests;

internal static class FlaUIWinFormsPopupHelpers
{
    public static bool WaitForFilterPopup(
        AutomationElement desktop,
        Window mainWindow,
        HashSet<nint>? knownHandles,
        TimeSpan timeout,
        out AutomationElement popup)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (TryFindFilterPopup(desktop, mainWindow, knownHandles, out var found) && found != null)
            {
                popup = found;
                return true;
            }

            Thread.Sleep(150);
        }

        popup = null!;
        return false;
    }

    public static bool IsFilterPopupClosed(AutomationElement desktop, Window mainWindow) =>
        !TryFindFilterPopup(desktop, mainWindow, knownHandles: null, out _);

    public static bool HasReliablePopupBounds(AutomationElement popup)
    {
        var r = popup.BoundingRectangle;
        return r.Width > 0
               && r.Height > 0
               && r.Width <= 1200
               && r.Height <= 1200
               && r.Top >= -50
               && r.Left >= -50;
    }

    private static bool TryFindFilterPopup(
        AutomationElement desktop,
        Window mainWindow,
        HashSet<nint>? knownHandles,
        out AutomationElement? popup)
    {
        popup = null;

        foreach (var scope in EnumerateSearchScopes(desktop, mainWindow, knownHandles))
        {
            if (TryFindFilterPopupMarker(scope, out var marker))
            {
                popup = ResolvePopupRoot(marker);
                return popup != null;
            }
        }

        return false;
    }

    private static IEnumerable<AutomationElement> EnumerateSearchScopes(
        AutomationElement desktop,
        Window mainWindow,
        HashSet<nint>? knownHandles)
    {
        yield return mainWindow;

        AutomationElement[] children;
        try
        {
            children = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window)
                .Or(cf.ByControlType(ControlType.Menu))
                .Or(cf.ByControlType(ControlType.Pane))
                .Or(cf.ByControlType(ControlType.ToolTip)));
        }
        catch (COMException)
        {
            yield break;
        }

        var mainHandle = mainWindow.Properties.NativeWindowHandle.ValueOrDefault;
        foreach (var child in children)
        {
            var h = child.Properties.NativeWindowHandle.ValueOrDefault;
            if (h == mainHandle)
                continue;
            if (knownHandles != null && knownHandles.Contains(h))
                continue;

            var r = child.BoundingRectangle;
            if (!FlaUIInputHelpers.IsPlausiblePopupBounds(r.Left, r.Top, r.Width, r.Height))
                continue;

            yield return child;
        }
    }

    private static bool TryFindFilterPopupMarker(AutomationElement root, out AutomationElement marker)
    {
        marker = null!;
        try
        {
            var found = root.FindFirstDescendant(cf => cf.ByName("df-ok"))
                ?? root.FindFirstDescendant(cf => cf.ByName("df-select-all"))
                ?? root.FindFirstDescendant(cf => cf.ByName("df-cancel"))
                ?? root.FindFirstDescendant(cf => cf.ByControlType(ControlType.CheckBox).And(cf.ByName("Select All")))
                ?? root.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName("OK")));

            if (found == null)
                return false;

            marker = found;
            return true;
        }
        catch (COMException)
        {
            return false;
        }
    }

    private static AutomationElement? ResolvePopupRoot(AutomationElement marker)
    {
        AutomationElement? best = null;
        var bestArea = double.MaxValue;
        var current = marker;
        for (var depth = 0; depth < 8 && current != null; depth++)
        {
            var r = current.BoundingRectangle;
            if (r.Width >= 100
                && r.Height >= 100
                && r.Width <= 700
                && r.Height <= 900
                && FlaUIInputHelpers.IsPlausiblePopupBounds(r.Left, r.Top, r.Width, r.Height))
            {
                var area = r.Width * r.Height;
                if (area < bestArea)
                {
                    best = current;
                    bestArea = area;
                }
            }

            current = current.Parent;
        }

        return best ?? marker;
    }
}
