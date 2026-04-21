using DataFilter.Maui.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace DataFilter.Maui.Attach;

/// <summary>
/// Simple modal host for <see cref="FilterPopupView"/> for apps that don't use a dedicated popup toolkit.
/// </summary>
public sealed class FilterPopupPage : ContentPage
{
    public FilterPopupPage(FilterPopupView popup)
    {
        ArgumentNullException.ThrowIfNull(popup);

        Padding = 10;
        BackgroundColor = Colors.Transparent;

        var border = new Border
        {
            Padding = 0,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = popup
        };

        border.Shadow = new Shadow
        {
            Opacity = 0.35f,
            Radius = 16,
            Offset = new Point(0, 4)
        };

        border.HorizontalOptions = LayoutOptions.Center;
        border.VerticalOptions = LayoutOptions.Center;

        Content = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            Children = { border }
        };
    }
}

