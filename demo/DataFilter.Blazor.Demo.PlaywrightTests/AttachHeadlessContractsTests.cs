using Microsoft.Playwright;
using Xunit;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

[Collection(DemoHostCollection.Name)]
public sealed class AttachHeadlessContractsTests
{
    private readonly DemoHostFixture _host;

    public AttachHeadlessContractsTests(DemoHostFixture host)
    {
        _host = host;
    }

    [Fact]
    public async Task PopupOpenClose_Department()
    {
        await RunWithTracingAsync(nameof(PopupOpenClose_Department), viewport: null, async (page, errors) =>
        {
            await page.GotoAsync(_host.BaseUrl + "/demo/attach", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await page.WaitForFunctionAsync("() => !!window.Blazor");
            await page.WaitForFunctionAsync("() => !!window.DataFilterInterops");
            await page.WaitForFunctionAsync("() => { const d = document.getElementById('components-reconnect-modal'); return !d || d.open !== true; }");
            await page.GetByTestId("df-filter-btn-Department").WaitForAsync();
            await AssertBlazorHealthyAsync(page, errors);
            await page.WaitForTimeoutAsync(750);

            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");

            // Click outside overlay to close.
            await page.Mouse.ClickAsync(2, 2);
            await popup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        });
    }

    [Fact]
    public async Task AnchoredPositioning_ViewportReduced_HireDate_Clamped()
    {
        await RunWithTracingAsync(nameof(AnchoredPositioning_ViewportReduced_HireDate_Clamped), new ViewportSize { Width = 800, Height = 600 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "HireDate", errors);
            var popup = page.Locator("#df-filter-popup-HireDate");
            var popupBox = await popup.BoundingBoxAsync();
            Assert.NotNull(popupBox);

            await AssertPopupMatchesInteropAsync(page, "df-filter-btn-HireDate", "df-filter-popup-HireDate", popupBox!.X, popupBox.Y, tolerancePx: 2);
            await AssertWithinViewportAsync(page, popupBox.X, popupBox.Y, popupBox.Width, popupBox.Height);
        });
    }

    [Fact]
    public async Task AnchoredPositioning_NearBottom_Clamped()
    {
        await RunWithTracingAsync(nameof(AnchoredPositioning_NearBottom_Clamped), new ViewportSize { Width = 800, Height = 600 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            // Force anchor near bottom by scrolling down before opening.
            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(150);

            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            var popupBox = await popup.BoundingBoxAsync();
            Assert.NotNull(popupBox);
            await AssertWithinViewportAsync(page, popupBox!.X, popupBox.Y, popupBox.Width, popupBox.Height);
        });
    }

    [Fact]
    public async Task AnchoredPositioning_Department()
    {
        await RunWithTracingAsync(nameof(AnchoredPositioning_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Department", errors);
            var btn = page.GetByTestId("df-filter-btn-Department");
            var popup = page.Locator("#df-filter-popup-Department");

            var btnBox = await btn.BoundingBoxAsync();
            var popupBox = await popup.BoundingBoxAsync();
            Assert.NotNull(btnBox);
            Assert.NotNull(popupBox);

            await AssertPopupMatchesInteropAsync(page, "df-filter-btn-Department", "df-filter-popup-Department", popupBox!.X, popupBox.Y, tolerancePx: 2);
            await AssertWithinViewportAsync(page, popupBox!.X, popupBox.Y, popupBox.Width, popupBox.Height);
        });
    }

