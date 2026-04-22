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
    public async Task AnchoredPositioning_Department()
    {
        await RunWithTracingAsync(nameof(AnchoredPositioning_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await page.GotoAsync(_host.BaseUrl + "/demo/attach", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await page.WaitForFunctionAsync("() => !!window.Blazor");
            await page.WaitForFunctionAsync("() => !!window.DataFilterInterops");
            await page.WaitForFunctionAsync("() => { const d = document.getElementById('components-reconnect-modal'); return !d || d.open !== true; }");
            await page.GetByTestId("df-filter-btn-Department").WaitForAsync();
            await AssertBlazorHealthyAsync(page, errors);
            await page.WaitForTimeoutAsync(750);

            await OpenPopupAsync(page, "Department", errors);
            var btn = page.GetByTestId("df-filter-btn-Department");
            var popup = page.Locator("#df-filter-popup-Department");

            var btnBox = await btn.BoundingBoxAsync();
            var popupBox = await popup.BoundingBoxAsync();
            Assert.NotNull(btnBox);
            Assert.NotNull(popupBox);

            await AssertPopupMatchesInteropAsync(page, "df-filter-btn-Department", "df-filter-popup-Department", popupBox!.X, popupBox.Y, tolerancePx: 2);
        });
    }

    [Fact]
    public async Task ScrollKeepsPopupAnchored_Department()
    {
        await RunWithTracingAsync(nameof(ScrollKeepsPopupAnchored_Department), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await page.GotoAsync(_host.BaseUrl + "/demo/attach", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await page.WaitForFunctionAsync("() => !!window.Blazor");
            await page.WaitForFunctionAsync("() => !!window.DataFilterInterops");
            await page.WaitForFunctionAsync("() => { const d = document.getElementById('components-reconnect-modal'); return !d || d.open !== true; }");
            await page.GetByTestId("df-filter-btn-Department").WaitForAsync();
            await AssertBlazorHealthyAsync(page, errors);
            await page.WaitForTimeoutAsync(750);

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
        });
    }

    [Fact]
    public async Task FilteringAffectsRows_DepartmentEqualsIT()
    {
        await RunWithTracingAsync(nameof(FilteringAffectsRows_DepartmentEqualsIT), new ViewportSize { Width = 1280, Height = 720 }, async (page, errors) =>
        {
            await page.GotoAsync(_host.BaseUrl + "/demo/attach", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await page.WaitForFunctionAsync("() => !!window.Blazor");
            await page.WaitForFunctionAsync("() => !!window.DataFilterInterops");
            await page.WaitForFunctionAsync("() => { const d = document.getElementById('components-reconnect-modal'); return !d || d.open !== true; }");
            await page.GetByTestId("df-filter-btn-Department").WaitForAsync();
            await AssertBlazorHealthyAsync(page, errors);
            await page.WaitForTimeoutAsync(750);

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

    private sealed class AnchoredPos
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }
}

