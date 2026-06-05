namespace DataFilter.Blazor.Demo.PlaywrightTests;

internal static class PlaywrightBrowserBootstrap
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static bool _installed;

    public static async Task EnsureChromiumAsync()
    {
        if (_installed)
            return;

        await Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_installed)
                return;

            if (Environment.GetEnvironmentVariable("SKIP_PLAYWRIGHT_INSTALL") == "1")
                return;

            await Task.Run(() => Microsoft.Playwright.Program.Main(new[] { "install", "chromium" }))
                .ConfigureAwait(false);
            _installed = true;
        }
        finally
        {
            Gate.Release();
        }
    }
}
