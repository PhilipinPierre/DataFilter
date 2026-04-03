namespace DataFilter.Filtering.ExcelLike.Abstractions;

/// <summary>
/// Extracts distinct values for a specific property from a collection.
/// </summary>
public interface IDistinctValuesExtractor
{
    /// <summary>
    /// Extracts and sorts the distinct values of a property from a non-generic sequence.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="elementType">The type of items (homogeneous sequence).</param>
    /// <param name="propertyName">The name of the property to extract values from.</param>
    /// <returns>An ordered collection of distinct values.</returns>
    IEnumerable<object> Extract(System.Collections.IEnumerable source, Type elementType, string propertyName);

    /// <summary>
    /// Extracts and sorts the distinct values of a property from a sequence.
    /// </summary>
    /// <typeparam name="T">The type of items in the sequence.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="propertyName">The name of the property to extract values from.</param>
    /// <returns>An ordered collection of distinct values.</returns>
    IEnumerable<object> Extract<T>(IEnumerable<T> source, string propertyName);
}
