using System.Collections;
using System.Linq.Expressions;
using DataFilter.Filtering.ExcelLike.Abstractions;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Default implementation of <see cref="IDistinctValuesExtractor"/> that extracts and orders distinct values.
/// </summary>
public class DistinctValuesExtractor : IDistinctValuesExtractor
{
    /// <inheritdoc />
    public IEnumerable<object> Extract(IEnumerable source, Type elementType, string propertyName)
    {
        if (source == null || string.IsNullOrWhiteSpace(propertyName))
        {
            return Enumerable.Empty<object>();
        }

        var propertyInfo = elementType.GetProperty(propertyName);
        if (propertyInfo == null)
        {
            var objParameter = Expression.Parameter(typeof(object), "obj");
            var castParameter = Expression.Convert(objParameter, elementType);
            Expression current = castParameter;
            foreach (var prop in propertyName.Split('.'))
            {
                current = Expression.PropertyOrField(current, prop);
            }

            if (current.Type.IsValueType)
            {
                current = Expression.Convert(current, typeof(object));
            }
            else if (current.Type != typeof(object))
            {
                current = Expression.Convert(current, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object>>(current, objParameter);
            var compiledFunc = lambda.Compile();

            var distinctMap = new List<object>();
            foreach (var x in source)
            {
                if (x == null) continue;
                var v = compiledFunc(x);
                if (v != null)
                {
                    distinctMap.Add(v);
                }
            }

            distinctMap = distinctMap.Distinct().ToList();
            return Sort(distinctMap);
        }

        var results = new List<object>();
        foreach (var x in source)
        {
            if (x == null) continue;
            var v = propertyInfo.GetValue(x);
            if (v != null)
            {
                results.Add(v);
            }
        }

        var distinctResults = results.Distinct().ToList();
        return Sort(distinctResults);
    }

    /// <inheritdoc />
    public IEnumerable<object> Extract<T>(IEnumerable<T> source, string propertyName)
    {
        return Extract(source!, typeof(T), propertyName);
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
