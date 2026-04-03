using System.Collections;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Abstractions;

namespace DataFilter.Filtering.ExcelLike.Services;

/// <summary>
/// An engine capable of combining standard filters with Excel-like functionality.
/// </summary>
public class ExcelFilterEngine : ReflectionFilterEngine, IExcelFilterEngine
{
    /// <inheritdoc />
    public IDistinctValuesExtractor DistinctValuesExtractor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelFilterEngine"/> class.
    /// </summary>
    /// <param name="extractor">The distinct values extractor. If null, a default instance is provided.</param>
    /// <param name="defaultLogicalOperator">The logical operator used to combine multiple descriptors.</param>
    public ExcelFilterEngine(IDistinctValuesExtractor? extractor = null, LogicalOperator defaultLogicalOperator = LogicalOperator.And)
        : base(defaultLogicalOperator)
    {
        DistinctValuesExtractor = extractor ?? new DistinctValuesExtractor();
    }
}

/// <summary>
/// An engine capable of combining standard filters with Excel-like functionality.
/// </summary>
/// <typeparam name="T">The type of the target items.</typeparam>
public class ExcelFilterEngine<T> : ExcelFilterEngine, IExcelFilterEngine<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelFilterEngine{T}"/> class.
    /// </summary>
    /// <param name="extractor">The distinct values extractor. If null, a default instance is provided.</param>
    /// <param name="defaultLogicalOperator">The logical operator used to combine multiple descriptors.</param>
    public ExcelFilterEngine(IDistinctValuesExtractor? extractor = null, LogicalOperator defaultLogicalOperator = LogicalOperator.And)
        : base(extractor, defaultLogicalOperator)
    {
    }

    /// <inheritdoc />
    public IEnumerable<T> Apply(IEnumerable<T> source, IReadOnlyList<IFilterDescriptor> descriptors)
    {
        foreach (var item in Apply((IEnumerable)source!, typeof(T), descriptors))
        {
            yield return (T)item!;
        }
    }
}
