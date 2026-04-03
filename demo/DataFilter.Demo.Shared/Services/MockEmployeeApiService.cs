using System.Threading.Tasks;
using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.Demo.Shared.Services;

/// <summary>
/// Mock implementation of an async data provider for employee data.
/// </summary>
public class MockEmployeeApiService : IMockEmployeeApiService
{
    private List<Employee> _allData;
    private readonly IExcelFilterEngine<Employee> _engine;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockEmployeeApiService"/> class.
    /// </summary>
    /// <param name="engine">The filter engine to use for evaluations.</param>
    /// <param name="initialCount">The initial number of employees to generate.</param>
    public MockEmployeeApiService(IExcelFilterEngine<Employee> engine, int initialCount = 1000)
    {
        _engine = engine;
        _allData = EmployeeDataGenerator.Generate(initialCount);
    }

    /// <inheritdoc />
    public void Regenerate(int count)
    {
        _allData = EmployeeDataGenerator.Generate(count);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Employee>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(500, cancellationToken);

        var filteredData = _engine.Apply(_allData, context.Descriptors).ToList();

        // Sort
        if (context.SortDescriptors != null && context.SortDescriptors.Any())
        {
            // Simple multi-sort support via engine or manual logic
            // For now, let's use the engine's sorting capabilities if implemented or simple LINQ
            var first = context.SortDescriptors.First();
            var query = filteredData.AsQueryable();
            
            // In a real API, we'd use dynamic LINQ or a query builder.
            // Here we use the engine's apply method which should handle descriptors.
            // Wait, the engine handles filter descriptors. Sorting is usually separate.
            
            // Simplified sorting for mock
            filteredData = ApplySort(query, first.PropertyName, !first.IsDescending).ToList();
        }

        // Pagination
        var paginated = filteredData
            .Skip((context.Page - 1) * context.PageSize)
            .Take(context.PageSize)
            .ToList();

        return new PagedResult<Employee>
        {
            Items = paginated,
            TotalCount = filteredData.Count,
            Page = context.Page,
            PageSize = context.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<object>> FetchDistinctValuesAsync(string propertyName, string searchText = "", CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(300, cancellationToken);

        var distincts = _engine.DistinctValuesExtractor.Extract(_allData, propertyName);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            distincts = distincts.Where(x => x?.ToString()?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
        }

        return distincts.ToList();
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

    async Task<PagedResult<object>> IAsyncDataProvider.FetchDataAsync(IFilterContext context, CancellationToken cancellationToken)
    {
        var r = await FetchDataAsync(context, cancellationToken).ConfigureAwait(false);
        return new PagedResult<object>
        {
            Items = r.Items.Cast<object>(),
            TotalCount = r.TotalCount,
            Page = r.Page,
            PageSize = r.PageSize
        };
    }

    Task<IEnumerable<object>> IAsyncDataProvider.FetchDistinctValuesAsync(string propertyName, string searchText, CancellationToken cancellationToken)
        => FetchDistinctValuesAsync(propertyName, searchText, cancellationToken);
}
