using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Engine;
using DataFilter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFilter.Blazor.Demo.Shared.Services;

public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>
{
}

public class MockEmployeeApiService : IMockEmployeeApiService
{
    private readonly int _count;

    public MockEmployeeApiService(int count)
    {
        _count = count;
    }

    public async Task<PagedResult<Employee>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken); // Simulate API delay

        var data = EmployeeDataGenerator.Generate(_count);
        var query = data.AsQueryable();
        
        // Filter
        if (context.Descriptors.Any())
        {
            var predicate = FilterExpressionBuilder.BuildExpression<Employee>(context.Descriptors.ToList());
            query = query.Where(predicate);
        }

        int totalCount = query.Count();

        // Sort
        if (context.SortDescriptors.Any())
        {
            var primary = context.SortDescriptors.First();
            query = ApplySort(query, primary.PropertyName, !primary.IsDescending); // Wait, IsDescending? I'll check IAsyncDataProvider again
            // In my previous impl I had bool sortAscending. 
            // In ApplySort I have bool ascending.
        }

        int skip = (context.Page - 1) * context.PageSize;
        int take = context.PageSize;
        var items = query.Skip(skip).Take(take).ToList();

        return new PagedResult<Employee>
        {
            Items = items,
            TotalCount = totalCount,
            Page = context.Page,
            PageSize = context.PageSize
        };
    }

    public async Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText = "", CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken); // Simulate API delay

        var query = EmployeeDataGenerator.Generate(_count).AsQueryable();

        // Simple distinct for mock
        var result = propertyName switch
        {
            "Department" => query.Select(x => x.Department).Distinct(),
            "Country" => query.Select(x => x.Country).Distinct(),
            "Name" => query.Select(x => x.Name).Distinct(),
            _ => query.Select(x => x.Id.ToString()).Distinct()
        };

        if (!string.IsNullOrEmpty(searchText))
        {
            result = result.Where(v => v != null && v.ToString()!.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        return result.Cast<object>().Take(100).ToList();
    }

    private IQueryable<Employee> ApplySort(IQueryable<Employee> query, string field, bool ascending)
    {
        return field switch
        {
            "Id" => ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
            "Name" => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
            "Department" => ascending ? query.OrderBy(x => x.Department) : query.OrderByDescending(x => x.Department),
            "Country" => ascending ? query.OrderBy(x => x.Country) : query.OrderByDescending(x => x.Country),
            "Salary" => ascending ? query.OrderBy(x => x.Salary) : query.OrderByDescending(x => x.Salary),
            "HireDate" => ascending ? query.OrderBy(x => x.HireDate) : query.OrderByDescending(x => x.HireDate),
            _ => query
        };
    }
}
