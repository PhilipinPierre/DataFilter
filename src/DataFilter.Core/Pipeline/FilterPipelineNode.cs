namespace DataFilter.Core.Pipeline;

/// <summary>
/// Base type for nodes in a <see cref="FilterPipeline"/> (ordered, enableable criteria or named groups).
/// </summary>
public abstract class FilterPipelineNode
{
    /// <summary>
    /// Stable identifier for UI and serialization (not the column property name).
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// When false, the node and its subtree are ignored when compiling to descriptors.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    protected FilterPipelineNode(string? id = null)
    {
        Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N") : id!;
    }
}
