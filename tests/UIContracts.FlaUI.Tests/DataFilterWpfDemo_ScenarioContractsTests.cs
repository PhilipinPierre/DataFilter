using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using UIContracts.Common;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWpfDemo_ScenarioContractsTests
{
    [Theory]
    [InlineData(nameof(FlaUIWpfDemoHost.NavigateToAsyncTab), DemoViewCatalog.Wpf.AsyncTab)]
    [InlineData(nameof(FlaUIWpfDemoHost.NavigateToHybridTab), DemoViewCatalog.Wpf.HybridTab)]
    [InlineData(nameof(FlaUIWpfDemoHost.NavigateToListViewTab), DemoViewCatalog.Wpf.ListViewTab)]
    [InlineData(nameof(FlaUIWpfDemoHost.NavigateToCollectionViewTab), DemoViewCatalog.Wpf.CollectionViewTab)]
    public void Filtering_DepartmentEqualsIT_OnScenarioTab(string navigateMethod, string _)
    {
        var exe = FlaUIWpfDemoHost.BuildAndGetExePath();
        using var app = Application.Launch(exe);
        try
        {
            using var automation = new UIA3Automation();
            var window = app.GetMainWindow(automation);
            Assert.NotNull(window);

            Navigate(window, navigateMethod);
            var settleMs = navigateMethod == nameof(FlaUIWpfDemoHost.NavigateToAsyncTab) ? 4000 : 1500;
            Thread.Sleep(settleMs);

            var btn = FlaUIInputHelpers.FindByAutomationId(window, "df-filter-btn-Department", TimeSpan.FromSeconds(12))
                ?? FlaUIInputHelpers.FindByAutomationId(window, "df-filter-btn-Name", TimeSpan.FromSeconds(3));
            Assert.NotNull(btn);
        }
        finally
        {
            FlaUIWpfDemoHost.TryCloseApp(app);
        }
    }

    private static void Navigate(Window window, string method)
    {
        switch (method)
        {
            case nameof(FlaUIWpfDemoHost.NavigateToAsyncTab): FlaUIWpfDemoHost.NavigateToAsyncTab(window); break;
            case nameof(FlaUIWpfDemoHost.NavigateToHybridTab): FlaUIWpfDemoHost.NavigateToHybridTab(window); break;
            case nameof(FlaUIWpfDemoHost.NavigateToListViewTab): FlaUIWpfDemoHost.NavigateToListViewTab(window); break;
            case nameof(FlaUIWpfDemoHost.NavigateToCollectionViewTab): FlaUIWpfDemoHost.NavigateToCollectionViewTab(window); break;
            default: throw new ArgumentOutOfRangeException(nameof(method));
        }
    }
}
