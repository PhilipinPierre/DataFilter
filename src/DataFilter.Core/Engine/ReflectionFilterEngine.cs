using System.Collections;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;

namespace DataFilter.Core.Engine;

/// <summary>
/// Applies a collection of filter descriptors to an enumerable source using compiled predicates over <see cref="object"/>.
/// </summary>
public class ReflectionFilterEngine : IFilterEngine
{
    private readonly LogicalOperator _defaultLogicalOperator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionFilterEngine"/> class.
    /// </summary>
    /// <param name="defaultLogicalOperator">The logical operator used to combine multiple descriptors.</param>
    public ReflectionFilterEngine(LogicalOperator defaultLogicalOperator = LogicalOperator.And)
    {
        _defaultLogicalOperator = defaultLogicalOperator;
    }

    /// <inheritdoc />
    public IEnumerable Apply(IEnumerable source, Type elementType, IReadOnlyList<IFilterDescriptor> descriptors)
    {
        if (source == null) return Enumerable.Empty<object>();
        if (descriptors == null || descriptors.Count == 0) return source;

        var predicate = FilterExpressionBuilder.BuildCombinedFunc(elementType, descriptors, _defaultLogicalOperator);
        return ApplyCore(source, predicate);
    }

    private static IEnumerable ApplyCore(IEnumerable source, Func<object, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item!))
            {
                yield return item;
            }
        }
    }
}

/// <summary>
/// Applies a collection of filter descriptors to an enumerable source using built expressions.
/// </summary>
/// <typeparam name="T">The type of the elements in the source.</typeparam>
public class ReflectionFilterEngine<T> : IFilterEngine<T>
{
    private readonly ReflectionFilterEngine _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionFilterEngine{T}"/> class.
    /// </summary>
    /// <param name="defaultLogicalOperator">The logical operator used to combine multiple descriptors.</param>
    public ReflectionFilterEngine(LogicalOperator defaultLogicalOperator = LogicalOperator.And)
    {
        _inner = new ReflectionFilterEngine(defaultLogicalOperator);
    }

    /// <inheritdoc />
    public IEnumerable<T> Apply(IEnumerable<T> source, IReadOnlyList<IFilterDescriptor> descriptors)
    {
        foreach (var item in _inner.Apply(source!, typeof(T), descriptors))
        {
            yield return (T)item!;
        }
    }
}
