using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;

namespace DataFilter.Core.Models;

/// <summary>
/// Represents a single filter criterion.
/// </summary>
public class FilterDescriptor : IFilterDescriptor
{
    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public FilterOperator Operator { get; }

    /// <inheritdoc />
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterDescriptor"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to filter on.</param>
    /// <param name="filterOperator">The operator used for the comparison.</param>
    /// <param name="value">The value to compare against.</param>
    public FilterDescriptor(string propertyName, FilterOperator filterOperator, object? value)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Operator = filterOperator;
        Value = value;
    }

    /// <inheritdoc />
    public virtual bool IsMatch(object item)
    {
        if (item == null)
        {
            return false;
        }

        // To support dynamic matching without compiling a lambda per item each time,
        // we delegate the matching logic to the FilterExpressionBuilder statically compiled helpers
        // (Implementation can be optimized by compiling the expression once per type, here we use a simple reflection check for demonstration, 
        // or we rely on the engine. For IsMatch on a single object, we can compile a Func).

        // Alternatively, the engine applies the expression to an IEnumerable.
        // For 'IsMatch', we can just use the builder to compile a Func.
        var func = FilterExpressionBuilder.BuildFunc(item.GetType(), this);
        return func(item);
    }
}
