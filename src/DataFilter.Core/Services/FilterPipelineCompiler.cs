using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;

namespace DataFilter.Core.Services;

/// <summary>
/// Compiles a <see cref="FilterPipeline"/> to <see cref="IFilterDescriptor"/> instances for the existing engine.
/// </summary>
public static class FilterPipelineCompiler
{
    /// <summary>
    /// Property name of the root <see cref="FilterGroup"/> created when <see cref="FilterPipeline.RootCombineOperator"/> is <see cref="LogicalOperator.Or"/>.
    /// </summary>
    public const string PipelineRootGroupPropertyName = "__pipeline_root";

    /// <summary>
    /// Compiles enabled nodes to descriptors (top-level order preserved). Empty groups are omitted.
    /// Root <see cref="FilterPipeline.RootCombineOperator"/> is honored: for <c>Or</c>, a single
    /// <see cref="FilterGroup"/> wraps all root-level descriptors so engines that combine siblings with <c>And</c> still behave correctly.
    /// </summary>
    public static IReadOnlyList<IFilterDescriptor> Compile(FilterPipeline pipeline)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

        var list = new List<IFilterDescriptor>();
        foreach (FilterPipelineNode node in pipeline.RootNodes)
        {
            IFilterDescriptor? d = CompileNode(node);
            if (d != null)
                list.Add(d);
        }

        if (pipeline.RootCombineOperator == LogicalOperator.Or && list.Count > 1)
        {
            var root = new FilterGroup(LogicalOperator.Or, PipelineRootGroupPropertyName);
            foreach (IFilterDescriptor d in list)
                root.Add(d);
            return new[] { root };
        }

        return list;
    }

    private static IFilterDescriptor? CompileNode(FilterPipelineNode node)
    {
        if (!node.IsEnabled)
            return null;

        switch (node)
        {
            case CriterionPipelineNode c:
                return CompileCriterion(c);
            case GroupPipelineNode g:
                return CompileGroup(g);
            default:
                throw new InvalidOperationException($"Unknown pipeline node type: {node.GetType().Name}");
        }
    }

    private static IFilterDescriptor CompileCriterion(CriterionPipelineNode c)
    {
        if (string.IsNullOrWhiteSpace(c.PropertyName))
            throw new ArgumentException("Criterion PropertyName is required.", nameof(c));

        FilterOperator op = (FilterOperator)Enum.Parse(typeof(FilterOperator), c.Operator, ignoreCase: true);
        return new FilterDescriptor(c.PropertyName, op, c.Value);
    }

    private static IFilterDescriptor? CompileGroup(GroupPipelineNode g)
    {
        var group = new FilterGroup(g.CombineOperator, "__group_" + g.Id);
        foreach (FilterPipelineNode child in g.Children)
        {
            IFilterDescriptor? d = CompileNode(child);
            if (d != null)
                group.Add(d);
        }

        if (group.Descriptors.Count == 0)
            return null;

        return group;
    }
}
