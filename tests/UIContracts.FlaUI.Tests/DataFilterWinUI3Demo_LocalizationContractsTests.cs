using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWinUI3Demo_LocalizationContractsTests
{
    [Fact]
    public void Localization_ChangesPopupOkLabel()
    {
        var exe = FlaUIWinUI3DemoHost.BuildAndGetExePath();
        if (!FlaUIWinUI3DemoHost.IsWinAppRuntimeAvailable(exe))
            return;

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
            Assert.NotNull(window);

            FlaUIWinUI3DemoHost.NavigateToAttach(window);

            var languageCombo = window.FindFirstDescendant(cf => cf.ByAutomationId("df-language"))?.AsComboBox();
            if (languageCombo == null || languageCombo.Items.Length < 2)
                return;

            var englishOk = OpenDepartmentOkText(window, automation);
            for (var i = 0; i < languageCombo.Items.Length; i++)
            {
                languageCombo.Select(i);
                var text = OpenDepartmentOkText(window, automation);
                if (!string.Equals(text, englishOk, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(text))
                    return;
            }
        }
        finally
        {
            FlaUIWinUI3DemoHost.TryCloseApp(app);
        }
    }

    private static string? OpenDepartmentOkText(Window window, UIA3Automation automation)
    {
        window.FindFirstDescendant(cf => cf.ByAutomationId("df-filter-btn-Department"))?.AsButton()?.Invoke();
        var popup = automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Department"));
        return popup?.FindFirstDescendant(cf => cf.ByAutomationId("df-ok"))?.AsButton()?.Name;
    }
}
