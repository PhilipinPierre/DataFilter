using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;

namespace DataFilter.Core.Engine;

/// <summary>
/// Applies a collection of filter descriptors to an enumerable source using built expressions.
/// </summary>
/// <typeparam name="T">The type of the elements in the source.</typeparam>
public class ReflectionFilterEngine<T> : IFilterEngine<T>
{
    private readonly LogicalOperator _defaultLogicalOperator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionFilterEngine{T}"/> class.
    /// </summary>
    /// <param name="defaultLogicalOperator">The logical operator used to combine multiple descriptors.</param>
    public ReflectionFilterEngine(LogicalOperator defaultLogicalOperator = LogicalOperator.And)
    {
        _defaultLogicalOperator = defaultLogicalOperator;
    }

    /// <inheritdoc />
    public IEnumerable<T> Apply(IEnumerable<T> source, IReadOnlyList<IFilterDescriptor> descriptors)
    {
        if (source == null) return Enumerable.Empty<T>();
        if (descriptors == null || descriptors.Count == 0) return source;

        var expression = FilterExpressionBuilder.BuildExpression<T>(descriptors, _defaultLogicalOperator);
        var compiledPredicate = expression.Compile();

        return source.Where(compiledPredicate);
    }
}
