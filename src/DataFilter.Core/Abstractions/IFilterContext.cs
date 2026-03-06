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
    /// Gets the current page number (1-based index).
    /// </summary>
    int Page { get; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    int PageSize { get; }
}
