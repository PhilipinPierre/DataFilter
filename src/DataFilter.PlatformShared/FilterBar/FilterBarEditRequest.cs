namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// Request to open the column filter popup from the filter bar.
/// </summary>
public sealed class FilterBarEditRequest
{
    public required string NodeId { get; init; }

    public string PropertyName { get; init; } = string.Empty;

    public bool IsNew { get; init; }

    public object? Anchor { get; init; }
}
