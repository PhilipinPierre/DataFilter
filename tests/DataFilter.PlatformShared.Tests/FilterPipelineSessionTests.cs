using DataFilter.Core.Abstractions;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Pipeline;
using DataFilter.PlatformShared.Pipeline;

namespace DataFilter.PlatformShared.Tests;

public class FilterPipelineSessionTests
{
    [Fact]
    public void SyncFromContext_Preserves_Id_When_Criterion_Unchanged()
    {
        var ctx = new FilterContext();
        ctx.AddOrUpdateDescriptor(new FilterDescriptor("Name", FilterOperator.Equals, "x"));

        var session = new FilterPipelineSession();
        session.SyncFromContext(ctx);
        string firstId = ((CriterionPipelineNode)session.Pipeline.RootNodes[0]).Id;

        session.SyncFromContext(ctx);

        Assert.Equal(firstId, ((CriterionPipelineNode)session.Pipeline.RootNodes[0]).Id);
    }

    [Fact]
    public void RemoveNode_Raises_PipelineChanged()
    {
        var session = new FilterPipelineSession();
        var c = new CriterionPipelineNode { PropertyName = "A", Operator = nameof(FilterOperator.Equals), Value = 1 };
        session.Pipeline.RootNodes.Add(c);
        int changes = 0;
        session.PipelineChanged += (_, _) => changes++;

        Assert.True(session.RemoveNode(c.Id));
        Assert.Equal(0, session.Pipeline.RootNodes.Count);
        Assert.Equal(1, changes);
    }
}
