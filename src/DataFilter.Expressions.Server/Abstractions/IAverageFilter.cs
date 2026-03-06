namespace DataFilter.Expressions.Server.Abstractions;

/// <summary>
/// Filters items based on a comparison to the computed average of a numeric column.
/// Requires the full dataset (or an <see cref="IQueryable{T}"/>) to compute the average.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IAverageFilter<T>
{
    /// <summary>
    /// Returns a query containing only items whose <paramref name="propertyName"/> value
    /// is strictly above the column average.
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="propertyName">The numeric property to compute the average on.</param>
    IQueryable<T> AboveAverage(IQueryable<T> query, string propertyName);

    /// <summary>
    /// Returns a query containing only items whose <paramref name="propertyName"/> value
    /// is strictly below the column average.
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="propertyName">The numeric property to compute the average on.</param>
    IQueryable<T> BelowAverage(IQueryable<T> query, string propertyName);
}
