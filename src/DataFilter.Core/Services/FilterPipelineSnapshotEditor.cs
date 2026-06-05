using DataFilter.Core.Enums;
using DataFilter.Core.Models;

namespace DataFilter.Core.Services;

/// <summary>
/// In-memory mutations on <see cref="FilterPipelineSnapshot"/> (criteria tree and sort list) without JSON round-trip.
/// </summary>
public static class FilterPipelineSnapshotEditor
{
    /// <summary>
    /// Deep-clones a snapshot so client edits do not affect the grid until applied.
    /// </summary>
    public static FilterPipelineSnapshot Clone(FilterPipelineSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        return new FilterPipelineSnapshot
        {
            SchemaVersion = snapshot.SchemaVersion,
            RootCombineOperator = snapshot.RootCombineOperator,
            Nodes = snapshot.Nodes.Select(CloneNode).ToList(),
            SortEntries = snapshot.SortEntries.Select(CloneSortEntry).ToList()
        };
    }

    /// <summary>
    /// Sets how root nodes are combined.
    /// </summary>
    public static void SetRootCombineOperator(FilterPipelineSnapshot snapshot, LogicalOperator combineOperator)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        snapshot.RootCombineOperator = combineOperator.ToString();
    }

    /// <summary>
    /// Appends a criterion at the pipeline root.
    /// </summary>
    public static FilterPipelineNodeDto AddRootCriterion(
        FilterPipelineSnapshot snapshot,
        string propertyName,
        string operatorName,
        object? value = null,
        string? id = null,
        bool isEnabled = true)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("PropertyName is required.", nameof(propertyName));

        var node = CreateCriterionDto(propertyName, operatorName, value, id, isEnabled);
        snapshot.Nodes.Add(node);
        return node;
    }

    /// <summary>
    /// Appends a criterion to an AND group identified by <paramref name="groupNodeId"/>.
    /// </summary>
    public static FilterPipelineNodeDto? AddCriterionToAndGroup(
        FilterPipelineSnapshot snapshot,
        string groupNodeId,
        string propertyName,
        string operatorName,
        object? value = null,
        string? id = null,
        bool isEnabled = true)
    {
        if (!TryFind(snapshot, groupNodeId, out FilterPipelineNodeDto? node, out _, out _))
            return null;

        if (!string.Equals(node!.Kind, "group", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(node.LogicalOperator, nameof(LogicalOperator.And), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        node.Children ??= new List<FilterPipelineNodeDto>();
        var created = CreateCriterionDto(propertyName, operatorName, value, id, isEnabled);
        node.Children.Add(created);
        return created;
    }

    /// <summary>
    /// Updates a criterion node.
    /// </summary>
    public static bool UpdateCriterion(
        FilterPipelineSnapshot snapshot,
        string nodeId,
        string propertyName,
        string operatorName,
        object? value,
        bool? isEnabled = null)
    {
        if (!TryFind(snapshot, nodeId, out FilterPipelineNodeDto? node, out _, out _)
            || !string.Equals(node!.Kind, "criterion", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        node.PropertyName = propertyName;
        node.Operator = operatorName ?? string.Empty;
        node.Value = value;
        if (isEnabled.HasValue)
            node.IsEnabled = isEnabled.Value;

        return true;
    }

    /// <summary>
    /// Sets <see cref="FilterPipelineNodeDto.IsEnabled"/> on any node kind.
    /// </summary>
    public static bool SetNodeEnabled(FilterPipelineSnapshot snapshot, string nodeId, bool isEnabled)
    {
        if (!TryFind(snapshot, nodeId, out FilterPipelineNodeDto? node, out _, out _) || node == null)
            return false;

        node.IsEnabled = isEnabled;
        return true;
    }

    /// <summary>
    /// Removes a node from the criteria tree.
    /// </summary>
    public static bool RemoveNode(FilterPipelineSnapshot snapshot, string nodeId)
    {
        if (!TryFind(snapshot, nodeId, out _, out List<FilterPipelineNodeDto>? container, out int index))
            return false;

        container!.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Replaces the ordered sort list.
    /// </summary>
    public static void ReplaceSortEntries(
        FilterPipelineSnapshot snapshot,
        IEnumerable<SortSnapshotEntry> sortEntries)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (sortEntries == null) throw new ArgumentNullException(nameof(sortEntries));

        snapshot.SortEntries.Clear();
        foreach (SortSnapshotEntry entry in sortEntries)
            snapshot.SortEntries.Add(CloneSortEntry(entry));
    }

    /// <summary>
    /// Clears all sort entries.
    /// </summary>
    public static void ClearSort(FilterPipelineSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        snapshot.SortEntries.Clear();
    }

    /// <summary>
    /// Appends a sort level (or replaces the same property if already present).
    /// </summary>
    public static void AddSort(FilterPipelineSnapshot snapshot, string propertyName, bool isDescending = false)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("PropertyName is required.", nameof(propertyName));

        snapshot.SortEntries.RemoveAll(e =>
            string.Equals(e.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
        snapshot.SortEntries.Add(new SortSnapshotEntry
        {
            PropertyName = propertyName,
            IsDescending = isDescending
        });
    }

    /// <summary>
    /// Sets the primary sort, replacing any existing sort levels.
    /// </summary>
    public static void SetPrimarySort(FilterPipelineSnapshot snapshot, string propertyName, bool isDescending = false)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        snapshot.SortEntries.Clear();
        AddSort(snapshot, propertyName, isDescending);
    }

    /// <summary>
    /// Removes a sort entry by index.
    /// </summary>
    public static bool RemoveSortAt(FilterPipelineSnapshot snapshot, int index)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (index < 0 || index >= snapshot.SortEntries.Count)
            return false;

        snapshot.SortEntries.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Moves a sort entry to another position.
    /// </summary>
    public static bool MoveSort(FilterPipelineSnapshot snapshot, int fromIndex, int toIndex)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (fromIndex < 0 || fromIndex >= snapshot.SortEntries.Count)
            return false;
        if (toIndex < 0 || toIndex >= snapshot.SortEntries.Count)
            return false;
        if (fromIndex == toIndex)
            return true;

        SortSnapshotEntry entry = snapshot.SortEntries[fromIndex];
        snapshot.SortEntries.RemoveAt(fromIndex);
        snapshot.SortEntries.Insert(toIndex, entry);
        return true;
    }

    /// <summary>
    /// Locates a node in the snapshot tree.
    /// </summary>
    public static bool TryFind(
        FilterPipelineSnapshot snapshot,
        string nodeId,
        out FilterPipelineNodeDto? node,
        out List<FilterPipelineNodeDto>? container,
        out int index)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (string.IsNullOrEmpty(nodeId))
        {
            node = null;
            container = null;
            index = -1;
            return false;
        }

        for (int i = 0; i < snapshot.Nodes.Count; i++)
        {
            if (TryFindDescendant(snapshot.Nodes[i], nodeId, snapshot.Nodes, i, out node, out container, out index))
                return true;
        }

        node = null;
        container = null;
        index = -1;
        return false;
    }

    private static bool TryFindDescendant(
        FilterPipelineNodeDto current,
        string nodeId,
        List<FilterPipelineNodeDto> container,
        int index,
        out FilterPipelineNodeDto? node,
        out List<FilterPipelineNodeDto>? nodeContainer,
        out int nodeIndex)
    {
        if (string.Equals(current.Id, nodeId, StringComparison.Ordinal))
        {
            node = current;
            nodeContainer = container;
            nodeIndex = index;
            return true;
        }

        if (current.Children == null)
        {
            node = null;
            nodeContainer = null;
            nodeIndex = -1;
            return false;
        }

        for (int i = 0; i < current.Children.Count; i++)
        {
            if (TryFindDescendant(current.Children[i], nodeId, current.Children, i, out node, out nodeContainer, out nodeIndex))
                return true;
        }

        node = null;
        nodeContainer = null;
        nodeIndex = -1;
        return false;
    }

    private static FilterPipelineNodeDto CreateCriterionDto(
        string propertyName,
        string operatorName,
        object? value,
        string? id,
        bool isEnabled)
    {
        return new FilterPipelineNodeDto
        {
            Kind = "criterion",
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N") : id,
            IsEnabled = isEnabled,
            PropertyName = propertyName,
            Operator = operatorName ?? string.Empty,
            Value = value
        };
    }

    private static FilterPipelineNodeDto CloneNode(FilterPipelineNodeDto node)
    {
        return new FilterPipelineNodeDto
        {
            Kind = node.Kind,
            Id = node.Id,
            IsEnabled = node.IsEnabled,
            PropertyName = node.PropertyName,
            Operator = node.Operator,
            Value = node.Value,
            DisplayName = node.DisplayName,
            LogicalOperator = node.LogicalOperator,
            Children = node.Children?.Select(CloneNode).ToList()
        };
    }

    private static SortSnapshotEntry CloneSortEntry(SortSnapshotEntry entry)
        => new()
        {
            PropertyName = entry.PropertyName,
            IsDescending = entry.IsDescending
        };
}
