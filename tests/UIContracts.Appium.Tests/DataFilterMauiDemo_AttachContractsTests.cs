using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using Xunit;

namespace UIContracts.Appium.Tests;

public sealed class DataFilterMauiDemo_AttachContractsTests
{
    [Fact]
    public void PopupOpenCloseAndFiltering_DepartmentEqualsIT()
    {
        // This suite is intentionally environment-driven.
        // Configure these variables in CI or locally when a device/emulator is available:
        //
        // UICT_APP_PLATFORM=android|ios
        // UICT_APPIUM_SERVER=http://127.0.0.1:4723/
        // UICT_APP_PATH=<path to built .apk/.app/.ipa>
        //
        var platform = (Environment.GetEnvironmentVariable("UICT_APP_PLATFORM") ?? "").Trim().ToLowerInvariant();
        var serverUrl = (Environment.GetEnvironmentVariable("UICT_APPIUM_SERVER") ?? "").Trim();
        var appPath = (Environment.GetEnvironmentVariable("UICT_APP_PATH") ?? "").Trim();

        if (string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(serverUrl) || string.IsNullOrWhiteSpace(appPath))
            return; // no-op until environment is configured

        var options = new AppiumOptions();
        options.AddAdditionalAppiumOption("app", appPath);
        options.AddAdditionalAppiumOption("newCommandTimeout", 180);

        if (platform == "android")
        {
            options.PlatformName = "Android";
            AndroidDriver? driver = null;
            try
            {
                driver = new AndroidDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(60));
                RunPopupOpenCloseAndFiltering(driver);
            }
            finally
            {
                try { driver?.Quit(); } catch { }
                driver?.Dispose();
            }
            return;
        }

        if (platform == "ios")
        {
            options.PlatformName = "iOS";
            IOSDriver? driver = null;
            try
            {
                driver = new IOSDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(60));
                RunPopupOpenCloseAndFiltering(driver);
            }
            finally
            {
                try { driver?.Quit(); } catch { }
                driver?.Dispose();
            }
            return;
        }
    }

    private static void RunPopupOpenCloseAndFiltering(AppiumDriver driver)
    {
        Assert.NotNull(driver.SessionId);

        // PopupOpenClose
        var btn = driver.FindElement(MobileBy.AccessibilityId("df-filter-btn-Department"));
        btn.Click();

        var popup = driver.FindElement(MobileBy.AccessibilityId("df-filter-popup-Department"));
        Assert.True(popup.Displayed);

        // FilteringAffectsRows (best-effort; relies on popup content exposing accessible labels for IT + OK)
        var it = FindByText(driver, "IT");
        it?.Click();

        var ok = FindByText(driver, "OK");
        ok?.Click();

        // Verify every visible department label is IT.
        var deptLabels = driver.FindElements(MobileBy.AccessibilityId("df-row-dept"));
        Assert.NotEmpty(deptLabels);
        foreach (var el in deptLabels)
        {
            if (!el.Displayed) continue;
            Assert.Equal("IT", (el.Text ?? "").Trim());
        }
    }

    private static IWebElement? FindByText(AppiumDriver driver, string text)
    {
        try
        {
            // Android / iOS generic fallback. Prefer AutomationId when available.
            return driver.FindElement(By.XPath($"//*[@text='{text}' or @name='{text}' or @label='{text}' or @value='{text}']"));
        }
        catch
        {
            return null;
        }
    }
}

