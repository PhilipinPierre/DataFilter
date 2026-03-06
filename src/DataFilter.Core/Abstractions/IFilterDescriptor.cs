using DataFilter.Core.Enums;

namespace DataFilter.Core.Abstractions;

/// <summary>
/// Defines a single filter criterion.
/// </summary>
public interface IFilterDescriptor
{
    /// <summary>
    /// Gets the name of the property to filter on.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the operator used for the comparison.
    /// </summary>
    FilterOperator Operator { get; }

    /// <summary>
    /// Gets the value to compare against.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Determines whether the specified item matches the filter criterion.
    /// </summary>
    /// <param name="item">The item to test.</param>
    /// <returns><c>true</c> if the item matches; otherwise, <c>false</c>.</returns>
    bool IsMatch(object item);
}
