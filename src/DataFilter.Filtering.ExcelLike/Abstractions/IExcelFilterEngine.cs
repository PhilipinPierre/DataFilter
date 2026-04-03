using DataFilter.Core.Abstractions;

namespace DataFilter.Filtering.ExcelLike.Abstractions;

/// <summary>
/// A specialized engine that supports applying Excel-like filters (non-generic).
/// </summary>
public interface IExcelFilterEngine : IFilterEngine
{
    /// <summary>
    /// Gets the distinct values extractor used by the engine.
    /// </summary>
    IDistinctValuesExtractor DistinctValuesExtractor { get; }
}

/// <summary>
/// A specialized engine that supports applying Excel-like filters.
/// </summary>
/// <typeparam name="T">The type of the elements in the source.</typeparam>
public interface IExcelFilterEngine<T> : IExcelFilterEngine, IFilterEngine<T>
{
}
