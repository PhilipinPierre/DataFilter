using DataFilter.Core.Abstractions;

namespace DataFilter.Core.Events;

/// <summary>
/// Provides data for events related to filter state changes.
/// </summary>
public class FilterChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current filter context at the time of the event.
    /// </summary>
    public IFilterContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterChangedEventArgs"/> class.
    /// </summary>
    /// <param name="context">The filter context.</param>
    public FilterChangedEventArgs(IFilterContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
