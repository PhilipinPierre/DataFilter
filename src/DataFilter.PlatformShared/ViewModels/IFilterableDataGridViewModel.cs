using System.Collections;
using System.ComponentModel;
using System.Globalization;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

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
}
