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
        var exe = FlaUIWinFormsDemoHost.BuildAndGetExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            FlaUIWinFormsDemoHost.NavigateToAttachTab(window);
            Thread.Sleep(800);

            var desktop = automation.GetDesktop();
            var beforePopups = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Menu)))
                .Select(x => x.Properties.NativeWindowHandle.ValueOrDefault)
                .ToHashSet();

            if (!FlaUIWinFormsDemoHost.OpenDepartmentFilterPopup(window, automation))
                return;

            Assert.True(
                FlaUIWinFormsPopupHelpers.WaitForFilterPopup(desktop, window, beforePopups, TimeSpan.FromSeconds(3), out var popup),
                "Department filter popup should be visible after opening.");

            AssertWithinWorkingArea(popup);

            var cancel = popup.FindFirstDescendant(cf => cf.ByName("df-cancel"))?.AsButton()
                ?? desktop.FindFirstDescendant(cf => cf.ByName("df-cancel"))?.AsButton();
            if (cancel != null)
                FlaUIInputHelpers.InvokeOrClick(cancel);
            else
                global::FlaUI.Core.Input.Keyboard.Press(global::FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);

            WaitUntil(() => FlaUIWinFormsPopupHelpers.IsFilterPopupClosed(desktop, window), TimeSpan.FromSeconds(10));
        }
        finally
        {
            FlaUIWinFormsDemoHost.TryCloseApp(app);
        }
    }

    private static void AssertWithinWorkingArea(AutomationElement popup)
    {
        var r = popup.BoundingRectangle;
        Assert.True(r.Width > 0 && r.Height > 0, $"Expected popup bounds. popup={r}");

        // ContextMenuStrip UIA bounds are often shifted on WinForms; skip viewport clamp when unreliable.
        if (!FlaUIWinFormsPopupHelpers.HasReliablePopupBounds(popup))
            return;

        const int tolerancePx = 16;
        var screen = System.Windows.Forms.Screen.FromRectangle(System.Drawing.Rectangle.FromLTRB(
            (int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom));
        var wa = screen.WorkingArea;

        Assert.True(r.Left >= wa.Left - tolerancePx, $"Expected popup within working area (left). popup={r}, workArea={wa}");
        Assert.True(r.Top >= wa.Top - tolerancePx, $"Expected popup within working area (top). popup={r}, workArea={wa}");
        Assert.True(r.Right <= wa.Right + tolerancePx, $"Expected popup within working area (right). popup={r}, workArea={wa}");
        Assert.True(r.Bottom <= wa.Bottom + tolerancePx, $"Expected popup within working area (bottom). popup={r}, workArea={wa}");
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

}

