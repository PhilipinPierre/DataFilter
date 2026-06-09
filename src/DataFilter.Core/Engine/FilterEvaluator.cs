using DataFilter.Core.Enums;
using System.Globalization;

namespace DataFilter.Core.Engine;

/// <summary>
/// Service responsible for evaluating filters against objects.
/// </summary>
public class FilterEvaluator : IFilterEvaluator
{
    /// <inheritdoc />
    public bool EvaluateOperator(object? itemValue, FilterOperator op, object? v1, object? v2 = null)
    {
        if (itemValue == null)
        {
            return op == FilterOperator.Equals ? v1 == null : op == FilterOperator.NotEquals && v1 != null;
        }

        // Support string-based comparisons (case-insensitive)
        if (itemValue is string s)
        {
            string s1 = v1?.ToString() ?? string.Empty;

            if (WildcardPattern.ContainsWildcard(s1))
            {
                bool m = WildcardPattern.IsMatch(s, s1);
                return op switch
                {
                    FilterOperator.Equals => m,
                    FilterOperator.NotEquals => !m,
                    FilterOperator.Contains => m,
                    FilterOperator.NotContains => !m,
                    FilterOperator.StartsWith => m,
                    FilterOperator.EndsWith => m,
                    _ => false
                };
            }

            return op switch
            {
                FilterOperator.Equals => s.Equals(s1, StringComparison.OrdinalIgnoreCase),
                FilterOperator.NotEquals => !s.Equals(s1, StringComparison.OrdinalIgnoreCase),
                FilterOperator.Contains => s.IndexOf(s1, StringComparison.OrdinalIgnoreCase) >= 0,
                FilterOperator.NotContains => s.IndexOf(s1, StringComparison.OrdinalIgnoreCase) < 0,
                FilterOperator.StartsWith => s.StartsWith(s1, StringComparison.OrdinalIgnoreCase),
                FilterOperator.EndsWith => s.EndsWith(s1, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        // Support numeric/comparable comparisons
        if (DateDistinctHelper.TryGetCalendarParts(itemValue, out _, out _, out _))
        {
            return EvaluateCalendarDateOperator(itemValue, op, v1, v2);
        }

        if (TimeDistinctHelper.TryGetTimeParts(itemValue, out _, out _, out _, out _))
        {
            return EvaluateTimeOfDayOperator(itemValue, op, v1, v2);
        }

        if (itemValue is IComparable comparable)
        {
            try
            {
                Type targetType = itemValue.GetType();
                
                // Convert comparison values to target type using InvariantCulture for numeric types
                object? convertedV1 = ConvertValue(v1, targetType);

                int cmp1 = convertedV1 != null ? comparable.CompareTo(convertedV1) : -1;

                return op switch
                {
                    FilterOperator.Equals => convertedV1 != null && cmp1 == 0,
                    FilterOperator.NotEquals => convertedV1 == null || cmp1 != 0,
                    FilterOperator.GreaterThan => convertedV1 != null && cmp1 > 0,
                    FilterOperator.GreaterThanOrEqual => convertedV1 != null && cmp1 >= 0,
                    FilterOperator.LessThan => convertedV1 != null && cmp1 < 0,
                    FilterOperator.LessThanOrEqual => convertedV1 != null && cmp1 <= 0,
                    FilterOperator.Between => EvaluateBetween(comparable, targetType, v1, v2),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static bool EvaluateCalendarDateOperator(object itemValue, FilterOperator op, object? v1, object? v2)
    {
        var convertedV1 = ConvertCalendarFilterValue(v1);
        var convertedV2 = ConvertCalendarFilterValue(v2);

        if (op == FilterOperator.Between)
        {
            if (convertedV1 == null || convertedV2 == null) return false;
            return DateDistinctHelper.CompareCalendarDates(itemValue, convertedV1) >= 0
                && DateDistinctHelper.CompareCalendarDates(itemValue, convertedV2) <= 0;
        }

        if (convertedV1 == null)
        {
            return op == FilterOperator.Equals;
        }

        var cmp = DateDistinctHelper.CompareCalendarDates(itemValue, convertedV1);
        return op switch
        {
            FilterOperator.Equals => cmp == 0,
            FilterOperator.NotEquals => cmp != 0,
            FilterOperator.GreaterThan => cmp > 0,
            FilterOperator.GreaterThanOrEqual => cmp >= 0,
            FilterOperator.LessThan => cmp < 0,
            FilterOperator.LessThanOrEqual => cmp <= 0,
            _ => false
        };
    }

    private static bool EvaluateTimeOfDayOperator(object itemValue, FilterOperator op, object? v1, object? v2)
    {
        var convertedV1 = ConvertTimeFilterValue(v1);
        var convertedV2 = ConvertTimeFilterValue(v2);

        if (op == FilterOperator.Between)
        {
            if (convertedV1 == null || convertedV2 == null) return false;
            return TimeDistinctHelper.CompareTimeOfDay(itemValue, convertedV1) >= 0
                && TimeDistinctHelper.CompareTimeOfDay(itemValue, convertedV2) <= 0;
        }

        if (convertedV1 == null)
        {
            return op == FilterOperator.Equals;
        }

        var cmp = TimeDistinctHelper.CompareTimeOfDay(itemValue, convertedV1);
        return op switch
        {
            FilterOperator.Equals => cmp == 0,
            FilterOperator.NotEquals => cmp != 0,
            FilterOperator.GreaterThan => cmp > 0,
            FilterOperator.GreaterThanOrEqual => cmp >= 0,
            FilterOperator.LessThan => cmp < 0,
            FilterOperator.LessThanOrEqual => cmp <= 0,
            _ => false
        };
    }

    private static object? ConvertTimeFilterValue(object? value)
    {
        if (value == null)
            return null;

        if (TimeDistinctHelper.TryGetTimeParts(value, out _, out _, out _, out _))
            return value;

        if (value is string s && TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        return null;
    }

    private static object? ConvertCalendarFilterValue(object? value)
    {
        if (value == null)
            return null;

        if (DateDistinctHelper.TryGetCalendarParts(value, out _, out _, out _))
            return value;

        if (value is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed.Date;

        return null;
    }

    private object? ConvertValue(object? value, Type targetType)
    {
        if (value == null) return null;
        if (value.GetType() == targetType) return value;

        try
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private bool EvaluateBetween(IComparable comparable, Type targetType, object? v1, object? v2)
    {
        object? convertedV1 = ConvertValue(v1, targetType);
        object? convertedV2 = ConvertValue(v2, targetType);

        if (convertedV1 == null || convertedV2 == null) return false;

        return comparable.CompareTo(convertedV1) >= 0 && comparable.CompareTo(convertedV2) <= 0;
    }
}
