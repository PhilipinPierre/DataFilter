using DataFilter.Filtering.ExcelLike.Abstractions;
using System.Linq.Expressions;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Default implementation of <see cref="IDistinctValuesExtractor"/> that extracts and orders distinct values.
/// </summary>
public class DistinctValuesExtractor : IDistinctValuesExtractor
{
    /// <inheritdoc />
    public IEnumerable<object> Extract<T>(IEnumerable<T> source, string propertyName)
    {
        if (source == null || string.IsNullOrWhiteSpace(propertyName))
        {
            return Enumerable.Empty<object>();
        }

        var propertyInfo = typeof(T).GetProperty(propertyName);
        if (propertyInfo == null)
        {
            // If the property is nested or complex, consider building a dynamic expression (similar to FilterExpressionBuilder)
            // For simplicity in this implementation, we handle direct properties.
            // If we need nested properties, we'd compile an Expression Tree. Let's do the rigorous Expression Tree way:
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression current = parameter;
            foreach (var prop in propertyName.Split('.'))
            {
                current = Expression.PropertyOrField(current, prop);
            }

            // convert to object
            if (current.Type.IsValueType)
            {
                current = Expression.Convert(current, typeof(object));
            }

            var lambda = Expression.Lambda<Func<T, object>>(current, parameter);
            var compiledFunc = lambda.Compile();

            var distinctMap = source.Select(compiledFunc)
                                    .Where(x => x != null)
                                    .Distinct()
                                    .ToList();

            return Sort(distinctMap);
        }

        var results = source.Select(x => propertyInfo.GetValue(x))
                            .Where(x => x != null)
                            .Distinct()
                            .ToList();

        return Sort(results!);
    }

    private static IEnumerable<object> Sort(List<object> values)
    {
        if (values.Count == 0) return values;

        var firstValidItem = values.FirstOrDefault(x => x != null);
        if (firstValidItem == null) return values;

        var type = firstValidItem.GetType();

        if (type == typeof(string))
            return values.OrderBy(x => (string)x);

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            return values.OrderBy(x => Convert.ToInt64(x));

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            return values.OrderBy(x => Convert.ToDecimal(x));

        if (type == typeof(DateTime))
            return values.OrderBy(x => (DateTime)x);

        return values.OrderBy(x => x?.ToString());
    }
}
