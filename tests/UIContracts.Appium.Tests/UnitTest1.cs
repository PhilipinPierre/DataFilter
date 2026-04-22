using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using Xunit;

namespace UIContracts.Appium.Tests;

public sealed class DataFilterMauiDemo_AttachContractsTests
{
    [Fact]
    public void PopupOpenClose_AttachPage()
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

        // NOTE: capabilities like deviceName/udid/platformVersion are typically required in real setups.
        // Those can be added through environment variables later.

        if (platform == "android")
        {
            options.PlatformName = "Android";
            using var driver = new AndroidDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(60));
            RunPopupOpenClose(driver);
            return;
        }

        if (platform == "ios")
        {
            options.PlatformName = "iOS";
            using var driver = new IOSDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(60));
            RunPopupOpenClose(driver);
            return;
        }
    }

    private static void RunPopupOpenClose(AppiumDriver driver)
    {
        // Minimal “contract hook”: we require the app to expose stable automation ids.
        // Once we add those to the MAUI demo UI, this test becomes deterministic.
        //
        // Expected ids (to be implemented next):
        // - df-filter-btn-Department
        // - df-filter-popup-Department
        //
        // For now, we only assert session is alive.
        Assert.NotNull(driver.SessionId);

        // Placeholder: Once ids exist:
        // var btn = driver.FindElement(MobileBy.AccessibilityId("df-filter-btn-Department"));
        // btn.Click();
        // var popup = driver.FindElement(MobileBy.AccessibilityId("df-filter-popup-Department"));
        // Assert.True(popup.Displayed);
    }
}
