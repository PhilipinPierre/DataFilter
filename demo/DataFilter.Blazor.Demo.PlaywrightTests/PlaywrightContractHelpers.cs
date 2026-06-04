using Microsoft.Playwright;
using UIContracts.Common;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

internal static class PlaywrightContractHelpers
{
    public static async Task NavigateToRouteAsync(
        IPage page,
        DemoHostFixture host,
        string route,
        string readyTestId,
        List<string> errors)
    {
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var beforeErrorCount = errors.Count;
            try
            {
                await page.GotoAsync(
                    host.BaseUrl + route,
                    new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 60_000
                    });

                await page.WaitForFunctionAsync("() => !!window.Blazor");
                await page.WaitForFunctionAsync("() => !!window.DataFilterInterops");
                await page.WaitForFunctionAsync(
                    "() => { const d = document.getElementById('components-reconnect-modal'); return !d || d.open !== true; }");
                await page.GetByTestId(readyTestId).WaitForAsync();
                await AssertBlazorHealthyAsync(page, errors);
                await page.WaitForTimeoutAsync(750);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientWasmNetworkError(ex, errors, beforeErrorCount))
            {
                await page.WaitForTimeoutAsync(1000);
                try
                {
                    await page.ReloadAsync(new PageReloadOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = 60_000
                    });
                }
                catch
                {
                    // ignored
                }
            }
        }

        throw new InvalidOperationException($"Failed to navigate to {route} after multiple attempts.");
    }

    public static async Task OpenPopupAsync(IPage page, DemoHostFixture host, string columnKey, List<string> errors)
    {
        var btnId = $"df-filter-btn-{columnKey}";
        var popupId = $"df-filter-popup-{columnKey}";

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
            // fall through
        }

        throw new TimeoutException(
            $"Popup did not open for column '{columnKey}'. RecentHostLogs:{Environment.NewLine}{host.GetRecentLogs()}{Environment.NewLine}" +
            $"CapturedBrowserErrors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    public static async Task WaitForGridDataRowsAsync(IPage page)
    {
        var rows = page.Locator("table.df-grid tbody tr");
        await rows.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 60_000 });
        await page.WaitForFunctionAsync(
            @"() => {
                const trs = Array.from(document.querySelectorAll('table.df-grid tbody tr'));
                return trs.length > 0 && !trs.some(tr => tr.textContent?.includes('Loading'));
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = 60_000 });
    }

    public static async Task ApplyCustomEqualsAsync(ILocator popup, string value)
    {
        await EnsureCustomFilterExpandedAsync(popup);
        await popup.Locator("select.df-custom-input").SelectOptionAsync(new[] { "Equals" });
        await popup.Locator("input.df-custom-input").First.FillAsync(value);
        await popup.Locator("button.df-btn-primary").ClickAsync();
    }

    public static string HeaderTextForProperty(string propertyName) => propertyName switch
    {
        "HireDate" => "Hire Date",
        "Id" => "ID",
        _ => propertyName
    };

    public static async Task ApplyColumnFilterCaseAsync(
        IPage page,
        DemoHostFixture host,
        ColumnFilterCase filterCase,
        List<string> errors)
    {
        await OpenPopupAsync(page, host, filterCase.PropertyName, errors);
        var popup = page.Locator($"#df-filter-popup-{filterCase.PropertyName}");
        await EnsureCustomFilterExpandedAsync(popup);
        await popup.Locator("select.df-custom-input").First.SelectOptionAsync(new[] { filterCase.UiOperator });

        if (string.Equals(filterCase.UiOperator, "Between", StringComparison.OrdinalIgnoreCase))
        {
            var parts = filterCase.FilterValue.Split('|');
            await popup.Locator("input.df-custom-input").Nth(0).FillAsync(parts[0]);
            await popup.Locator("input.df-custom-input").Nth(1).FillAsync(parts.Length > 1 ? parts[1] : parts[0]);
        }
        else
        {
            await popup.Locator("input.df-custom-input").First.FillAsync(filterCase.FilterValue);
        }

        await popup.Locator("button.df-btn-primary").ClickAsync();
        await page.WaitForTimeoutAsync(300);
    }

    public static async Task AssertColumnFilterInvariantAsync(IPage page, ColumnFilterCase filterCase)
    {
        var header = HeaderTextForProperty(filterCase.PropertyName);
        switch (filterCase.PropertyName)
        {
            case "Department":
                var depts = await GetColumnValuesAsync(page, header);
                Assert.NotEmpty(depts);
                Assert.All(depts, d => Assert.True(RowInvariants.DepartmentEquals(d, filterCase.FilterValue)));
                break;
            case "Name":
                var names = await GetColumnValuesAsync(page, header);
                Assert.NotEmpty(names);
                Assert.All(names, n => Assert.True(RowInvariants.NameContains(n, filterCase.FilterValue)));
                break;
            case "Country":
                var countries = await GetColumnValuesAsync(page, header);
                Assert.NotEmpty(countries);
                Assert.All(countries, c => Assert.True(RowInvariants.CountryEquals(c, filterCase.FilterValue)));
                break;
            case "Salary" when filterCase.UiOperator == "GreaterThan":
                var salaries = await GetColumnDataValuesAsync(page, header);
                Assert.NotEmpty(salaries);
                Assert.All(salaries, s => Assert.True(RowInvariants.SalaryGreaterThan(s, decimal.Parse(filterCase.FilterValue))));
                break;
            case "HireDate" when filterCase.UiOperator == "Between":
                var parts = filterCase.FilterValue.Split('|');
                var min = DateTime.Parse(parts[0]).Ticks;
                var max = DateTime.Parse(parts[1]).Ticks;
                var dates = await GetColumnDataValuesAsync(page, header);
                Assert.NotEmpty(dates);
                Assert.All(dates, d => Assert.True(RowInvariants.HireDateBetweenTicks(d, min, max)));
                break;
            default:
                var values = await GetColumnValuesAsync(page, header);
                Assert.NotEmpty(values);
                break;
        }
    }

    public static async Task ApplyDepartmentEqualsItAsync(IPage page, DemoHostFixture host, List<string> errors)
    {
        await OpenPopupAsync(page, host, "Department", errors);
        var popup = page.Locator("#df-filter-popup-Department");
        await ApplyCustomEqualsAsync(popup, "IT");
        await page.WaitForTimeoutAsync(300);
    }

    public static async Task<List<string>> GetColumnDataValuesAsync(IPage page, string headerText)
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
            values.Add(((await cells.Nth(i).GetAttributeAsync("data-value")) ?? string.Empty).Trim());
        return values;
    }

    public static async Task<List<string>> GetColumnValuesAsync(IPage page, string headerText)
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
            values.Add((await cells.Nth(i).InnerTextAsync()).Trim());
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

    private static async Task AssertBlazorHealthyAsync(IPage page, List<string> errors)
    {
        var isErrorUiVisible = await page.EvaluateAsync<bool>(
            "() => { const el = document.getElementById('blazor-error-ui'); if (!el) return false; return getComputedStyle(el).display !== 'none'; }");

        if (!isErrorUiVisible)
            return;

        var text = await page.EvaluateAsync<string?>("() => document.getElementById('blazor-error-ui')?.textContent");
        throw new InvalidOperationException(
            $"Blazor error UI is visible. Text={text}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    private static bool IsTransientWasmNetworkError(Exception ex, List<string> errors, int fromIndex)
    {
        static bool LooksLikeTransient(string s) =>
            s.Contains("ERR_NETWORK_CHANGED", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Failed to fetch", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("mono_download_assets", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Timeout", StringComparison.OrdinalIgnoreCase);

        if (LooksLikeTransient(ex.ToString()))
            return true;

        for (var i = Math.Max(0, fromIndex); i < errors.Count; i++)
        {
            if (LooksLikeTransient(errors[i]))
                return true;
        }

        return false;
    }
}
