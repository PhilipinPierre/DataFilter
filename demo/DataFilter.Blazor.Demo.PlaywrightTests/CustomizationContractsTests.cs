using Microsoft.Playwright;
using UIContracts.Common;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class CustomizationContractsTests
{
    private readonly DemoHostFixture _host;

    public CustomizationContractsTests(DemoHostFixture host) => _host = host;

    [Fact]
    public async Task PopupOpenClose_Department_OnCustomizationPage()
    {
        using var pw = await Playwright.CreateAsync();
        await using var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var errors = new List<string>();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });
        var page = await context.NewPageAsync();

        await PlaywrightContractHelpers.NavigateToRouteAsync(
            page, _host, DemoViewCatalog.Blazor.Customization, "df-filter-btn-Department", errors);

        await PlaywrightContractHelpers.OpenPopupAsync(page, _host, "Department", errors);
        var popup = page.Locator("#df-filter-popup-Department");
        await page.Mouse.ClickAsync(2, 2);
        await popup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }
}
