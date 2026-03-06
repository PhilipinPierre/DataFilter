using DataFilter.Wpf.Demo.Models;

namespace DataFilter.Wpf.Demo.Services;

public static class EmployeeDataGenerator
{
    public static List<Employee> Generate(int count)
    {
        var random = new Random(42);
        var depts = new[] { "IT", "HR", "Sales", "Marketing", "Engineering" };
        var countries = new[] { "France", "USA", "UK", "Germany", "Japan" };
        var names = new[] { "Alice", "Bob", "Charlie", "David", "Eva", "Frank", "Grace", "Henry", "Ivy", "Jack" };

        var list = new List<Employee>();
        for (int i = 1; i <= count; i++)
        {
            list.Add(new Employee
            {
                Id = i,
                Name = $"{names[random.Next(names.Length)]} {random.Next(100, 999)}",
                Department = depts[random.Next(depts.Length)],
                Country = countries[random.Next(countries.Length)],
                Salary = random.Next(40000, 150000),
                HireDate = DateTime.Today.AddDays(-random.Next(1, 3650)),
                IsActive = random.NextDouble() > 0.2
            });
        }
        return list;
    }
}
