using System.Collections;
using System.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.PlatformShared.ViewModels;

public interface IFilterableDataGridViewModel : INotifyPropertyChanged
{
    IEnumerable FilteredItems { get; }
    IFilterContext Context { get; }
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
    IExcelFilterEngine<T> FilterEngine { get; }
    IAsyncDataProvider<T>? AsyncDataProvider { get; set; }
    IEnumerable<T> LocalDataSource { get; set; }
    IEnumerable<T> FilteredItems { get; }
    new Task RefreshDataAsync();
}
