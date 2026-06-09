using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.WinUI3.Attach;

/// <summary>
/// Attachable adapter that injects a filterable header into an existing <see cref="ListView"/>.
/// </summary>
public sealed class ListViewFilterHeaderAdapter : IDisposable
{
    public sealed record Column(
        string Title,
        string PropertyName,
        double Width = 150,
        bool IsFilterable = true,
        ColumnFilterTriggerMode TriggerMode = ColumnFilterTriggerMode.Inherit);

    private readonly ListView _listView;
    private readonly object? _previousHeader;
    private bool _isDisposed;

    public ListViewFilterHeaderAdapter(
        ListView listView,
        IFilterableDataGridViewModel viewModel,
        IReadOnlyList<Column> columns,
        bool areColumnFiltersEnabled = true,
        ColumnFilterTriggerMode columnFilterTriggerMode = ColumnFilterTriggerMode.FilterButton)
    {
        _listView = listView ?? throw new ArgumentNullException(nameof(listView));
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        AreColumnFiltersEnabled = areColumnFiltersEnabled;
        ColumnFilterTriggerMode = columnFilterTriggerMode;

        _previousHeader = _listView.Header;
        _listView.Header = BuildHeader();
    }

    public static ListViewFilterHeaderAdapter Attach(
        ListView listView,
        IFilterableDataGridViewModel viewModel,
        params Column[] columns)
        => new(listView, viewModel, columns);

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

    private UIElement BuildHeader()
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
