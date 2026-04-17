using System.Text.Json;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;

namespace DataFilter.Core.Tests;

public class FilterPipelineTests
{
    [Fact]
    public void ReplaceDescriptors_Allows_Duplicate_PropertyNames()
    {
        var ctx = new FilterContext();
        var list = new IFilterDescriptor[]
        {
            new FilterDescriptor("Name", FilterOperator.Contains, "a"),
            new FilterDescriptor("Name", FilterOperator.Contains, "b")
        };

        ctx.ReplaceDescriptors(list);

        Assert.Equal(2, ctx.Descriptors.Count);
        Assert.Same(list[0], ctx.Descriptors[0]);
        Assert.Same(list[1], ctx.Descriptors[1]);
    }

    [Fact]
    public void Compiler_Skips_Disabled_Nodes()
    {
        var pipeline = new FilterPipeline();
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "X", Operator = nameof(FilterOperator.Equals), Value = 1 });
        var disabled = new CriterionPipelineNode { PropertyName = "Y", Operator = nameof(FilterOperator.Equals), Value = 2, IsEnabled = false };
        pipeline.RootNodes.Add(disabled);

        var compiled = FilterPipelineCompiler.Compile(pipeline);

        Assert.Single(compiled);
        Assert.Equal("X", compiled[0].PropertyName);
    }

    [Fact]
    public void Compiler_Or_Root_Wraps_In_Single_Group()
    {
        var pipeline = new FilterPipeline { RootCombineOperator = LogicalOperator.Or };
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 });
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "B", Operator = nameof(FilterOperator.Equals), Value = 2 });

        var compiled = FilterPipelineCompiler.Compile(pipeline);

        Assert.Single(compiled);
        Assert.IsAssignableFrom<IFilterGroup>(compiled[0]);
        var g = (IFilterGroup)compiled[0];
        Assert.Equal(LogicalOperator.Or, g.LogicalOperator);
        Assert.Equal(2, g.Descriptors.Count);
    }

    [Fact]
    public void SnapshotMapper_RoundTrip_Preserves_Structure()
    {
        var original = new FilterPipeline();
        original.RootNodes.Add(new GroupPipelineNode { DisplayName = "G", CombineOperator = LogicalOperator.Or });
        var g = (GroupPipelineNode)original.RootNodes[0];
        g.Children.Add(new CriterionPipelineNode { PropertyName = "P", Operator = nameof(FilterOperator.GreaterThan), Value = 3 });

        var dto = FilterPipelineSnapshotMapper.ToSnapshot(original);
        var back = FilterPipelineSnapshotMapper.ToPipeline(dto);

        Assert.Equal(original.RootCombineOperator, back.RootCombineOperator);
        Assert.Single(back.RootNodes);
        var g2 = Assert.IsType<GroupPipelineNode>(back.RootNodes[0]);
        Assert.Equal("G", g2.DisplayName);
        Assert.Equal(LogicalOperator.Or, g2.CombineOperator);
        Assert.Single(g2.Children);
        var c = Assert.IsType<CriterionPipelineNode>(g2.Children[0]);
        Assert.Equal("P", c.PropertyName);
    }

    [Fact]
    public void Interop_FromLegacySnapshot_Builds_Pipeline()
    {
        var snapshot = new FilterSnapshot(
            new[]
            {
                new FilterSnapshotEntry { PropertyName = "Age", Operator = nameof(FilterOperator.GreaterThan), Value = 30 }
            },
            Array.Empty<SortSnapshotEntry>());

        var pipeline = FilterPipelineInterop.FromLegacySnapshot(snapshot);

        Assert.Single(pipeline.RootNodes);
        var c = Assert.IsType<CriterionPipelineNode>(pipeline.RootNodes[0]);
        Assert.Equal("Age", c.PropertyName);
    }

    [Fact]
    public void SnapshotMapper_Deserializes_Json_Primitives_To_CLR_Values()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "rootCombineOperator": "And",
              "nodes": [
                {
                  "kind": "criterion",
                  "id": "x",
                  "isEnabled": true,
                  "propertyName": "IsActive",
                  "operator": "Equals",
                  "value": true
                }
              ]
            }
            """;
        var snapshot = JsonSerializer.Deserialize<FilterPipelineSnapshot>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.NotNull(snapshot);
        var pipeline = FilterPipelineSnapshotMapper.ToPipeline(snapshot!);
        var nodes = pipeline.RootNodes;
        Assert.Single(nodes);
        var c = Assert.IsType<CriterionPipelineNode>(nodes[0]);
        Assert.IsType<bool>(c.Value);
        Assert.True((bool)c.Value!);
    }

    [Fact]
    public void Compiled_Pipeline_Filters_With_BuildExpression()
    {
        var pipeline = new FilterPipeline();
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "Value", Operator = nameof(FilterOperator.Equals), Value = 2 });

        var compiled = FilterPipelineCompiler.Compile(pipeline);
        var expr = FilterExpressionBuilder.BuildExpression<Item>(compiled, pipeline.RootCombineOperator);

        Assert.True(expr.Compile()(new Item { Value = 2 }));
        Assert.False(expr.Compile()(new Item { Value = 1 }));
    }

    private sealed class Item
    {
        public int Value { get; set; }
    }
}
