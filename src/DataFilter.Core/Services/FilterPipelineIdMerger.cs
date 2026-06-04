using System.Text.Json;
using DataFilter.Core.Pipeline;

namespace DataFilter.Core.Services;

/// <summary>
/// Reassigns stable node <see cref="FilterPipelineNode.Id"/> values from an existing pipeline onto a freshly built graph.
/// </summary>
public static class FilterPipelineIdMerger
{
    /// <summary>
    /// Copies IDs from <paramref name="existing"/> onto matching nodes in <paramref name="incoming"/> (criteria by signature; groups by id when structure matches).
    /// </summary>
    public static void MergeIds(FilterPipeline existing, FilterPipeline incoming)
    {
        if (existing == null) throw new ArgumentNullException(nameof(existing));
        if (incoming == null) throw new ArgumentNullException(nameof(incoming));

        incoming.RootCombineOperator = existing.RootCombineOperator;

        var idBySignature = new Dictionary<string, string>(StringComparer.Ordinal);
        CollectCriterionIds(existing, idBySignature);

        ApplyIds(incoming, idBySignature);
    }

    private static void CollectCriterionIds(FilterPipeline pipeline, Dictionary<string, string> idBySignature)
    {
        foreach (FilterPipelineNode node in pipeline.RootNodes)
            CollectCriterionIds(node, idBySignature);
    }

    private static void CollectCriterionIds(FilterPipelineNode node, Dictionary<string, string> idBySignature)
    {
        switch (node)
        {
            case CriterionPipelineNode c:
                string key = CriterionSignature(c);
#if NETSTANDARD2_0
                if (idBySignature.ContainsKey(key))
                    idBySignature[key] = c.Id;
                else
                    idBySignature.Add(key, c.Id);
#else
                idBySignature.TryAdd(key, c.Id);
#endif
                break;
            case GroupPipelineNode g:
                foreach (FilterPipelineNode child in g.Children)
                    CollectCriterionIds(child, idBySignature);
                break;
        }
    }

    private static void ApplyIds(FilterPipeline pipeline, Dictionary<string, string> idBySignature)
    {
        foreach (FilterPipelineNode node in pipeline.RootNodes)
            ApplyIds(node, idBySignature);
    }

    private static void ApplyIds(FilterPipelineNode node, Dictionary<string, string> idBySignature)
    {
        switch (node)
        {
            case CriterionPipelineNode c:
                string key = CriterionSignature(c);
                if (idBySignature.TryGetValue(key, out string? id))
                    c.Id = id;
                break;
            case GroupPipelineNode g:
                foreach (FilterPipelineNode child in g.Children)
                    ApplyIds(child, idBySignature);
                break;
        }
    }

    /// <summary>
    /// Builds a stable signature for criterion identity during merge.
    /// </summary>
    public static string CriterionSignature(CriterionPipelineNode c) =>
        $"{c.PropertyName}\0{c.Operator}\0{SerializeValue(c.Value)}";

    private static string SerializeValue(object? value)
    {
        if (value == null)
            return string.Empty;

        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch
        {
            return value.ToString() ?? string.Empty;
        }
    }
}
