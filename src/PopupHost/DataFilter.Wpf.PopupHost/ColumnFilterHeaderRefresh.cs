using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DataFilter.Wpf.Behaviors;

internal static class ColumnFilterHeaderRefresh
{
    internal static void OnGridHeaderSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == e.OldValue)
            return;

        // Inherited attached properties propagate to descendants; only grid hosts own header chrome.
        if (d is not (DataGrid or ListView))
            return;

        RefreshHeaders(d);
    }

    internal static void RefreshHeaders(DependencyObject gridHost)
    {
        if (gridHost is FrameworkElement { IsLoaded: false } element)
        {
            element.Loaded -= OnGridHostLoaded;
            element.Loaded += OnGridHostLoaded;
            return;
        }

        RefreshHeadersInVisualTree(gridHost);
    }

    private static void OnGridHostLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        element.Loaded -= OnGridHostLoaded;
        RefreshHeadersInVisualTree(element);
    }

    private static void RefreshHeadersInVisualTree(DependencyObject root)
    {
        for (int i = 0, count = VisualTreeHelper.GetChildrenCount(root); i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is DataGridColumnHeader or GridViewColumnHeader)
                FilterableColumnHeaderBehavior.RefreshHeaderChrome((FrameworkElement)child);

            RefreshHeadersInVisualTree(child);
        }
    }
}
