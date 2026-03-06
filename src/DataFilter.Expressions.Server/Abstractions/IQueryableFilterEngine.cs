using DataFilter.Core.Abstractions;

namespace DataFilter.Expressions.Server.Abstractions;

/// <summary>
/// Applies filter and sort criteria to an <see cref="IQueryable{T}"/> data source,
/// enabling efficient server-side (EF Core, Dapper, etc.) query execution.
/// </summary>
/// <typeparam name="T">The entity type being filtered.</typeparam>
public interface IQueryableFilterEngine<T>
{
    /// <summary>
    /// Applies all active filter descriptors and sort criteria from <paramref name="context"/>
    /// to the given <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The base <see cref="IQueryable{T}"/> to filter.</param>
    /// <param name="context">The current filter context.</param>
    /// <returns>A filtered and sorted <see cref="IQueryable{T}"/>.</returns>
    IQueryable<T> Apply(IQueryable<T> query, IFilterContext context);
}
