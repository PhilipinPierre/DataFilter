using DataFilter.Core.Abstractions;

namespace DataFilter.Core.Models;

/// <summary>
/// Represents a sort criterion applied to a data source.
/// </summary>
public sealed class SortDescriptor : ISortDescriptor
{
    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public bool IsDescending { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SortDescriptor"/>.
    /// </summary>
    /// <param name="propertyName">The name of the property to sort by.</param>
    /// <param name="isDescending">Whether to sort descending.</param>
    public SortDescriptor(string propertyName, bool isDescending = false)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        IsDescending = isDescending;
    }
}
