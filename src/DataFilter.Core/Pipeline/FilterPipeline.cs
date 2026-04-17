using DataFilter.Core.Enums;

namespace DataFilter.Core.Pipeline;

/// <summary>
/// Ordered filter pipeline: root-level nodes combined with <see cref="RootCombineOperator"/>.
/// </summary>
public sealed class FilterPipeline
{
    /// <summary>
    /// How top-level nodes are combined when building a predicate (default: AND, typical for grids).
    /// </summary>
    public LogicalOperator RootCombineOperator { get; set; } = LogicalOperator.And;

    /// <summary>
    /// Ordered root nodes (groups or criteria).
    /// </summary>
    public List<FilterPipelineNode> RootNodes { get; } = new();
}
