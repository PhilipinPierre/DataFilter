using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DataFilter.Wpf.Behaviors;

internal static class FilterColumnPopupTracker
{
    private sealed class WindowState
    {
        public HashSet<FilterableColumnHeaderBehavior> OpenBehaviors { get; } = new();
        public bool IsHandlerAttached { get; set; }
    }

    private static readonly ConditionalWeakTable<Window, WindowState> WindowStates = new();

    internal static void OnPopupOpened(FilterableColumnHeaderBehavior behavior)
    {
        var window = Window.GetWindow(behavior.AssociatedHeader);
        if (window == null)
            return;

        var state = WindowStates.GetOrCreateValue(window);
        state.OpenBehaviors.Add(behavior);

        if (state.IsHandlerAttached)
            return;

        window.PreviewMouseLeftButtonDown += OnWindowPreviewMouseLeftButtonDown;
        state.IsHandlerAttached = true;
    }

    internal static void OnPopupClosed(FilterableColumnHeaderBehavior behavior)
    {
        var window = Window.GetWindow(behavior.AssociatedHeader);
        if (window == null)
            return;

        if (!WindowStates.TryGetValue(window, out var state))
            return;

        state.OpenBehaviors.Remove(behavior);
        if (state.OpenBehaviors.Count > 0)
            return;

        window.PreviewMouseLeftButtonDown -= OnWindowPreviewMouseLeftButtonDown;
        state.IsHandlerAttached = false;
        WindowStates.Remove(window);
    }

    private static void OnWindowPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Window window || !WindowStates.TryGetValue(window, out var state))
            return;

        foreach (var behavior in state.OpenBehaviors.ToArray())
        {
            if (!IsMouseOverOpenPopup(behavior, e))
                behavior.CloseFilterPopup();
        }
    }

    private static bool IsMouseOverOpenPopup(FilterableColumnHeaderBehavior behavior, MouseButtonEventArgs e)
    {
        if (behavior.TryGetOpenPopupChild() is not FrameworkElement child)
            return false;

        // Before first layout, RenderSize can be (0,0). Rect with zero area contains no points,
        // so every position looks "outside" and we would close the popup immediately after open.
        double w = child.RenderSize.Width;
        double h = child.RenderSize.Height;
        if (w <= 0) w = child.ActualWidth;
        if (h <= 0) h = child.ActualHeight;

        if (w <= 0 || h <= 0)
            return true;

        return new Rect(0, 0, w, h).Contains(e.GetPosition(child));
    }
}
