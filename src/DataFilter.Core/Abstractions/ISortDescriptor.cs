namespace DataFilter.Core.Abstractions;

/// <summary>
/// Defines a sort criterion applied to a data source.
/// </summary>
public interface ISortDescriptor
{
    /// <summary>
    /// Gets the name of the property to sort by.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets a value indicating whether the sort direction is descending.
    /// </summary>
    bool IsDescending { get; }
}
