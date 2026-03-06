using Xunit;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Filtering.ExcelLike.Services;
using DataFilter.Core.Enums;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class ExcelFilterDescriptorTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void IsMatch_WithSelectedValues_FiltersCorrectly()
    {
        // Arrange
        var state = new ExcelFilterState();
        state.DistinctValues.AddRange(new[] { "Alice", "Bob", "Charlie" });
        state.SelectedValues.Add("Alice");
        state.SelectedValues.Add("Charlie");
        state.SelectAll = false;

        var descriptor = new ExcelFilterDescriptor("Name", state);

        var engine = new ExcelFilterEngine<TestItem>();
        var items = new List<TestItem>
        {
            new TestItem { Name = "Alice" },
            new TestItem { Name = "Bob" },
            new TestItem { Name = "Charlie" }
        };

        // Act
        var result = engine.Apply(items, new[] { descriptor }).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, x => x.Name == "Bob");
    }

    [Fact]
    public void IsMatch_SelectAll_NoSearch_ReturnsTrueForAll()
    {
        // Arrange
        var state = new ExcelFilterState();
        state.DistinctValues.AddRange(new[] { "Alice", "Bob" });
        state.SelectedValues.Add("Alice");
        state.SelectedValues.Add("Bob");
        state.SelectAll = true;

        var descriptor = new ExcelFilterDescriptor("Name", state);

        // Act
        var isMatchAlice = descriptor.IsMatch(new TestItem { Name = "Alice" });
        var isMatchX = descriptor.IsMatch(new TestItem { Name = "X" }); // Because Select All is true and no search, it lets everything pass

        // Assert
        Assert.True(isMatchAlice);
        Assert.True(isMatchX);
    }

    [Fact]
    public void Descriptors_WithCustomOperator_ReturnsCustomFilterOnly()
    {
        // Arrange
        var state = new ExcelFilterState();
        state.CustomOperator = FilterOperator.Contains;
        state.CustomValue1 = "lic";
        state.SelectedValues.Add("Bob"); // Manual selection that should be ignored when custom operator is set
        state.SelectAll = false;

        var descriptor = new ExcelFilterDescriptor("Name", state);

        // Act
        var descriptors = descriptor.Descriptors;

        // Assert
        Assert.Single(descriptors);
        Assert.Equal(FilterOperator.Contains, descriptors[0].Operator);
        Assert.Equal("lic", descriptors[0].Value);

        Assert.True(descriptor.IsMatch(new TestItem { Name = "Alice" }));
        Assert.False(descriptor.IsMatch(new TestItem { Name = "Bob" }));
    }

    [Fact]
    public void Descriptors_WithBetweenOperator_ProducesRangeValue()
    {
        // Arrange
        var state = new ExcelFilterState();
        state.CustomOperator = FilterOperator.Between;
        state.CustomValue1 = "10";
        state.CustomValue2 = "20";

        var descriptor = new ExcelFilterDescriptor("Age", state);

        // Act
        var descriptors = descriptor.Descriptors;

        // Assert
        Assert.Single(descriptors);
        Assert.Equal(FilterOperator.Between, descriptors[0].Operator);
        Assert.IsType<Core.Models.RangeValue>(descriptors[0].Value);
        
        var range = (Core.Models.RangeValue)descriptors[0].Value!;
        Assert.Equal("10", range.Min);
        Assert.Equal("20", range.Max);
    }
}
