using global::FlaUI.Core;
using global::FlaUI.Core.AutomationElements;
using global::FlaUI.UIA3;
using UIContracts.Common;
using Xunit;

namespace UIContracts.FlaUI.Tests;

public sealed class DataFilterWinUI3Demo_ScenarioContractsTests
{
    [Theory]
    [InlineData(nameof(FlaUIWinUI3DemoHost.NavigateToAsync), DemoViewCatalog.WinUi3.AsyncNav)]
    [InlineData(nameof(FlaUIWinUI3DemoHost.NavigateToHybrid), DemoViewCatalog.WinUi3.HybridNav)]
    [InlineData(nameof(FlaUIWinUI3DemoHost.NavigateToListView), DemoViewCatalog.WinUi3.ListViewNav)]
    [InlineData(nameof(FlaUIWinUI3DemoHost.NavigateToCollectionView), DemoViewCatalog.WinUi3.CollectionViewNav)]
    public void Filtering_DepartmentEqualsIT_OnScenarioPage(string navigateMethod, string _)
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

            Navigate(window, navigateMethod);
            Thread.Sleep(navigateMethod == nameof(FlaUIWinUI3DemoHost.NavigateToAsync) ? 2500 : 800);

            FlaUIWinUI3DemoHost.ApplyEqualsFilter(window, automation, "Department", "IT");

            var btn = window.FindFirstDescendant(cf => cf.ByAutomationId("df-filter-btn-Department"));
            Assert.NotNull(btn);
        }
        finally
        {
            FlaUIWinUI3DemoHost.TryCloseApp(app);
        }
    }

    private static void Navigate(Window window, string method)
    {
        switch (method)
        {
            case nameof(FlaUIWinUI3DemoHost.NavigateToAsync): FlaUIWinUI3DemoHost.NavigateToAsync(window); break;
            case nameof(FlaUIWinUI3DemoHost.NavigateToHybrid): FlaUIWinUI3DemoHost.NavigateToHybrid(window); break;
            case nameof(FlaUIWinUI3DemoHost.NavigateToListView): FlaUIWinUI3DemoHost.NavigateToListView(window); break;
            case nameof(FlaUIWinUI3DemoHost.NavigateToCollectionView): FlaUIWinUI3DemoHost.NavigateToCollectionView(window); break;
            default: throw new ArgumentOutOfRangeException(nameof(method));
        }
    }
}
