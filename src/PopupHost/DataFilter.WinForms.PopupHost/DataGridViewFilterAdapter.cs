using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.WinForms.Attach;
using System.ComponentModel;
using System.Windows.Forms;

namespace DataFilter.WinForms.Attach;

/// <summary>
/// Attachable adapter enabling DataFilter column filtering on an existing <see cref="DataGridView"/>.
/// </summary>
public sealed class DataGridViewFilterAdapter : IDisposable
{
    private readonly DataGridViewFilterHeaderEventHandler _headerEvents;
    private bool _isDisposed;

    public DataGridViewFilterAdapter(DataGridView grid, IFilterableDataGridViewModel viewModel)
    {
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _headerEvents = new DataGridViewFilterHeaderEventHandler(grid, () => ViewModel, () => InteractionSettings);
    }

    public static DataGridViewFilterAdapter Attach(DataGridView grid, IFilterableDataGridViewModel viewModel)
        => new(grid, viewModel);

    public DataGridView Grid { get; }

    [Browsable(false)]
    public IFilterableDataGridViewModel ViewModel { get; }

    public bool AreColumnFiltersEnabled { get; set; } = true;

    public ColumnFilterTriggerMode ColumnFilterTriggerMode { get; set; } = ColumnFilterTriggerMode.FilterButton;

    public bool AppendHeaderGlyph { get; set; } = true;

    public int HeaderButtonHitWidth { get; set; } = 20;

    private DataGridViewFilterHeaderInteractions.Settings InteractionSettings =>
        new()
        {
            AreColumnFiltersEnabled = AreColumnFiltersEnabled,
            ColumnFilterTriggerMode = ColumnFilterTriggerMode,
            AppendHeaderGlyph = AppendHeaderGlyph,
            HeaderButtonHitWidth = HeaderButtonHitWidth,
        };

    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        _headerEvents.Dispose();
    }
}
