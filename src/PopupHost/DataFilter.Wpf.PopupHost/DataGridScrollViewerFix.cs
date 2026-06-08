using DataFilter.Wpf.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Fixes WPF DataGrid horizontal-scroll binding errors where CellsPanelHorizontalOffset becomes negative
/// and is assigned to the internal SelectAll button Width.
/// </summary>
internal static class DataGridScrollViewerFix
{
    private static readonly NonNegativeDoubleConverter Converter = NonNegativeDoubleConverter.Instance;

    public static void Apply(DataGrid grid)
    {
        void SchedulePatch()
        {
            grid.Dispatcher.BeginInvoke(
                () => PatchSelectAllButton(grid),
                DispatcherPriority.Loaded);
        }

        if (grid.IsLoaded)
            SchedulePatch();
        else
            grid.Loaded += OnGridLoaded;

        void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dg)
            {
                dg.Loaded -= OnGridLoaded;
                SchedulePatch();
            }
        }
    }

    private static void PatchSelectAllButton(DataGrid grid, int attempt = 0)
    {
        if (attempt > 8)
            return;

        var button = FindSelectAllButton(grid);
        if (button == null)
        {
            grid.Dispatcher.BeginInvoke(
                () => PatchSelectAllButton(grid, attempt + 1),
                DispatcherPriority.Loaded);
            return;
        }

        BindingOperations.SetBinding(
            button,
            FrameworkElement.WidthProperty,
            new Binding(nameof(DataGrid.CellsPanelHorizontalOffset))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1),
                Converter = Converter,
                Mode = BindingMode.OneWay,
            });
    }

    private static Button? FindSelectAllButton(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Button { Command: not null } button
                && ReferenceEquals(button.Command, DataGrid.SelectAllCommand))
            {
                return button;
            }

            var found = FindSelectAllButton(child);
            if (found != null)
                return found;
        }

        return null;
    }
}
