using DataFilter.Core.Enums;
using DataFilter.Core.Pipeline;
using DataFilter.PlatformShared.FilterBar;

namespace DataFilter.PlatformShared.Tests;

public class FilterBarDisplayBuilderTests
{
    [Fact]
    public void Build_Root_And_Produces_Single_Cluster()
    {
        var pipeline = new FilterPipeline();
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 });
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "B", Operator = nameof(FilterOperator.Equals), Value = 2 });

        var items = FilterBarDisplayBuilder.Build(pipeline);

        var cluster = Assert.Single(items);
        var and = Assert.IsType<FilterBarAndClusterItem>(cluster);
        Assert.Equal(2, and.Chips.Count);
    }

    [Fact]
    public void Build_Root_Or_With_Two_And_Groups_Produces_Separator()
    {
        var pipeline = new FilterPipeline { RootCombineOperator = LogicalOperator.Or };
        var g1 = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g1.Children.Add(new CriterionPipelineNode { PropertyName = "Department", Operator = nameof(FilterOperator.Equals), Value = "IT" });
        g1.Children.Add(new CriterionPipelineNode { PropertyName = "Name", Operator = nameof(FilterOperator.StartsWith), Value = "Alice" });
        var g2 = new GroupPipelineNode { CombineOperator = LogicalOperator.And };
        g2.Children.Add(new CriterionPipelineNode { PropertyName = "Department", Operator = nameof(FilterOperator.Equals), Value = "RH" });
        g2.Children.Add(new CriterionPipelineNode { PropertyName = "Name", Operator = nameof(FilterOperator.StartsWith), Value = "Bob" });
        pipeline.RootNodes.Add(g1);
        pipeline.RootNodes.Add(g2);

        var items = FilterBarDisplayBuilder.Build(pipeline);

        Assert.Equal(3, items.Count);
        Assert.IsType<FilterBarAndClusterItem>(items[0]);
        Assert.IsType<FilterBarOrSeparatorItem>(items[1]);
        Assert.IsType<FilterBarAndClusterItem>(items[2]);
        Assert.Equal(2, ((FilterBarAndClusterItem)items[0]).Chips.Count);
    }

    [Fact]
    public void Build_Root_Or_Produces_Separator()
    {
        var pipeline = new FilterPipeline { RootCombineOperator = LogicalOperator.Or };
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 });
        pipeline.RootNodes.Add(new CriterionPipelineNode { PropertyName = "B", Operator = nameof(FilterOperator.Equals), Value = 2 });

        var items = FilterBarDisplayBuilder.Build(pipeline);

        Assert.Equal(3, items.Count);
        Assert.IsType<FilterBarAndClusterItem>(items[0]);
        Assert.IsType<FilterBarOrSeparatorItem>(items[1]);
        Assert.IsType<FilterBarAndClusterItem>(items[2]);
    }
}
