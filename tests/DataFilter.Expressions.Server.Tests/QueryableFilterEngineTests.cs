using Xunit;
using DataFilter.Core.Enums;
using DataFilter.Core.Models;
using DataFilter.Expressions.Server.Services;

namespace DataFilter.Expressions.Server.Tests;

public class QueryableFilterEngineTests
{
    private class Employee
    {
        public string Name { get; set; } = string.Empty;
        public int Salary { get; set; }
    }

    private static IQueryable<Employee> GetTestData()
    {
        return new List<Employee>
        {
            new Employee { Name = "Alice", Salary = 80000 },
            new Employee { Name = "Bob", Salary = 50000 },
            new Employee { Name = "Charlie", Salary = 70000 }
        }.AsQueryable();
    }

    [Fact]
    public void Apply_WithGreaterThanFilter_FiltersCorrectly()
    {
        FilterContext context = new();
        context.AddOrUpdateDescriptor(new FilterDescriptor("Salary", FilterOperator.GreaterThan, 60000));

        QueryableFilterEngine<Employee> engine = new();
        List<Employee> result = engine.Apply(GetTestData(), context).ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, e => e.Name == "Bob");
    }

    [Fact]
    public void Apply_WithContainsFilter_FiltersCorrectly()
    {
        FilterContext context = new();
        context.AddOrUpdateDescriptor(new FilterDescriptor("Name", FilterOperator.Contains, "li"));

        QueryableFilterEngine<Employee> engine = new();
        List<Employee> result = engine.Apply(GetTestData(), context).ToList();

        Assert.Equal(2, result.Count); // Alice + Charlie
    }

    [Fact]
    public void Apply_WithSort_SortsCorrectly()
    {
        FilterContext context = new();
        context.SetSort("Salary", isDescending: true);

        QueryableFilterEngine<Employee> engine = new();
        List<Employee> result = engine.Apply(GetTestData(), context).ToList();

        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Charlie", result[1].Name);
        Assert.Equal("Bob", result[2].Name);
    }

    [Fact]
    public void Apply_NoFilters_ReturnsAllItems()
    {
        FilterContext context = new();
        QueryableFilterEngine<Employee> engine = new();
        List<Employee> result = engine.Apply(GetTestData(), context).ToList();

        Assert.Equal(3, result.Count);
    }
}
