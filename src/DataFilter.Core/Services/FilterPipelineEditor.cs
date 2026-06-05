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
    /// Appends a new AND-grouped cluster at root level (combined with existing roots via OR).
    /// Wraps current root nodes in an AND group when <see cref="FilterPipeline.RootCombineOperator"/> is not yet OR.
    /// </summary>
    public static CriterionPipelineNode? AddOrGroup(FilterPipeline pipeline, string propertyName)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("PropertyName is required.", nameof(propertyName));

        var newGroup = new GroupPipelineNode
        {
            CombineOperator = LogicalOperator.And,
            DisplayName = string.Empty,
            IsEnabled = true
        };
        var newCriterion = CreateCriterionDraft(propertyName);
        newGroup.Children.Add(newCriterion);

        if (pipeline.RootNodes.Count == 0)
        {
            pipeline.RootNodes.Add(newGroup);
            pipeline.RootCombineOperator = LogicalOperator.And;
            return newCriterion;
        }

        EnsureRootOrCombine(pipeline);
        pipeline.RootNodes.Add(newGroup);
        return newCriterion;
    }

    private static void EnsureRootOrCombine(FilterPipeline pipeline)
    {
        if (pipeline.RootCombineOperator == LogicalOperator.Or)
            return;

        var wrapper = new GroupPipelineNode
        {
            CombineOperator = LogicalOperator.And,
            DisplayName = string.Empty,
            IsEnabled = true
        };

        foreach (FilterPipelineNode node in pipeline.RootNodes)
            wrapper.Children.Add(node);

        pipeline.RootNodes.Clear();
        pipeline.RootNodes.Add(wrapper);
        pipeline.RootCombineOperator = LogicalOperator.Or;
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
    /// Moves a criterion into the AND cluster identified by <paramref name="targetClusterAnchorNodeId"/>
    /// (an AND group id, or any node id inside the target cluster from the filter bar).
    /// </summary>
    public static bool MoveCriterionToCluster(FilterPipeline pipeline, string criterionNodeId, string targetClusterAnchorNodeId)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (string.IsNullOrEmpty(criterionNodeId) || string.IsNullOrEmpty(targetClusterAnchorNodeId))
            return false;

        if (!TryFind(pipeline, criterionNodeId, out _, out FilterPipelineNode? moving) || moving is not CriterionPipelineNode criterion)
            return false;

        if (!TryResolveAndClusterContainer(pipeline, targetClusterAnchorNodeId, out List<FilterPipelineNode>? container, out _))
            return false;

        if (ContainerAlreadyContains(container, criterionNodeId))
            return false;

        if (!TryFind(pipeline, criterionNodeId, out NodeLocation sourceLocation, out _))
            return false;

        var detached = (CriterionPipelineNode)CloneNodeShallow(criterion);
        sourceLocation.Container.RemoveAt(sourceLocation.Index);
        Compact(pipeline);

        if (!TryResolveAndClusterContainer(pipeline, targetClusterAnchorNodeId, out container, out _))
            return false;

        if (ContainerAlreadyContains(container, detached.Id))
            return false;

        container.Add(detached);
        Compact(pipeline);
        return true;
    }

    /// <summary>
    /// Moves a criterion into a new OR sibling at <paramref name="orInsertIndex"/> (0..RootNodes.Count).
    /// </summary>
    public static bool MoveCriterionToOrGap(FilterPipeline pipeline, string criterionNodeId, int orInsertIndex)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
        if (string.IsNullOrEmpty(criterionNodeId))
            return false;

        if (!TryFind(pipeline, criterionNodeId, out NodeLocation sourceLocation, out FilterPipelineNode? moving)
            || moving is not CriterionPipelineNode criterion)
        {
            return false;
        }

        var detached = (CriterionPipelineNode)CloneNodeShallow(criterion);
        sourceLocation.Container.RemoveAt(sourceLocation.Index);
        Compact(pipeline);

        if (pipeline.RootNodes.Count == 0)
        {
            var loneGroup = new GroupPipelineNode
            {
                CombineOperator = LogicalOperator.And,
                DisplayName = string.Empty,
                IsEnabled = detached.IsEnabled
            };
            loneGroup.Children.Add(detached);
            pipeline.RootNodes.Add(loneGroup);
            pipeline.RootCombineOperator = LogicalOperator.Or;
            Compact(pipeline);
            return true;
        }

        if (pipeline.RootCombineOperator != LogicalOperator.Or)
            EnsureRootOrCombine(pipeline);

        int index = orInsertIndex;
        if (index < 0)
            index = 0;
        else if (index > pipeline.RootNodes.Count)
            index = pipeline.RootNodes.Count;

        var group = new GroupPipelineNode
        {
            CombineOperator = LogicalOperator.And,
            DisplayName = string.Empty,
            IsEnabled = detached.IsEnabled
        };
        group.Children.Add(detached);
        pipeline.RootNodes.Insert(index, group);
        Compact(pipeline);
        return true;
    }

    /// <summary>
    /// Removes empty groups and unwraps trivial single-child AND groups where safe.
    /// </summary>
    public static void Compact(FilterPipeline pipeline)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

        CompactList(pipeline.RootNodes, pipeline.RootCombineOperator == LogicalOperator.And);

        if (pipeline.RootCombineOperator == LogicalOperator.Or)
        {
            for (int i = pipeline.RootNodes.Count - 1; i >= 0; i--)
            {
                if (pipeline.RootNodes[i] is GroupPipelineNode g)
                    CompactAndGroupChildren(g);
            }
        }
    }

    private static bool ContainerAlreadyContains(List<FilterPipelineNode> container, string nodeId) =>
        container.Any(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));

    private static bool TryResolveAndClusterContainer(
        FilterPipeline pipeline,
        string anchorNodeId,
        out List<FilterPipelineNode> container,
        out GroupPipelineNode? andGroup)
    {
        container = null!;
        andGroup = null;

        if (!TryFind(pipeline, anchorNodeId, out NodeLocation location, out FilterPipelineNode? anchor) || anchor == null)
            return false;

        if (anchor is GroupPipelineNode group && group.CombineOperator == LogicalOperator.And)
        {
            andGroup = group;
            container = group.Children;
            return true;
        }

        if (anchor is CriterionPipelineNode)
        {
            if (location.ParentAndOrGroup is GroupPipelineNode parentAnd
                && parentAnd.CombineOperator == LogicalOperator.And)
            {
                andGroup = parentAnd;
                container = parentAnd.Children;
                return true;
            }

            if (location.ParentAndOrGroup == null && pipeline.RootCombineOperator == LogicalOperator.And)
            {
                container = pipeline.RootNodes;
                return true;
            }

            var wrapper = new GroupPipelineNode
            {
                CombineOperator = LogicalOperator.And,
                DisplayName = string.Empty,
                IsEnabled = anchor.IsEnabled
            };
            wrapper.Children.Add(CloneNodeShallow(anchor));
            location.Container[location.Index] = wrapper;
            andGroup = wrapper;
            container = wrapper.Children;
            return true;
        }

        return false;
    }

    private static void CompactList(List<FilterPipelineNode> nodes, bool unwrapSingleChildAndGroups)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (nodes[i] is GroupPipelineNode g)
            {
                CompactAndGroupChildren(g);
                if (g.Children.Count == 0)
                    nodes.RemoveAt(i);
                else if (unwrapSingleChildAndGroups
                         && g.CombineOperator == LogicalOperator.And
                         && g.Children.Count == 1)
                    nodes[i] = g.Children[0];
            }
        }
    }

    private static void CompactAndGroupChildren(GroupPipelineNode group)
    {
        for (int i = group.Children.Count - 1; i >= 0; i--)
        {
            if (group.Children[i] is GroupPipelineNode nested)
            {
                CompactAndGroupChildren(nested);
                if (nested.Children.Count == 0)
                    group.Children.RemoveAt(i);
                else if (nested.CombineOperator == LogicalOperator.And && nested.Children.Count == 1)
                    group.Children[i] = nested.Children[0];
            }
        }
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
