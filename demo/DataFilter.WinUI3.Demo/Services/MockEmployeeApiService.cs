using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Filtering.ExcelLike.Services;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.WinUI3.Demo.Services;

public class MockEmployeeApiService : IMockEmployeeApiService
{
    private List<Employee> _allData;
    private readonly ExcelFilterEngine<Employee> _engine;

    public MockEmployeeApiService(int count = 1000)
    {
        _allData = DataFilter.Demo.Shared.Services.EmployeeDataGenerator.Employees;
        _engine = new ExcelFilterEngine<Employee>();
    }

    public void Regenerate(int count)
    {
        _allData = DataFilter.Demo.Shared.Services.EmployeeDataGenerator.Employees;
    }

    public async Task<PagedResult<Employee>> FetchDataAsync(IFilterContext context, CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(500, cancellationToken);

        var filteredData = _engine.Apply(_allData, context.Descriptors).ToList();

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
}
