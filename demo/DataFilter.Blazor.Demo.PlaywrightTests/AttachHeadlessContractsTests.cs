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

    private static float ParseFloatInvariant(string raw)
    {
        raw = raw.Trim();
        if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
            return v;

        raw = raw.Replace(',', '.');
        return float.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class AnchoredPos
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }
}

