using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Attach;

internal static class MauiHeaderPointerHelpers
{
    internal static bool IsShiftKeyDown()
    {
#if WINDOWS
        return Microsoft.UI.Input.InputKeyboardSource
            .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
#else
        return false;
#endif
    }

    internal static bool IsControlKeyDown()
    {
#if WINDOWS
        return Microsoft.UI.Input.InputKeyboardSource
            .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
#else
        return false;
#endif
    }

    /// <summary>
    /// MAUI only exposes Primary/Secondary in <see cref="ButtonsMask"/>; on Windows we hook the native view.
    /// </summary>
    internal static void AttachMiddleClick(View target, Func<Task> onMiddleClick)
    {
#if WINDOWS
        void WireHandler()
        {
            if (target.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement fe)
                return;

            fe.PointerPressed += async (_, e) =>
            {
                if (!e.GetCurrentPoint(fe).Properties.IsMiddleButtonPressed)
                    return;

                e.Handled = true;
                await onMiddleClick();
            };
        }

        if (target.Handler != null)
            WireHandler();
        else
            target.HandlerChanged += (_, _) => WireHandler();
#else
        _ = target;
        _ = onMiddleClick;
#endif
    }

    internal static void AttachKeyboardShortcut(View target, Func<Task> onShortcut)
    {
#if WINDOWS
        void WireHandler()
        {
            if (target.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement fe)
                return;

            fe.IsTabStop = true;
            fe.KeyDown += async (_, e) =>
            {
                if (e.Key != Windows.System.VirtualKey.Down)
                    return;

                if (!Microsoft.UI.Input.InputKeyboardSource
                        .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu)
                        .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                    return;

                e.Handled = true;
                await onShortcut();
            };
        }

        if (target.Handler != null)
            WireHandler();
        else
            target.HandlerChanged += (_, _) => WireHandler();
#else
        target.Focus();
        _ = onShortcut;
#endif
    }
}
