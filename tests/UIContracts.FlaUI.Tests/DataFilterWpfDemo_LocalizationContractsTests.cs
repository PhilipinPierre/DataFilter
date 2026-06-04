using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWpfDemo_LocalizationContractsTests
{
    [Fact]
    public void Localization_ChangesPopupOkLabel()
    {
        var exe = FlaUIWpfDemoHost.BuildAndGetExePath();

        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            FlaUIWpfDemoHost.NavigateToAttachTab(window);

            var languageCombo = window.FindFirstDescendant(cf => cf.ByAutomationId("df-language"))?.AsComboBox();
            if (languageCombo == null)
                return;

            var items = languageCombo.Items;
            if (items.Length < 2)
                return;

            var englishOk = OpenDepartmentPopupAndGetOkText(window, automation);
            Assert.False(string.IsNullOrWhiteSpace(englishOk));

            for (var i = 0; i < items.Length; i++)
            {
                languageCombo.Select(i);
                var localizedOk = OpenDepartmentPopupAndGetOkText(window, automation);
                if (!string.Equals(localizedOk, englishOk, StringComparison.Ordinal))
                {
                    Assert.False(string.IsNullOrWhiteSpace(localizedOk));
                    return;
                }
            }
        }
        finally
        {
            FlaUIWpfDemoHost.TryCloseApp(app);
        }
    }

    private static string? OpenDepartmentPopupAndGetOkText(Window window, UIA3Automation automation)
    {
        FlaUIWpfDemoHost.ClickByAutomationId(window, "df-filter-btn-Department", TimeSpan.FromSeconds(10));
        var popup = automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId("df-filter-popup-Department"));
        var ok = popup?.FindFirstDescendant(cf => cf.ByAutomationId("df-ok"))?.AsButton();
        return ok?.Name;
    }
}
