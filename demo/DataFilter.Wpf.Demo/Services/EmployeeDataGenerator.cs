using DataFilter.Wpf.Demo.Models;
using System;
using System.Collections.Concurrent;

namespace DataFilter.Wpf.Demo.Services;

public static class EmployeeDataGenerator
{
    private static string[] depts = new[] { "IT", "HR", "Sales", "Marketing", "Engineering" };
    private static string[] countries = new[] { "France", "USA", "UK", "Germany", "Japan" };
    private static string[] names = new[] { "Alice", "Bob", "Charlie", "David", "Eva", "Frank", "Grace", "Henry", "Ivy", "Jack" };
    private static string[] pooledNames;

    static EmployeeDataGenerator()
    {
        // Pre-generate all combinations to avoid string allocations during generation
        pooledNames = new string[names.Length * 900];
        int idx = 0;
        foreach (var name in names)
        {
            for (int i = 100; i < 1000; i++)
            {
                pooledNames[idx++] = $"{name} {i}";
            }
        }
        Employees = Generate(1000);
    }

    public static List<Employee> Employees { get; private set; }

    public static void Regenerate(long count)
    {
        Employees = Generate(count);
    }

    public static List<Employee> Generate_Short(int count)
    {
        var today = DateTime.Today;
        var result = new Employee[count];

        for (long i = 0; i < count; i++)
        {
            var rng = new XorShift128((uint)(i + 1));

            int ni = (int)i;

            result[ni] = new Employee
            {
                Id = ni + 1,
                Name = names[rng.Next(names.Length)] + " " + rng.Next(100, 999),
                Department = depts[rng.Next(depts.Length)],
                Country = countries[rng.Next(countries.Length)],
                Salary = rng.Next(40000, 150000),
                HireDate = today.AddDays(-rng.Next(1, 3650)),
                Time = new TimeSpan(rng.Next(24), rng.Next(60), rng.Next(60)),
                IsActive = rng.NextDouble() > 0.2
            };

        }

        return result.ToList();
    }

    public static List<Employee> Generate(long count)
    {
        if(count <= 100000)
            return Generate_Short((int)count);

        var today = DateTime.Today;
        var result = new Employee[count];

        // Use a single loop with parallel partitioner for all counts
        var rangePartitioner = Partitioner.Create(0L, count, 100_000);

        Parallel.ForEach(rangePartitioner, range =>
        {
            var rng = new XorShift128((uint)(range.Item1 + 1));

            for (long i = range.Item1; i < range.Item2; i++)
            {
                int ni = (int)i;
                result[ni] = new Employee
                {
                    Id = ni + 1,
                    Name = pooledNames[rng.Next(pooledNames.Length)],
                    Department = depts[rng.Next(depts.Length)],
                    Country = countries[rng.Next(countries.Length)],
                    Salary = (float)rng.Next(40000, 150000),
                    HireDate = today.AddDays(-rng.Next(1, 3650)),
                    Time = new TimeSpan(rng.Next(24), rng.Next(60), rng.Next(60)),
                    IsActive = rng.NextDouble() > 0.2
                };
            }
        });

        return new List<Employee>(result);
    }
}
