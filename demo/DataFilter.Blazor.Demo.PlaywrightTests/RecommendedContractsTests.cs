using Microsoft.Playwright;
using UIContracts.Common;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class RecommendedContractsTests
{
    private readonly DemoHostFixture _host;

    public RecommendedContractsTests(DemoHostFixture host) => _host = host;

    [Fact]
    public async Task PopupClose_Escape_Department()
    {
        await RunAsync(nameof(PopupClose_Escape_Department), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Attach, "df-filter-btn-Department", errors);

            await PlaywrightContractHelpers.OpenPopupAsync(page, _host, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            await page.Keyboard.PressAsync("Escape");
            await popup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        });
    }

    [Fact]
    public async Task RtlLayout_AnchoredPositioning_Department()
    {
        await RunAsync(nameof(RtlLayout_AnchoredPositioning_Department), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Attach, "df-filter-btn-Department", errors);

            await page.GetByTestId("df-direction").SelectOptionAsync(new[] { "RTL" });
            await page.WaitForTimeoutAsync(200);

            await PlaywrightContractHelpers.OpenPopupAsync(page, _host, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            var box = await popup.BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box!.X >= 0);
            Assert.True(box.Y >= 0);

            var viewport = page.ViewportSize!;
            Assert.True(box.X + box.Width <= viewport.Width + 1);
            Assert.True(box.Y + box.Height <= viewport.Height + 1);
        });
    }

    private async Task RunAsync(string testName, Func<IPage, List<string>, Task> run)
    {
        using var pw = await Playwright.CreateAsync();
        await using var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var errors = new List<string>();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });
        var page = await context.NewPageAsync();
        await run(page, errors);
    }
}
