namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// Base type for filter bar visual segments.
/// </summary>
public abstract class FilterBarDisplayItem;

/// <summary>
/// OR separator between visual segments.
/// </summary>
public sealed class FilterBarOrSeparatorItem : FilterBarDisplayItem
{
    public string Text { get; init; } = string.Empty;

    /// <summary>Index at which a criterion dropped on this separator is inserted as a new OR group.</summary>
    public int OrInsertIndex { get; init; }
}

/// <summary>
/// AND-grouped chips in one bordered cluster.
/// </summary>
public sealed class FilterBarAndClusterItem : FilterBarDisplayItem
{
    /// <summary>Pipeline node id when the cluster maps to a <see cref="Core.Pipeline.GroupPipelineNode"/>; otherwise null.</summary>
    public string? GroupNodeId { get; init; }

    public bool CanAddAnd { get; init; } = true;

    public IList<FilterBarChipItem> Chips { get; init; } = new List<FilterBarChipItem>();

    /// <summary>Anchor for the cluster « + » button (<see cref="GroupNodeId"/> or last chip).</summary>
    public string AddAndAnchorNodeId =>
        !string.IsNullOrEmpty(GroupNodeId)
            ? GroupNodeId!
            : Chips.LastOrDefault()?.NodeId ?? string.Empty;
}

/// <summary>
/// A single filter chip in the bar.
/// </summary>
public sealed class FilterBarChipItem
{
    public required string NodeId { get; init; }

    public required string PropertyName { get; init; }

    public required string DisplayText { get; init; }

    public bool IsEnabled { get; init; } = true;

    public bool CanAddAnd { get; init; }
}
