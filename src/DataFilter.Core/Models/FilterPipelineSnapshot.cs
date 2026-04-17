using DataFilter.Core.Enums;

namespace DataFilter.Core.Models;

/// <summary>
/// JSON-friendly snapshot of a <see cref="Pipeline.FilterPipeline"/> (schema versioned for persistence and presets).
/// </summary>
public sealed class FilterPipelineSnapshot
{
    /// <summary>
    /// Increment when the shape of this contract changes.
    /// </summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// <see cref="LogicalOperator"/> name for combining root nodes (e.g. And, Or).
    /// </summary>
    public string RootCombineOperator { get; set; } = nameof(LogicalOperator.And);

    /// <summary>
    /// Ordered root nodes.
    /// </summary>
    public List<FilterPipelineNodeDto> Nodes { get; set; } = new();
}

/// <summary>
/// Serializable node: <see cref="Kind"/> discriminates criterion vs group.
/// </summary>
public sealed class FilterPipelineNodeDto
{
    /// <summary>
    /// <c>criterion</c> or <c>group</c>.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    // Criterion
    public string? PropertyName { get; set; }

    public string? Operator { get; set; }

    public object? Value { get; set; }

    // Group
    public string? DisplayName { get; set; }

    /// <summary>
    /// <see cref="Enums.LogicalOperator"/> name for children when <see cref="Kind"/> is group.
    /// </summary>
    public string? LogicalOperator { get; set; }

    public List<FilterPipelineNodeDto>? Children { get; set; }
}
