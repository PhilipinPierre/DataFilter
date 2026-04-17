using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Core.Services;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;
using DataFilter.Core.Pipeline;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class FilterDescriptorToExcelConverterTests
{
    [Fact]
    public void Convert_SingleEqualsCriterion_ProducesExcelDescriptorWithCustomOperator()
    {
        var pipeline = new FilterPipeline();
        pipeline.RootNodes.Add(new CriterionPipelineNode
        {
            PropertyName = "IsActive",
            Operator = nameof(FilterOperator.Equals),
            Value = true,
            IsEnabled = true
        });

        var compiled = FilterPipelineCompiler.Compile(pipeline);
        var converted = FilterDescriptorToExcelConverter.ConvertCompiledPipeline(compiled);

        Assert.Single(converted);
        var excel = Assert.IsType<ExcelFilterDescriptor>(converted[0]);
        Assert.Equal("IsActive", excel.PropertyName);
        Assert.Equal(FilterOperator.Equals, excel.State.CustomOperator);
        Assert.Equal(true, excel.State.CustomValue1);
    }
}
