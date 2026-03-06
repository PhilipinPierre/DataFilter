namespace DataFilter.Core.Abstractions;

/// <summary>
/// Represents an immutable snapshot of the current filtering and sorting state.
/// This snapshot is built from serialization-friendly POCOs, allowing the consuming
/// application to persist and restore filter configurations using any preferred 
/// serialization format (JSON, XML, custom binary, etc.).
/// </summary>
public interface IFilterSnapshot
{
    /// <summary>
    /// Gets the filter criterion entries captured in this snapshot.
    /// Each entry may represent a single criterion or a composite group.
    /// </summary>
    IReadOnlyList<Models.FilterSnapshotEntry> Entries { get; }

    /// <summary>
    /// Gets the sort criterion entries captured in this snapshot.
    /// </summary>
    IReadOnlyList<Models.SortSnapshotEntry> SortEntries { get; }
}
