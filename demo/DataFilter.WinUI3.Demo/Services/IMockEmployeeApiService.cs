using DataFilter.Core.Abstractions;
using DataFilter.Core.Models;
using DataFilter.Demo.Shared.Models;

namespace DataFilter.WinUI3.Demo.Services;

public interface IMockEmployeeApiService : IAsyncDataProvider<Employee>
{
    void Regenerate(int count);
}
