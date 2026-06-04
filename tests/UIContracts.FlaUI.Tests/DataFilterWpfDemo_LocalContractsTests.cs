using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using UIContracts.Common;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWpfDemo_LocalContractsTests
{
    [Fact]
    public void FilterPipelineJson_ApplyMultiColumn_DoesNotCrash()
    {
        var exe = FlaUIWpfDemoHost.BuildAndGetExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            FlaUIWpfDemoHost.NavigateToLocalTab(window);
            Thread.Sleep(800);

            var jsonBox = FlaUIInputHelpers.FindByAutomationId(window, "df-pipeline-json", TimeSpan.FromSeconds(15));
            Assert.NotNull(jsonBox);
            jsonBox!.AsTextBox().Text = FilterPipelinePresets.MultiColumnAndDeptItCountryUsaJson;

            FlaUIWpfDemoHost.ClickByAutomationId(window, "df-pipeline-apply", TimeSpan.FromSeconds(10));

            Assert.NotNull(app.GetMainWindow(automation));
        }
        finally
        {
            FlaUIWpfDemoHost.TryCloseApp(app);
        }
    }
}
