using DataFilter.Core.Abstractions;

namespace DataFilter.Filtering.ExcelLike.Abstractions;

/// <summary>
/// Creates the appropriate <see cref="IFilterDescriptor"/> subtype
/// based on the CLR type of a column's data.
/// </summary>
public interface IFilterDescriptorFactory
{
    /// <summary>
    /// Creates a filter descriptor appropriate for the given CLR type.
    /// </summary>
    /// <param name="propertyName">The property to filter.</param>
    /// <param name="clrType">The CLR type of the property (e.g. <see cref="string"/>, <see cref="int"/>, <see cref="System.DateTime"/>).</param>
    /// <returns>A descriptor instance ready to be configured.</returns>
    IFilterDescriptor CreateDescriptor(string propertyName, System.Type clrType);
}
