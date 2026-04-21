using DataFilter.Maui.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace DataFilter.Maui.Attach;

/// <summary>
/// Simple modal host for <see cref="FilterPopupView"/> for apps that don't use a dedicated popup toolkit.
/// </summary>
public sealed class FilterPopupPage : ContentPage
{
    public event EventHandler? DismissRequested;

    private readonly Border _border;
    private readonly Point? _anchorTopLeft;
    private readonly double _anchorHeight;

    public FilterPopupPage(FilterPopupView popup, Point? anchorTopLeft = null, double anchorHeight = 0)
    {
        ArgumentNullException.ThrowIfNull(popup);

        Padding = 10;
        BackgroundColor = Colors.Transparent;

        _anchorTopLeft = anchorTopLeft;
        _anchorHeight = anchorHeight;

        // Capture "click-away" / tap outside to dismiss.
        // Using a dedicated overlay element ensures taps on the popup content don't trigger dismissal.
        var overlay = new BoxView
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            InputTransparent = false
        };
        overlay.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => DismissRequested?.Invoke(this, EventArgs.Empty))
        });

        _border = new Border
        {
            Padding = 0,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = popup
        };

        _border.Shadow = new Shadow
        {
            Opacity = 0.35f,
            Radius = 16,
            Offset = new Point(0, 4)
        };

        // Default to centered (legacy behavior) if no anchor is provided.
        _border.HorizontalOptions = _anchorTopLeft != null ? LayoutOptions.Start : LayoutOptions.Center;
        _border.VerticalOptions = _anchorTopLeft != null ? LayoutOptions.Start : LayoutOptions.Center;

        Content = new Grid
        {
            Children = { overlay, _border }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TryPositionNearAnchor();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        TryPositionNearAnchor();
    }

    private void TryPositionNearAnchor()
    {
        if (_anchorTopLeft == null) return;
        if (Width <= 0 || Height <= 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var anchor = _anchorTopLeft.Value;

            var desiredX = anchor.X;
            var desiredY = anchor.Y + _anchorHeight + 4;

            var popupWidth = _border.Width > 0 ? _border.Width : (_border.WidthRequest > 0 ? _border.WidthRequest : 280);
            var popupHeight = _border.Height > 0 ? _border.Height : (_border.HeightRequest > 0 ? _border.HeightRequest : 350);

            var maxX = Math.Max(0, Width - popupWidth - 8);
            var maxY = Math.Max(0, Height - popupHeight - 8);

            _border.TranslationX = Clamp(desiredX, 8, maxX);
            _border.TranslationY = Clamp(desiredY, 8, maxY);
        });
    }

    private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);
}

