using DataFilter.Core.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Abstractions;

/// <summary>
/// Orchestrates the asynchronous retrieval of distinct values and data applying Excel-like filters.
/// </summary>
/// <typeparam name="T">The type of the target entity.</typeparam>
public interface IExcelLikeAsyncFilter<T>
{
    /// <summary>
    /// Binds an asynchronous provider to the filter orchestrator.
    /// </summary>
    void BindProvider(IAsyncDataProvider<T> provider);

    /// <summary>
    /// Fetches the distinct values asynchronously using the current search context.
    /// </summary>
    Task ApplySearchAndFetchDistinctValuesAsync(string propertyName, ExcelFilterState state, CancellationToken cancellationToken = default);
}
