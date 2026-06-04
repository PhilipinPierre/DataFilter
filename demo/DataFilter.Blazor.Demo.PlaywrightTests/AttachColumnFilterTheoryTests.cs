using Microsoft.Playwright;
using UIContracts.Common;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class AttachColumnFilterTheoryTests
{
    private readonly DemoHostFixture _host;

    public AttachColumnFilterTheoryTests(DemoHostFixture host) => _host = host;

    [Theory]
    [MemberData(nameof(ColumnMatrix.AttachCustomFilterTheoryData), MemberType = typeof(ColumnMatrix))]
    public async Task Filtering_ColumnMatrix_Invariant(ColumnFilterCase filterCase)
    {
        await RunAsync($"Filtering_{filterCase.PropertyName}_{filterCase.UiOperator}", async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Attach, "df-filter-btn-Department", errors);

            await PlaywrightContractHelpers.ApplyColumnFilterCaseAsync(page, _host, filterCase, errors);
            await PlaywrightContractHelpers.AssertColumnFilterInvariantAsync(page, filterCase);
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
