using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;

namespace DataFilter.Filtering.ExcelLike.Models;

/// <summary>
/// A filter descriptor designed for numeric columns (int, double, decimal, float, long).
/// Supports comparison operators and the Between range operator.
/// </summary>
public sealed class NumericFilterDescriptor : IFilterDescriptor
{
    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public FilterOperator Operator { get; }

    /// <inheritdoc />
    public object? Value { get; }

    /// <summary>
    /// Initializes a new <see cref="NumericFilterDescriptor"/>.
    /// </summary>
    /// <param name="propertyName">The name of the numeric property to filter.</param>
    /// <param name="filterOperator">The operator to apply.</param>
    /// <param name="value">
    /// The value to compare against. For <see cref="FilterOperator.Between"/>, pass a <see cref="RangeValue"/>.
    /// </param>
    public NumericFilterDescriptor(string propertyName, FilterOperator filterOperator, object? value)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Operator = filterOperator;
        Value = value;
    }

    /// <summary>
    /// Creates a Between descriptor with a min/max range.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="min">The inclusive minimum.</param>
    /// <param name="max">The inclusive maximum.</param>
    public static NumericFilterDescriptor Between(string propertyName, object min, object max)
    {
        return new NumericFilterDescriptor(propertyName, FilterOperator.Between, new RangeValue(min, max));
    }

    /// <inheritdoc />
    public bool IsMatch(object item)
    {
        if (item == null) return false;

        Type type = item.GetType();
        System.Reflection.PropertyInfo? prop = type.GetProperty(PropertyName);
        if (prop == null) return false;

        object? rawValue = prop.GetValue(item);

        if (rawValue == null)
        {
            return Operator == FilterOperator.IsNull;
        }

        if (Operator == FilterOperator.IsNotNull) return true;

        if (Operator == FilterOperator.Between)
        {
            if (Value is not RangeValue range) return false;
            double doubleValue = Convert.ToDouble(rawValue);
            double min = Convert.ToDouble(range.Min);
            double max = Convert.ToDouble(range.Max);
            return doubleValue >= min && doubleValue <= max;
        }

        double left = Convert.ToDouble(rawValue);
        double right = Convert.ToDouble(Value);

        return Operator switch
        {
            FilterOperator.Equals => left == right,
            FilterOperator.NotEquals => left != right,
            FilterOperator.GreaterThan => left > right,
            FilterOperator.GreaterThanOrEqual => left >= right,
            FilterOperator.LessThan => left < right,
            FilterOperator.LessThanOrEqual => left <= right,
            _ => throw new NotSupportedException($"Operator {Operator} is not supported for numeric filters.")
        };
    }
}
