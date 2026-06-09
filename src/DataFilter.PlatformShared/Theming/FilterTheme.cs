namespace DataFilter.PlatformShared.Theming;

/// <summary>
/// Cross-framework visual theme for filter popups, filter bar, and header chrome.
/// Set <see cref="Current"/> at runtime or pass an instance to platform-specific apply methods.
/// </summary>
public sealed class FilterTheme
{
    private static FilterTheme _current = null!;

    static FilterTheme() => _current = Light;

    /// <summary>Application-wide theme used when a control does not specify its own.</summary>
    public static FilterTheme Current
    {
        get => _current;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_current, value))
                return;

            _current = value;
            CurrentChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>Raised when <see cref="Current"/> changes.</summary>
    public static event EventHandler? CurrentChanged;

    /// <summary>Built-in light palette (aligned with WPF <c>FilterLightTheme.xaml</c> and Blazor <c>:root</c>).</summary>
    public static FilterTheme Light { get; } = new()
    {
        PopupBackground = "#FFFFFF",
        PopupForeground = "#333333",
        PopupBorder = "#CCCCCC",
        PrimaryColor = "#007BFF",
        SecondaryBackground = "#EEEEEE",
        SecondaryBorder = "#CCCCCC",
        HeaderBackground = "#F9F9F9",
        ButtonActiveColor = "#1E90FF",
        ButtonInactiveColor = "#808080",
        PrimaryButtonForeground = "#FFFFFF",
        OverlayBackground = "#80000000",
        FilterBarClusterBackground = "#08000000",
        PopupShadow = "0 2px 10px rgba(0,0,0,0.2)",
        ResizeHandleColor = "#CCCCCC",
        FontFamily = "sans-serif",
        FontSizePt = 13,
        PopupMaxHeight = 450,
    };

    /// <summary>Built-in dark palette (aligned with WPF <c>FilterDarkTheme.xaml</c> and Blazor <c>.df-theme-dark</c>).</summary>
    public static FilterTheme Dark { get; } = new()
    {
        PopupBackground = "#252526",
        PopupForeground = "#F1F1F1",
        PopupBorder = "#3F3F46",
        PrimaryColor = "#3794FF",
        SecondaryBackground = "#444444",
        SecondaryBorder = "#555555",
        HeaderBackground = "#3A3A3A",
        ButtonActiveColor = "#007ACC",
        ButtonInactiveColor = "#999999",
        PrimaryButtonForeground = "#FFFFFF",
        OverlayBackground = "#80000000",
        FilterBarClusterBackground = "#14FFFFFF",
        PopupShadow = "0 2px 10px rgba(0,0,0,0.45)",
        ResizeHandleColor = "#666666",
        FontFamily = "sans-serif",
        FontSizePt = 13,
        PopupMaxHeight = 850,
    };

    /// <summary>Popup surface background.</summary>
    public string PopupBackground { get; init; } = "#FFFFFF";

    /// <summary>Primary text/icon color on the popup surface.</summary>
    public string PopupForeground { get; init; } = "#333333";

    /// <summary>Popup and chip borders.</summary>
    public string PopupBorder { get; init; } = "#CCCCCC";

    /// <summary>Accent color (active filter indicator, primary buttons, active header button).</summary>
    public string PrimaryColor { get; init; } = "#007BFF";

    /// <summary>Secondary surfaces (sort buttons hover, secondary buttons).</summary>
    public string SecondaryBackground { get; init; } = "#EEEEEE";

    /// <summary>Borders on inputs and secondary panels.</summary>
    public string SecondaryBorder { get; init; } = "#CCCCCC";

    /// <summary>Advanced-filter section header background.</summary>
    public string HeaderBackground { get; init; } = "#F9F9F9";

    /// <summary>Column filter button when a filter is active.</summary>
    public string ButtonActiveColor { get; init; } = "#1E90FF";

    /// <summary>Column filter button when no filter is active.</summary>
    public string ButtonInactiveColor { get; init; } = "#808080";

    /// <summary>Text on primary action buttons (OK).</summary>
    public string PrimaryButtonForeground { get; init; } = "#FFFFFF";

    /// <summary>Modal overlay behind popups (<c>#AARRGGBB</c>).</summary>
    public string OverlayBackground { get; init; } = "#80000000";

    /// <summary>Filter-bar AND-cluster background (CSS color, may include alpha).</summary>
    public string FilterBarClusterBackground { get; init; } = "#08000000";

    /// <summary>CSS <c>box-shadow</c> value for floating popups.</summary>
    public string PopupShadow { get; init; } = "0 2px 10px rgba(0,0,0,0.2)";

    /// <summary>Resize grip accent in Blazor popups.</summary>
    public string ResizeHandleColor { get; init; } = "#CCCCCC";

    /// <summary>Optional font family; <c>null</c> keeps the host default.</summary>
    public string? FontFamily { get; init; } = "sans-serif";

    /// <summary>Optional base font size in points; <c>null</c> keeps the host default.</summary>
    public double? FontSizePt { get; init; } = 13;

    /// <summary>Maximum popup height (WPF <c>MaxHeight</c>; other stacks may ignore).</summary>
    public double? PopupMaxHeight { get; init; } = 450;

    /// <summary>Returns a copy with selective overrides (immutable-friendly editing).</summary>
    public FilterTheme With(
        string? popupBackground = null,
        string? popupForeground = null,
        string? popupBorder = null,
        string? primaryColor = null,
        string? secondaryBackground = null,
        string? secondaryBorder = null,
        string? headerBackground = null,
        string? buttonActiveColor = null,
        string? buttonInactiveColor = null,
        string? primaryButtonForeground = null,
        string? overlayBackground = null,
        string? filterBarClusterBackground = null,
        string? popupShadow = null,
        string? resizeHandleColor = null,
        string? fontFamily = null,
        double? fontSizePt = null,
        double? popupMaxHeight = null) =>
        new()
        {
            PopupBackground = popupBackground ?? PopupBackground,
            PopupForeground = popupForeground ?? PopupForeground,
            PopupBorder = popupBorder ?? PopupBorder,
            PrimaryColor = primaryColor ?? PrimaryColor,
            SecondaryBackground = secondaryBackground ?? SecondaryBackground,
            SecondaryBorder = secondaryBorder ?? SecondaryBorder,
            HeaderBackground = headerBackground ?? HeaderBackground,
            ButtonActiveColor = buttonActiveColor ?? ButtonActiveColor,
            ButtonInactiveColor = buttonInactiveColor ?? ButtonInactiveColor,
            PrimaryButtonForeground = primaryButtonForeground ?? PrimaryButtonForeground,
            OverlayBackground = overlayBackground ?? OverlayBackground,
            FilterBarClusterBackground = filterBarClusterBackground ?? FilterBarClusterBackground,
            PopupShadow = popupShadow ?? PopupShadow,
            ResizeHandleColor = resizeHandleColor ?? ResizeHandleColor,
            FontFamily = fontFamily ?? FontFamily,
            FontSizePt = fontSizePt ?? FontSizePt,
            PopupMaxHeight = popupMaxHeight ?? PopupMaxHeight,
        };

    /// <summary>CSS custom properties for Blazor (keys include the <c>--</c> prefix).</summary>
    public IReadOnlyDictionary<string, string> ToCssVariables()
    {
        var map = new Dictionary<string, string>
        {
            [FilterThemeResourceKeys.CssPopupBackground] = PopupBackground,
            [FilterThemeResourceKeys.CssPopupForeground] = PopupForeground,
            [FilterThemeResourceKeys.CssPopupBorder] = PopupBorder,
            [FilterThemeResourceKeys.CssPrimaryColor] = PrimaryColor,
            [FilterThemeResourceKeys.CssSecondaryBackground] = SecondaryBackground,
            [FilterThemeResourceKeys.CssSecondaryBorder] = SecondaryBorder,
            [FilterThemeResourceKeys.CssHeaderBackground] = HeaderBackground,
            [FilterThemeResourceKeys.CssButtonActiveColor] = ButtonActiveColor,
            [FilterThemeResourceKeys.CssButtonInactiveColor] = ButtonInactiveColor,
            [FilterThemeResourceKeys.CssPrimaryButtonForeground] = PrimaryButtonForeground,
            [FilterThemeResourceKeys.CssOverlayBackground] = OverlayBackground,
            [FilterThemeResourceKeys.CssFilterBarClusterBackground] = FilterBarClusterBackground,
            [FilterThemeResourceKeys.CssPopupShadow] = PopupShadow,
            [FilterThemeResourceKeys.CssResizeHandleColor] = ResizeHandleColor,
        };

        if (!string.IsNullOrWhiteSpace(FontFamily))
            map[FilterThemeResourceKeys.CssFontFamily] = FontFamily;
        if (FontSizePt is double size)
            map[FilterThemeResourceKeys.CssFontSize] = $"{size.ToString(System.Globalization.CultureInfo.InvariantCulture)}px";

        return map;
    }

    /// <summary>Inline CSS <c>style</c> attribute fragment (<c>--df-popup-bg: …; …</c>).</summary>
    public string ToCssVariableStyle() =>
        string.Join(' ', ToCssVariables().Select(kv => $"{kv.Key}: {kv.Value};"));
}
