using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Expressions.Server.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace DataFilter.Expressions.Server.Services;

/// <summary>
/// Applies filter and sort criteria to an <see cref="IQueryable{T}"/> using
/// compiled LINQ expression trees for optimal server-side execution.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class QueryableFilterEngine<T> : IQueryableFilterEngine<T>
{
    /// <inheritdoc />
    public IQueryable<T> Apply(IQueryable<T> query, IFilterContext context)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (context.Descriptors.Count > 0)
        {
            System.Linq.Expressions.Expression<Func<T, bool>> predicate =
                FilterExpressionBuilder.BuildExpression<T>(context.Descriptors);
            query = query.Where(predicate);
        }

        bool firstSort = true;
        foreach (ISortDescriptor sort in context.SortDescriptors)
        {
            query = ApplySort(query, sort.PropertyName, sort.IsDescending, firstSort);
            firstSort = false;
        }

        return query;
    }

    private static IQueryable<T> ApplySort(
        IQueryable<T> query,
        string propertyName,
        bool descending,
        bool primary)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        MemberExpression member = Expression.Property(param, propertyName);
        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), member.Type);
        LambdaExpression lambda = Expression.Lambda(delegateType, member, param);

        string methodName = primary
            ? (descending ? "OrderByDescending" : "OrderBy")
            : (descending ? "ThenByDescending" : "ThenBy");

        MethodInfo method = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), member.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }
}
