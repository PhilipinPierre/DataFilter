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
    private readonly Rect? _anchorRect;
    private readonly FlowDirection _anchorFlowDirection;

    public FilterPopupPage(FilterPopupView popup, Rect? anchorRect = null, FlowDirection anchorFlowDirection = FlowDirection.LeftToRight)
    {
        ArgumentNullException.ThrowIfNull(popup);

        Padding = 10;
        BackgroundColor = Colors.Transparent;

        _anchorRect = anchorRect;
        _anchorFlowDirection = anchorFlowDirection;

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
        _border.HorizontalOptions = _anchorRect != null ? LayoutOptions.Start : LayoutOptions.Center;
        _border.VerticalOptions = _anchorRect != null ? LayoutOptions.Start : LayoutOptions.Center;

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
        if (_anchorRect == null) return;
        if (Width <= 0 || Height <= 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var anchor = _anchorRect.Value;
            bool isRtl = _anchorFlowDirection == FlowDirection.RightToLeft;

            var popupWidth = _border.Width > 0 ? _border.Width : (_border.WidthRequest > 0 ? _border.WidthRequest : 280);
            var popupHeight = _border.Height > 0 ? _border.Height : (_border.HeightRequest > 0 ? _border.HeightRequest : 350);

            // Default anchor rule:
            // - LTR: popup top-left at button bottom-right
            // - RTL: popup top-right at button bottom-left
            var desiredX = isRtl ? (anchor.X - popupWidth) : (anchor.X + anchor.Width);
            var desiredY = anchor.Y + anchor.Height + 4;

            // Keep as visible as possible within the page.
            var minX = 8;
            var minY = 8;
            var maxX = Math.Max(minX, Width - popupWidth - 8);
            var maxY = Math.Max(minY, Height - popupHeight - 8);

            _border.TranslationX = Clamp(desiredX, minX, maxX);
            _border.TranslationY = Clamp(desiredY, minY, maxY);
        });
    }

    private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);
}

