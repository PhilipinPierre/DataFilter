using DataFilter.Core.Enums;
using Xunit;
using DataFilter.Filtering.ExcelLike.Models;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class TextFilterDescriptorTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void IsMatch_Contains_CaseInsensitive_ReturnsTrue()
    {
        var descriptor = new TextFilterDescriptor("Name", FilterOperator.Contains, "ALICE");
        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice Smith" }));
    }

    [Fact]
    public void IsMatch_NotContains_ReturnsTrue_WhenNotPresent()
    {
        var descriptor = new TextFilterDescriptor("Name", FilterOperator.NotContains, "bob");
        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice Smith" }));
        Assert.False(descriptor.IsMatch(new TestItem { Name = "Bob Jones" }));
    }

    [Fact]
    public void IsMatch_StartsWith_FiltersCorrectly()
    {
        var descriptor = new TextFilterDescriptor("Name", FilterOperator.StartsWith, "Al");
        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice" }));
        Assert.False(descriptor.IsMatch(new TestItem { Name = "Bob Alice" }));
    }

    [Fact]
    public void IsMatch_EndsWith_FiltersCorrectly()
    {
        var descriptor = new TextFilterDescriptor("Name", FilterOperator.EndsWith, "smith");
        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice Smith" }));
        Assert.False(descriptor.IsMatch(new TestItem { Name = "Smith Alice" }));
    }

    [Fact]
    public void IsMatch_CompositeOr_FiltersCorrectly()
    {
        var descriptor = new TextFilterDescriptor(
            "Name",
            FilterOperator.Contains, "Alice",
            Core.Enums.LogicalOperator.Or,
            FilterOperator.Contains, "Bob");

        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice" }));
        Assert.True(descriptor.IsMatch(new TestItem { Name = "Bob" }));
        Assert.False(descriptor.IsMatch(new TestItem { Name = "Charlie" }));
    }

    [Fact]
    public void IsMatch_CompositeAnd_FiltersCorrectly()
    {
        var descriptor = new TextFilterDescriptor(
            "Name",
            FilterOperator.Contains, "Alice",
            Core.Enums.LogicalOperator.And,
            FilterOperator.Contains, "Smith");

        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice Smith" }));
        Assert.False(descriptor.IsMatch(new TestItem { Name = "Alice Jones" }));
    }
}
