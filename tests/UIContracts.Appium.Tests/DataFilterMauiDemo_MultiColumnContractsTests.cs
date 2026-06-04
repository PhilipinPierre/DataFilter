using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using UIContracts.Common;
using Xunit;

namespace UIContracts.Appium.Tests;

public sealed class DataFilterMauiDemo_MultiColumnContractsTests
{
    [Fact]
    public void MultiColumnFilter_DeptItThenCountryUsa()
    {
        var platform = (Environment.GetEnvironmentVariable("UICT_APP_PLATFORM") ?? "").Trim().ToLowerInvariant();
        var serverUrl = (Environment.GetEnvironmentVariable("UICT_APPIUM_SERVER") ?? "").Trim();
        var appPath = (Environment.GetEnvironmentVariable("UICT_APP_PATH") ?? "").Trim();

        if (string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(serverUrl) || string.IsNullOrWhiteSpace(appPath))
            return;

        var options = new AppiumOptions();
        options.AddAdditionalAppiumOption("app", appPath);
        options.AddAdditionalAppiumOption("newCommandTimeout", 180);

        if (platform == "android")
        {
            options.PlatformName = "Android";
            using var driver = new AndroidDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(60));
            try
            {
                RunMultiColumn(driver);
            }
            finally
            {
                try { driver.Quit(); } catch { }
                driver.Dispose();
            }
            return;
        }

        if (platform == "ios")
        {
            options.PlatformName = "iOS";
            using var driver = new IOSDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(60));
            try
            {
                RunMultiColumn(driver);
            }
            finally
            {
                try { driver.Quit(); } catch { }
                driver.Dispose();
            }
        }
    }

    private static void RunMultiColumn(AppiumDriver driver)
    {
        NavigateToAttachTab(driver);

        var deptBtn = driver.FindElement(MobileBy.AccessibilityId("df-filter-btn-Department"));
        deptBtn.Click();
        var deptPopup = driver.FindElement(MobileBy.AccessibilityId("df-filter-popup-Department"));
        Assert.True(deptPopup.Displayed);
        FindByText(driver, "IT")?.Click();
        FindByText(driver, "OK")?.Click();

        var deptLabels = driver.FindElements(MobileBy.AccessibilityId("df-row-dept"));
        Assert.NotEmpty(deptLabels);
        foreach (var el in deptLabels)
        {
            if (!el.Displayed) continue;
            Assert.Equal("IT", (el.Text ?? "").Trim());
        }

        var countryBtn = driver.FindElements(MobileBy.AccessibilityId("df-filter-btn-Country"));
        if (countryBtn.Count == 0)
            return;

        countryBtn[0].Click();
        driver.FindElement(MobileBy.AccessibilityId("df-filter-popup-Country"));
        FindByText(driver, "USA")?.Click();
        FindByText(driver, "OK")?.Click();

        foreach (var el in driver.FindElements(MobileBy.AccessibilityId("df-row-dept")))
        {
            if (!el.Displayed) continue;
            Assert.Equal("IT", (el.Text ?? "").Trim());
        }

        var countries = driver.FindElements(MobileBy.AccessibilityId("df-row-country"));
        foreach (var el in countries)
        {
            if (!el.Displayed) continue;
            Assert.Equal("USA", (el.Text ?? "").Trim());
        }
    }

    private static void NavigateToAttachTab(AppiumDriver driver)
    {
        try
        {
            driver.FindElement(By.XPath("//*[contains(@text,'Attach') or contains(@name,'Attach') or contains(@label,'Attach')]")).Click();
        }
        catch
        {
            // Already on attach or single-page host.
        }
    }

    private static IWebElement? FindByText(AppiumDriver driver, string text)
    {
        try
        {
            return driver.FindElement(By.XPath($"//*[@text='{text}' or @name='{text}' or @label='{text}' or @value='{text}']"));
        }
        catch
        {
            return null;
        }
    }
}
