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
