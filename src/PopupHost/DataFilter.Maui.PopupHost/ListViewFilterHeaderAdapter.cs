using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Attach;

/// <summary>
/// Attachable adapter that injects a filterable header into an existing <see cref="ListView"/>.
/// </summary>
public sealed class ListViewFilterHeaderAdapter : IDisposable
{
    public sealed record Column(
        string Title,
        string PropertyName,
        GridLength Width,
        bool IsFilterable = true,
        ColumnFilterTriggerMode TriggerMode = ColumnFilterTriggerMode.Inherit);

    private readonly Page _hostPage;
    private readonly ListView _listView;
    private readonly object? _previousHeader;
    private bool _isDisposed;

    public ListViewFilterHeaderAdapter(
        Page hostPage,
        ListView listView,
        IFilterableDataGridViewModel viewModel,
        IReadOnlyList<Column> columns,
        bool areColumnFiltersEnabled = true,
        ColumnFilterTriggerMode columnFilterTriggerMode = ColumnFilterTriggerMode.FilterButton)
    {
        _hostPage = hostPage ?? throw new ArgumentNullException(nameof(hostPage));
        _listView = listView ?? throw new ArgumentNullException(nameof(listView));
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        AreColumnFiltersEnabled = areColumnFiltersEnabled;
        ColumnFilterTriggerMode = columnFilterTriggerMode;

        _previousHeader = _listView.Header;
        _listView.Header = BuildHeader();
    }

    public static ListViewFilterHeaderAdapter Attach(
        Page hostPage,
        ListView listView,
        IFilterableDataGridViewModel viewModel,
        params Column[] columns)
        => new(hostPage, listView, viewModel, columns);

    public IFilterableDataGridViewModel ViewModel { get; }

    public IReadOnlyList<Column> Columns { get; }

    public bool AreColumnFiltersEnabled { get; private set; }

    public ColumnFilterTriggerMode ColumnFilterTriggerMode { get; private set; }

    public void ApplyHeaderSettings(bool areColumnFiltersEnabled, ColumnFilterTriggerMode columnFilterTriggerMode)
    {
        AreColumnFiltersEnabled = areColumnFiltersEnabled;
        ColumnFilterTriggerMode = columnFilterTriggerMode;
        _listView.Header = BuildHeader();
    }

    private View BuildHeader()
    {
        var specs = Columns.Select(c => new ListViewFilterHeaderInteractions.ColumnSpec
        {
            Title = c.Title,
            PropertyName = c.PropertyName,
            Width = c.Width,
            IsFilterable = c.IsFilterable,
            TriggerMode = c.TriggerMode,
        }).ToList();

        return ListViewFilterHeaderInteractions.BuildHeader(
            _hostPage,
            ViewModel,
            specs,
            new ListViewFilterHeaderInteractions.Settings
            {
                AreColumnFiltersEnabled = AreColumnFiltersEnabled,
                ColumnFilterTriggerMode = ColumnFilterTriggerMode,
            });
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        _listView.Header = _previousHeader;
    }
}
