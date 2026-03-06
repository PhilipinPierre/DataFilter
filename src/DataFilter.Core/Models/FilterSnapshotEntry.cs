namespace DataFilter.Core.Models;

/// <summary>
/// A flat, serialization-friendly representation of a single filter criterion.
/// This POCO is intentionally simple so that consuming applications can
/// serialize it using any preferred format (JSON, XML, binary, etc.).
/// </summary>
public sealed class FilterSnapshotEntry
{
    /// <summary>
    /// Gets or sets the name of the filtered property.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operator name (matches <see cref="Enums.FilterOperator"/> enum names).
    /// </summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter value. May be null for operators such as IsNull/IsNotNull.
    /// For <see cref="Enums.FilterOperator.Between"/>, this is a <see cref="RangeValue"/>.
    /// For <see cref="Enums.FilterOperator.In"/> / <see cref="Enums.FilterOperator.NotIn"/>, this is an array.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the logical operator combining child entries (used when this entry represents a group).
    /// Null when this entry is a leaf criterion.
    /// </summary>
    public string? LogicalOperator { get; set; }

    /// <summary>
    /// Gets or sets the child entries when this entry represents a filter group.
    /// Null or empty for leaf criteria.
    /// </summary>
    public List<FilterSnapshotEntry>? Children { get; set; }

    /// <summary>
    /// Gets a value indicating whether this entry is a filter group (has children).
    /// </summary>
    public bool IsGroup => Children is { Count: > 0 };
}
