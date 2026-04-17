using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;

namespace DataFilter.Core.Services;

/// <summary>
/// Builds a <see cref="FilterPipeline"/> from legacy <see cref="IFilterSnapshot"/> / <see cref="FilterSnapshotEntry"/> trees.
/// </summary>
public static class FilterPipelineInterop
{
    /// <summary>
    /// Converts a flat or grouped legacy snapshot into a pipeline (new GUIDs for all nodes).
    /// </summary>
    public static FilterPipeline FromLegacySnapshot(IFilterSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        var pipeline = new FilterPipeline();
        foreach (FilterSnapshotEntry entry in snapshot.Entries)
        {
            pipeline.RootNodes.Add(EntryToNode(entry));
        }

        return pipeline;
    }

    private static FilterPipelineNode EntryToNode(FilterSnapshotEntry entry)
    {
        if (entry.IsGroup && entry.Children is { Count: > 0 })
        {
            var g = new GroupPipelineNode
            {
                DisplayName = entry.PropertyName ?? string.Empty,
                CombineOperator = Enum.TryParse<LogicalOperator>(
                    entry.LogicalOperator ?? nameof(LogicalOperator.And),
                    ignoreCase: true,
                    out LogicalOperator op)
                    ? op
                    : LogicalOperator.And
            };
            foreach (FilterSnapshotEntry child in entry.Children)
                g.Children.Add(EntryToNode(child));
            return g;
        }

        return new CriterionPipelineNode
        {
            PropertyName = entry.PropertyName ?? string.Empty,
            Operator = entry.Operator,
            Value = entry.Value
        };
    }
}
