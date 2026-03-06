using DataFilter.Core.Abstractions;
using DataFilter.Wpf.Demo.Models;

namespace DataFilter.Wpf.Demo.Services;

public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>
{
}
