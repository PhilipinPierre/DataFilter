using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.Maui.Demo.Services;

public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>
{
    void Regenerate(int count);
}
