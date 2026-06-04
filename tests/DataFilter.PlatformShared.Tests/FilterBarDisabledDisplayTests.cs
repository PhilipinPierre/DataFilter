using DataFilter.Core.Enums;
using DataFilter.Core.Pipeline;
using DataFilter.PlatformShared.FilterBar;

namespace DataFilter.PlatformShared.Tests;

public class FilterBarDisabledDisplayTests
{
    [Fact]
    public void Build_Includes_Disabled_Criterion_As_Chip()
    {
        var pipeline = new FilterPipeline();
        var c = new CriterionPipelineNode
        {
            PropertyName = "Name",
            Operator = nameof(FilterOperator.Equals),
            Value = "A",
            IsEnabled = false
        };
        pipeline.RootNodes.Add(c);

        var items = FilterBarDisplayBuilder.Build(pipeline);

        var cluster = Assert.IsType<FilterBarAndClusterItem>(Assert.Single(items));
        var chip = Assert.Single(cluster.Chips);
        Assert.False(chip.IsEnabled);
        Assert.Equal(c.Id, chip.NodeId);
    }
}
