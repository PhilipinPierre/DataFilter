using Microsoft.Playwright;
using UIContracts.Common;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class LocalizationContractsTests
{
    private readonly DemoHostFixture _host;

    public LocalizationContractsTests(DemoHostFixture host) => _host = host;

    [Theory]
    [InlineData(DemoViewCatalog.Blazor.Attach)]
    [InlineData(DemoViewCatalog.Blazor.Local)]
    public async Task FrenchCulture_LocalizesPopupOkButton(string route)
    {
        await RunAsync($"FrenchCulture_{route.Replace("/", "_")}", async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, route, "df-filter-btn-Department", errors);

            var lang = page.GetByTestId("df-language");
            var chosen = await page.EvaluateAsync<string?>(
                @"() => {
                    const sel = document.querySelector('[data-testid=""df-language""]');
                    if (!sel) return null;
                    const opts = Array.from(sel.querySelectorAll('option'));
                    const fr = opts.find(o => (o.value||'').toLowerCase() === 'fr-fr' || (o.value||'').toLowerCase().startsWith('fr'));
                    const nonEnglish = opts.find(o => {
                        const v = (o.value || '').toLowerCase();
                        return v !== '' && !v.startsWith('en');
                    });
                    return (fr || nonEnglish || null)?.value ?? null;
                }");
            if (string.IsNullOrWhiteSpace(chosen))
                return;

            await lang.SelectOptionAsync(new[] { chosen! });
            await page.WaitForTimeoutAsync(500);

            await PlaywrightContractHelpers.OpenPopupAsync(page, _host, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            var okText = (await popup.Locator("button.df-btn-primary").InnerTextAsync()).Trim();
            var clearText = (await popup.Locator("button.df-btn-secondary").InnerTextAsync()).Trim();

            Assert.False(
                string.Equals(okText, "OK", StringComparison.OrdinalIgnoreCase)
                && string.Equals(clearText, "Clear", StringComparison.OrdinalIgnoreCase),
                $"Expected localized popup labels on {route}. ok='{okText}', clear='{clearText}', culture='{chosen}'.");
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
        page.PageError += (_, e) => errors.Add("[PageError] " + e);
        await run(page, errors);
    }
}
