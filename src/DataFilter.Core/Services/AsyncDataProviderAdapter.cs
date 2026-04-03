using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;

namespace DataFilter.Core.Services;

/// <summary>
/// Adapts a typed <see cref="IAsyncDataProvider{T}"/> to <see cref="IAsyncDataProvider"/>.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class AsyncDataProviderAdapter<T> : IAsyncDataProvider
{
    private readonly IAsyncDataProvider<T> _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDataProviderAdapter{T}"/> class.
    /// </summary>
    /// <param name="inner">The typed provider.</param>
    public AsyncDataProviderAdapter(IAsyncDataProvider<T> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    public async Task<PagedResult<object>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default)
    {
        var r = await _inner.FetchDataAsync(context, cancellationToken).ConfigureAwait(false);
        return new PagedResult<object>
        {
            Items = r.Items?.Cast<object>() ?? Enumerable.Empty<object>(),
            TotalCount = r.TotalCount,
            Page = r.Page,
            PageSize = r.PageSize
        };
    }

    /// <inheritdoc />
    public Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText = "", CancellationToken cancellationToken = default)
        => _inner.FetchDistinctValuesAsync(propertyName, searchText, cancellationToken);
}
