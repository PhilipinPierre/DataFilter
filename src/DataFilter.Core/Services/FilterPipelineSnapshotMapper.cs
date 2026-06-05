using System.Text.Json;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;

namespace DataFilter.Core.Services;

/// <summary>
/// Maps between <see cref="FilterPipeline"/> and <see cref="FilterPipelineSnapshot"/> DTOs.
/// </summary>
public static class FilterPipelineSnapshotMapper
{
    public const int CurrentSchemaVersion = 1;

    public static FilterPipelineSnapshot ToSnapshot(FilterPipeline pipeline)
        => ToSnapshot(pipeline, sortEntries: null);

    public static FilterPipelineSnapshot ToSnapshot(
        FilterPipeline pipeline,
        IReadOnlyList<SortSnapshotEntry>? sortEntries)
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

        return new FilterPipelineSnapshot
        {
            SchemaVersion = CurrentSchemaVersion,
            RootCombineOperator = pipeline.RootCombineOperator.ToString(),
            Nodes = pipeline.RootNodes.Select(ToDto).ToList(),
            SortEntries = sortEntries?.Select(CloneSortEntry).ToList() ?? new List<SortSnapshotEntry>()
        };
    }

    public static FilterPipelineSnapshot ToSnapshot(IFilterSnapshot legacySnapshot)
    {
        if (legacySnapshot == null) throw new ArgumentNullException(nameof(legacySnapshot));

        return ToSnapshot(
            FilterPipelineInterop.FromLegacySnapshot(legacySnapshot),
            legacySnapshot.SortEntries);
    }

    /// <param name="replaceSort">
    /// When <see langword="true"/>, replaces the context sort (clears when <paramref name="sortEntries"/> is empty).
    /// When <see langword="false"/>, leaves the current sort unchanged.
    /// </param>
    public static void ApplySortEntries(
        IFilterContext context,
        IEnumerable<SortSnapshotEntry>? sortEntries,
        bool replaceSort = true)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!replaceSort)
            return;

        if (context is not FilterContext concreteContext)
        {
            throw new ArgumentException(
                $"ApplySortEntries requires a {nameof(FilterContext)} instance.", nameof(context));
        }

        concreteContext.ClearSort();
        if (sortEntries == null)
            return;

        List<SortSnapshotEntry> sortList = sortEntries as List<SortSnapshotEntry> ?? sortEntries.ToList();
        if (sortList.Count == 0)
            return;

        foreach (SortSnapshotEntry sortEntry in sortList)
            concreteContext.AddSort(sortEntry.PropertyName, sortEntry.IsDescending);
    }

    public static FilterPipeline ToPipeline(FilterPipelineSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        if (snapshot.SchemaVersion > CurrentSchemaVersion)
            throw new NotSupportedException($"SchemaVersion {snapshot.SchemaVersion} is not supported.");

        var pipeline = new FilterPipeline
        {
            RootCombineOperator = ParseLogical(snapshot.RootCombineOperator)
        };

        foreach (FilterPipelineNodeDto dto in snapshot.Nodes)
        {
            pipeline.RootNodes.Add(FromDto(dto));
        }

        return pipeline;
    }

    private static FilterPipelineNodeDto ToDto(FilterPipelineNode node)
    {
        switch (node)
        {
            case CriterionPipelineNode c:
                return new FilterPipelineNodeDto
                {
                    Kind = "criterion",
                    Id = c.Id,
                    IsEnabled = c.IsEnabled,
                    PropertyName = c.PropertyName,
                    Operator = c.Operator,
                    Value = c.Value
                };
            case GroupPipelineNode g:
                return new FilterPipelineNodeDto
                {
                    Kind = "group",
                    Id = g.Id,
                    IsEnabled = g.IsEnabled,
                    DisplayName = g.DisplayName,
                    LogicalOperator = g.CombineOperator.ToString(),
                    Children = g.Children.Select(ToDto).ToList()
                };
            default:
                throw new InvalidOperationException($"Unknown node type: {node.GetType().Name}");
        }
    }

    private static FilterPipelineNode FromDto(FilterPipelineNodeDto dto)
    {
        if (string.Equals(dto.Kind, "group", StringComparison.OrdinalIgnoreCase))
        {
            var g = new GroupPipelineNode(dto.Id)
            {
                IsEnabled = dto.IsEnabled,
                DisplayName = dto.DisplayName ?? string.Empty,
                CombineOperator = ParseLogical(dto.LogicalOperator ?? nameof(LogicalOperator.And))
            };
            if (dto.Children != null)
            {
                foreach (FilterPipelineNodeDto child in dto.Children)
                    g.Children.Add(FromDto(child));
            }

            return g;
        }

        if (string.Equals(dto.Kind, "criterion", StringComparison.OrdinalIgnoreCase))
        {
            return new CriterionPipelineNode(dto.Id)
            {
                IsEnabled = dto.IsEnabled,
                PropertyName = dto.PropertyName ?? string.Empty,
                Operator = dto.Operator ?? string.Empty,
                Value = NormalizeJsonValue(dto.Value)
            };
        }

        throw new ArgumentException($"Unknown Kind '{dto.Kind}'.", nameof(dto));
    }

    private static SortSnapshotEntry CloneSortEntry(SortSnapshotEntry entry)
        => new()
        {
            PropertyName = entry.PropertyName,
            IsDescending = entry.IsDescending
        };

    private static LogicalOperator ParseLogical(string value)
    {
        return Enum.TryParse<LogicalOperator>(value, ignoreCase: true, out LogicalOperator op)
            ? op
            : LogicalOperator.And;
    }

    /// <summary>
    /// System.Text.Json deserializes JSON primitives into <see cref="JsonElement"/> when the target is <see cref="object"/>.
    /// </summary>
    private static object? NormalizeJsonValue(object? value)
    {
        if (value is not JsonElement je)
            return value;

        return je.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.String => je.GetString(),
            JsonValueKind.Number => je.TryGetInt32(out int i) ? i : je.GetDouble(),
            _ => je.ToString()
        };
    }
}
