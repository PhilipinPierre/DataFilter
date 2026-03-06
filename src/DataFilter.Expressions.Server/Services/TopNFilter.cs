using DataFilter.Expressions.Server.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace DataFilter.Expressions.Server.Services;

/// <summary>
/// Filters a query retaining only the top N highest or lowest values for a numeric property.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class TopNFilter<T> : ITopNFilter<T>
{
    /// <inheritdoc />
    public IQueryable<T> TopHighest(IQueryable<T> query, string propertyName, int count)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

        return OrderByDescending(query, propertyName).Take(count);
    }

    /// <inheritdoc />
    public IQueryable<T> TopLowest(IQueryable<T> query, string propertyName, int count)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

        return OrderByAscending(query, propertyName).Take(count);
    }

    private static IOrderedQueryable<T> OrderByDescending(IQueryable<T> query, string propertyName)
    {
        return ApplyOrderBy(query, propertyName, "OrderByDescending");
    }

    private static IOrderedQueryable<T> OrderByAscending(IQueryable<T> query, string propertyName)
    {
        return ApplyOrderBy(query, propertyName, "OrderBy");
    }

    private static IOrderedQueryable<T> ApplyOrderBy(IQueryable<T> query, string propertyName, string methodName)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        MemberExpression member = Expression.Property(param, propertyName);
        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), member.Type);
        LambdaExpression lambda = Expression.Lambda(delegateType, member, param);

        MethodInfo method = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), member.Type);

        return (IOrderedQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }
}
