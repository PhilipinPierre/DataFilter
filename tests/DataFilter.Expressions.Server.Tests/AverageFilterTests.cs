using Xunit;
using DataFilter.Expressions.Server.Services;

namespace DataFilter.Expressions.Server.Tests;

public class AverageFilterTests
{
    private class Employee
    {
        public string Name { get; set; } = string.Empty;
        public int Salary { get; set; }
    }

    // Average = (80000 + 50000 + 70000 + 30000) / 4 = 57500
    private static IQueryable<Employee> Employees() => new List<Employee>
    {
        new Employee { Name = "Alice", Salary = 80000 },
        new Employee { Name = "Bob", Salary = 50000 },
        new Employee { Name = "Charlie", Salary = 70000 },
        new Employee { Name = "Dave", Salary = 30000 }
    }.AsQueryable();

    [Fact]
    public void AboveAverage_ReturnsItemsWithSalaryAbove57500()
    {
        AverageFilter<Employee> filter = new();
        List<Employee> result = filter.AboveAverage(Employees(), "Salary").ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Name == "Alice");
        Assert.Contains(result, e => e.Name == "Charlie");
    }

    [Fact]
    public void BelowAverage_ReturnsItemsWithSalaryBelow57500()
    {
        AverageFilter<Employee> filter = new();
        List<Employee> result = filter.BelowAverage(Employees(), "Salary").ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Name == "Bob");
        Assert.Contains(result, e => e.Name == "Dave");
    }
}
