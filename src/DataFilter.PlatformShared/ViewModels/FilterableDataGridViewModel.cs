using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Core.Services;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;

namespace DataFilter.PlatformShared.ViewModels;

public partial class FilterableDataGridViewModel<T> : ObservableObject, IFilterableDataGridViewModel<T>
{
    public IFilterContext Context { get; } = new FilterContext();
    public IExcelFilterEngine<T> FilterEngine { get; } = new ExcelFilterEngine<T>();
    public HashSet<string> FilterableProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty] private IAsyncDataProvider<T>? _asyncDataProvider;
    [ObservableProperty] private IEnumerable<T> _localDataSource = Enumerable.Empty<T>();
    [ObservableProperty] private IEnumerable<T> _filteredItems = Enumerable.Empty<T>();

    public async Task RefreshDataAsync()
    {
        if (AsyncDataProvider != null)
        {
            FilteredItems = (await AsyncDataProvider.FetchDataAsync(Context)).Items;
            return;
        }

        var filtered = FilterEngine.Apply(LocalDataSource, Context.Descriptors);
        IEnumerable<T> result = filtered;
        IOrderedEnumerable<T>? ordered = null;
        foreach (var sort in Context.SortDescriptors)
        {
            var pi = typeof(T).GetProperty(sort.PropertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (pi == null) continue;
            ordered = ordered == null
                ? (sort.IsDescending ? filtered.OrderByDescending(x => pi.GetValue(x)) : filtered.OrderBy(x => pi.GetValue(x)))
                : (sort.IsDescending ? ordered.ThenByDescending(x => pi.GetValue(x)) : ordered.ThenBy(x => pi.GetValue(x)));
        }
        if (ordered != null) result = ordered;
        FilteredItems = result;
    }

    public async void ApplyColumnFilter(string propertyName, ExcelFilterState state)
    {
        if (Context is FilterContext ctx)
        {
            ctx.AddOrUpdateDescriptor(new ExcelFilterDescriptor(propertyName, state));
            ctx.Page = 1;
        }
        await RefreshDataAsync();
    }

    public async void ClearColumnFilter(string propertyName)
    {
        if (Context is FilterContext ctx)
        {
            ctx.RemoveDescriptor(propertyName);
            ctx.Page = 1;
        }
        await RefreshDataAsync();
    }

    public async void ApplySort(string propertyName, bool isDescending)
    {
        if (Context is FilterContext ctx)
        {
            ctx.SetSort(propertyName, isDescending);
            ctx.Page = 1;
        }
        await RefreshDataAsync();
    }

    public async void AddSubSort(string propertyName, bool isDescending)
    {
        if (Context is FilterContext ctx)
        {
            ctx.AddSort(propertyName, isDescending);
            ctx.Page = 1;
        }
        await RefreshDataAsync();
    }

    public Task<IEnumerable<object>> GetDistinctValuesAsync(string propertyName, string searchText)
    {
        if (AsyncDataProvider != null)
        {
            return AsyncDataProvider.FetchDistinctValuesAsync(propertyName, searchText);
        }

        return Task.Run(() =>
        {
            var distincts = FilterEngine.DistinctValuesExtractor.Extract(LocalDataSource, propertyName);
            if (string.IsNullOrWhiteSpace(searchText)) return distincts;
            var matcher = new WildcardMatcher();
            return matcher.ContainsWildcard(searchText)
                ? distincts.Where(x => x != null && matcher.IsMatch(x.ToString() ?? string.Empty, searchText))
                : distincts.Where(x => x?.ToString()?.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) == true);
        });
    }

    public ExcelFilterState? GetColumnFilterState(string propertyName)
        => Context.Descriptors.FirstOrDefault(d => string.Equals(d.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase)) is ExcelFilterDescriptor d ? d.State : null;

    public Type? GetPropertyType(string propertyName)
        => typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.PropertyType;

    public IFilterSnapshot ExtractSnapshot() => new FilterSnapshotBuilder().CreateSnapshot(Context);

    public async void RestoreSnapshot(IFilterSnapshot snapshot)
    {
        new FilterSnapshotBuilder().RestoreSnapshot(Context, snapshot);
        await RefreshDataAsync();
    }
}
