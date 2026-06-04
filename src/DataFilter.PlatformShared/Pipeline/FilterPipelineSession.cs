using DataFilter.Core.Abstractions;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;

namespace DataFilter.PlatformShared.Pipeline;

/// <summary>
/// Maintains a live <see cref="FilterPipeline"/> with stable IDs, synced from <see cref="IFilterContext"/> and mutated by the filter bar.
/// </summary>
public sealed class FilterPipelineSession
{
    private FilterPipeline _pipeline = new();

    /// <summary>
    /// Raised when the pipeline graph changes.
    /// </summary>
    public event EventHandler? PipelineChanged;

    /// <summary>
    /// Current pipeline (mutable).
    /// </summary>
    public FilterPipeline Pipeline => _pipeline;

    /// <summary>
    /// Rebuilds from <paramref name="context"/> and merges IDs from the previous graph.
    /// </summary>
    public void SyncFromContext(IFilterContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var snapshot = new FilterSnapshotBuilder().CreateSnapshot(context);
        var incoming = FilterPipelineInterop.FromLegacySnapshot(snapshot);
        var previous = ClonePipelineShallow(_pipeline);
        FilterPipelineIdMerger.MergeIds(previous, incoming);
        _pipeline = incoming;
        OnPipelineChanged();
    }

    /// <summary>
    /// Replaces the entire pipeline (e.g. after JSON preset).
    /// </summary>
    public void ReplacePipeline(FilterPipeline pipeline)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        OnPipelineChanged();
    }

    /// <summary>
    /// Removes a node by id.
    /// </summary>
    public bool RemoveNode(string nodeId)
    {
        if (!FilterPipelineEditor.RemoveNode(_pipeline, nodeId))
            return false;

        OnPipelineChanged();
        return true;
    }

    /// <summary>
    /// Toggles or sets enabled state.
    /// </summary>
    public bool SetEnabled(string nodeId, bool isEnabled)
    {
        if (!FilterPipelineEditor.SetEnabled(_pipeline, nodeId, isEnabled))
            return false;

        OnPipelineChanged();
        return true;
    }

    /// <summary>
    /// Adds an AND criterion relative to an anchor node or AND group.
    /// </summary>
    public CriterionPipelineNode? AddAndCriterion(string anchorNodeId)
    {
        CriterionPipelineNode? created = FilterPipelineEditor.AddAndCriterion(_pipeline, anchorNodeId);
        if (created != null)
            OnPipelineChanged();
        return created;
    }

    /// <summary>
    /// Updates a criterion node from edited state (property, operator, value, enabled).
    /// </summary>
    public bool UpdateCriterion(string nodeId, string propertyName, string operatorName, object? value, bool isEnabled = true)
    {
        if (!FilterPipelineEditor.TryFind(_pipeline, nodeId, out _, out FilterPipelineNode? node)
            || node is not CriterionPipelineNode c)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        c.PropertyName = propertyName;
        c.Operator = operatorName ?? string.Empty;
        c.Value = value;
        c.IsEnabled = isEnabled;
        OnPipelineChanged();
        return true;
    }

    /// <summary>
    /// Finds a node by id.
    /// </summary>
    public bool TryGetNode(string nodeId, out FilterPipelineNode? node) =>
        FilterPipelineEditor.TryFind(_pipeline, nodeId, out _, out node);

    private void OnPipelineChanged() => PipelineChanged?.Invoke(this, EventArgs.Empty);

    private static FilterPipeline ClonePipelineShallow(FilterPipeline source) =>
        FilterPipelineSnapshotMapper.ToPipeline(FilterPipelineSnapshotMapper.ToSnapshot(source));
}
