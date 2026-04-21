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
        Func<bool> getIsLoading)
    {
        GetColumnFilterViewModel = getColumnFilterViewModel ?? throw new ArgumentNullException(nameof(getColumnFilterViewModel));
        _getFilteredItems = getFilteredItems ?? throw new ArgumentNullException(nameof(getFilteredItems));
        _getIsLoading = getIsLoading ?? throw new ArgumentNullException(nameof(getIsLoading));
    }

    private readonly Func<IReadOnlyList<TItem>> _getFilteredItems;
    private readonly Func<bool> _getIsLoading;

    /// <summary>
    /// Gets (or creates) the filter popup view model for a column by its <c>ColumnDefinition.Id</c>.
    /// </summary>
    public Func<string, IColumnFilterViewModel> GetColumnFilterViewModel { get; }

    public IReadOnlyList<TItem> FilteredItems => _getFilteredItems();

    public bool IsLoading => _getIsLoading();
}

