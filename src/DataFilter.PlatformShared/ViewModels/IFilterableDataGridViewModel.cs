using System.Collections;
using System.ComponentModel;
using System.Globalization;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.Pipeline;

namespace DataFilter.PlatformShared.ViewModels;

public interface IFilterableDataGridViewModel : INotifyPropertyChanged
{
    IEnumerable FilteredItems { get; }
    /// <summary>
    /// Optional override culture used by UI integrations to localize filter popups at runtime.
    /// When null, integrations should use <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    CultureInfo? CultureOverride { get; }
    /// <summary>CLR type of items in <see cref="LocalDataSource"/> (homogeneous collection).</summary>
    Type ItemType { get; set; }
    IFilterContext Context { get; }
    IExcelFilterEngine FilterEngine { get; }
    IAsyncDataProvider? AsyncDataProvider { get; set; }
    IEnumerable LocalDataSource { get; set; }
    Task RefreshDataAsync();
    void ApplyColumnFilter(string propertyName, ExcelFilterState state);
    void ClearColumnFilter(string propertyName);
    void ApplySort(string propertyName, bool isDescending);
    void AddSubSort(string propertyName, bool isDescending);
    Task<IEnumerable<object>> GetDistinctValuesAsync(string propertyName, string searchText);
    ExcelFilterState? GetColumnFilterState(string propertyName);
    Type? GetPropertyType(string propertyName);
    IFilterSnapshot ExtractSnapshot();
    void RestoreSnapshot(IFilterSnapshot snapshot);
    /// <summary>
    /// Replaces context filters with the compiled pipeline and refreshes data (page reset to 1).
    /// </summary>
    Task ApplyFilterPipelineAsync(FilterPipeline pipeline);
    /// <summary>
    /// Builds a mutable <see cref="FilterPipeline"/> from the current legacy snapshot (for UI editing / presets).
    /// </summary>
    FilterPipeline CreatePipelineFromCurrentSnapshot();
    /// <summary>
    /// Builds a JSON-friendly snapshot of the active pipeline and multi-column sort order.
    /// </summary>
    FilterPipelineSnapshot CreateFilterPipelineSnapshot();
    /// <summary>
    /// Applies a pipeline preset and restores <see cref="FilterPipelineSnapshot.SortEntries"/> when present.
    /// </summary>
    Task ApplyFilterPipelineSnapshotAsync(FilterPipelineSnapshot snapshot);
    /// <summary>
    /// Applies the live <see cref="PipelineSession"/> pipeline and sort entries (no JSON).
    /// </summary>
    Task ApplyPipelineSessionAsync();
    /// <summary>
    /// Live pipeline used by the filter bar (stable node ids).
    /// </summary>
    FilterPipelineSession PipelineSession { get; }
    /// <summary>
    /// Active-filters bar view model.
    /// </summary>
    FilterBarViewModel FilterBar { get; }
    /// <summary>
    /// Applies popup state to a single pipeline criterion (filter bar edit).
    /// </summary>
    Task ApplyBarCriterionAsync(string nodeId, string propertyName, ExcelFilterState state);
    /// <summary>
    /// Removes a pipeline node and refreshes data (filter bar).
    /// </summary>
    Task RemoveBarNodeAsync(string nodeId);
    HashSet<string> FilterableProperties { get; }

    /// <summary>
    /// Raised when filters in <see cref="Context"/> change from outside the column popup (pipeline JSON, snapshot restore, clear column).
    /// Column headers use this to resync <see cref="ExcelFilterState"/> and the active-filter indicator.
    /// </summary>
    event EventHandler<FilterDescriptorsChangedEventArgs>? FilterDescriptorsChanged;
}

public interface IFilterableDataGridViewModel<T> : IFilterableDataGridViewModel
{
    new IFilterContext Context { get; }
    new IExcelFilterEngine<T> FilterEngine { get; }
    new IAsyncDataProvider<T>? AsyncDataProvider { get; set; }
    new IEnumerable<T> LocalDataSource { get; set; }
    new IEnumerable<T> FilteredItems { get; }
    new Task RefreshDataAsync();
    new Task ApplyFilterPipelineAsync(FilterPipeline pipeline);
    new FilterPipeline CreatePipelineFromCurrentSnapshot();
    new FilterPipelineSnapshot CreateFilterPipelineSnapshot();
    new Task ApplyFilterPipelineSnapshotAsync(FilterPipelineSnapshot snapshot);
    new Task ApplyPipelineSessionAsync();
}
