using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Keeps the DataGrid header row background consistent with styled column headers.
/// WPF leaves the header filler area (after the last column) on the default light chrome,
/// which shows through with Material Design and other dark column header styles.
/// </summary>
internal static class DataGridHeaderRowBackgroundFix
{
    private static readonly DependencyProperty IsPatchedProperty =
        DependencyProperty.RegisterAttached(
            "IsHeaderRowBackgroundPatched",
            typeof(bool),
            typeof(DataGridHeaderRowBackgroundFix),
            new PropertyMetadata(false));

    public static void Apply(DataGrid grid)
    {
        if (grid.GetValue(IsPatchedProperty) is true)
            return;

        grid.SetValue(IsPatchedProperty, true);

        void SchedulePatch()
        {
            grid.Dispatcher.BeginInvoke(
                () => TryPatch(grid),
                DispatcherPriority.Loaded);
        }

        if (grid.IsLoaded)
            SchedulePatch();
        else
            grid.Loaded += OnGridLoaded;

        grid.SizeChanged += (_, _) => SchedulePatch();

        void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dg)
            {
                dg.Loaded -= OnGridLoaded;
                SchedulePatch();
            }
        }
    }

    private static void TryPatch(DataGrid grid, int attempt = 0)
    {
        if (attempt > 8)
            return;

        var presenter = FindColumnHeadersPresenter(grid);
        var referenceHeader = FindReferenceColumnHeader(presenter);
        if (presenter == null || referenceHeader == null)
        {
            grid.Dispatcher.BeginInvoke(
                () => TryPatch(grid, attempt + 1),
                DispatcherPriority.Loaded);
            return;
        }

        BindBackground(presenter, referenceHeader);

        foreach (var header in GetColumnHeaders(presenter))
        {
            if (header.Column == null)
                BindBackground(header, referenceHeader);
        }
    }

    private static void BindBackground(FrameworkElement target, DataGridColumnHeader referenceHeader)
    {
        if (BindingOperations.IsDataBound(target, Control.BackgroundProperty))
            return;

        BindingOperations.SetBinding(
            target,
            Control.BackgroundProperty,
            new Binding(nameof(Control.Background))
            {
                Source = referenceHeader,
                Mode = BindingMode.OneWay,
            });
    }

    private static DataGridColumnHeader? FindReferenceColumnHeader(DataGridColumnHeadersPresenter? presenter)
    {
        if (presenter == null)
            return null;

        return GetColumnHeaders(presenter).FirstOrDefault(header => header.Column != null);
    }

    private static DataGridColumnHeadersPresenter? FindColumnHeadersPresenter(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is DataGridColumnHeadersPresenter presenter)
                return presenter;

            var found = FindColumnHeadersPresenter(child);
            if (found != null)
                return found;
        }

        return null;
    }

    private static IEnumerable<DataGridColumnHeader> GetColumnHeaders(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is DataGridColumnHeader header)
                yield return header;

            foreach (var nested in GetColumnHeaders(child))
                yield return nested;
        }
    }
}
