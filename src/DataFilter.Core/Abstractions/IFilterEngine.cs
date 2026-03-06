namespace DataFilter.Core.Abstractions;

/// <summary>
/// Applies a collection of filter descriptors to an enumerable source.
/// </summary>
/// <typeparam name="T">The type of the elements in the source.</typeparam>
public interface IFilterEngine<T>
{
    /// <summary>
    /// Applies the specified filters to the source collection.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="descriptors">The filter descriptors to apply.</param>
    /// <returns>A filtered collection.</returns>
    IEnumerable<T> Apply(IEnumerable<T> source, IReadOnlyList<IFilterDescriptor> descriptors);
}
