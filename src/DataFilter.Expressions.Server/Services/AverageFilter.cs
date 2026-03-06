using DataFilter.Expressions.Server.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace DataFilter.Expressions.Server.Services;

/// <summary>
/// Filters an <see cref="IQueryable{T}"/> retaining only items whose numeric property value
/// is strictly above or below the column average.
/// Note: This computes the average in-memory from the materialised query, 
/// then filters. For large datasets, consider computing the average server-side first.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class AverageFilter<T> : IAverageFilter<T>
{
    /// <inheritdoc />
    public IQueryable<T> AboveAverage(IQueryable<T> query, string propertyName)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

        double average = ComputeAverage(query, propertyName);
        return ApplyComparison(query, propertyName, average, aboveAverage: true);
    }

    /// <inheritdoc />
    public IQueryable<T> BelowAverage(IQueryable<T> query, string propertyName)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

        double average = ComputeAverage(query, propertyName);
        return ApplyComparison(query, propertyName, average, aboveAverage: false);
    }

    private static double ComputeAverage(IQueryable<T> query, string propertyName)
    {
        PropertyInfo prop = typeof(T).GetProperty(propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        // Materialise to compute average; keeping this in AsEnumerable for broad compatibility.
        double average = query
            .AsEnumerable()
            .Select(item => Convert.ToDouble(prop.GetValue(item)))
            .Average();

        return average;
    }

    private static IQueryable<T> ApplyComparison(
        IQueryable<T> query,
        string propertyName,
        double average,
        bool aboveAverage)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        MemberExpression member = Expression.Property(param, propertyName);

        // Convert the property value to double for comparison
        UnaryExpression convertedMember = Expression.Convert(member, typeof(double));
        ConstantExpression avgConst = Expression.Constant(average, typeof(double));

        BinaryExpression comparison = aboveAverage
            ? Expression.GreaterThan(convertedMember, avgConst)
            : Expression.LessThan(convertedMember, avgConst);

        Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(comparison, param);
        return query.Where(predicate);
    }
}
