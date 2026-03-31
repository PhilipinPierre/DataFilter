using DataFilter.Demo.Shared.Models;
using DataFilter.Demo.Shared.Services;
using DataFilter.Filtering.ExcelLike.Abstractions;
using DataFilter.Filtering.ExcelLike.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DataFilter.Demo.Shared;

/// <summary>
/// Service registration extensions for demo applications.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers common DataFilter demo services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDataFilterDemoServices(this IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<IExcelFilterEngine<Employee>, ExcelFilterEngine<Employee>>();
        
        // Mock API
        services.AddSingleton<IMockEmployeeApiService, MockEmployeeApiService>();
        
        return services;
    }
}
