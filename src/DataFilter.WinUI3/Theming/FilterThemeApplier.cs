using DataFilter.PlatformShared.Theming;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace DataFilter.WinUI3.Theming;

/// <summary>Applies <see cref="FilterTheme"/> to WinUI 3 filter controls.</summary>
public static class FilterThemeApplier
{
    public static Color ToWinUIColor(string hex)
    {
        var c = FilterColor.Parse(hex);
        return Color.FromArgb(c.A, c.R, c.G, c.B);
    }

    public static SolidColorBrush ToBrush(string hex) => new(ToWinUIColor(hex));

    public static void ApplyToPopup(Controls.FilterPopupControl popup, FilterTheme? theme = null)
    {
        theme ??= FilterTheme.Current;
        popup.Background = ToBrush(theme.PopupBackground);
        popup.BorderBrush = ToBrush(theme.PopupBorder);
    }
}
