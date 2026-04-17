namespace DataFilter.Core.Pipeline;

/// <summary>
/// A leaf filter criterion (same information as a <see cref="Models.FilterSnapshotEntry"/> leaf).
/// </summary>
public sealed class CriterionPipelineNode : FilterPipelineNode
{
    /// <summary>
    /// Target property path (e.g. column name).
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="Enums.FilterOperator"/> enum name (e.g. Equals, Contains).
    /// </summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// Operand value; shape matches <see cref="Models.FilterSnapshotEntry.Value"/>.
    /// </summary>
    public object? Value { get; set; }

    public CriterionPipelineNode(string? id = null)
        : base(id)
    {
    }
}
