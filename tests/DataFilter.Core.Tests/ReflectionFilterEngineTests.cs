using Xunit;
using DataFilter.Core.Engine;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;

namespace DataFilter.Core.Tests;

public class ReflectionFilterEngineTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void Apply_MultipleDescriptors_FiltersListCorrectly()
    {
        // Arrange
        var engine = new ReflectionFilterEngine<TestItem>();
        var items = new List<TestItem>
        {
            new TestItem { Name = "Alice", Age = 30 },
            new TestItem { Name = "Bob", Age = 25 },
            new TestItem { Name = "Charlie", Age = 35 }
        };

        var descriptors = new List<FilterDescriptor>
        {
            new FilterDescriptor("Age", FilterOperator.GreaterThan, 28)
        };

        // Act
        var result = engine.Apply(items, descriptors).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, x => x.Name == "Bob");
    }

    [Fact]
    public void Apply_NonGenericEnumerable_MatchesGenericEngine()
    {
        var arrayList = new System.Collections.ArrayList
        {
            new TestItem { Name = "Alice", Age = 30 },
            new TestItem { Name = "Bob", Age = 25 },
            new TestItem { Name = "Charlie", Age = 35 }
        };

        var descriptors = new List<FilterDescriptor>
        {
            new FilterDescriptor("Age", FilterOperator.GreaterThan, 28)
        };

        var generic = new ReflectionFilterEngine<TestItem>().Apply(arrayList.Cast<TestItem>(), descriptors).ToList();
        var untyped = new ReflectionFilterEngine().Apply(arrayList, typeof(TestItem), descriptors).Cast<TestItem>().ToList();

        Assert.Equal(generic.Count, untyped.Count);
        Assert.Equal(generic.Select(x => x.Name).OrderBy(x => x), untyped.Select(x => x.Name).OrderBy(x => x));
    }
}
