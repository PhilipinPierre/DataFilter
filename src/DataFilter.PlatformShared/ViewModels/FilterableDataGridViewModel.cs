using System.Collections;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;

namespace DataFilter.PlatformShared.ViewModels;

/// <summary>
/// Implementation of the data grid filtering orchestrator (non-generic).
/// </summary>
public partial class FilterableDataGridViewModel : ObservableObject, IFilterableDataGridViewModel
{
    IEnumerable IFilterableDataGridViewModel.FilteredItems => FilteredItems;

    public IFilterContext Context { get; } = new FilterContext();
    public IExcelFilterEngine FilterEngine { get; }
    public HashSet<string> FilterableProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance with the default <see cref="ExcelFilterEngine"/>.
    /// </summary>
    public FilterableDataGridViewModel()
        : this(new ExcelFilterEngine())
    {
    }

    /// <summary>
    /// Initializes a new instance with a specific filter engine.
    /// </summary>
    protected FilterableDataGridViewModel(IExcelFilterEngine filterEngine)
    {
        FilterEngine = filterEngine ?? throw new ArgumentNullException(nameof(filterEngine));
    }

    [ObservableProperty]
    private IAsyncDataProvider? _asyncDataProvider;

    [ObservableProperty]
    private IEnumerable _localDataSource = Array.Empty<object>();

    [ObservableProperty]
    private Type _itemType = typeof(object);

    /// <summary>
    /// Resulting items available to the UI.
    /// </summary>
    [ObservableProperty]
    private IEnumerable _filteredItems = Array.Empty<object>();

    /// <inheritdoc />
    public async Task RefreshDataAsync()
    {
        if (AsyncDataProvider != null)
        {
            var pagedResult = await AsyncDataProvider.FetchDataAsync(Context);
            FilteredItems = pagedResult.Items;
        }
        else
        {
            // Materialize immediately: Apply returns a deferred IEnumerable; binding to it can skip
            // re-enumeration when the source list is replaced, so filters never visibly "reapply".
            var filtered = FilterEngine.Apply(LocalDataSource, ItemType, Context.Descriptors);
            var list = filtered.Cast<object>().ToList();

            if (Context.SortDescriptors.Count > 0)
            {
                IOrderedEnumerable<object>? orderedItems = null;

                foreach (var sort in Context.SortDescriptors)
                {
                    var propInfo = ItemType.GetProperty(sort.PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo == null) continue;

                    if (orderedItems == null)
                    {
                        orderedItems = sort.IsDescending
                            ? list.OrderByDescending(x => propInfo.GetValue(x))
                            : list.OrderBy(x => propInfo.GetValue(x));
                    }
                    else
                    {
                        orderedItems = sort.IsDescending
                            ? orderedItems.ThenByDescending(x => propInfo.GetValue(x))
                            : orderedItems.ThenBy(x => propInfo.GetValue(x));
                    }
                }

                FilteredItems = orderedItems != null ? orderedItems.ToList() : list;
            }
            else
            {
                FilteredItems = list;
            }
        }
    }

    public async void ApplyColumnFilter(string propertyName, ExcelFilterState state)
    {
        var descriptor = new ExcelFilterDescriptor(propertyName, state);
        if (Context is FilterContext ctx)
        {
            ctx.AddOrUpdateDescriptor(descriptor);
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

    public async void ClearSort()
    {
        if (Context is FilterContext ctx)
        {
            ctx.ClearSort();
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
            var distincts = FilterEngine.DistinctValuesExtractor.Extract(LocalDataSource, ItemType, propertyName);
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
        });
    }

    public ExcelFilterState? GetColumnFilterState(string propertyName)
    {
        return Context.Descriptors.FirstOrDefault(d => string.Equals(d.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase)) is ExcelFilterDescriptor excelDesc
            ? excelDesc.State
            : null;
    }

    public Type? GetPropertyType(string propertyName)
    {
        return ItemType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.PropertyType;
    }

    public IFilterSnapshot ExtractSnapshot()
    {
        return new FilterSnapshotBuilder().CreateSnapshot(Context);
    }

    /// <inheritdoc />
    public async Task ApplyFilterPipelineAsync(FilterPipeline pipeline)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (Context is FilterContext ctx)
        {
            pipeline.ApplyToContext(ctx);
            ctx.Page = 1;
        }

        await RefreshDataAsync();
    }

    /// <inheritdoc />
    public FilterPipeline CreatePipelineFromCurrentSnapshot()
    {
        return FilterPipelineInterop.FromLegacySnapshot(ExtractSnapshot());
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
                        PropertyName = entry.PropertyName ?? string.Empty,
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
                    : ItemType.GetProperty(entry.PropertyName) != null;

                if (isValid)
                {
                    valid.Add(entry);
                }
            }
        }
        return valid;
    }

    public async void RestoreSnapshot(IFilterSnapshot snapshot)
    {
        var validEntries = CleanSnapshotEntries(snapshot.Entries);
        var validSorts = snapshot.SortEntries.Where(e =>
            FilterableProperties.Count > 0
                ? FilterableProperties.Contains(e.PropertyName)
                : ItemType.GetProperty(e.PropertyName) != null
        ).ToList();

        var filteredSnapshot = new FilterSnapshot(
            (IReadOnlyList<FilterSnapshotEntry>)validEntries,
            (IReadOnlyList<SortSnapshotEntry>)validSorts);

        new FilterSnapshotBuilder().RestoreSnapshot(Context, filteredSnapshot);
        await RefreshDataAsync();
    }
}

/// <summary>
/// Typed projection of <see cref="FilterableDataGridViewModel"/> for homogeneous item collections.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
public class FilterableDataGridViewModel<T> : FilterableDataGridViewModel, IFilterableDataGridViewModel<T>
{
    private IAsyncDataProvider<T>? _asyncDataProvider;

    /// <summary>
    /// Initializes a new instance with <see cref="ItemType"/> set to <typeparamref name="T"/>.
    /// </summary>
    public FilterableDataGridViewModel()
        : base(new ExcelFilterEngine<T>())
    {
        ItemType = typeof(T);
    }

    IEnumerable IFilterableDataGridViewModel.FilteredItems => FilteredItems;

    /// <inheritdoc />
    public new IExcelFilterEngine<T> FilterEngine => (IExcelFilterEngine<T>)base.FilterEngine;

    /// <inheritdoc />
    public new IEnumerable<T> LocalDataSource
    {
        get => base.LocalDataSource.Cast<T>();
        set => base.LocalDataSource = value ?? Enumerable.Empty<T>();
    }

    /// <inheritdoc />
    public new IEnumerable<T> FilteredItems
    {
        get => base.FilteredItems.Cast<T>();
        set => base.FilteredItems = value ?? Enumerable.Empty<T>();
    }

    /// <inheritdoc />
    public new IAsyncDataProvider<T>? AsyncDataProvider
    {
        get => _asyncDataProvider;
        set
        {
            if (SetProperty(ref _asyncDataProvider, value))
            {
                base.AsyncDataProvider = value is null ? null : new AsyncDataProviderAdapter<T>(value);
            }
        }
    }
}
