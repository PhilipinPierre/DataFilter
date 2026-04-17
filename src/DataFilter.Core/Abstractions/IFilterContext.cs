namespace DataFilter.Core.Abstractions;

/// <summary>
/// Encapsulates the current filtering and sorting state.
/// </summary>
public interface IFilterContext
{
    /// <summary>
    /// Gets the active filter descriptors.
    /// </summary>
    IReadOnlyList<IFilterDescriptor> Descriptors { get; }

    /// <summary>
    /// Gets the active sort descriptors, applied in order.
    /// </summary>
    IReadOnlyList<ISortDescriptor> SortDescriptors { get; }

    /// <summary>
    /// Gets or sets the current page number (1-based index).
    /// </summary>
    int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Clears all filter descriptors.
    /// </summary>
    void ClearDescriptors();

    /// <summary>
    /// Replaces all filter descriptors with an ordered list. Unlike column-oriented updates, duplicate
    /// <see cref="IFilterDescriptor.PropertyName"/> values are allowed (e.g. from a compiled DataFilter pipeline model).
    /// </summary>
    /// <param name="descriptors">New descriptors, applied in order.</param>
    void ReplaceDescriptors(IReadOnlyList<IFilterDescriptor> descriptors);

    /// <summary>
    /// Clears all sort criteria.
    /// </summary>
    void ClearSort();
}
