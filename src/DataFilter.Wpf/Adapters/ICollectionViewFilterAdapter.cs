using System.ComponentModel;
using DataFilter.Core.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Wpf.ViewModels;

namespace DataFilter.Wpf.Adapters;

/// <summary>
/// Defines an adapter that bridges WPF's <see cref="ICollectionView"/> with the DataFilter system.
/// </summary>
public interface ICollectionViewFilterAdapter : IFilterableDataGridViewModel
{
    /// <summary>
    /// Gets the underlying collection view.
    /// </summary>
    ICollectionView CollectionView { get; }
}

/// <summary>
/// Generic version of the CollectionView filter adapter.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public interface ICollectionViewFilterAdapter<T> : ICollectionViewFilterAdapter, IFilterableDataGridViewModel<T>
{
}
