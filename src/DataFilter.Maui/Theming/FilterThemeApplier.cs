using DataFilter.PlatformShared.Theming;

namespace DataFilter.Maui.Theming;

/// <summary>Applies <see cref="FilterTheme"/> to MAUI filter controls.</summary>
public static class FilterThemeApplier
{
    public static Color ToMauiColor(string hex)
    {
        var c = FilterColor.Parse(hex);
        return Color.FromRgba(c.R, c.G, c.B, c.A);
    }

    public static void ApplyToPopup(Controls.FilterPopupView popup, FilterTheme? theme = null)
    {
        theme ??= FilterTheme.Current;
        popup.BackgroundColor = ToMauiColor(theme.PopupBackground);
    }
}
