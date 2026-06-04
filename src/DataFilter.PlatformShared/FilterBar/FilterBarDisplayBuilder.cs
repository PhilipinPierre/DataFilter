using DataFilter.Core.Enums;
using DataFilter.Core.Pipeline;
using DataFilter.Localization;

namespace DataFilter.PlatformShared.FilterBar;

/// <summary>
/// Builds visual segment models from a <see cref="FilterPipeline"/>.
/// </summary>
public static class FilterBarDisplayBuilder
{
    /// <summary>
    /// Produces ordered display items (OR separators and AND clusters).
    /// </summary>
    public static IReadOnlyList<FilterBarDisplayItem> Build(
        FilterPipeline pipeline,
        Func<string, string>? resolveColumnTitle = null)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

        var items = new List<FilterBarDisplayItem>();
        if (pipeline.RootNodes.Count == 0)
            return items;

        if (pipeline.RootCombineOperator == LogicalOperator.Or)
        {
            for (int i = 0; i < pipeline.RootNodes.Count; i++)
            {
                if (i > 0)
                    items.Add(new FilterBarOrSeparatorItem { Text = LocalizationManager.Instance["FilterBar_Or"] });

                AppendRootSegment(pipeline.RootNodes[i], items, resolveColumnTitle);
            }
        }
        else
        {
            var cluster = BuildAndCluster(pipeline.RootNodes, groupNodeId: null, resolveColumnTitle);
            if (cluster.Chips.Count > 0)
                items.Add(cluster);
        }

        return items;
    }

    private static void AppendRootSegment(
        FilterPipelineNode node,
        List<FilterBarDisplayItem> items,
        Func<string, string>? resolveColumnTitle)
    {
        switch (node)
        {
            case CriterionPipelineNode c:
                items.Add(SingleChipCluster(c, resolveColumnTitle));
                break;
            case GroupPipelineNode g when g.CombineOperator == LogicalOperator.And:
                items.Add(BuildAndCluster(g.Children, g.Id, resolveColumnTitle, g.IsEnabled));
                break;
            case GroupPipelineNode g when g.CombineOperator == LogicalOperator.Or:
                for (int i = 0; i < g.Children.Count; i++)
                {
                    if (i > 0)
                        items.Add(new FilterBarOrSeparatorItem { Text = LocalizationManager.Instance["FilterBar_Or"] });

                    AppendRootSegment(g.Children[i], items, resolveColumnTitle);
                }
                break;
        }
    }

    private static FilterBarAndClusterItem SingleChipCluster(
        CriterionPipelineNode c,
        Func<string, string>? resolveColumnTitle) =>
        new()
        {
            GroupNodeId = null,
            CanAddAnd = true,
            Chips = new List<FilterBarChipItem> { ToChip(c, canAddAnd: true, resolveColumnTitle) }
        };

    private static FilterBarAndClusterItem BuildAndCluster(
        IEnumerable<FilterPipelineNode> nodes,
        string? groupNodeId,
        Func<string, string>? resolveColumnTitle,
        bool isEnabled = true)
    {
        var chips = new List<FilterBarChipItem>();
        foreach (FilterPipelineNode node in nodes)
            CollectAndChips(node, chips, resolveColumnTitle, isEnabled);

        return new FilterBarAndClusterItem
        {
            GroupNodeId = groupNodeId,
            CanAddAnd = groupNodeId != null || chips.Count > 0,
            Chips = chips
        };
    }

    private static void CollectAndChips(
        FilterPipelineNode node,
        List<FilterBarChipItem> chips,
        Func<string, string>? resolveColumnTitle,
        bool parentEnabled)
    {
        bool enabled = parentEnabled && node.IsEnabled;

        switch (node)
        {
            case CriterionPipelineNode c:
                chips.Add(ToChip(c, canAddAnd: true, resolveColumnTitle, enabled));
                break;
            case GroupPipelineNode g when g.CombineOperator == LogicalOperator.And:
                foreach (FilterPipelineNode child in g.Children)
                    CollectAndChips(child, chips, resolveColumnTitle, enabled && g.IsEnabled);
                break;
            case GroupPipelineNode g when g.CombineOperator == LogicalOperator.Or:
                chips.Add(new FilterBarChipItem
                {
                    NodeId = g.Id,
                    PropertyName = g.DisplayName,
                    DisplayText = string.IsNullOrEmpty(g.DisplayName)
                        ? LocalizationManager.Instance["FilterBar_OrGroup"]
                        : g.DisplayName,
                    IsEnabled = enabled && g.IsEnabled,
                    CanAddAnd = false
                });
                break;
        }
    }

    private static FilterBarChipItem ToChip(
        CriterionPipelineNode c,
        bool canAddAnd,
        Func<string, string>? resolveColumnTitle,
        bool? isEnabled = null) =>
        new()
        {
            NodeId = c.Id,
            PropertyName = c.PropertyName,
            DisplayText = FilterCriterionFormatter.Format(c, resolveColumnTitle),
            IsEnabled = isEnabled ?? c.IsEnabled,
            CanAddAnd = canAddAnd
        };
}
