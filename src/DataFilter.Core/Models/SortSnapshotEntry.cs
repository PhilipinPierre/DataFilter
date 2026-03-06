namespace DataFilter.Core.Models;

/// <summary>
/// A flat, serialization-friendly representation of a sort criterion.
/// </summary>
public sealed class SortSnapshotEntry
{
    /// <summary>
    /// Gets or sets the name of the sorted property.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the sort direction is descending.
    /// </summary>
    public bool IsDescending { get; set; }
}
