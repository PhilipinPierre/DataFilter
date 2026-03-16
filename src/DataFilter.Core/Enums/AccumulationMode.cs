namespace DataFilter.Core.Enums;

/// <summary>
/// Defines how new filter criteria are merged with existing ones.
/// </summary>
public enum AccumulationMode
{
    /// <summary>
    /// Logical OR: The new criteria are added to the existing filter (Union).
    /// </summary>
    Union,

    /// <summary>
    /// Logical AND: The existing filter is refined by the new criteria (Intersection).
    /// </summary>
    Intersection
}
