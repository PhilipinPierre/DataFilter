using Xunit;
using DataFilter.Filtering.ExcelLike.Services;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class DistinctValuesExtractorTests
{
    private class TestItem
    {
        public string Category { get; set; } = string.Empty;
        public int Number { get; set; }
    }

    [Fact]
    public void Extract_ReturnsDistinctAndSortedValues()
    {
        // Arrange
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { Category = "B", Number = 2 },
            new TestItem { Category = "A", Number = 1 },
            new TestItem { Category = "B", Number = 3 },
            new TestItem { Category = "C", Number = 1 }
        };

        // Act
        var categories = extractor.Extract(items, "Category").ToList();
        var numbers = extractor.Extract(items, "Number").ToList();

        // Assert
        Assert.Equal(3, categories.Count);
        Assert.Equal("A", categories[0]);
        Assert.Equal("B", categories[1]);
        Assert.Equal("C", categories[2]);

        Assert.Equal(3, numbers.Count);
        Assert.Equal(1, numbers[0]);
        Assert.Equal(2, numbers[1]);
        Assert.Equal(3, numbers[2]);
    }
}
