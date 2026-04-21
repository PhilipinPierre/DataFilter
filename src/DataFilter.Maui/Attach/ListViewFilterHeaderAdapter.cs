using DataFilter.Maui.Behaviors;
using DataFilter.PlatformShared.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Attach;

/// <summary>
/// Attachable adapter that injects a filterable header into an existing <see cref="ListView"/>.
/// </summary>
public sealed class ListViewFilterHeaderAdapter : IDisposable
{
    public sealed record Column(string Title, string PropertyName, GridLength Width);

    private readonly Page _hostPage;
    private readonly ListView _listView;
    private readonly IFilterableDataGridViewModel _viewModel;
    private readonly object? _previousHeader;
    private bool _isDisposed;

    public ListViewFilterHeaderAdapter(Page hostPage, ListView listView, IFilterableDataGridViewModel viewModel, IReadOnlyList<Column> columns)
    {
        _hostPage = hostPage ?? throw new ArgumentNullException(nameof(hostPage));
        _listView = listView ?? throw new ArgumentNullException(nameof(listView));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));

        _previousHeader = _listView.Header;
        _listView.Header = BuildHeader();
    }

    public static ListViewFilterHeaderAdapter Attach(Page hostPage, ListView listView, IFilterableDataGridViewModel viewModel, params Column[] columns)
        => new(hostPage, listView, viewModel, columns);

    public IReadOnlyList<Column> Columns { get; }

    private View BuildHeader()
    {
        var grid = new Grid { Padding = new Thickness(10, 0, 10, 10), ColumnSpacing = 6 };
        foreach (var c in Columns)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = c.Width });
        }

        for (int i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];
            var btn = new Button
            {
                Text = $"{col.Title} \uD83D\uDD0D",
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(6, 2),
                HorizontalOptions = LayoutOptions.Start
            };
            Grid.SetColumn(btn, i);

            btn.Clicked += async (_, _) =>
            {
                var popupView = FilterHeaderBehavior.CreatePopup(_viewModel, col.PropertyName);
                var anchorPos = GetAbsolutePosition(btn);
                var page = new FilterPopupPage(popupView, anchorPos, btn.Height);
                popupView.CloseRequested += async (_, __) =>
                {
                    if (page.Navigation.ModalStack.Contains(page))
                        await page.Navigation.PopModalAsync();
                };
                page.DismissRequested += async (_, __) =>
                {
                    if (page.Navigation.ModalStack.Contains(page))
                        await page.Navigation.PopModalAsync();
                };

                await _hostPage.Navigation.PushModalAsync(page);
            };

            grid.Children.Add(btn);
        }

        return grid;
    }

    private static Point GetAbsolutePosition(VisualElement element)
    {
        double x = element.X;
        double y = element.Y;

        var parent = element.Parent as VisualElement;
        while (parent != null)
        {
            x += parent.X;
            y += parent.Y;
            parent = parent.Parent as VisualElement;
        }

        return new Point(x, y);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _listView.Header = _previousHeader;
    }
}

