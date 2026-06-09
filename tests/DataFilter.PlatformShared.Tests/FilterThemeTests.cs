using DataFilter.PlatformShared.Theming;

namespace DataFilter.PlatformShared.Tests;

public sealed class FilterThemeTests
{
    [Theory]
    [InlineData("#FFFFFF", 255, 255, 255, 255)]
    [InlineData("#FF252526", 255, 37, 37, 38)]
    [InlineData("252526", 255, 37, 37, 38)]
    public void FilterColor_Parse_RoundTrips(string hex, byte a, byte r, byte g, byte b)
    {
        var color = FilterColor.Parse(hex);
        Assert.Equal(a, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }

    [Fact]
    public void FilterTheme_ToCssVariables_ContainsPrimaryKeys()
    {
        var vars = FilterTheme.Light.ToCssVariables();
        Assert.Equal(FilterTheme.Light.PopupBackground, vars[FilterThemeResourceKeys.CssPopupBackground]);
        Assert.Equal(FilterTheme.Light.PrimaryColor, vars[FilterThemeResourceKeys.CssPrimaryColor]);
    }

    [Fact]
    public void FilterTheme_With_OverridesSingleProperty()
    {
        var custom = FilterTheme.Light.With(primaryColor: "#ABCDEF");
        Assert.Equal("#ABCDEF", custom.PrimaryColor);
        Assert.Equal(FilterTheme.Light.PopupBackground, custom.PopupBackground);
    }
}
