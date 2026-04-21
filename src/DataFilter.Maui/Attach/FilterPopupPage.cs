using DataFilter.Maui.Controls;
using Microsoft.Maui.Controls;

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

        var frame = new Frame
        {
            Padding = 0,
            HasShadow = true,
            CornerRadius = 8,
            Content = popup,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        Content = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            Children = { frame }
        };
    }
}

