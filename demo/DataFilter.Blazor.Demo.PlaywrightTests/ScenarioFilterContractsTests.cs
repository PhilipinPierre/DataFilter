using Microsoft.Playwright;
using UIContracts.Common;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class ScenarioFilterContractsTests
{
    private readonly DemoHostFixture _host;

    public ScenarioFilterContractsTests(DemoHostFixture host) => _host = host;

    [Fact]
    public async Task AsyncFiltering_DepartmentEqualsIT_AffectsRows()
    {
        await RunAsync(nameof(AsyncFiltering_DepartmentEqualsIT_AffectsRows), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Async, "df-filter-btn-Department", errors);
            await PlaywrightContractHelpers.WaitForGridDataRowsAsync(page);

            var before = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Department");
            Assert.True(before.Count > 1);

            await PlaywrightContractHelpers.ApplyDepartmentEqualsItAsync(page, _host, errors);

            var depts = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Department");
            Assert.NotEmpty(depts);
            Assert.All(depts, d => Assert.True(RowInvariants.DepartmentEquals(d, "IT")));
        });
    }

    [Fact]
    public async Task HybridFiltering_DepartmentEqualsIT_AffectsRows()
    {
        await RunAsync(nameof(HybridFiltering_DepartmentEqualsIT_AffectsRows), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Hybrid, "df-filter-btn-Department", errors);
            await PlaywrightContractHelpers.WaitForGridDataRowsAsync(page);

            await PlaywrightContractHelpers.ApplyDepartmentEqualsItAsync(page, _host, errors);

            var depts = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Department");
            Assert.NotEmpty(depts);
            Assert.All(depts, d => Assert.True(RowInvariants.DepartmentEquals(d, "IT")));
        });
    }

    [Fact]
    public async Task CollectionViewFiltering_DepartmentEqualsIT_AffectsRows()
    {
        await RunAsync(nameof(CollectionViewFiltering_DepartmentEqualsIT_AffectsRows), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.CollectionView, "df-filter-btn-Department", errors);
            await PlaywrightContractHelpers.WaitForGridDataRowsAsync(page);

            await PlaywrightContractHelpers.ApplyDepartmentEqualsItAsync(page, _host, errors);

            var depts = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Department");
            Assert.NotEmpty(depts);
            Assert.All(depts, d => Assert.True(RowInvariants.DepartmentEquals(d, "IT")));
        });
    }

    [Fact]
    public async Task HybridFiltering_ClearFilters_RestoresRows()
    {
        await RunAsync(nameof(HybridFiltering_ClearFilters_RestoresRows), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Hybrid, "df-filter-btn-Department", errors);
            await PlaywrightContractHelpers.WaitForGridDataRowsAsync(page);

            var rows = page.Locator("table.df-grid tbody tr");
            var before = await rows.CountAsync();
            await PlaywrightContractHelpers.ApplyDepartmentEqualsItAsync(page, _host, errors);
            var filtered = await rows.CountAsync();
            Assert.True(filtered <= before);

            await page.GetByTestId("df-clear-filters").ClickAsync();
            await page.WaitForTimeoutAsync(300);
            var after = await rows.CountAsync();
            Assert.Equal(before, after);
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
        page.Console += (_, e) => errors.Add($"[Console:{e.Type}] {e.Text}");
        await run(page, errors);
    }
}
