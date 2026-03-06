using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// Creates filter descriptors based on the CLR type of the target property.
/// <list type="bullet">
///   <item><see cref="string"/> → <see cref="TextFilterDescriptor"/></item>
///   <item>Numeric types (int, double, decimal, float, long) → <see cref="NumericFilterDescriptor"/></item>
///   <item><see cref="DateTime"/> / <see cref="DateTimeOffset"/> → <see cref="DateFilterDescriptor"/></item>
/// </list>
/// </summary>
public sealed class FilterDescriptorFactory : IFilterDescriptorFactory
{
    /// <inheritdoc />
    public IFilterDescriptor CreateDescriptor(string propertyName, Type clrType)
    {
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
        if (clrType == null) throw new ArgumentNullException(nameof(clrType));

        Type underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (underlyingType == typeof(string))
        {
            return new TextFilterDescriptor(propertyName, FilterOperator.Contains, null);
        }

        if (IsNumericType(underlyingType))
        {
            return new NumericFilterDescriptor(propertyName, FilterOperator.Equals, null);
        }

        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
        {
            return new DateFilterDescriptor(propertyName, FilterOperator.Equals, DateTime.Today);
        }

        // Default fallback to a standard Equals descriptor
        return new Core.Models.FilterDescriptor(propertyName, FilterOperator.Equals, null);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int)
            || type == typeof(long)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(decimal)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong);
    }
}
