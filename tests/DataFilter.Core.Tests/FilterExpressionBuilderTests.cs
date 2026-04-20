using Xunit;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;

namespace DataFilter.Core.Tests;

public class FilterExpressionBuilderTests
{
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void BuildExpression_Contains_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.Contains, "test");
        var items = new List<TestItem>
        {
            new TestItem { Name = "test item 1" },
            new TestItem { Name = "another item" }
        };

        var expr = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor);
        var func = expr.Compile();

        Assert.True(func(items[0]));
        Assert.False(func(items[1]));
    }

    [Fact]
    public void BuildExpression_Contains_IsCaseInsensitive()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.Contains, "TEST");
        var item = new TestItem { Name = "test item" };

        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(item));
    }

    [Fact]
    public void BuildExpression_NotContains_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.NotContains, "test");
        var items = new List<TestItem>
        {
            new TestItem { Name = "test item" },
            new TestItem { Name = "another item" }
        };

        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.False(func(items[0]));
        Assert.True(func(items[1]));
    }

    [Fact]
    public void BuildExpression_GreaterThan_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Value", FilterOperator.GreaterThan, 5);
        var items = new List<TestItem>
        {
            new TestItem { Value = 10 },
            new TestItem { Value = 2 }
        };

        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(items[0]));
        Assert.False(func(items[1]));
    }

    [Fact]
    public void BuildExpression_GreaterThanOrEqual_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Value", FilterOperator.GreaterThanOrEqual, 5);
        var items = new List<TestItem>
        {
            new TestItem { Value = 5 },
            new TestItem { Value = 4 }
        };

        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(items[0]));
        Assert.False(func(items[1]));
    }

    [Fact]
    public void BuildExpression_LessThanOrEqual_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Value", FilterOperator.LessThanOrEqual, 10);
        var items = new List<TestItem>
        {
            new TestItem { Value = 10 },
            new TestItem { Value = 11 }
        };

        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(items[0]));
        Assert.False(func(items[1]));
    }

    [Fact]
    public void BuildExpression_Between_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Value", FilterOperator.Between, new RangeValue(5, 10));
        var items = new List<TestItem>
        {
            new TestItem { Value = 7 },
            new TestItem { Value = 3 },
            new TestItem { Value = 15 }
        };

        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(items[0]));
        Assert.False(func(items[1]));
        Assert.False(func(items[2]));
    }

    [Fact]
    public void BuildExpression_StartsWith_IsCaseInsensitive()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.StartsWith, "ALICE");
        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(new TestItem { Name = "Alice Smith" }));
        Assert.False(func(new TestItem { Name = "Bob Alice" }));
    }

    [Fact]
    public void BuildExpression_StartsWith_Wildcard_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.StartsWith, "Al*");
        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(new TestItem { Name = "Alice Smith" }));
        Assert.True(func(new TestItem { Name = "Al" }));
        Assert.False(func(new TestItem { Name = "Bob" }));
    }

    [Fact]
    public void BuildExpression_EndsWith_IsCaseInsensitive()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.EndsWith, "SMITH");
        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(new TestItem { Name = "Alice Smith" }));
        Assert.False(func(new TestItem { Name = "Smith Alice" }));
    }

    [Fact]
    public void BuildExpression_Equals_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.Equals, "alice");
        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(new TestItem { Name = "alice" }));
        Assert.False(func(new TestItem { Name = "alice smith" }));
    }

    [Fact]
    public void BuildExpression_Equals_Wildcard_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Name", FilterOperator.Equals, "ali*");
        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(new TestItem { Name = "alice" }));
        Assert.True(func(new TestItem { Name = "alice smith" }));
        Assert.False(func(new TestItem { Name = "bob" }));
    }

    [Fact]
    public void BuildExpression_In_FiltersCorrectly()
    {
        var descriptor = new FilterDescriptor("Value", FilterOperator.In, new[] { 1, 3, 5 });
        var func = Engine.FilterExpressionBuilder.BuildExpression<TestItem>(descriptor).Compile();

        Assert.True(func(new TestItem { Value = 3 }));
        Assert.False(func(new TestItem { Value = 4 }));
    }
}
