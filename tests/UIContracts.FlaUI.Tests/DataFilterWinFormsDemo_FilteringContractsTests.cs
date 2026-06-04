using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using UIContracts.Common;
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
            Thread.Sleep(500);

            var before = FlaUIWinFormsDemoHost.ReadDataGridColumn(window, "Department");
            Assert.True(before.Count > 1);

            FlaUIWinFormsDemoHost.OpenDepartmentFilterPopup(window, automation);
            FlaUIWinFormsDemoHost.ApplyListFilterItInPopup(automation.GetDesktop(), window);
            Thread.Sleep(500);

            var after = FlaUIWinFormsDemoHost.ReadDataGridColumn(window, "Department");
            Assert.NotEmpty(after);
            Assert.All(after, d => Assert.Equal("IT", d));
        }
        finally
        {
            FlaUIWinFormsDemoHost.TryCloseApp(app);
        }
    }
}
