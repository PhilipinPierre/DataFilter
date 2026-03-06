namespace DataFilter.Expressions.Server.Abstractions;

/// <summary>
/// Provides "Top N" filtering for a numeric property — retains only the highest
/// or lowest N values (by absolute count or by percentage of total) from a query.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface ITopNFilter<T>
{
    /// <summary>
    /// Returns a query containing only the top <paramref name="count"/> highest values
    /// of the specified <paramref name="propertyName"/>, sorted descending.
    /// </summary>
    /// <param name="query">The base query to filter.</param>
    /// <param name="propertyName">The numeric property to rank on.</param>
    /// <param name="count">The number of items to retain.</param>
    IQueryable<T> TopHighest(IQueryable<T> query, string propertyName, int count);

    /// <summary>
    /// Returns a query containing only the top <paramref name="count"/> lowest values
    /// of the specified <paramref name="propertyName"/>, sorted ascending.
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="propertyName">The numeric property to rank on.</param>
    /// <param name="count">The number of items to retain.</param>
    IQueryable<T> TopLowest(IQueryable<T> query, string propertyName, int count);
}
