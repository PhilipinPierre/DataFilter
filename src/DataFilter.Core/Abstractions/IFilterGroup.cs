using DataFilter.Core.Enums;

namespace DataFilter.Core.Abstractions;

/// <summary>
/// Defines a group of filter descriptors combined with a logical operator.
/// </summary>
public interface IFilterGroup : IFilterDescriptor
{
    /// <summary>
    /// Gets the logical operator used to combine the child descriptors.
    /// </summary>
    LogicalOperator LogicalOperator { get; }

    /// <summary>
    /// Gets the child filter descriptors or groups.
    /// </summary>
    IReadOnlyList<IFilterDescriptor> Descriptors { get; }

    /// <summary>
    /// Determines whether the specified item matches the filter group.
    /// </summary>
    /// <param name="item">The item to test.</param>
    /// <returns><c>true</c> if the item matches; otherwise, <c>false</c>.</returns>
    bool IsMatch(object item);
}
