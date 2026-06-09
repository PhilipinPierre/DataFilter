using DataFilter.PlatformShared.Theming;
using System.Drawing;

namespace DataFilter.WinForms.Theming;

/// <summary>Color helpers for applying <see cref="FilterTheme"/> in WinForms.</summary>
public static class FilterThemeApplier
{
    public static Color ToDrawingColor(string hex) => ToDrawingColor(FilterColor.Parse(hex));

    public static Color ToDrawingColor(FilterColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);
}
