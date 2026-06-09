namespace DataFilter.PlatformShared.Theming;

/// <summary>
/// Canonical resource key names for filter UI theming across UI stacks.
/// </summary>
public static class FilterThemeResourceKeys
{
    // ── WPF ResourceDictionary x:Key names ──────────────────────────────────

    public const string WpfPopupBackground = "FilterPopupBackground";
    public const string WpfPopupBackgroundColor = "FilterPopupBackgroundColor";
    public const string WpfPopupForeground = "FilterPopupForeground";
    public const string WpfPopupForegroundColor = "FilterPopupForegroundColor";
    public const string WpfPopupBorder = "FilterPopupBorder";
    public const string WpfPopupBorderColor = "FilterPopupBorderColor";
    public const string WpfButtonActiveColor = "FilterButtonActiveColor";
    public const string WpfButtonInactiveColor = "FilterButtonInactiveColor";
    public const string WpfPopupMaxHeight = "FilterPopupMaxHeight";
    public const string WpfButtonStyle = "FilterButtonStyle";
    public const string WpfCheckBoxStyle = "FilterCheckBoxStyle";
    public const string WpfSearchBoxStyle = "FilterSearchBoxStyle";
    public const string WpfExpanderStyle = "FilterExpanderStyle";
    public const string WpfSearchBlockStyle = "FilterSearchBlockStyle";

    // ── Blazor CSS custom properties (set on :root or a theme wrapper) ──────

    public const string CssPopupBackground = "--df-popup-bg";
    public const string CssPopupForeground = "--df-popup-fg";
    public const string CssPopupBorder = "--df-popup-border";
    public const string CssPrimaryColor = "--df-primary-color";
    public const string CssSecondaryBackground = "--df-secondary-bg";
    public const string CssSecondaryBorder = "--df-secondary-border";
    public const string CssHeaderBackground = "--df-header-bg";
    public const string CssFontFamily = "--df-font-family";
    public const string CssFontSize = "--df-font-size";
    public const string CssButtonActiveColor = "--df-button-active-color";
    public const string CssButtonInactiveColor = "--df-button-inactive-color";
    public const string CssPrimaryButtonForeground = "--df-btn-primary-fg";
    public const string CssOverlayBackground = "--df-overlay-bg";
    public const string CssFilterBarClusterBackground = "--df-filter-bar-cluster-bg";
    public const string CssPopupShadow = "--df-popup-shadow";
    public const string CssResizeHandleColor = "--df-resize-handle-color";

    /// <summary>Blazor class applied for built-in dark preset (<see cref="FilterTheme.Dark"/>).</summary>
    public const string BlazorDarkThemeClass = "df-theme-dark";

    /// <summary>Blazor class for light preset wrapper (optional; :root defaults apply without it).</summary>
    public const string BlazorLightThemeClass = "df-theme-light";
}
