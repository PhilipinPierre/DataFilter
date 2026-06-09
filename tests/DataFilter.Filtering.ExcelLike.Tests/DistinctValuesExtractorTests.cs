using Xunit;
using DataFilter.Filtering.ExcelLike.Services;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class DistinctValuesExtractorTests
{
    private class TestItem
    {
        public string Category { get; set; } = string.Empty;
        public int Number { get; set; }
        public DateTime HireDate { get; set; }
        public TimeSpan ShiftStart { get; set; }
#if NET6_0_OR_GREATER
        public DateOnly BirthDate { get; set; }
        public TimeOnly BreakTime { get; set; }
#endif
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

    [Fact]
    public void Extract_IncludesNullOnce_WhenPropertyHasNullValues()
    {
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { Category = "A" },
            new TestItem { Category = null! },
            new TestItem { Category = "B" },
            new TestItem { Category = null! }
        };

        var categories = extractor.Extract(items, "Category").ToList();

        Assert.Equal(3, categories.Count);
        Assert.Equal("A", categories[0]);
        Assert.Equal("B", categories[1]);
        Assert.Null(categories[2]);
    }

    [Fact]
    public void Extract_ReturnsOnlyNull_WhenAllValuesAreNull()
    {
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { Category = null! },
            new TestItem { Category = null! }
        };

        var categories = extractor.Extract(items, "Category").ToList();

        Assert.Single(categories);
        Assert.Null(categories[0]);
    }

    [Fact]
    public void Extract_DateTime_ReturnsDistinctCalendarDays()
    {
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { HireDate = new DateTime(2024, 3, 15, 8, 30, 0) },
            new TestItem { HireDate = new DateTime(2024, 3, 15, 17, 45, 0) },
            new TestItem { HireDate = new DateTime(2024, 6, 1, 12, 0, 0) }
        };

        var dates = extractor.Extract(items, "HireDate").Cast<DateTime>().ToList();

        Assert.Equal(2, dates.Count);
        Assert.Equal(new DateTime(2024, 3, 15), dates[0]);
        Assert.Equal(new DateTime(2024, 6, 1), dates[1]);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void Extract_DateOnly_ReturnsDistinctSortedValues()
    {
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { BirthDate = new DateOnly(2024, 12, 1) },
            new TestItem { BirthDate = new DateOnly(2024, 3, 15) },
            new TestItem { BirthDate = new DateOnly(2024, 3, 15) }
        };

        var dates = extractor.Extract(items, "BirthDate").Cast<DateOnly>().ToList();

        Assert.Equal(2, dates.Count);
        Assert.Equal(new DateOnly(2024, 3, 15), dates[0]);
        Assert.Equal(new DateOnly(2024, 12, 1), dates[1]);
    }
#endif

    [Fact]
    public void Extract_TimeSpan_ReturnsDistinctSortedValues()
    {
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { ShiftStart = new TimeSpan(14, 30, 0) },
            new TestItem { ShiftStart = new TimeSpan(8, 15, 30) },
            new TestItem { ShiftStart = new TimeSpan(8, 15, 30) }
        };

        var times = extractor.Extract(items, "ShiftStart").Cast<TimeSpan>().ToList();

        Assert.Equal(2, times.Count);
        Assert.Equal(new TimeSpan(8, 15, 30), times[0]);
        Assert.Equal(new TimeSpan(14, 30, 0), times[1]);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void Extract_TimeOnly_ReturnsDistinctSortedValues()
    {
        var extractor = new DistinctValuesExtractor();
        var items = new List<TestItem>
        {
            new TestItem { BreakTime = new TimeOnly(12, 0, 0) },
            new TestItem { BreakTime = new TimeOnly(9, 30, 15) },
            new TestItem { BreakTime = new TimeOnly(9, 30, 15) }
        };

        var times = extractor.Extract(items, "BreakTime").Cast<TimeOnly>().ToList();

        Assert.Equal(2, times.Count);
        Assert.Equal(new TimeOnly(9, 30, 15), times[0]);
        Assert.Equal(new TimeOnly(12, 0, 0), times[1]);
    }
#endif
}
