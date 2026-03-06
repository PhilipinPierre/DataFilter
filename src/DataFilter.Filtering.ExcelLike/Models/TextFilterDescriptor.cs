using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;

namespace DataFilter.Filtering.ExcelLike.Models;

/// <summary>
/// A filter descriptor designed for text (string) columns.
/// Supports standard string operators and an optional combined custom criterion using AND/OR.
/// </summary>
public sealed class TextFilterDescriptor : IFilterDescriptor
{
    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public FilterOperator Operator { get; }

    /// <inheritdoc />
    public object? Value { get; }

    /// <summary>
    /// Gets the optional second value used in a composite custom filter.
    /// </summary>
    public object? SecondValue { get; }

    /// <summary>
    /// Gets the optional second operator used in a composite custom filter.
    /// </summary>
    public FilterOperator? SecondOperator { get; }

    /// <summary>
    /// Gets the logical operator combining the two criteria in a composite filter.
    /// </summary>
    public Core.Enums.LogicalOperator CompositeLogical { get; }

    /// <summary>
    /// Initializes a new <see cref="TextFilterDescriptor"/> with a single criterion.
    /// </summary>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="filterOperator">The operator to apply.</param>
    /// <param name="value">The value to compare against.</param>
    public TextFilterDescriptor(string propertyName, FilterOperator filterOperator, object? value)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Operator = filterOperator;
        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="TextFilterDescriptor"/> with two combined criteria (custom filter).
    /// </summary>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="firstOperator">The first operator.</param>
    /// <param name="firstValue">The first value.</param>
    /// <param name="logical">The logical operator combining both criteria.</param>
    /// <param name="secondOperator">The second operator.</param>
    /// <param name="secondValue">The second value.</param>
    public TextFilterDescriptor(
        string propertyName,
        FilterOperator firstOperator,
        object? firstValue,
        Core.Enums.LogicalOperator logical,
        FilterOperator secondOperator,
        object? secondValue)
        : this(propertyName, firstOperator, firstValue)
    {
        SecondOperator = secondOperator;
        SecondValue = secondValue;
        CompositeLogical = logical;
    }

    /// <inheritdoc />
    public bool IsMatch(object item)
    {
        if (item == null) return false;

        bool first = EvaluateSingle(item, Operator, Value);

        if (SecondOperator == null)
        {
            return first;
        }

        bool second = EvaluateSingle(item, SecondOperator.Value, SecondValue);

        return CompositeLogical == Core.Enums.LogicalOperator.And
            ? first && second
            : first || second;
    }

    private bool EvaluateSingle(object item, FilterOperator op, object? val)
    {
        Type type = item.GetType();
        System.Reflection.PropertyInfo? prop = type.GetProperty(PropertyName);
        if (prop == null) return false;

        string? propValue = prop.GetValue(item)?.ToString();
        string? filterValue = val?.ToString();

        if (propValue == null)
        {
            return op == FilterOperator.IsNull;
        }

        return op switch
        {
            FilterOperator.Equals => propValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotEquals => !propValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Contains => filterValue != null && propValue.Contains(filterValue, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotContains => filterValue == null || !propValue.Contains(filterValue, StringComparison.OrdinalIgnoreCase),
            FilterOperator.StartsWith => filterValue != null && propValue.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase),
            FilterOperator.EndsWith => filterValue != null && propValue.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase),
            FilterOperator.IsNull => false,
            FilterOperator.IsNotNull => true,
            _ => throw new NotSupportedException($"Operator {op} is not supported for text filters.")
        };
    }
}
