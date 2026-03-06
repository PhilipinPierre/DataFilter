using DataFilter.Core.Abstractions;

namespace DataFilter.Core.Models;

/// <summary>
/// Immutable snapshot of the current filtering and sorting state.
/// </summary>
public sealed class FilterSnapshot : IFilterSnapshot
{
    /// <inheritdoc />
    public IReadOnlyList<FilterSnapshotEntry> Entries { get; }

    /// <inheritdoc />
    public IReadOnlyList<SortSnapshotEntry> SortEntries { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FilterSnapshot"/>.
    /// </summary>
    /// <param name="entries">The filter criterion entries.</param>
    /// <param name="sortEntries">The sort criterion entries.</param>
    public FilterSnapshot(IReadOnlyList<FilterSnapshotEntry> entries, IReadOnlyList<SortSnapshotEntry> sortEntries)
    {
        Entries = entries ?? new List<FilterSnapshotEntry>().AsReadOnly();
        SortEntries = sortEntries ?? new List<SortSnapshotEntry>().AsReadOnly();
    }
}
