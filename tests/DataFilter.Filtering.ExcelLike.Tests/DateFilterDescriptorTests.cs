using DataFilter.Core.Enums;
using DataFilter.Filtering.ExcelLike.Enums;
using Xunit;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class DateFilterDescriptorTests
{
    private class TestItem
    {
        public DateTime HireDate { get; set; }
    }

    [Fact]
    public void IsMatch_Before_FiltersCorrectly()
    {
        var descriptor = new DateFilterDescriptor("HireDate", FilterOperator.LessThan, new DateTime(2024, 1, 1));
        Assert.True(descriptor.IsMatch(new TestItem { HireDate = new DateTime(2023, 6, 15) }));
        Assert.False(descriptor.IsMatch(new TestItem { HireDate = new DateTime(2025, 1, 1) }));
    }

    [Fact]
    public void IsMatch_Between_FiltersCorrectly()
    {
        var descriptor = new DateFilterDescriptor(
            "HireDate",
            new DateTime(2023, 1, 1),
            new DateTime(2023, 12, 31));

        Assert.True(descriptor.IsMatch(new TestItem { HireDate = new DateTime(2023, 6, 15) }));
        Assert.False(descriptor.IsMatch(new TestItem { HireDate = new DateTime(2024, 1, 1) }));
    }

    [Fact]
    public void IsMatch_Today_Period_FiltersCorrectly()
    {
        var descriptor = new DateFilterDescriptor("HireDate", DatePeriod.Today);
        Assert.True(descriptor.IsMatch(new TestItem { HireDate = DateTime.Today }));
        Assert.False(descriptor.IsMatch(new TestItem { HireDate = DateTime.Today.AddDays(-1) }));
    }

    [Fact]
    public void IsMatch_ThisYear_Period_FiltersCorrectly()
    {
        var descriptor = new DateFilterDescriptor("HireDate", DatePeriod.ThisYear);
        Assert.True(descriptor.IsMatch(new TestItem { HireDate = new DateTime(DateTime.Today.Year, 6, 15) }));
        Assert.False(descriptor.IsMatch(new TestItem { HireDate = new DateTime(DateTime.Today.Year - 1, 6, 15) }));
    }
}
