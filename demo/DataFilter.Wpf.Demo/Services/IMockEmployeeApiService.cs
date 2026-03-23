using DataFilter.Core.Abstractions;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.Wpf.Demo.Services;

public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>
{
}
