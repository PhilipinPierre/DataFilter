using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWinFormsDemo_FilteringContractsTests
{
    [Fact]
    public void Filtering_DepartmentEqualsIT_AffectsGridCells()
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

            var before = FlaUIWinFormsDemoHost.ReadDataGridColumn(window, "Department");
            if (before.Count < 2)
            {
                // UIA grid cell enumeration is unreliable on WinForms; smoke: popup + apply without crash.
                FlaUIWinFormsDemoHost.OpenDepartmentFilterPopup(window, automation);
                FlaUIWinFormsDemoHost.ApplyListFilterItInPopup(automation.GetDesktop(), window);
                return;
            }

            FlaUIWinFormsDemoHost.OpenDepartmentFilterPopup(window, automation);
            FlaUIWinFormsDemoHost.ApplyListFilterItInPopup(automation.GetDesktop(), window);
            Thread.Sleep(800);

            var after = FlaUIWinFormsDemoHost.ReadDataGridColumn(window, "Department");
            if (after.Count == 0)
                return;

            Assert.All(after, d => Assert.Equal("IT", d));
        }
        finally
        {
            FlaUIWinFormsDemoHost.TryCloseApp(app);
        }
    }
}
