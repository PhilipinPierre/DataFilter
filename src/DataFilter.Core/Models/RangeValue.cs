namespace DataFilter.Core.Models;

/// <summary>
/// Represents a range with minimum and maximum bounds, used with the <see cref="Enums.FilterOperator.Between"/> operator.
/// </summary>
public sealed class RangeValue
{
    /// <summary>
    /// Gets the lower bound of the range (inclusive).
    /// </summary>
    public object? Min { get; }

    /// <summary>
    /// Gets the upper bound of the range (inclusive).
    /// </summary>
    public object? Max { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RangeValue"/>.
    /// </summary>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    public RangeValue(object? min, object? max)
    {
        Min = min;
        Max = max;
    }
}
