using DataFilter.Core.Enums;

namespace DataFilter.Core.Pipeline;

/// <summary>
/// A named group of child nodes combined with <see cref="CombineOperator"/>.
/// </summary>
public sealed class GroupPipelineNode : FilterPipelineNode
{
    /// <summary>
    /// User-visible label (independent of the internal scope key used when compiling to <see cref="Models.FilterGroup"/>).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// How children are combined inside this group.
    /// </summary>
    public LogicalOperator CombineOperator { get; set; } = LogicalOperator.And;

    public List<FilterPipelineNode> Children { get; } = new();

    public GroupPipelineNode(string? id = null)
        : base(id)
    {
    }
}
