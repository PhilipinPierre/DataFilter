using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Services;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Adapters;

/// <summary>
/// Implementation of <see cref="ICollectionViewFilterAdapter{T}"/> that bridges WPF's collection view mechanisms
/// with DataFilter's Excel-like filtering and sorting.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public partial class CollectionViewFilterAdapter<T> : ObservableObject, ICollectionViewFilterAdapter<T>
{
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
        ApplyFilterToCollectionView();
        CollectionView.Refresh();
        return Task.CompletedTask;
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
        // Re-use logic from FilterableDataGridViewModel or ideally centralize it in a service
        // For now, let's implement basic restoration
        if (Context is FilterContext ctx)
        {
            new FilterSnapshotBuilder().RestoreSnapshot(ctx, snapshot);
            
            // Sync local dictionary
            _columnFilterStates.Clear();
            foreach (var descriptor in Context.Descriptors.OfType<ExcelFilterDescriptor>())
            {
                _columnFilterStates[descriptor.PropertyName] = descriptor.State;
            }

            // Sync sort descriptions
            CollectionView.SortDescriptions.Clear();
            foreach (var sort in Context.SortDescriptors)
            {
                CollectionView.SortDescriptions.Add(new SortDescription(sort.PropertyName, sort.IsDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
            }

            RefreshDataAsync();
        }
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
