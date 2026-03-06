using DataFilter.Core.Abstractions;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Orchestrates the asynchronous retrieval of distinct values and synchronizes the filter state.
/// </summary>
/// <typeparam name="T">The type of the item being filtered.</typeparam>
public class ExcelLikeAsyncFilter<T> : IExcelLikeAsyncFilter<T>
{
    private IAsyncDataProvider<T>? _provider;

    /// <inheritdoc />
    public void BindProvider(IAsyncDataProvider<T> provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <inheritdoc />
    public async Task ApplySearchAndFetchDistinctValuesAsync(string propertyName, ExcelFilterState state, CancellationToken cancellationToken = default)
    {
        if (_provider == null)
        {
            throw new InvalidOperationException("An IAsyncDataProvider must be bound before fetching values asynchronously.");
        }

        var distinctValues = await _provider.FetchDistinctValuesAsync(propertyName, state.SearchText, cancellationToken).ConfigureAwait(true);

        // Update the distinct values in the state
        state.DistinctValues.Clear();
        foreach (var val in distinctValues)
        {
            state.DistinctValues.Add(val);
        }

        // Adjust 'SelectedValues' so that we don't drop explicitly selected values that are suddenly filtered out by search.
        // Wait, if a user searches for "A", un-selects ALL, then selects "Apple", and clears the search:
        // By default Excel behavior: previous selections are kept, checkmarks reflect the Current Distinct Values intersection.
        // For WPF bindings we only replace the distinct values displayed to the user.
    }
}
