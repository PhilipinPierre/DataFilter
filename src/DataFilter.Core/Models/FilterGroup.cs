using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;

namespace DataFilter.Core.Models;

/// <summary>
/// Represents a group of filter descriptors combined with a logical operator.
/// </summary>
public class FilterGroup : IFilterGroup
{
    private readonly List<IFilterDescriptor> _descriptors = new();

    /// <inheritdoc />
    public LogicalOperator LogicalOperator { get; }

    /// <inheritdoc />
    /// <remarks>
    /// For column-scoped Excel-like snapshots, this must match the column key used by <see cref="FilterContext.AddOrUpdateDescriptor"/>.
    /// </remarks>
    public string PropertyName { get; }

    /// <inheritdoc />
    public FilterOperator Operator => FilterOperator.Equals;

    /// <inheritdoc />
    public object? Value => null;

    /// <inheritdoc />
    public IReadOnlyList<IFilterDescriptor> Descriptors => _descriptors.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterGroup"/> class.
    /// </summary>
    /// <param name="logicalOperator">The logical operator to use for combining descriptors.</param>
    /// <param name="propertyName">Column or scope key for this group (must be unique per column when stored in <see cref="FilterContext"/>).</param>
    public FilterGroup(LogicalOperator logicalOperator, string propertyName = "")
    {
        LogicalOperator = logicalOperator;
        PropertyName = propertyName ?? string.Empty;
    }

    /// <summary>
    /// Adds a descriptor to the group.
    /// </summary>
    /// <param name="descriptor">The descriptor to add.</param>
    public void Add(IFilterDescriptor descriptor)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        _descriptors.Add(descriptor);
    }

    /// <inheritdoc />
    public bool IsMatch(object item)
    {
        if (item == null || _descriptors.Count == 0)
        {
            return true; // Empty group matches everything
        }

        if (LogicalOperator == LogicalOperator.And)
        {
            return _descriptors.All(d => d.IsMatch(item));
        }
        else
        {
            return _descriptors.Any(d => d.IsMatch(item));
        }
    }
}