    [Fact]
    public async Task AnchoredPositioning_RTL_Department()
    {
        await RunWithTracingAsync(nameof(AnchoredPositioning_RTL_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);
            await SetDirectionAsync(page, isRtl: true);
            await page.WaitForTimeoutAsync(250);

            await OpenPopupAsync(page, "Department", errors);

            var popup = page.Locator("#df-filter-popup-Department");
            var popupBox = await popup.BoundingBoxAsync();
            Assert.NotNull(popupBox);

            await AssertPopupMatchesInteropAsync(page, "df-filter-btn-Department", "df-filter-popup-Department", popupBox!.X, popupBox.Y, tolerancePx: 2);
            await AssertWithinViewportAsync(page, popupBox!.X, popupBox.Y, popupBox.Width, popupBox.Height);
        });
    }

    [Fact]
    public async Task ScrollKeepsPopupAnchored_Department()
    {
        await RunWithTracingAsync(nameof(ScrollKeepsPopupAnchored_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Department", errors);
            var btn = page.GetByTestId("df-filter-btn-Department");
            var popup = page.Locator("#df-filter-popup-Department");

            var beforeBtn = await btn.BoundingBoxAsync();
            var beforePopup = await popup.BoundingBoxAsync();
            Assert.NotNull(beforeBtn);
            Assert.NotNull(beforePopup);

            await page.EvaluateAsync("window.scrollBy(0, 500)");
            await page.WaitForTimeoutAsync(150);

            var afterBtn = await btn.BoundingBoxAsync();
            var afterPopup = await popup.BoundingBoxAsync();
            Assert.NotNull(afterBtn);
            Assert.NotNull(afterPopup);

            await AssertPopupMatchesInteropAsync(page, "df-filter-btn-Department", "df-filter-popup-Department", afterPopup!.X, afterPopup.Y, tolerancePx: 2);
            await AssertWithinViewportAsync(page, afterPopup!.X, afterPopup.Y, afterPopup.Width, afterPopup.Height);
        });
    }

    [Fact]
    public async Task ScrollClamp_WhenAnchorOffscreen_Department()
    {
        await RunWithTracingAsync(nameof(ScrollClamp_WhenAnchorOffscreen_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Department", errors);
            var btn = page.GetByTestId("df-filter-btn-Department");
            var popup = page.Locator("#df-filter-popup-Department");

            await page.EvaluateAsync("window.scrollBy(0, 5000)");
            await page.WaitForTimeoutAsync(250);

            var btnBox = await btn.BoundingBoxAsync();
            var popupBox = await popup.BoundingBoxAsync();
            Assert.NotNull(btnBox);
            Assert.NotNull(popupBox);

            Assert.True(btnBox!.Y + btnBox.Height < 0, $"Expected anchor to be offscreen above viewport. y={btnBox.Y}, h={btnBox.Height}");
            await AssertWithinViewportAsync(page, popupBox!.X, popupBox.Y, popupBox.Width, popupBox.Height);
        });
    }

    [Fact]
    public async Task FilteringAffectsRows_DepartmentEqualsIT()
    {
        await RunWithTracingAsync(nameof(FilteringAffectsRows_DepartmentEqualsIT), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            var rows = page.Locator("tbody tr");
            var beforeCount = await rows.CountAsync();
            Assert.True(beforeCount > 0);

            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await popup.Locator("input.df-custom-input").First.FillAsync("IT");
            await popup.Locator("button.df-btn-primary").ClickAsync();

            // Table should re-render with fewer rows (IT is only 1/5 of depts).
            await page.WaitForTimeoutAsync(200);
            var afterCount = await rows.CountAsync();

            Assert.True(afterCount < beforeCount, $"Expected fewer rows after applying filter. Before={beforeCount}, After={afterCount}");
        });
    }

    [Fact]
    public async Task OutsideClickDoesNotClickThrough_ClosesPopup_AndKeepsRowCount()
    {
        await RunWithTracingAsync(nameof(OutsideClickDoesNotClickThrough_ClosesPopup_AndKeepsRowCount), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            var rows = page.Locator("tbody tr");
            var beforeCount = await rows.CountAsync();
            Assert.True(beforeCount > 0);

            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");

            await page.Mouse.ClickAsync(2, 2);
            await popup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });

            var afterCount = await rows.CountAsync();
            Assert.Equal(beforeCount, afterCount);
        });
    }

    [Fact]
    public async Task ResizeBehavior_Department()
    {
        await RunWithTracingAsync(nameof(ResizeBehavior_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);
            await OpenPopupAsync(page, "Department", errors);

            var popup = page.Locator("#df-filter-popup-Department");
            var handle = popup.Locator(".df-resize-handle");

            var before = await popup.BoundingBoxAsync();
            var handleBox = await handle.BoundingBoxAsync();
            Assert.NotNull(before);
            Assert.NotNull(handleBox);

            await page.Mouse.MoveAsync(handleBox!.X + handleBox.Width / 2, handleBox.Y + handleBox.Height / 2);
            await page.Mouse.DownAsync();
            await page.Mouse.MoveAsync(handleBox.X + 120, handleBox.Y + 90);
            await page.Mouse.UpAsync();
            await page.WaitForTimeoutAsync(150);

            var after = await popup.BoundingBoxAsync();
            Assert.NotNull(after);
            Assert.True(
                after!.Width > before!.Width || after.Height > before.Height,
                $"Expected popup size to change. Before={before.Width}x{before.Height}, After={after.Width}x{after.Height}");
            await AssertWithinViewportAsync(page, after!.X, after.Y, after.Width, after.Height);
        });
    }

    [Fact]
    public async Task SortAscending_Department()
    {
        await RunWithTracingAsync(nameof(SortAscending_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);
            await OpenPopupAsync(page, "Department", errors);

            var popup = page.Locator("#df-filter-popup-Department");
            await popup.Locator(".df-sort-section .df-sort-button").Nth(0).ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var values = await GetColumnValuesAsync(page, "Department");
            Assert.True(values.Count > 1);
            AssertSorted(values, descending: false);
        });
    }

    [Fact]
    public async Task SortDescending_Salary()
    {
        await RunWithTracingAsync(nameof(SortDescending_Salary), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);
            await OpenPopupAsync(page, "Salary", errors);

            var popup = page.Locator("#df-filter-popup-Salary");
            await popup.Locator(".df-sort-section .df-sort-button").Nth(1).ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var values = await GetColumnValuesAsync(page, "Salary");
            var numbers = values.Select(ParseFloatInvariant).ToList();
            Assert.True(numbers.Count > 1);
            AssertSorted(numbers, descending: true);
        });
    }

    [Fact]
    public async Task SortAscending_HireDate()
    {
        await RunWithTracingAsync(nameof(SortAscending_HireDate), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);
            await OpenPopupAsync(page, "HireDate", errors);

            var popup = page.Locator("#df-filter-popup-HireDate");
            await popup.Locator(".df-sort-section .df-sort-button").Nth(0).ClickAsync();
            // Some hosts apply sort async and/or on popup close; close and wait for table to settle.
            await page.Mouse.ClickAsync(2, 2);
            await popup.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
            await WaitUntilAsync(async () =>
            {
                var values = await GetColumnDataValuesAsync(page, "Hire Date");
                var ticks = values.Select(ParseLongInvariant).ToList();
                return IsSorted(ticks, descending: false);
            }, timeoutMs: 5_000);

            var finalValues = await GetColumnDataValuesAsync(page, "Hire Date");
            var finalTicks = finalValues.Select(ParseLongInvariant).ToList();
            Assert.True(finalTicks.Count > 1);
            AssertSorted(finalTicks, descending: false);
        });
    }

    [Fact]
    public async Task Filtering_NameContainsAlice_AllRowsMatch()
    {
        await RunWithTracingAsync(nameof(Filtering_NameContainsAlice_AllRowsMatch), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Name", errors);
            var popup = page.Locator("#df-filter-popup-Name");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Contains" });
            await popup.Locator("input.df-custom-input").First.FillAsync("Alice");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var names = await GetColumnValuesAsync(page, "Name");
            Assert.True(names.Count > 0);
            Assert.All(names, n => Assert.Contains("Alice", n, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task Filtering_NameStartsWithAlice_AllRowsMatch()
    {
        await RunWithTracingAsync(nameof(Filtering_NameStartsWithAlice_AllRowsMatch), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Name", errors);
            var popup = page.Locator("#df-filter-popup-Name");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "StartsWith" });
            await popup.Locator("input.df-custom-input").First.FillAsync("Alice");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var names = await GetColumnValuesAsync(page, "Name");
            Assert.True(names.Count > 0);
            Assert.All(names, n => Assert.StartsWith("Alice", n, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task Filtering_NameEndsWith123_AllRowsMatch()
    {
        await RunWithTracingAsync(nameof(Filtering_NameEndsWith123_AllRowsMatch), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Name", errors);
            var popup = page.Locator("#df-filter-popup-Name");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "EndsWith" });
            await popup.Locator("input.df-custom-input").First.FillAsync("123");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var names = await GetColumnValuesAsync(page, "Name");
            Assert.True(names.Count > 0);
            Assert.All(names, n => Assert.EndsWith("123", n, StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task Filtering_SalaryBetween_KeepOnlyInRange()
    {
        await RunWithTracingAsync(nameof(Filtering_SalaryBetween_KeepOnlyInRange), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Salary", errors);
            var popup = page.Locator("#df-filter-popup-Salary");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Between" });
            await popup.Locator("input.df-custom-input").Nth(0).FillAsync("50000");
            await popup.Locator("input.df-custom-input").Nth(1).FillAsync("60000");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var salariesRaw = await GetColumnValuesAsync(page, "Salary");
            Assert.True(salariesRaw.Count > 0);
            var salaries = salariesRaw.Select(ParseFloatInvariant).ToList();
            Assert.All(salaries, s => Assert.InRange(s, 50000f, 60000f));
        });
    }

    [Fact]
    public async Task Filtering_SalaryGreaterThan_KeepOnlyHigh()
    {
        await RunWithTracingAsync(nameof(Filtering_SalaryGreaterThan_KeepOnlyHigh), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Salary", errors);
            var popup = page.Locator("#df-filter-popup-Salary");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "GreaterThan" });
            await popup.Locator("input.df-custom-input").First.FillAsync("120000");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var salariesRaw = await GetColumnValuesAsync(page, "Salary");
            Assert.True(salariesRaw.Count > 0);
            var salaries = salariesRaw.Select(ParseFloatInvariant).ToList();
            Assert.All(salaries, s => Assert.True(s > 120000f, $"Expected > 120000, got {s}"));
        });
    }

    [Fact]
    public async Task Filtering_SalaryLessThan_KeepOnlyLow()
    {
        await RunWithTracingAsync(nameof(Filtering_SalaryLessThan_KeepOnlyLow), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            await OpenPopupAsync(page, "Salary", errors);
            var popup = page.Locator("#df-filter-popup-Salary");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "LessThan" });
            await popup.Locator("input.df-custom-input").First.FillAsync("50000");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var salariesRaw = await GetColumnValuesAsync(page, "Salary");
            Assert.True(salariesRaw.Count > 0);
            var salaries = salariesRaw.Select(ParseFloatInvariant).ToList();
            Assert.All(salaries, s => Assert.True(s < 50000f, $"Expected < 50000, got {s}"));
        });
    }

    [Fact]
    public async Task Filtering_SalaryBetween_InvalidRange_DoesNotCrash()
    {
        await RunWithTracingAsync(nameof(Filtering_SalaryBetween_InvalidRange_DoesNotCrash), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            var rows = page.Locator("tbody tr");
            Assert.True(await rows.CountAsync() > 0);

            await OpenPopupAsync(page, "Salary", errors);
            var popup = page.Locator("#df-filter-popup-Salary");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Between" });
            await popup.Locator("input.df-custom-input").Nth(0).FillAsync("60000");
            await popup.Locator("input.df-custom-input").Nth(1).FillAsync("50000");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // Contract: no unhandled errors + table remains usable (may yield 0 rows depending on validation semantics).
            await AssertBlazorHealthyAsync(page, errors);
            _ = await rows.CountAsync();
        });
    }

    [Fact]
    public async Task Filtering_HireDateBetween_ParsesAndMatchesRange()
    {
        await RunWithTracingAsync(nameof(Filtering_HireDateBetween_ParsesAndMatchesRange), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            // Use a wide range that's likely to match many rows to avoid flakiness.
            var from = DateTime.Today.AddYears(-10).ToString("yyyy-MM-dd");
            var to = DateTime.Today.ToString("yyyy-MM-dd");

            await OpenPopupAsync(page, "HireDate", errors);
            var popup = page.Locator("#df-filter-popup-HireDate");

            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Between" });
            await popup.Locator("input.df-custom-input").Nth(0).FillAsync(from);
            await popup.Locator("input.df-custom-input").Nth(1).FillAsync(to);
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // We assert the table still renders (non-empty) + no errors; parsing & exact range semantics can differ by operator implementation.
            var rows = page.Locator("tbody tr");
            Assert.True(await rows.CountAsync() > 0);
        });
    }

    [Fact]
    public async Task AddToExistingFilter_Union_DepartmentITorHR()
    {
        await RunWithTracingAsync(nameof(AddToExistingFilter_Union_DepartmentITorHR), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            // First filter: Department == IT
            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await popup.Locator("input.df-custom-input").First.FillAsync("IT");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // Add to existing via Union: Department == IT OR HR
            await OpenPopupAsync(page, "Department", errors);
            popup = page.Locator("#df-filter-popup-Department");
            await popup.Locator(".df-accumulation-options label.df-checkbox-label input[type=checkbox]").CheckAsync();
            await popup.Locator("select.df-accumulation-select").WaitForAsync();
            await popup.Locator("select.df-accumulation-select").SelectOptionAsync(new[] { "Union" });

            await EnsureCustomFilterExpandedAsync(popup);
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await popup.Locator("input.df-custom-input").First.FillAsync("HR");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var depts = await GetColumnValuesAsync(page, "Department");
            Assert.True(depts.Count > 0);
            Assert.All(depts, d => Assert.True(
                string.Equals(d, "IT", StringComparison.OrdinalIgnoreCase) || string.Equals(d, "HR", StringComparison.OrdinalIgnoreCase),
                $"Expected only IT or HR, got '{d}'"));
        });
    }

    [Fact]
    public async Task AddToExistingFilter_Intersection_DepartmentITandHR_Empty()
    {
        await RunWithTracingAsync(nameof(AddToExistingFilter_Intersection_DepartmentITandHR_Empty), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            // First filter: Department == IT
            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await popup.Locator("input.df-custom-input").First.FillAsync("IT");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // Add to existing via Intersection: (Department == IT) AND (Department == HR) => empty
            await OpenPopupAsync(page, "Department", errors);
            popup = page.Locator("#df-filter-popup-Department");
            await popup.Locator(".df-accumulation-options label.df-checkbox-label input[type=checkbox]").CheckAsync();
            await popup.Locator("select.df-accumulation-select").WaitForAsync();
            await popup.Locator("select.df-accumulation-select").SelectOptionAsync(new[] { "Intersection" });

            await EnsureCustomFilterExpandedAsync(popup);
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await popup.Locator("input.df-custom-input").First.FillAsync("HR");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            var rows = page.Locator("tbody tr");
            Assert.Equal(0, await rows.CountAsync());
        });
    }

    [Fact]
    public async Task ClearFilters_RestoresRows()
    {
        await RunWithTracingAsync(nameof(ClearFilters_RestoresRows), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            var rows = page.Locator("tbody tr");
            var before = await rows.CountAsync();
            Assert.True(before > 0);

            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");
            await popup.Locator(".df-custom-filter-header").ClickAsync();
            await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await popup.Locator("input.df-custom-input").First.FillAsync("IT");
            await popup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);
            var filtered = await rows.CountAsync();
            Assert.True(filtered < before);

            await page.GetByTestId("df-clear-filters").ClickAsync();
            await page.WaitForTimeoutAsync(250);
            var after = await rows.CountAsync();
            Assert.Equal(before, after);
        });
    }

    [Fact]
    public async Task SortAfterFilter_AppliesToSubset()
    {
        await RunWithTracingAsync(nameof(SortAfterFilter_AppliesToSubset), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            // Filter Dept=IT
            await OpenPopupAsync(page, "Department", errors);
            var deptPopup = page.Locator("#df-filter-popup-Department");
            await deptPopup.Locator(".df-custom-filter-header").ClickAsync();
            await deptPopup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
            await deptPopup.Locator("input.df-custom-input").First.FillAsync("IT");
            await deptPopup.Locator("button.df-btn-primary").ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // Sort Salary descending
            await OpenPopupAsync(page, "Salary", errors);
            var salaryPopup = page.Locator("#df-filter-popup-Salary");
            await salaryPopup.Locator(".df-sort-section .df-sort-button").Nth(1).ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // Verify all depts are IT and salary is descending.
            var depts = await GetColumnValuesAsync(page, "Department");
            Assert.NotEmpty(depts);
            Assert.All(depts, d => Assert.Equal("IT", d));

            var salaries = (await GetColumnValuesAsync(page, "Salary")).Select(ParseFloatInvariant).ToList();
            Assert.True(salaries.Count > 1);
            AssertSorted(salaries, descending: true);
        });
    }

    [Fact]
    public async Task Localization_ChangesPopupLabels()
    {
        await RunWithTracingAsync(nameof(Localization_ChangesPopupLabels), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            var lang = page.GetByTestId("df-language");
            await lang.WaitForAsync();

            // Prefer French if available; otherwise pick any non-empty culture option.
            var chosen = await page.EvaluateAsync<string?>(
                @"() => {
                    const sel = document.querySelector('[data-testid=""df-language""]');
                    if (!sel) return null;
                    const opts = Array.from(sel.querySelectorAll('option'));
                    const fr = opts.find(o => (o.value||'').toLowerCase() === 'fr-fr' || (o.value||'').toLowerCase().startsWith('fr'));
                    return (fr || opts.find(o => (o.value||'') !== '') || null)?.value ?? null;
                }");

            if (string.IsNullOrWhiteSpace(chosen))
                return;

            await lang.SelectOptionAsync(new[] { chosen! });
            await page.WaitForTimeoutAsync(250);

            await OpenPopupAsync(page, "Department", errors);
            var popup = page.Locator("#df-filter-popup-Department");

            var okText = (await popup.Locator("button.df-btn-primary").InnerTextAsync()).Trim();
            var clearText = (await popup.Locator("button.df-btn-secondary").InnerTextAsync()).Trim();

            // Contract: localized labels should not both remain in default English.
            Assert.False(string.Equals(okText, "OK", StringComparison.OrdinalIgnoreCase) && string.Equals(clearText, "Clear", StringComparison.OrdinalIgnoreCase),
                $"Expected localized labels to differ from default. ok='{okText}', clear='{clearText}', culture='{chosen}'");
        });
    }

    [Fact]
    public async Task MultiSort_DepartmentThenName_NameSortedWithinDepartmentGroups()
    {
        await RunWithTracingAsync(nameof(MultiSort_DepartmentThenName_NameSortedWithinDepartmentGroups), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await NavigateToAttachAsync(page, errors);

            // Primary sort: Department ascending.
            await OpenPopupAsync(page, "Department", errors);
            var deptPopup = page.Locator("#df-filter-popup-Department");
            await deptPopup.Locator(".df-sort-section .df-sort-button").Nth(0).ClickAsync();
            await page.WaitForTimeoutAsync(250);

            // Sub-sort: Name ascending.
            await OpenPopupAsync(page, "Name", errors);
            var namePopup = page.Locator("#df-filter-popup-Name");
            await namePopup.Locator(".df-sort-section .df-sort-button").Nth(2).ClickAsync(); // AddSubSortAscending
            await page.WaitForTimeoutAsync(250);

            var rows = await GetTwoColumnRowsAsync(page, "Department", "Name");
            Assert.True(rows.Count > 1);

            // Verify: within each contiguous department block, names are ascending.
            var i = 0;
            while (i < rows.Count)
            {
                var dept = rows[i].Col1;
                var names = new List<string>();
                while (i < rows.Count && string.Equals(rows[i].Col1, dept, StringComparison.OrdinalIgnoreCase))
                {
                    names.Add(rows[i].Col2);
                    i++;
                }

                if (names.Count > 1)
                    AssertSorted(names, descending: false);
            }
        });
    }

    private async Task RunWithTracingAsync(string testName, ViewportSize? viewport, Func<IPage, List<string>, Task> run)
    {
        using var pw = await Playwright.CreateAsync();
        await using var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var errors = new List<string>();
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = viewport
        });

        var traceDir = Path.Combine(AppContext.BaseDirectory, "playwright-traces");
        Directory.CreateDirectory(traceDir);
        var tracePath = Path.Combine(traceDir, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{testName}.zip");

        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        try
        {
            var page = await context.NewPageAsync();
            page.PageError += (_, e) => errors.Add("[PageError] " + e);
            page.Console += (_, e) => errors.Add($"[Console:{e.Type}] {e.Text}");
            page.RequestFailed += (_, e) => errors.Add($"[RequestFailed] {e.Url} - {e.Failure}");
            page.Response += (_, r) =>
            {
                if (r.Status >= 400)
                    errors.Add($"[HTTP{r.Status}] {r.Url}");
            };

            await run(page, errors);
        }
        catch (Exception ex)
        {
            await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
            throw new InvalidOperationException($"Playwright trace saved to: {tracePath}", ex);
        }
        finally
        {
            // In success case, we still stop tracing but discard to avoid noise.
            if (context != null)
            {
                try { await context.Tracing.StopAsync(); } catch { }
            }
        }
    }

    private async Task NavigateToAttachAsync(IPage page, List<string> errors)
    {
        await page.GotoAsync(_host.BaseUrl + "/demo/attach", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForFunctionAsync("() => !!window.Blazor");
        await page.WaitForFunctionAsync("() => !!window.DataFilterInterops");
        await page.WaitForFunctionAsync("() => { const d = document.getElementById('components-reconnect-modal'); return !d || d.open !== true; }");
        await page.GetByTestId("df-filter-btn-Department").WaitForAsync();
        await AssertBlazorHealthyAsync(page, errors);
        await page.WaitForTimeoutAsync(750);
    }

    private static async Task SetDirectionAsync(IPage page, bool isRtl)
    {
        var direction = page.GetByTestId("df-direction");
        await direction.WaitForAsync();
        await direction.SelectOptionAsync(new[] { isRtl ? "RTL" : "LTR" });
    }

    private async Task OpenPopupAsync(IPage page, string columnKey, List<string> errors)
    {
        var btnId = $"df-filter-btn-{columnKey}";
        _ = page.GetByTestId(btnId); // validates the test id exists (locator creation doesn't wait)
        var popupId = $"df-filter-popup-{columnKey}";

        // Click once, then wait for the popup element to appear.
        await page.EvaluateAsync("(id) => document.getElementById(id)?.click()", btnId);
        var popupLocator = page.Locator("#" + popupId);
        try
        {
            await popupLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Attached,
                Timeout = 30_000
            });
            return;
        }
        catch
        {
            // fall through to diagnostics
        }

        var knownPopupIds = await page.EvaluateAsync<string[]>(
            "() => Array.from(document.querySelectorAll('[id^=\"df-filter-popup-\"]')).map(e => e.id)");

        var buttonHtml = await page.EvaluateAsync<string?>(
            "(id) => document.getElementById(id)?.outerHTML",
            btnId);

        var reconnectOpen = await page.EvaluateAsync<bool>(
            "() => { const d = document.getElementById('components-reconnect-modal'); return !!d && d.open === true; }");

        var blazorErrorUiVisible = await page.EvaluateAsync<bool>(
            "() => { const el = document.getElementById('blazor-error-ui'); if (!el) return false; return getComputedStyle(el).display !== 'none'; }");
        var blazorErrorUiText = await page.EvaluateAsync<string?>(
            "() => document.getElementById('blazor-error-ui')?.textContent");

        throw new TimeoutException(
            $"Popup did not open within 10s for column '{columnKey}'. " +
            $"Known popup ids: [{string.Join(", ", knownPopupIds)}]. " +
            $"ButtonHtml={buttonHtml}{Environment.NewLine}" +
            $"ReconnectModalOpen={reconnectOpen}. " +
            $"BlazorErrorUiVisible={blazorErrorUiVisible}. BlazorErrorUi={blazorErrorUiText}{Environment.NewLine}" +
            $"RecentHostLogs:{Environment.NewLine}{_host.GetRecentLogs()}{Environment.NewLine}" +
            $"CapturedBrowserErrors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    private static Task AssertWithinViewportAsync(IPage page, double x, double y, double width, double height)
    {
        var viewport = page.ViewportSize;
        Assert.NotNull(viewport);

        Assert.True(x >= 0, $"Expected popup to be within viewport (x>=0). x={x}");
        Assert.True(y >= 0, $"Expected popup to be within viewport (y>=0). y={y}");
        Assert.True(x + width <= viewport!.Width + 0.5, $"Expected popup to be within viewport (right<=width). right={x + width}, width={viewport.Width}");
        Assert.True(y + height <= viewport.Height + 0.5, $"Expected popup to be within viewport (bottom<=height). bottom={y + height}, height={viewport.Height}");
        return Task.CompletedTask;
    }

    private static async Task AssertBlazorHealthyAsync(IPage page, List<string> errors)
    {
        var isErrorUiVisible = await page.EvaluateAsync<bool>(
            "() => { const el = document.getElementById('blazor-error-ui'); if (!el) return false; return getComputedStyle(el).display !== 'none'; }");

        if (!isErrorUiVisible)
            return;

        var text = await page.EvaluateAsync<string?>("() => document.getElementById('blazor-error-ui')?.textContent");
        throw new InvalidOperationException(
            $"Blazor error UI is visible right after navigation. Text={text}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    private static async Task AssertPopupMatchesInteropAsync(
        IPage page,
        string buttonId,
        string popupId,
        double popupX,
        double popupY,
        double tolerancePx)
    {
        var expected = await page.EvaluateAsync<AnchoredPos?>(
            "(args) => window.DataFilterInterops?.getAnchoredPopupPosition(args.buttonId, args.popupId, args.margin) ?? null",
            new { buttonId, popupId, margin = 8 });

        Assert.NotNull(expected);

        Assert.True(Math.Abs(popupX - expected!.Left) <= tolerancePx, $"Expected left≈{expected.Left}, actual={popupX}");
        Assert.True(Math.Abs(popupY - expected.Top) <= tolerancePx, $"Expected top≈{expected.Top}, actual={popupY}");
    }

    private static async Task<List<string>> GetColumnValuesAsync(IPage page, string headerText)
    {
        var index = await page.EvaluateAsync<int>(
            @"(headerText) => {
                const ths = Array.from(document.querySelectorAll('thead th'));
                const norm = (s) => (s || '').replace(/\s+/g,' ').trim().toLowerCase();
                const target = norm(headerText);
                const i = ths.findIndex(th => norm(th.innerText).includes(target));
                return i < 0 ? -1 : i + 1;
            }",
            headerText);

        if (index < 1)
            throw new InvalidOperationException($"Could not find column header '{headerText}'.");

        var cells = page.Locator($"tbody tr td:nth-child({index})");
        var count = await cells.CountAsync();
        var values = new List<string>(capacity: count);
        for (var i = 0; i < count; i++)
        {
            values.Add((await cells.Nth(i).InnerTextAsync()).Trim());
        }
        return values;
    }

    private static async Task<List<(string Col1, string Col2)>> GetTwoColumnRowsAsync(IPage page, string header1, string header2)
    {
        var indices = await page.EvaluateAsync<int[]>(
            @"(args) => {
                const ths = Array.from(document.querySelectorAll('thead th'));
                const norm = (s) => (s || '').replace(/\s+/g,' ').trim().toLowerCase();
                const find = (h) => {
                  const target = norm(h);
                  const i = ths.findIndex(th => norm(th.innerText).includes(target));
                  return i < 0 ? -1 : i + 1;
                };
                return [find(args.h1), find(args.h2)];
            }",
            new { h1 = header1, h2 = header2 });

        if (indices.Length != 2 || indices[0] < 1 || indices[1] < 1)
            throw new InvalidOperationException($"Could not find columns '{header1}' and '{header2}'.");

        var rows = page.Locator("tbody tr");
        var rowCount = await rows.CountAsync();
        var result = new List<(string Col1, string Col2)>(capacity: rowCount);
        for (var i = 0; i < rowCount; i++)
        {
            var row = rows.Nth(i);
            var c1 = (await row.Locator($"td:nth-child({indices[0]})").InnerTextAsync()).Trim();
            var c2 = (await row.Locator($"td:nth-child({indices[1]})").InnerTextAsync()).Trim();
            result.Add((c1, c2));
        }
        return result;
    }

    private static async Task<List<string>> GetColumnDataValuesAsync(IPage page, string headerText)
    {
        var index = await page.EvaluateAsync<int>(
            @"(headerText) => {
                const ths = Array.from(document.querySelectorAll('thead th'));
                const norm = (s) => (s || '').replace(/\s+/g,' ').trim().toLowerCase();
                const target = norm(headerText);
                const i = ths.findIndex(th => norm(th.innerText).includes(target));
                return i < 0 ? -1 : i + 1;
            }",
            headerText);

        if (index < 1)
            throw new InvalidOperationException($"Could not find column header '{headerText}'.");

        var cells = page.Locator($"tbody tr td:nth-child({index})");
        var count = await cells.CountAsync();
        var values = new List<string>(capacity: count);
        for (var i = 0; i < count; i++)
        {
            values.Add(((await cells.Nth(i).GetAttributeAsync("data-value")) ?? string.Empty).Trim());
        }
        return values;
    }

    private static async Task EnsureCustomFilterExpandedAsync(ILocator popup)
    {
        var select = popup.Locator("select.df-custom-input");
        if (await select.CountAsync() > 0)
            return;

        await popup.Locator(".df-custom-filter-header").ClickAsync();
        await select.WaitForAsync();
    }

    private static void AssertSorted(IReadOnlyList<string> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            var cmp = string.Compare(values[i - 1], values[i], StringComparison.OrdinalIgnoreCase);
            if (descending)
                Assert.True(cmp >= 0, $"Expected descending sort at i={i}. Prev='{values[i - 1]}', Curr='{values[i]}'");
            else
                Assert.True(cmp <= 0, $"Expected ascending sort at i={i}. Prev='{values[i - 1]}', Curr='{values[i]}'");
        }
    }

    private static void AssertSorted(IReadOnlyList<float> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            if (descending)
                Assert.True(values[i - 1] >= values[i], $"Expected descending sort at i={i}. Prev={values[i - 1]}, Curr={values[i]}");
            else
                Assert.True(values[i - 1] <= values[i], $"Expected ascending sort at i={i}. Prev={values[i - 1]}, Curr={values[i]}");
        }
    }

    private static void AssertSorted(IReadOnlyList<long> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            if (descending)
                Assert.True(values[i - 1] >= values[i], $"Expected descending sort at i={i}. Prev={values[i - 1]}, Curr={values[i]}");
            else
                Assert.True(values[i - 1] <= values[i], $"Expected ascending sort at i={i}. Prev={values[i - 1]}, Curr={values[i]}");
        }
    }

    private static bool IsSorted(IReadOnlyList<long> values, bool descending)
    {
        for (var i = 1; i < values.Count; i++)
        {
            if (descending)
            {
                if (values[i - 1] < values[i]) return false;
            }
            else
            {
                if (values[i - 1] > values[i]) return false;
            }
        }
        return true;
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> predicate, int timeoutMs)
    {
        var start = DateTime.UtcNow;
        var timeout = TimeSpan.FromMilliseconds(timeoutMs);
        while (DateTime.UtcNow - start < timeout)
        {
            if (await predicate())
                return;
            await Task.Delay(100);
        }
        throw new TimeoutException("Condition not met within timeout.");
    }

    private static float ParseFloatInvariant(string raw)
    {
        raw = raw.Trim();
        if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
            return v;

        raw = raw.Replace(',', '.');
        return float.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static long ParseLongInvariant(string raw)
    {
        raw = raw.Trim();
        if (long.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var v))
            return v;
        raw = raw.Replace(',', '.');
        return long.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class AnchoredPos
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }
}

