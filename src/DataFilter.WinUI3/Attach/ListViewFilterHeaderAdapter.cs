using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinUI3.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace DataFilter.WinUI3.Attach;

/// <summary>
/// Attachable adapter that injects a filterable header into an existing <see cref="ListView"/>.
/// </summary>
public sealed class ListViewFilterHeaderAdapter : IDisposable
{
    public sealed record Column(string Title, string PropertyName, double Width);

    private readonly ListView _listView;
    private readonly IFilterableDataGridViewModel _viewModel;
    private readonly object? _previousHeader;
    private bool _isDisposed;

    public ListViewFilterHeaderAdapter(ListView listView, IFilterableDataGridViewModel viewModel, IReadOnlyList<Column> columns)
    {
        _listView = listView ?? throw new ArgumentNullException(nameof(listView));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));

        _previousHeader = _listView.Header;
        _listView.Header = BuildHeader();
    }

    public static ListViewFilterHeaderAdapter Attach(ListView listView, IFilterableDataGridViewModel viewModel, params Column[] columns)
        => new(listView, viewModel, columns);

    public IReadOnlyList<Column> Columns { get; }

    private UIElement BuildHeader()
    {
        var headerScroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var grid = new Grid { Padding = new Thickness(10, 0, 10, 10) };
        foreach (var c in Columns)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(c.Width) });
        }

        for (int i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];
            var btn = new Button
            {
                Content = $"{col.Title} \uD83D\uDD0D",
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                Padding = new Thickness(5)
            };
            Grid.SetColumn(btn, i);

            btn.Click += (_, _) =>
            {
                var popup = FilterHeaderBehavior.CreatePopup(_viewModel, col.PropertyName);
                var flyout = new Flyout { Content = popup };
                if (popup.ViewModel != null)
                {
                    popup.ViewModel.OnApply += (_, __) => flyout.Hide();
                    popup.ViewModel.OnClear += (_, __) => flyout.Hide();
                }

                bool isRtl = btn.FlowDirection == FlowDirection.RightToLeft;
                var desired = new Point(isRtl ? -popup.Width : btn.ActualWidth, btn.ActualHeight);
                flyout.ShowAt(btn, new FlyoutShowOptions
                {
                    Placement = FlyoutPlacementMode.Bottom,
                    Position = desired
                });
            };

            grid.Children.Add(btn);
        }

        headerScroll.Content = grid;
        return headerScroll;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _listView.Header = _previousHeader;
    }
}

