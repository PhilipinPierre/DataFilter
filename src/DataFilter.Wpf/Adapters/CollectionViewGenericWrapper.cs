using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataFilter.Wpf.Adapters;

/// <summary>
/// A wrapper that implements <see cref="IEnumerable{T}"/> and <see cref="INotifyCollectionChanged"/>
/// by proxying an <see cref="ICollectionView"/>. This allows a DataGrid to bind to the generic 
/// interface while listening to the view's native filtering and sorting events.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
internal class CollectionViewGenericWrapper<T> : IEnumerable<T>, INotifyCollectionChanged
{
    private readonly ICollectionView _view;
    private readonly INotifyCollectionChanged? _notifySource;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionViewGenericWrapper{T}"/> class.
    /// </summary>
    /// <param name="view">The collection view to wrap.</param>
    public CollectionViewGenericWrapper(ICollectionView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _notifySource = view as INotifyCollectionChanged;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        // Use OfType<T> to safely skip MS.Internal.NamedObject (DataGrid placeholder)
        foreach (var item in _view)
        {
            if (item is T typedItem)
            {
                yield return typedItem;
            }
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add
        {
            if (_notifySource != null)
                _notifySource.CollectionChanged += value;
        }
        remove
        {
            if (_notifySource != null)
                _notifySource.CollectionChanged -= value;
        }
    }
}
