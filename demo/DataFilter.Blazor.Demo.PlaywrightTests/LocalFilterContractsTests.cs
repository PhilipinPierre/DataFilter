using Microsoft.Playwright;
using UIContracts.Common;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class LocalFilterContractsTests
{
    private readonly DemoHostFixture _host;

    public LocalFilterContractsTests(DemoHostFixture host) => _host = host;

    [Fact]
    public async Task FilterPipelineJson_ApplyIsActive_FiltersRows()
    {
        await RunAsync(nameof(FilterPipelineJson_ApplyIsActive_FiltersRows), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Local, "df-filter-btn-Department", errors);

            var rows = page.Locator("table.df-grid tbody tr");
            var before = await rows.CountAsync();
            Assert.True(before > 0);

            await page.GetByTestId("df-pipeline-json").FillAsync(FilterPipelinePresets.SingleCriterionIsActiveTrueJson);
            await page.GetByTestId("df-pipeline-apply").ClickAsync();
            await page.WaitForTimeoutAsync(400);

            var after = await rows.CountAsync();
            Assert.True(after > 0);
            Assert.True(after <= before);
        });
    }

    [Fact]
    public async Task FilterPipelineJson_MultiColumnAnd_DeptItCountryUsa_Invariant()
    {
        await RunAsync(nameof(FilterPipelineJson_MultiColumnAnd_DeptItCountryUsa_Invariant), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Local, "df-filter-btn-Department", errors);

            await page.GetByTestId("df-pipeline-json").FillAsync(FilterPipelinePresets.MultiColumnAndDeptItCountryUsaJson);
            await page.GetByTestId("df-pipeline-apply").ClickAsync();
            await page.WaitForTimeoutAsync(400);

            var depts = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Department");
            var countries = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Country");
            Assert.NotEmpty(depts);
            Assert.All(depts, d => Assert.True(RowInvariants.DepartmentEquals(d, "IT")));
            Assert.All(countries, c => Assert.True(RowInvariants.CountryEquals(c, "USA")));
        });
    }

    [Fact]
    public async Task FilterPipelineJson_OrGroupNameStartsWith_Invariant()
    {
        await RunAsync(nameof(FilterPipelineJson_OrGroupNameStartsWith_Invariant), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Local, "df-filter-btn-Name", errors);

            await page.GetByTestId("df-pipeline-json").FillAsync(FilterPipelinePresets.OrGroupNameStartsWithJson);
            await page.GetByTestId("df-pipeline-apply").ClickAsync();
            await page.WaitForTimeoutAsync(400);

            var names = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Name");
            Assert.NotEmpty(names);
            Assert.All(names, n => Assert.True(RowInvariants.NameStartsWithAny(n, "A", "H")));
        });
    }

    [Fact]
    public async Task FilterPipelineJson_DisabledCriterion_DoesNotApplyDisabledFilter()
    {
        await RunAsync(nameof(FilterPipelineJson_DisabledCriterion_DoesNotApplyDisabledFilter), async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Local, "df-filter-btn-Department", errors);

            var rows = page.Locator("table.df-grid tbody tr");
            var unfiltered = await rows.CountAsync();
            Assert.True(unfiltered > 0);

            await page.GetByTestId("df-pipeline-json").FillAsync(FilterPipelinePresets.DisabledCriterionIgnoredJson);
            await page.GetByTestId("df-pipeline-apply").ClickAsync();
            await page.WaitForTimeoutAsync(400);

            var after = await rows.CountAsync();
            Assert.True(after > 0);
            Assert.True(after < unfiltered);

            var depts = await PlaywrightContractHelpers.GetColumnValuesAsync(page, "Department");
            if (depts.Count > 0)
                Assert.Contains(depts, d => !RowInvariants.DepartmentEquals(d, "IT"));
        });
    }

    [Theory]
    [MemberData(nameof(ColumnMatrix.DefaultPropertyNameTheoryData), MemberType = typeof(ColumnMatrix))]
    public async Task PopupOpenClose_AllColumns(string propertyName)
    {
        await RunAsync($"PopupOpenClose_{propertyName}", async (page, errors) =>
        {
            await PlaywrightContractHelpers.NavigateToRouteAsync(
                page, _host, DemoViewCatalog.Blazor.Local, $"df-filter-btn-{propertyName}", errors);

            await PlaywrightContractHelpers.OpenPopupAsync(page, _host, propertyName, errors);
            var popup = page.Locator($"#df-filter-popup-{propertyName}");
            await page.Mouse.ClickAsync(2, 2);
            await popup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
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
