using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;
using DataFilter.Core.Services;

namespace DataFilter.Wpf.ViewModels;

/// <summary>
/// Implementation of the data grid filtering orchestrator.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
public partial class FilterableDataGridViewModel<T> : ObservableObject, IFilterableDataGridViewModel<T>
{
    public IFilterContext Context { get; } = new FilterContext();
    public IExcelFilterEngine<T> FilterEngine { get; } = new ExcelFilterEngine<T>();
    public HashSet<string> FilterableProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private IAsyncDataProvider<T>? _asyncDataProvider;

    [ObservableProperty]
    private IEnumerable<T> _localDataSource = Enumerable.Empty<T>();

    /// <summary>
    /// Resulting items available to the UI.
    /// </summary>
    [ObservableProperty]
    private IEnumerable<T> _filteredItems = Enumerable.Empty<T>();

    /// <inheritdoc />
    public async void RefreshData()
    {
        if (AsyncDataProvider != null)
        {
            var pagedResult = await AsyncDataProvider.FetchDataAsync(Context);
            FilteredItems = pagedResult.Items;
        }
        else
        {
            var items = FilterEngine.Apply(LocalDataSource, Context.Descriptors);

            if (Context.SortDescriptors.Count > 0)
            {
                IOrderedEnumerable<T>? orderedItems = null;

                foreach (var sort in Context.SortDescriptors)
                {
                    var propInfo = typeof(T).GetProperty(sort.PropertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (propInfo == null) continue;

                    if (orderedItems == null)
                    {
                        orderedItems = sort.IsDescending
                            ? items.OrderByDescending(x => propInfo.GetValue(x))
                            : items.OrderBy(x => propInfo.GetValue(x));
                    }
                    else
                    {
                        orderedItems = sort.IsDescending
                            ? orderedItems.ThenByDescending(x => propInfo.GetValue(x))
                            : orderedItems.ThenBy(x => propInfo.GetValue(x));
                    }
                }

                if (orderedItems != null)
                {
                    items = orderedItems;
                }
            }

            FilteredItems = items.ToList();
        }
    }

    public void ApplyColumnFilter(string propertyName, ExcelFilterState state)
    {
        var descriptor = new ExcelFilterDescriptor(propertyName, state);
        if (Context is FilterContext ctx)
        {
            ctx.AddOrUpdateDescriptor(descriptor);
            ctx.Page = 1;
        }
        RefreshData();
    }

    public void ClearColumnFilter(string propertyName)
    {
        if (Context is FilterContext ctx)
        {
            ctx.RemoveDescriptor(propertyName);
            ctx.Page = 1;
        }
        RefreshData();
    }

    public void ApplySort(string propertyName, bool isDescending)
    {
        if (Context is FilterContext ctx)
        {
            ctx.SetSort(propertyName, isDescending);
            ctx.Page = 1;
        }
        RefreshData();
    }

    public void AddSubSort(string propertyName, bool isDescending)
    {
        if (Context is FilterContext ctx)
        {
            ctx.AddSort(propertyName, isDescending);
            ctx.Page = 1;
        }
        RefreshData();
    }

    public void ClearSort()
    {
        if (Context is FilterContext ctx)
        {
            ctx.ClearSort();
            ctx.Page = 1;
        }
        RefreshData();
    }

    public async Task<IEnumerable<object>> GetDistinctValuesAsync(string propertyName, string searchText)
    {
        if (AsyncDataProvider != null)
        {
            return await AsyncDataProvider.FetchDistinctValuesAsync(propertyName, searchText);
        }

        var distincts = FilterEngine.DistinctValuesExtractor.Extract(LocalDataSource, propertyName);
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var matcher = new WildcardMatcher();
            if (matcher.ContainsWildcard(searchText))
            {
                distincts = distincts.Where(x => x != null && matcher.IsMatch(x.ToString() ?? string.Empty, searchText));
            }
            else
            {
                distincts = distincts.Where(x => x?.ToString()?.StartsWith(searchText, StringComparison.OrdinalIgnoreCase) == true);
            }
        }
        return distincts;
    }

    public ExcelFilterState? GetColumnFilterState(string propertyName)
    {
        return Context.Descriptors.FirstOrDefault(d => string.Equals(d.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase)) is ExcelFilterDescriptor excelDesc 
            ? excelDesc.State 
            : null;
    }

    public Type? GetPropertyType(string propertyName)
    {
        return typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.PropertyType;
    }

    public IFilterSnapshot ExtractSnapshot()
    {
        return new FilterSnapshotBuilder().CreateSnapshot(Context);
    }

    private IReadOnlyList<FilterSnapshotEntry> CleanSnapshotEntries(IEnumerable<FilterSnapshotEntry> entries)
    {
        var valid = new List<FilterSnapshotEntry>();
        foreach (var entry in entries)
        {
            if (entry.IsGroup)
            {
                var validChildren = CleanSnapshotEntries(entry.Children ?? Enumerable.Empty<FilterSnapshotEntry>());
                if (validChildren.Count > 0)
                {
                    valid.Add(new FilterSnapshotEntry
                    {
                        PropertyName = string.Empty,
                        Operator = string.Empty,
                        LogicalOperator = entry.LogicalOperator,
                        Children = validChildren.ToList()
                    });
                }
            }
            else
            {
                bool isValid = FilterableProperties.Count > 0 
                    ? FilterableProperties.Contains(entry.PropertyName)
                    : typeof(T).GetProperty(entry.PropertyName) != null;
                
                if (isValid)
                {
                    valid.Add(entry);
                }
            }
        }
        return valid;
    }

    public void RestoreSnapshot(IFilterSnapshot snapshot)
    {
        var validEntries = CleanSnapshotEntries(snapshot.Entries);
        var validSorts = snapshot.SortEntries.Where(e => 
            FilterableProperties.Count > 0 
                ? FilterableProperties.Contains(e.PropertyName) 
                : typeof(T).GetProperty(e.PropertyName) != null
        ).ToList();

        var filteredSnapshot = new FilterSnapshot(
            (IReadOnlyList<FilterSnapshotEntry>)validEntries, 
            (IReadOnlyList<SortSnapshotEntry>)validSorts);

        new FilterSnapshotBuilder().RestoreSnapshot(Context, filteredSnapshot);
        RefreshData();
    }
}
