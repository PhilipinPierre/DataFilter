using Xunit;
using DataFilter.Expressions.Server.Services;

namespace DataFilter.Expressions.Server.Tests;

public class TopNFilterTests
{
    private class Employee
    {
        public string Name { get; set; } = string.Empty;
        public int Salary { get; set; }
    }

    private static IQueryable<Employee> Employees() => new List<Employee>
    {
        new Employee { Name = "Alice", Salary = 80000 },
        new Employee { Name = "Bob", Salary = 50000 },
        new Employee { Name = "Charlie", Salary = 70000 },
        new Employee { Name = "Dave", Salary = 30000 }
    }.AsQueryable();

    [Fact]
    public void TopHighest_Returns_CorrectCount_SortedDescending()
    {
        TopNFilter<Employee> filter = new();
        List<Employee> result = filter.TopHighest(Employees(), "Salary", 2).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Charlie", result[1].Name);
    }

    [Fact]
    public void TopLowest_Returns_CorrectCount_SortedAscending()
    {
        TopNFilter<Employee> filter = new();
        List<Employee> result = filter.TopLowest(Employees(), "Salary", 2).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Dave", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
    }
}
