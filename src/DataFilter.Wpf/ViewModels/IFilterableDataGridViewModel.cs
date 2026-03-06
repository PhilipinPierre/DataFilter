using DataFilter.Core.Abstractions;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// Non-generic base for orchestrator to be used by behaviors without knowing T.
/// </summary>
public interface IFilterableDataGridViewModel
{
    IFilterContext Context { get; }
    void RefreshData();

    // Expose methods for the popup
    void ApplyColumnFilter(string propertyName, ExcelFilterState state);
    void ClearColumnFilter(string propertyName);
    void ApplySort(string propertyName, bool isDescending);
    void AddSubSort(string propertyName, bool isDescending);
    System.Threading.Tasks.Task<IEnumerable<object>> GetDistinctValuesAsync(string propertyName, string searchText);
    ExcelFilterState? GetColumnFilterState(string propertyName);
    Type? GetPropertyType(string propertyName);

    // Snapshot management
    IFilterSnapshot ExtractSnapshot();
    void RestoreSnapshot(IFilterSnapshot snapshot);

    /// <summary>
    /// Gets the list of property names that are currently filterable from the UI.
    /// This is used to ignore obsolete filters during snapshot restoration.
    /// </summary>
    HashSet<string> FilterableProperties { get; }
}

/// <summary>
/// Orchestrates an Excel-like filtering process over a data grid.
/// </summary>
/// <typeparam name="T">The type of the item in the data grid.</typeparam>
public interface IFilterableDataGridViewModel<T> : IFilterableDataGridViewModel
{
    /// <summary>
    /// Gets the filter context.
    /// </summary>
    new IFilterContext Context { get; }

    /// <summary>
    /// Gets the engine used to apply filters locally.
    /// </summary>
    IExcelFilterEngine<T> FilterEngine { get; }

    /// <summary>
    /// Gets or sets the data provider for asynchronous fetching.
    /// </summary>
    IAsyncDataProvider<T>? AsyncDataProvider { get; set; }

    /// <summary>
    /// Binds an external collection that serves as the local data source (when not using async).
    /// </summary>
    IEnumerable<T> LocalDataSource { get; set; }

    /// <summary>
    /// Executes the filter pipeline, either locally or via the async data provider.
    /// </summary>
    new void RefreshData();
}
