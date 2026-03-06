using Xunit;
using DataFilter.Core.Models;

namespace DataFilter.Core.Tests;

/// <summary>
/// Unit tests for the <see cref="FilterContext"/> class.
/// </summary>
public class FilterContextTests
{
    [Fact]
    public void SetSort_ReplacesExistingSort()
    {
        // Arrange
        var context = new FilterContext();
        context.SetSort("Property1", isDescending: true);

        // Act
        context.SetSort("Property2", isDescending: false);

        // Assert
        Assert.Single(context.SortDescriptors);
        Assert.Equal("Property2", context.SortDescriptors[0].PropertyName);
        Assert.False(context.SortDescriptors[0].IsDescending);
    }

    [Fact]
    public void AddSort_AppendsToSortList()
    {
        // Arrange
        var context = new FilterContext();
        context.SetSort("Property1", isDescending: true);

        // Act
        context.AddSort("Property2", isDescending: false);

        // Assert
        Assert.Equal(2, context.SortDescriptors.Count);
        Assert.Equal("Property1", context.SortDescriptors[0].PropertyName);
        Assert.Equal("Property2", context.SortDescriptors[1].PropertyName);
    }

    [Fact]
    public void AddSort_WithExistingProperty_UpdatesInPlace()
    {
        // Arrange
        var context = new FilterContext();
        context.SetSort("Property1", isDescending: true);
        context.AddSort("Property2", isDescending: false);

        // Act - change direction for Property1
        context.AddSort("Property1", isDescending: false);

        // Assert - still 2 descriptors, but Property1 is updated
        Assert.Equal(2, context.SortDescriptors.Count);
        var p1Sort = context.SortDescriptors.FirstOrDefault(s => s.PropertyName == "Property1");
        Assert.NotNull(p1Sort);
        Assert.False(p1Sort!.IsDescending);
    }

    [Fact]
    public void ClearSort_RemovesAllSorts()
    {
        // Arrange
        var context = new FilterContext();
        context.SetSort("Property1");
        context.AddSort("Property2");

        // Act
        context.ClearSort();

        // Assert
        Assert.Empty(context.SortDescriptors);
    }
}
