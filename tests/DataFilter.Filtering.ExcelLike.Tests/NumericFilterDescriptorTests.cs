using DataFilter.Core.Enums;
using Xunit;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class NumericFilterDescriptorTests
{
    private class TestItem
    {
        public double Salary { get; set; }
    }

    [Fact]
    public void IsMatch_GreaterThan_FiltersCorrectly()
    {
        var descriptor = new NumericFilterDescriptor("Salary", FilterOperator.GreaterThan, 50000.0);
        Assert.True(descriptor.IsMatch(new TestItem { Salary = 60000 }));
        Assert.False(descriptor.IsMatch(new TestItem { Salary = 40000 }));
    }

    [Fact]
    public void IsMatch_LessThanOrEqual_FiltersCorrectly()
    {
        var descriptor = new NumericFilterDescriptor("Salary", FilterOperator.LessThanOrEqual, 50000.0);
        Assert.True(descriptor.IsMatch(new TestItem { Salary = 50000 }));
        Assert.False(descriptor.IsMatch(new TestItem { Salary = 50001 }));
    }

    [Fact]
    public void IsMatch_Between_FiltersCorrectly()
    {
        var descriptor = NumericFilterDescriptor.Between("Salary", 30000.0, 60000.0);
        Assert.True(descriptor.IsMatch(new TestItem { Salary = 45000 }));
        Assert.True(descriptor.IsMatch(new TestItem { Salary = 30000 }));
        Assert.True(descriptor.IsMatch(new TestItem { Salary = 60000 }));
        Assert.False(descriptor.IsMatch(new TestItem { Salary = 70000 }));
    }
}
