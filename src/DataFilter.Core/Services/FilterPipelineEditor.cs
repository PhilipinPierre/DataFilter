using DataFilter.Core.Enums;
using DataFilter.Core.Pipeline;

namespace DataFilter.Core.Services;

/// <summary>
/// Mutable operations on a <see cref="FilterPipeline"/> graph (find, remove, enable, add AND criterion).
/// </summary>
public static class FilterPipelineEditor
{
    /// <summary>
    /// Locates a node within a pipeline tree.
    /// </summary>
    public readonly record struct NodeLocation(
        List<FilterPipelineNode> Container,
        int Index,
        FilterPipeline Pipeline,
        GroupPipelineNode? ParentAndOrGroup);

    /// <summary>
    /// Gets whether siblings in <paramref name="location"/> are AND-combined.
    /// </summary>
    public static bool IsAndSiblingContext(NodeLocation location)
    {
        if (location.ParentAndOrGroup != null)
            return location.ParentAndOrGroup.CombineOperator == LogicalOperator.And;

        return location.Pipeline.RootCombineOperator == LogicalOperator.And;
    }

    /// <summary>
    /// Finds a node by <paramref name="nodeId"/>.
    /// </summary>
    public static bool TryFind(
        FilterPipeline pipeline,
        string nodeId,
        out NodeLocation location,
        out FilterPipelineNode? node)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (string.IsNullOrEmpty(nodeId))
        {
            location = default;
            node = null;
            return false;
        }

        for (int i = 0; i < pipeline.RootNodes.Count; i++)
        {
            if (string.Equals(pipeline.RootNodes[i].Id, nodeId, StringComparison.Ordinal))
            {
                location = new NodeLocation(pipeline.RootNodes, i, pipeline, null);
                node = pipeline.RootNodes[i];
                return true;
            }

            if (TryFindDescendant(pipeline.RootNodes[i], nodeId, pipeline, null, out location, out node))
                return true;
        }

        location = default;
        node = null;
        return false;
    }

    private static bool TryFindDescendant(
        FilterPipelineNode parent,
        string nodeId,
        FilterPipeline pipeline,
        GroupPipelineNode? parentGroup,
        out NodeLocation location,
        out FilterPipelineNode? node)
    {
        if (parent is not GroupPipelineNode group)
        {
            location = default;
            node = null;
            return false;
        }

        for (int i = 0; i < group.Children.Count; i++)
        {
            if (string.Equals(group.Children[i].Id, nodeId, StringComparison.Ordinal))
            {
                location = new NodeLocation(group.Children, i, pipeline, group);
                node = group.Children[i];
                return true;
            }

            if (TryFindDescendant(group.Children[i], nodeId, pipeline, group, out location, out node))
                return true;
        }

        location = default;
        node = null;
        return false;
    }

    /// <summary>
    /// Removes a node from the pipeline.
    /// </summary>
    public static bool RemoveNode(FilterPipeline pipeline, string nodeId)
    {
        if (!TryFind(pipeline, nodeId, out NodeLocation location, out _))
            return false;

        location.Container.RemoveAt(location.Index);
        return true;
    }

    /// <summary>
    /// Sets <see cref="FilterPipelineNode.IsEnabled"/> on a node.
    /// </summary>
    public static bool SetEnabled(FilterPipeline pipeline, string nodeId, bool isEnabled)
    {
        if (!TryFind(pipeline, nodeId, out _, out FilterPipelineNode? node) || node == null)
            return false;

        node.IsEnabled = isEnabled;
        return true;
    }

    /// <summary>
    /// Resolves the column <see cref="CriterionPipelineNode.PropertyName"/> for bar edits and AND inserts.
    /// For AND groups, uses the first descendant criterion with a non-empty property name.
    /// </summary>
    public static string? TryResolveColumnPropertyName(FilterPipelineNode node)
    {
        if (node == null) throw new ArgumentNullException(nameof(node));

        switch (node)
        {
            case CriterionPipelineNode c:
                return string.IsNullOrWhiteSpace(c.PropertyName) ? null : c.PropertyName;
            case GroupPipelineNode g when g.CombineOperator == LogicalOperator.And:
                foreach (FilterPipelineNode child in g.Children)
                {
                    string? name = TryResolveColumnPropertyName(child);
                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }

                return null;
            default:
                return null;
        }
    }

    /// <summary>
    /// Adds a new AND-combined criterion on the same column as <paramref name="anchorNodeId"/>.
    /// Returns null when the anchor is missing or its column cannot be resolved.
    /// </summary>
    public static CriterionPipelineNode? AddAndCriterion(FilterPipeline pipeline, string anchorNodeId)
    {
        if (!TryFind(pipeline, anchorNodeId, out NodeLocation location, out FilterPipelineNode? anchor) || anchor == null)
            return null;

        string? propertyName = TryResolveColumnPropertyName(anchor);
        if (string.IsNullOrWhiteSpace(propertyName))
            return null;

        var newCriterion = CreateCriterionDraft(propertyName);

        if (anchor is GroupPipelineNode andGroup && andGroup.CombineOperator == LogicalOperator.And)
        {
            andGroup.Children.Add(newCriterion);
            return newCriterion;
        }

        if (anchor is CriterionPipelineNode)
            return InsertAndRelativeToCriterion(location, anchor, newCriterion);

        return null;
    }

    private static CriterionPipelineNode InsertAndRelativeToCriterion(
        NodeLocation location,
        FilterPipelineNode anchor,
        CriterionPipelineNode newCriterion)
    {
        if (IsAndSiblingContext(location))
        {
            location.Container.Insert(location.Index + 1, newCriterion);
            return newCriterion;
        }

        var andGroup = new GroupPipelineNode
        {
            DisplayName = string.Empty,
            CombineOperator = LogicalOperator.And,
            IsEnabled = anchor.IsEnabled
        };
        andGroup.Children.Add(CloneNodeShallow(anchor));
        andGroup.Children.Add(newCriterion);
        location.Container[location.Index] = andGroup;
        return newCriterion;
    }

    private static FilterPipelineNode CloneNodeShallow(FilterPipelineNode node)
    {
        return node switch
        {
            CriterionPipelineNode c => new CriterionPipelineNode(c.Id)
            {
                IsEnabled = c.IsEnabled,
                PropertyName = c.PropertyName,
                Operator = c.Operator,
                Value = c.Value
            },
            GroupPipelineNode g => new GroupPipelineNode(g.Id)
            {
                IsEnabled = g.IsEnabled,
                DisplayName = g.DisplayName,
                CombineOperator = g.CombineOperator
            },
            _ => throw new InvalidOperationException($"Unknown node type: {node.GetType().Name}")
        };
    }

    /// <summary>
    /// Creates a draft criterion for popup editing on a known column.
    /// </summary>
    public static CriterionPipelineNode CreateCriterionDraft(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("PropertyName is required.", nameof(propertyName));

        return new CriterionPipelineNode
        {
            PropertyName = propertyName,
            Operator = nameof(FilterOperator.Equals),
            Value = null,
            IsEnabled = true
        };
    }
}
