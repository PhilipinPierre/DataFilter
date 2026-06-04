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

    [Fact]
    public void Editor_AddAnd_At_Root_And_Inserts_Sibling()
    {
        var pipeline = new FilterPipeline();
        var a = new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 };
        pipeline.RootNodes.Add(a);

        var added = FilterPipelineEditor.AddAndCriterion(pipeline, a.Id);

        Assert.NotNull(added);
        Assert.Equal("A", added.PropertyName);
        Assert.Equal(2, pipeline.RootNodes.Count);
        Assert.Equal(a.Id, pipeline.RootNodes[0].Id);
    }

    [Fact]
    public void Editor_AddAnd_Returns_Null_When_Anchor_Has_No_Column()
    {
        var pipeline = new FilterPipeline();
        var a = new CriterionPipelineNode { PropertyName = "", Operator = nameof(FilterOperator.Equals), Value = 1 };
        pipeline.RootNodes.Add(a);

        Assert.Null(FilterPipelineEditor.AddAndCriterion(pipeline, a.Id));
    }

    [Fact]
    public void Editor_AddAnd_Under_Or_Root_Wraps_In_And_Group()
    {
        var pipeline = new FilterPipeline { RootCombineOperator = LogicalOperator.Or };
        var a = new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 };
        pipeline.RootNodes.Add(a);

        var added = FilterPipelineEditor.AddAndCriterion(pipeline, a.Id);

        Assert.NotNull(added);
        Assert.Single(pipeline.RootNodes);
        var g = Assert.IsType<GroupPipelineNode>(pipeline.RootNodes[0]);
        Assert.Equal(LogicalOperator.And, g.CombineOperator);
        Assert.Equal(2, g.Children.Count);
    }

    [Fact]
    public void Compiler_Skips_Criterion_With_Empty_PropertyName()
    {
        var pipeline = new FilterPipeline();
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 });
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "", Operator = nameof(FilterOperator.Equals), Value = 2 });

        var compiled = FilterPipelineCompiler.Compile(pipeline);

        Assert.Single(compiled);
    }

    [Fact]
    public void Editor_AddAnd_On_And_Group_Adds_Child()
    {
        var pipeline = new FilterPipeline();
        var g = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g.Children.Add(new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 });
        pipeline.RootNodes.Add(g);

        var added = FilterPipelineEditor.AddAndCriterion(pipeline, g.Id);

        Assert.NotNull(added);
        Assert.Equal("A", added.PropertyName);
        Assert.Equal(2, g.Children.Count);
    }

    [Fact]
    public void Editor_AddOrGroup_On_Empty_Pipeline_Creates_And_Group()
    {
        var pipeline = new FilterPipeline();
        var added = FilterPipelineEditor.AddOrGroup(pipeline, "Department");

        Assert.NotNull(added);
        Assert.Equal("Department", added.PropertyName);
        Assert.Single(pipeline.RootNodes);
        var g = Assert.IsType<GroupPipelineNode>(pipeline.RootNodes[0]);
        Assert.Equal(LogicalOperator.And, g.CombineOperator);
    }

    [Fact]
    public void Editor_AddOrGroup_Wraps_Existing_And_Root_In_Or()
    {
        var pipeline = new FilterPipeline();
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "Department", Operator = nameof(FilterOperator.Equals), Value = "IT" });
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "Name", Operator = nameof(FilterOperator.StartsWith), Value = "Alice" });

        var added = FilterPipelineEditor.AddOrGroup(pipeline, "Department");

        Assert.NotNull(added);
        Assert.Equal(LogicalOperator.Or, pipeline.RootCombineOperator);
        Assert.Equal(2, pipeline.RootNodes.Count);
        var first = Assert.IsType<GroupPipelineNode>(pipeline.RootNodes[0]);
        Assert.Equal(2, first.Children.Count);
        var second = Assert.IsType<GroupPipelineNode>(pipeline.RootNodes[1]);
        Assert.Single(second.Children);
    }

    [Fact]
    public void Editor_MoveCriterion_To_And_Group()
    {
        var pipeline = new FilterPipeline { RootCombineOperator = LogicalOperator.Or };
        var g1 = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g1.Children.Add(new CriterionPipelineNode("a") { PropertyName = "Department", Operator = nameof(FilterOperator.Equals), Value = "IT" });
        var g2 = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g2.Children.Add(new CriterionPipelineNode("b") { PropertyName = "Name", Operator = nameof(FilterOperator.StartsWith), Value = "Bob" });
        pipeline.RootNodes.Add(g1);
        pipeline.RootNodes.Add(g2);

        Assert.True(FilterPipelineEditor.MoveCriterionToCluster(pipeline, "b", g1.Id));
        Assert.Equal(2, g1.Children.Count);
        Assert.Single(pipeline.RootNodes);
    }

    [Fact]
    public void Editor_MoveCriterion_To_Or_Gap_Creates_New_Group()
    {
        var pipeline = new FilterPipeline { RootCombineOperator = LogicalOperator.Or };
        var g1 = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g1.Children.Add(new CriterionPipelineNode("a") { PropertyName = "Department", Operator = nameof(FilterOperator.Equals), Value = "IT" });
        var g2 = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g2.Children.Add(new CriterionPipelineNode("b") { PropertyName = "Name", Operator = nameof(FilterOperator.StartsWith), Value = "Bob" });
        pipeline.RootNodes.Add(g1);
        pipeline.RootNodes.Add(g2);

        Assert.True(FilterPipelineEditor.MoveCriterionToOrGap(pipeline, "b", 1));
        Assert.Equal(2, pipeline.RootNodes.Count);
        Assert.Single(g1.Children);
        var inserted = Assert.IsType<GroupPipelineNode>(pipeline.RootNodes[1]);
        Assert.Single(inserted.Children);
        Assert.Equal("b", inserted.Children[0].Id);
    }

    [Fact]
    public void IdMerger_Preserves_Criterion_Id_On_Sync()
    {
        var existing = new FilterPipeline();
        var c = new CriterionPipelineNode("stable-id") { PropertyName = "X", Operator = nameof(FilterOperator.Equals), Value = 1 };
        existing.RootNodes.Add(c);

        var incoming = new FilterPipeline();
        incoming.RootNodes.Add(new CriterionPipelineNode { PropertyName = "X", Operator = nameof(FilterOperator.Equals), Value = 1 });

        FilterPipelineIdMerger.MergeIds(existing, incoming);

        var merged = Assert.IsType<CriterionPipelineNode>(incoming.RootNodes[0]);
        Assert.Equal("stable-id", merged.Id);
    }

    private sealed class Item
    {
        public int Value { get; set; }
    }
}
