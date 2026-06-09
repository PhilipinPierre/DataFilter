using DataFilter.PlatformShared.ColumnFilter;
using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Blazor.Components;

/// <summary>
/// Headless integration surface for <c>DataFilterGrid</c> so apps can keep their own table markup.
/// </summary>
public sealed class DataFilterGridContext<TItem>
{
    internal DataFilterGridContext(
        Func<string, IColumnFilterViewModel> getColumnFilterViewModel,
        Func<IReadOnlyList<TItem>> getFilteredItems,
        Func<bool> getIsLoading,
        bool areColumnFiltersEnabled,
        ColumnFilterTriggerMode columnFilterTriggerMode,
        Func<DataFilterGrid<TItem>.ColumnDefinition<TItem>, bool> isColumnFilterEnabled,
        Func<DataFilterGrid<TItem>.ColumnDefinition<TItem>, ColumnFilterTriggerMode> getEffectiveTriggerMode)
    {
        GetColumnFilterViewModel = getColumnFilterViewModel ?? throw new ArgumentNullException(nameof(getColumnFilterViewModel));
        _getFilteredItems = getFilteredItems ?? throw new ArgumentNullException(nameof(getFilteredItems));
        _getIsLoading = getIsLoading ?? throw new ArgumentNullException(nameof(getIsLoading));
        AreColumnFiltersEnabled = areColumnFiltersEnabled;
        ColumnFilterTriggerMode = columnFilterTriggerMode;
        _isColumnFilterEnabled = isColumnFilterEnabled ?? throw new ArgumentNullException(nameof(isColumnFilterEnabled));
        _getEffectiveTriggerMode = getEffectiveTriggerMode ?? throw new ArgumentNullException(nameof(getEffectiveTriggerMode));
    }

    private readonly Func<IReadOnlyList<TItem>> _getFilteredItems;
    private readonly Func<bool> _getIsLoading;
    private readonly Func<DataFilterGrid<TItem>.ColumnDefinition<TItem>, bool> _isColumnFilterEnabled;
    private readonly Func<DataFilterGrid<TItem>.ColumnDefinition<TItem>, ColumnFilterTriggerMode> _getEffectiveTriggerMode;

    /// <summary>
    /// Gets (or creates) the filter popup view model for a column by its <c>ColumnDefinition.Id</c>.
    /// </summary>
    public Func<string, IColumnFilterViewModel> GetColumnFilterViewModel { get; }

    public bool AreColumnFiltersEnabled { get; }

    public ColumnFilterTriggerMode ColumnFilterTriggerMode { get; }

    public IReadOnlyList<TItem> FilteredItems => _getFilteredItems();

    public bool IsLoading => _getIsLoading();

    public bool IsColumnFilterEnabled(DataFilterGrid<TItem>.ColumnDefinition<TItem> column) =>
        _isColumnFilterEnabled(column);

    public ColumnFilterTriggerMode GetEffectiveTriggerMode(DataFilterGrid<TItem>.ColumnDefinition<TItem> column) =>
        _getEffectiveTriggerMode(column);
}
