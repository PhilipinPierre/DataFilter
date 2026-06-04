using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.Core.Definitions;
using global::FlaUI.UIA3;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWinFormsDemo_LocalizationContractsTests
{
    [Fact]
    public void Localization_LanguageCombo_IsPresent()
    {
        var exe = FlaUIWinFormsDemoHost.BuildAndGetExePath();
        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            var combo = window.FindFirstDescendant(cf => cf.ByName("df-language"))?.AsComboBox()
                ?? window.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox))
                    .Select(c => c.AsComboBox())
                    .LastOrDefault();
            Assert.NotNull(combo);
            Assert.True(combo!.Items.Length >= 1);
        }
        finally
        {
            FlaUIWinFormsDemoHost.TryCloseApp(app);
        }
    }
}
