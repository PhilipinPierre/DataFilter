using System.Text.Json;
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
    {
        if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

        return new FilterPipelineSnapshot
        {
            SchemaVersion = CurrentSchemaVersion,
            RootCombineOperator = pipeline.RootCombineOperator.ToString(),
            Nodes = pipeline.RootNodes.Select(ToDto).ToList()
        };
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
