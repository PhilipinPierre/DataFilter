using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using DataFilter.Wpf.Controls;

namespace DataFilter.Wpf.PopupHost.Tests;

public sealed class DataGridHeaderRowBackgroundFixTests
{
    [Fact]
    public void Header_row_filler_and_presenter_match_column_header_background()
        => RunSta(() =>
        {
            var expected = Brushes.DarkGray;
            var headerStyle = new Style(typeof(DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(Control.BackgroundProperty, expected));

            var grid = new FilterableDataGrid
            {
                Width = 600,
                Height = 200,
                ColumnHeaderStyle = headerStyle,
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Name", Width = 120 });
            grid.ItemsSource = new[] { "Alice", "Bob" };

            var window = new Window
            {
                Content = grid,
                Width = 650,
                Height = 250,
            };

            window.Show();
            PumpDispatcher(window.Dispatcher);

            var presenter = FindColumnHeadersPresenter(grid);
            Assert.NotNull(presenter);

            var referenceHeader = GetColumnHeaders(presenter!).FirstOrDefault(header => header.Column != null);
            Assert.NotNull(referenceHeader);
            Assert.Same(expected, referenceHeader!.Background);

            var fillerHeader = GetColumnHeaders(presenter!).FirstOrDefault(header => header.Column == null);
            Assert.NotNull(fillerHeader);
            Assert.Same(expected, fillerHeader!.Background);
            Assert.Same(expected, presenter!.Background);

            window.Close();
        });

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

    private static void PumpDispatcher(Dispatcher dispatcher)
    {
        for (var i = 0; i < 20; i++)
            dispatcher.Invoke(() => { }, DispatcherPriority.Background);

        dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static void RunSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (captured != null)
            throw captured;
    }
}
