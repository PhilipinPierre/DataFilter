using System.Collections;
using System.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.PlatformShared.ViewModels;

public interface IFilterableDataGridViewModel : INotifyPropertyChanged
{
    IEnumerable FilteredItems { get; }
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
    HashSet<string> FilterableProperties { get; }
}

public interface IFilterableDataGridViewModel<T> : IFilterableDataGridViewModel
{
    new IFilterContext Context { get; }
    new IExcelFilterEngine<T> FilterEngine { get; }
    new IAsyncDataProvider<T>? AsyncDataProvider { get; set; }
    new IEnumerable<T> LocalDataSource { get; set; }
    new IEnumerable<T> FilteredItems { get; }
    new Task RefreshDataAsync();
}
