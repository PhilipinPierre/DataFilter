using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;
using DataFilter.PlatformShared.ViewModels;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Adapters;

/// <summary>
/// Implementation of <see cref="ICollectionViewFilterAdapter{T}"/> that bridges WPF's collection view mechanisms
/// with DataFilter's Excel-like filtering and sorting.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public partial class CollectionViewFilterAdapter<T> : ObservableObject, ICollectionViewFilterAdapter<T>
{
    private IEnumerable? _lastSourceCollectionSyncRef;
    private readonly Dictionary<string, ExcelFilterState> _columnFilterStates = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public ICollectionView CollectionView { get; }

    /// <inheritdoc />
    public IFilterContext Context { get; } = new FilterContext();

    /// <inheritdoc />
    public IExcelFilterEngine<T> FilterEngine { get; } = new ExcelFilterEngine<T>();

    IExcelFilterEngine DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel.FilterEngine => FilterEngine;

    /// <inheritdoc />
    public Type ItemType { get; set; } = typeof(T);

    /// <inheritdoc />
    public HashSet<string> FilterableProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IAsyncDataProvider<T>? AsyncDataProvider { get; set; }

    IAsyncDataProvider? DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel.AsyncDataProvider
    {
        get => AsyncDataProvider is null ? null : new AsyncDataProviderAdapter<T>(AsyncDataProvider);
        set
        {
            if (value is null)
            {
                AsyncDataProvider = null;
                return;
            }

            throw new NotSupportedException("Assign the typed AsyncDataProvider property instead.");
        }
    }

    /// <inheritdoc />
    public IEnumerable<T> LocalDataSource
    {
        get => CollectionView.SourceCollection.Cast<object>().OfType<T>();
        set => throw new NotSupportedException("LocalDataSource cannot be set directly on CollectionViewFilterAdapter. It uses the CollectionView's SourceCollection.");
    }

    IEnumerable DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel.LocalDataSource
    {
        get => CollectionView.SourceCollection;
        set => throw new NotSupportedException("LocalDataSource cannot be set directly on CollectionViewFilterAdapter. It uses the CollectionView's SourceCollection.");
    }

    private readonly CollectionViewGenericWrapper<T> _filteredItemsWrapper;

    /// <inheritdoc />
    public IEnumerable<T> FilteredItems => _filteredItemsWrapper;

    IEnumerable DataFilter.PlatformShared.ViewModels.IFilterableDataGridViewModel.FilteredItems => FilteredItems;

    /// <inheritdoc />
    public event EventHandler<FilterDescriptorsChangedEventArgs>? FilterDescriptorsChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionViewFilterAdapter{T}"/> class.
    /// </summary>
    /// <param name="collectionView">The collection view to adapt.</param>
    public CollectionViewFilterAdapter(ICollectionView collectionView)
    {
        CollectionView = collectionView ?? throw new ArgumentNullException(nameof(collectionView));
        _filteredItemsWrapper = new CollectionViewGenericWrapper<T>(collectionView);
    }

    /// <inheritdoc />
    public Task RefreshDataAsync()
    {
        var sourceChanged = !ReferenceEquals(_lastSourceCollectionSyncRef, CollectionView.SourceCollection);
        _lastSourceCollectionSyncRef = CollectionView.SourceCollection;

        ReconcileExcelFilterStatesWithLocalDistincts();
        ApplyFilterToCollectionView();
        CollectionView.Refresh();

        if (sourceChanged && Context.Descriptors.Count > 0)
            FilterDescriptorsChanged?.Invoke(this, new FilterDescriptorsChangedEventArgs());

        return Task.CompletedTask;
    }

    private void ReconcileExcelFilterStatesWithLocalDistincts()
    {
        foreach (var d in Context.Descriptors)
        {
            if (d is ExcelFilterDescriptor ed)
            {
                var distincts = FilterEngine.DistinctValuesExtractor.Extract(LocalDataSource, ItemType, ed.PropertyName);
                ExcelFilterSelectionReconciler.ReconcileSelectedValues(ed.State, distincts);
            }
        }
    }

    /// <inheritdoc />
    public void ApplyColumnFilter(string propertyName, ExcelFilterState state)
    {
        _columnFilterStates[propertyName] = state;
        
        var descriptor = new ExcelFilterDescriptor(propertyName, state);
        if (Context is FilterContext ctx)
        {
            ctx.AddOrUpdateDescriptor(descriptor);
        }

        RefreshDataAsync();
    }

    /// <inheritdoc />
    public void ClearColumnFilter(string propertyName)
    {
        _columnFilterStates.Remove(propertyName);
        if (Context is FilterContext ctx)
        {
            ctx.RemoveDescriptor(propertyName);
        }

        RefreshDataAsync();
        FilterDescriptorsChanged?.Invoke(this, new FilterDescriptorsChangedEventArgs { AffectedPropertyName = propertyName });
    }

    /// <inheritdoc />
    public void ApplySort(string propertyName, bool isDescending)
    {
        if (Context is FilterContext ctx)
        {
            ctx.SetSort(propertyName, isDescending);
        }

        CollectionView.SortDescriptions.Clear();
        CollectionView.SortDescriptions.Add(new SortDescription(propertyName, isDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        
        // Refresh filter as well to ensure consistency
        RefreshDataAsync();
    }

    /// <inheritdoc />
    public void AddSubSort(string propertyName, bool isDescending)
    {
        if (Context is FilterContext ctx)
        {
            ctx.AddSort(propertyName, isDescending);
        }

        // Remove existing sort for same property if any (WPF SortDescriptionCollection doesn't handle duplicates gracefully)
        var existing = CollectionView.SortDescriptions.FirstOrDefault(s => s.PropertyName == propertyName);
        if (existing != default)
        {
            CollectionView.SortDescriptions.Remove(existing);
        }

        CollectionView.SortDescriptions.Add(new SortDescription(propertyName, isDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        
        RefreshDataAsync();
    }

    /// <inheritdoc />
    public void ClearSort()
    {
        if (Context is FilterContext ctx)
        {
            ctx.ClearSort();
        }
        CollectionView.SortDescriptions.Clear();
        RefreshDataAsync();
    }

    /// <inheritdoc />
    public Task<IEnumerable<object>> GetDistinctValuesAsync(string propertyName, string searchText)
    {
        return Task.Run(() =>
        {
            var source = CollectionView.SourceCollection.Cast<T>();
            var distincts = FilterEngine.DistinctValuesExtractor.Extract(source, propertyName);

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

    /// <inheritdoc />
    public ExcelFilterState? GetColumnFilterState(string propertyName)
    {
        return _columnFilterStates.TryGetValue(propertyName, out var state) ? state : null;
    }

    /// <inheritdoc />
    public Type? GetPropertyType(string propertyName)
    {
        return typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.PropertyType;
    }

    /// <inheritdoc />
    public IFilterSnapshot ExtractSnapshot()
    {
        return new FilterSnapshotBuilder().CreateSnapshot(Context);
    }

    /// <inheritdoc />
    public void RestoreSnapshot(IFilterSnapshot snapshot)
    {
        if (Context is FilterContext ctx)
        {
            new FilterSnapshotBuilder().RestoreSnapshot(ctx, snapshot);

            _columnFilterStates.Clear();
            SyncColumnFilterStatesFromContext();

            CollectionView.SortDescriptions.Clear();
            foreach (var sort in Context.SortDescriptors)
            {
                CollectionView.SortDescriptions.Add(new SortDescription(sort.PropertyName, sort.IsDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
            }

            RefreshDataAsync();
            FilterDescriptorsChanged?.Invoke(this, new FilterDescriptorsChangedEventArgs());
        }
    }

    /// <inheritdoc />
    public async Task ApplyFilterPipelineAsync(FilterPipeline pipeline)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (Context is FilterContext ctx)
        {
            var compiled = FilterPipelineCompiler.Compile(pipeline);
            var excelDescriptors = FilterDescriptorToExcelConverter.ConvertCompiledPipeline(compiled);
            ctx.ReplaceDescriptors(excelDescriptors);
            _columnFilterStates.Clear();
            SyncColumnFilterStatesFromContext();
        }

        await RefreshDataAsync();
        FilterDescriptorsChanged?.Invoke(this, new FilterDescriptorsChangedEventArgs());
    }

    /// <inheritdoc />
    public FilterPipeline CreatePipelineFromCurrentSnapshot()
    {
        return FilterPipelineInterop.FromLegacySnapshot(ExtractSnapshot());
    }

    private void SyncColumnFilterStatesFromContext()
    {
        foreach (var descriptor in Context.Descriptors)
        {
            var propertyName = GetFilterPropertyName(descriptor);
            if (string.IsNullOrEmpty(propertyName))
                continue;

            ExcelFilterState state;
            if (descriptor is ExcelFilterDescriptor excelDesc)
            {
                state = CloneExcelFilterState(excelDesc.State);
            }
            else if (descriptor is IFilterGroup group)
            {
                state = BuildExcelFilterStateFromGroup(group);
            }
            else
            {
                continue;
            }

            _columnFilterStates[propertyName] = state;
        }
    }

    private static string GetFilterPropertyName(IFilterDescriptor descriptor)
    {
        if (!string.IsNullOrEmpty(descriptor.PropertyName))
            return descriptor.PropertyName;

        if (descriptor is IFilterGroup g)
        {
            var first = g.Descriptors.FirstOrDefault();
            return first?.PropertyName ?? string.Empty;
        }

        return string.Empty;
    }

    private static ExcelFilterState CloneExcelFilterState(ExcelFilterState s)
    {
        var clone = new ExcelFilterState
        {
            SearchText = s.SearchText,
            UseWildcards = s.UseWildcards,
            SelectAll = s.SelectAll,
            CustomOperator = s.CustomOperator,
            CustomValue1 = s.CustomValue1,
            CustomValue2 = s.CustomValue2
        };

        foreach (var d in s.DistinctValues)
            clone.DistinctValues.Add(d);
        foreach (var v in s.SelectedValues)
            clone.SelectedValues.Add(v);
        foreach (var c in s.AdditionalCustomCriteria)
        {
            clone.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
            {
                Operator = c.Operator,
                Value1 = c.Value1,
                Value2 = c.Value2
            });
        }

        return clone;
    }

    /// <summary>
    /// Rebuilds Excel-like UI state from a restored filter group (e.g. after snapshot round-trip).
    /// Custom/advanced rules are stored as operators on <see cref="FilterDescriptor"/>; manual list selection uses In/NotIn.
    /// </summary>
    private static ExcelFilterState BuildExcelFilterStateFromGroup(IFilterGroup group)
    {
        var state = new ExcelFilterState();
        var children = group.Descriptors.OfType<FilterDescriptor>().ToList();

        var customs = children.Where(d => d.Operator != FilterOperator.In).ToList();
        var inRule = children.FirstOrDefault(d => d.Operator == FilterOperator.In);

        if (customs.Count > 0)
        {
            ExcelFilterStateFromFilterDescriptor.ApplyCustomRuleToState(state, customs[0]);
            for (int i = 1; i < customs.Count; i++)
            {
                var fd = customs[i];
                if (fd.Operator == FilterOperator.Between && fd.Value is RangeValue rv)
                {
                    state.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
                    {
                        Operator = fd.Operator,
                        Value1 = rv.Min,
                        Value2 = rv.Max
                    });
                }
                else
                {
                    state.AdditionalCustomCriteria.Add(new ExcelFilterAdditionalCriterion
                    {
                        Operator = fd.Operator,
                        Value1 = fd.Value,
                        Value2 = null
                    });
                }
            }
        }
        else if (inRule != null)
        {
            ExcelFilterStateFromFilterDescriptor.ApplyInRuleToState(state, inRule);
        }

        return state;
    }

    private Func<T, bool>? _cachedPredicate;

    private void ApplyFilterToCollectionView()
    {
        if (Context.Descriptors.Count == 0)
        {
            CollectionView.Filter = null;
            _cachedPredicate = null;
        }
        else
        {
            var expression = FilterExpressionBuilder.BuildExpression<T>(Context.Descriptors);
            _cachedPredicate = expression.Compile();

            CollectionView.Filter = item =>
            {
                if (item is T typedItem && _cachedPredicate != null)
                {
                    return _cachedPredicate(typedItem);
                }
                return true;
            };
        }
    }
}
