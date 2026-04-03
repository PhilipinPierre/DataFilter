using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.Demo.Shared.Services;

/// <summary>
/// Service simulating an API for employee data.
/// </summary>
public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>, IAsyncDataProvider
{
    /// <summary>
    /// Regenerates the underlying employee dataset with the specified count.
    /// </summary>
    /// <param name="count">The number of items to generate.</param>
    void Regenerate(int count);
}
