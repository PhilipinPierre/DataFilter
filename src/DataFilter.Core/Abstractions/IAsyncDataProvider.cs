using DataFilter.Core.Models;

namespace DataFilter.Core.Abstractions;

/// <summary>
/// Provides asynchronous data retrieval for filtering.
/// </summary>
/// <typeparam name="T">The type of the data items.</typeparam>
public interface IAsyncDataProvider<T>
{
    /// <summary>
    /// Fetches filtered data from the external source.
    /// </summary>
    /// <param name="context">The filter context defining the criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the paged result.</returns>
    Task<PagedResult<T>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the distinct values of a specific property for the filter panel.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve distinct values for.</param>
    /// <param name="searchText">Optional text to filter the distinct values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of distinct values.</returns>
    Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText = "", CancellationToken cancellationToken = default);
}
