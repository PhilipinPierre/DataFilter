using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using DataFilter.Core.Engine;
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
            var valueType = UnwrapNullableType(current.Type);

            return ExtractDistinct(
                source,
                item =>
                {
                    if (item == null) return null;
                    return compiledFunc(item);
                },
                valueType);
        }

        var propertyValueType = UnwrapNullableType(propertyInfo.PropertyType);
        return ExtractDistinct(
            source,
            item =>
            {
                if (item == null) return null;
                return propertyInfo.GetValue(item);
            },
            propertyValueType);
    }

    /// <inheritdoc />
    public IEnumerable<object> Extract<T>(IEnumerable<T> source, string propertyName)
    {
        return Extract(source!, typeof(T), propertyName);
    }

    private static IEnumerable<object> ExtractDistinct(
        IEnumerable source,
        Func<object?, object?> getPropertyValue,
        Type propertyValueType)
    {
        var hasNull = false;
        var seen = new HashSet<object>();
        var values = new List<object>();
        var isDateColumn = DateDistinctHelper.IsCalendarDateType(propertyValueType);
        var isTimeColumn = TimeDistinctHelper.IsTimeOfDayType(propertyValueType);

        foreach (var item in source)
        {
            var value = getPropertyValue(item);
            if (value == null)
            {
                hasNull = true;
                continue;
            }

            var canonical = isDateColumn
                ? DateDistinctHelper.CanonicalizeDistinctValue(value, propertyValueType)
                : isTimeColumn
                    ? TimeDistinctHelper.CanonicalizeDistinctValue(value, propertyValueType)
                    : value;

            if (seen.Add(canonical))
            {
                values.Add(canonical);
            }
        }

        var sorted = Sort(values, propertyValueType).ToList();

        if (hasNull)
        {
            sorted.Add(null!);
        }

        return sorted;
    }

    private static IEnumerable<object> Sort(List<object> values, Type propertyValueType)
    {
        if (values.Count == 0) return values;

        propertyValueType = UnwrapNullableType(propertyValueType);

        if (DateDistinctHelper.IsCalendarDateType(propertyValueType))
            return values.OrderBy(x => x, Comparer<object>.Create(DateDistinctHelper.CompareCalendarDates));

        if (TimeDistinctHelper.IsTimeOfDayType(propertyValueType))
            return values.OrderBy(x => x, Comparer<object>.Create(TimeDistinctHelper.CompareTimeOfDay));

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

        if (type == typeof(DateTimeOffset))
            return values.OrderBy(x => (DateTimeOffset)x);

        return values.OrderBy(x => x?.ToString());
    }

    private static Type UnwrapNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
